using System;
using System.Collections.Generic;
using System.Text;
using LanguageNet.Grammarian;

namespace LanguageNet.WordNet
{
	public class Thesaurus
	{
        public enum SynonymLevel {
            OneFull,
            OnePartials,
            TwoPartials,
        }
		
        #region GetSynonyms
        /// <summary>
        /// Find all first-level synonyms
        /// </summary>
        /// <param name="word">The word to look up</param>
        /// <returns>A list of all synonyms for all senses, and how many of each</returns>
        public static Dictionary<string, double> GetSynonyms(WordNetInterface iface, string word, WordNetAccess.PartOfSpeech part, SynonymLevel level, double scalePower, out List<WordNetAccess.PartOfSpeech> partsFound)
        {
            Dictionary<string, double> retVal = new Dictionary<string, double>();
            partsFound = new List<WordNetAccess.PartOfSpeech>();
            List<Index> indices = iface.GetIndex(word.ToLower(), part);
            foreach (Index index in indices)
            {
                partsFound.Add(index.DbPartOfSpeech);
				// TODO: redo GetDefinitionSynonyms and friends (below) to not need filenames
                List<string> fileNames = DbFileHelper.GetDBaseForType(index.DbPartOfSpeech);
                foreach (long synSetOffset in index.SynSetsOffsets)
                {
                    List<string> synwords;
                    if (level == SynonymLevel.OneFull)
                        synwords = GetDefinitionSynonyms(synSetOffset, fileNames[0]);
                    else if (level == SynonymLevel.OnePartials)
                        synwords = GetPartialDefinitionSynonyms(synSetOffset, fileNames[0]);
                    else
                        synwords = GetDoublePartialDefinitionSynonyms(synSetOffset, fileNames[0]);
                    foreach (string synword in synwords)
                    {
                        string hiword = synword.ToUpper();
                        double count = 0;
                        retVal.TryGetValue(hiword, out count);
                        retVal[hiword] = count + 1;
                    }
                }
            }

            return CountsToSynonyms(word, scalePower, retVal);
        }
        #endregion GetSynonyms

        #region GetExactSynonyms
        /// <summary>
        /// Find only those synonyms which unambiguously mean the same thing
        /// </summary>
        public static List<string> GetExactSynonyms(string word, WordNetAccess.PartOfSpeech part)
        {
            List<Index> indices = DictionaryHelper.GetIndex(word.ToLower(), part);
            if (indices.Count != 1)
                return null;    // ambiguous or none

            Index index = indices[0];
            if (index.SynSetsOffsets.Count != 1)
                return null;    // ambiguous

            List<string> fileNames = DbFileHelper.GetDBaseForType(index.DbPartOfSpeech);
            long synSetOffset = index.SynSetsOffsets[0];

            List<string> synwords = FileParser.GetDefinitionSynonyms(synSetOffset, fileNames[0]);
            if (synwords.Count == 0)
                return null;

            return synwords;
        }
        #endregion

        #region CountsToSynonyms
        // Returns weighted synonyms
        // Returns null if word not found
        internal static Dictionary<string, double> CountsToSynonyms(string word, double scalePower, Dictionary<string, double> counts)
        {
            if (counts.Count == 0)
                return null;

            string hiword = word.ToUpper();
            double hicount = 0;
            if (counts.TryGetValue(hiword, out hicount))
                counts.Remove(hiword);
            hicount++;

            foreach (KeyValuePair<string, double> kvp in counts)
                if (kvp.Value > hicount)
                    hicount = kvp.Value + 1;

            Dictionary<string, double> result = new Dictionary<string, double>();
            foreach (KeyValuePair<string, double> kvp in counts)
                result[kvp.Key] = Math.Pow(kvp.Value / hicount, scalePower);

            return result;
        }
        #endregion CountsToSynonyms
		
