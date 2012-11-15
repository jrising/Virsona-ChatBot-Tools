/******************************************************************\
 *      Class Name:     WordFrequencies
 *      Written By:     James.R
 *      Copyright:      Virsona Inc
 *
 *      Modifications:
 *      -----------------------------------------------------------
 *      Date            Author          Modification
 *
\******************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using GenericTools;
using GenericTools.DataSources;

namespace MetricSalad
{
    public class WordFrequencies
    {
        public string baseDirectory;
        public static readonly string wordFilename = "wordFrequencies.csv";
        public static readonly string stemFilename = "stemFrequencies.csv";
        public static readonly string wfPrefix = "%";
        public static readonly string sfPrefix = "%~";
		
		public BackedMemcachedSource<ThreeTuple<uint, double, double>> wordCache;
		public BackedMemcachedSource<ThreeTuple<uint, double, double>> stemCache;
		
		public WordFrequencies(string baseDirectory) {
			this.baseDirectory = baseDirectory;
			WordFrequencyFileSource wordSource = new WordFrequencyFileSource(baseDirectory + wordFilename);
			WordFrequencyFileSource stemSource = new WordFrequencyFileSource(baseDirectory + stemFilename);
			
			this.wordCache = new BackedMemcachedSource<ThreeTuple<uint, double, double>>(wordSource, wfPrefix, MemcacheSource.DefaultClient());
			this.stemCache = new BackedMemcachedSource<ThreeTuple<uint, double, double>>(stemSource, sfPrefix, MemcacheSource.DefaultClient());			
		}
		
        #region GetWordFrequency
        /// <summary>
        /// Get the frequency of a word. This method does not currently use Memcached
        /// </summary>
        /// <param name="word">The word to look up</param>
        /// <returns>A number between 0 and 1.</returns>
        public double GetWordFrequency(string word)
        {
			ThreeTuple<uint, double, double> value;
			if (wordCache.TryGetValue(word, out value))
				return value.two;
			
			return 1.0;	// full frequency!
        }
        #endregion GetWordFrequency

        #region Weight Methods
        /// <summary>
        /// Get the weight associated with a word
        /// </summary>
        /// <param name="word">The word to look up</param>
        /// <returns>A number between 0 and 1.</returns>
        public double GetWordWeight(string word)
        {
            double result = 1.0;
            if (word.Contains(" "))
            {
                // split this up into pieces
                string[] subwords = word.Split(null);
                foreach (string subword in subwords)
                    result *= 1.0 - GetWordWeight(subword);

                return 1.0 - result;
            }
						
			ThreeTuple<uint, double, double> value;
			if (wordCache.TryGetValue(word, out value))
				return value.three;
			
			return 1.0;	// full weight!
        }

        /// <summary>
        /// Get the weight associated with a stem
        /// </summary>
        /// <param name="word">The stem to look up</param>
        /// <returns>A number between 0 and 1.</returns>
        public double GetStemWeight(string stem)
        {
			ThreeTuple<uint, double, double> value;
			if (stemCache.TryGetValue(stem, out value))
				return value.two;
			
			return 1.0;	// full weight!
        }
        #endregion Weight Methods

        #region Loading Functions
        /// <summary>
        /// Load all word and stem weights into memcached
        /// </summary>
        public void LoadIntoCache()
        {
			wordCache.LoadIntoMemcached();
			stemCache.LoadIntoMemcached();
        }

        /// <summary>
        /// Load all word weights into a hash table
        /// </summary>
        public Dictionary<string, double> GetAllFrequencies(string filePath)
        {
            Dictionary<string, double> retVal = new Dictionary<string, double>();

            using (StreamReader reader = new StreamReader(filePath, true))
            {
                reader.ReadLine();  // skip the first line
                while (!reader.EndOfStream)
                {
                    string line = reader.ReadLine();
                    double weight = 0;
                    string word = ParseColumn(line, 3, out weight);
                    if (word != null)
                        retVal.Add(word, weight);
                }

                reader.Close();
            }

            return retVal;
        }
        #endregion Loading Functions

        #region Utilities
        /// <summary>
        /// Parse a CSV line
        /// </summary>
        static String[] CsvLineToArray(String line)
        {
          String pattern = ",(?=(?:[^\"]*\"[^\"]*\")*(?![^\"]*\"))";
          Regex r = new Regex(pattern);

          return r.Split(line);
        }

        /// <summary>
        /// Parse a CSV line and return the word and a single column
        /// </summary>
        static string ParseColumn(String line, int which, out double weight)
        {
            string[] tokens = CsvLineToArray(line);
            if (!double.TryParse(tokens[which], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out weight))
                return null;
            return tokens[0].Trim('"');
        }
        #endregion
    }
}