        #region GetDefinitionSynonyms
        /// <summary>
        /// Parses a word definition at the specified offset in the specified file
        ///  and just returns the synonyms
        /// </summary>
        /// <param name="offset">The offset in the file at which to begin parsing</param>
        /// <param name="dbFileName">The full path of the file to open</param>
        /// <returns>A populated Definition object is successful; otherwise null</returns>
        public static List<string> GetDefinitionSynonyms(long offset, string dbFileName)
        {
            List<string> retVal = new List<string>();
            try
            {
                string data = ReadRecord(offset, dbFileName);
                if (!string.IsNullOrEmpty(data))
                {
                    int i = 0;
                    string[] tokens = data.Split(Constants.Tokenizer, StringSplitOptions.RemoveEmptyEntries);

                    long position = Convert.ToInt64(tokens[i]);
                    i++;

                    if (position != offset)
                        throw new ArithmeticException("The stream position is not aligned with the specified offset!");

                    i += 2;

                    int wordCount = Convert.ToInt32(tokens[i], 16);
                    i++;

                    for (int j = 0; j < wordCount * 2; j += 2) //Step by two for lexid
                    {
                        string tempWord = tokens[i + j];
                        if (!string.IsNullOrEmpty(tempWord))
                            retVal.Add(DecodeWord(tempWord));
                    }
                }
            }
            catch
            {
                // don't do anything-- just don't add the word!
            }
            return retVal;
        }
        #endregion GetDefinitionSynonyms

        #region GetDoublePartialDefinitionSynonyms
        /// <summary>
        /// Parses a word definition at the specified offset in the specified file
        ///  and returns the full set of synonyms (two levels of synsets)
        /// </summary>
        /// <param name="offset">The offset in the file at which to begin parsing</param>
        /// <param name="dbFileName">The full path of the file to open</param>
        /// <returns>A populated Definition object is successful; otherwise null</returns>
        public static List<string> GetDoublePartialDefinitionSynonyms(long offset, string dbFileName)
        {
            List<string> retVal = new List<string>();
            try
            {
                string data = ReadRecord(offset, dbFileName);
                if (!string.IsNullOrEmpty(data))
                {
                    int i = 0;
                    string[] tokens = data.Split(Constants.Tokenizer, StringSplitOptions.RemoveEmptyEntries);

                    long position = Convert.ToInt64(tokens[i]);
                    i++;

                    if (position != offset)
                        throw new ArithmeticException("The stream position is not aligned with the specified offset!");

                    i++; // skip file number
                    char partOfSpeech = tokens[i][0];
                    i++;

                    int wordCount = Convert.ToInt32(tokens[i], 16);
                    i++;

                    for (int j = 0; j < wordCount * 2; j += 2) //Step by two for lexid
                    {
                        string tempWord = tokens[i + j];
                        if (!string.IsNullOrEmpty(tempWord))
                        {
                            // it's a first level synonym-- add it twice!
                            retVal.Add(DecodeWord(tempWord));
                            retVal.Add(DecodeWord(tempWord));
                        }
                    }
                    i += wordCount * 2;

                    int ptrCount = Convert.ToInt32(tokens[i]);
                    i++;

                    for (int j = i; j < (i + (ptrCount * 4)); j += 4)
                    {
                        if (tokens[j + 2][0] == partOfSpeech && tokens[j][0] != '!')
                        {
                            // Look up these too!
                            long pointerOffset = Convert.ToInt64(tokens[j + 1]);
                            List<string> synonyms = GetPartialDefinitionSynonyms(pointerOffset, dbFileName);
                            retVal.AddRange(synonyms);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                // don't do anything-- just don't add the word!
            }
            return retVal;
        }
        #endregion GetDefinitionSynonyms

        #region GetPartialDefinitionSynonyms
        /// <summary>
        /// Parses a word definition at the specified offset in the specified file
        ///  and just returns the synonyms
        /// </summary>
        /// <param name="offset">The offset in the file at which to begin parsing</param>
        /// <param name="dbFileName">The full path of the file to open</param>
        /// <returns>A populated Definition object is successful; otherwise null</returns>
        public static List<string> GetPartialDefinitionSynonyms(long offset, string dbFileName)
        {
            List<string> retVal = new List<string>();
            try
            {
                string data = ReadPartialRecord(offset, dbFileName, 128);
                if (!string.IsNullOrEmpty(data))
                {
                    int i = 0;
                    string[] tokens = data.Split(Constants.Tokenizer, 24, StringSplitOptions.RemoveEmptyEntries);

                    long position = Convert.ToInt64(tokens[i]);
                    i++;

                    if (position != offset)
                        throw new ArithmeticException("The stream position is not aligned with the specified offset!");
                    i += 2;

                    int wordCount = Convert.ToInt32(tokens[i], 16);
                    i++;

                    for (int j = 0; j < wordCount * 2 && j + i < tokens.Length; j += 2) //Step by two for lexid
                    {
                        string tempWord = tokens[i + j];
                        if (!string.IsNullOrEmpty(tempWord))
                            retVal.Add(DecodeWord(tempWord));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                // don't do anything-- just don't add the word!
            }
            return retVal;
        }
        #endregion GetPartialDefinitionSynonyms

    }
}
