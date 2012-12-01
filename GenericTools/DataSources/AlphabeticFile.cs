/******************************************************************\
 *      Class Name:     AlphabeticFile
 *      Written By:     James Rising
 *      Copyright:      2011, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * An abstract class which manages the data in an alphabetically
 * sorted file.  Subclasses may produce the rest of the lines of
 * data associated with each term differently.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using PluggerBase;

namespace GenericTools.DataSources
{
    public abstract class AlphabeticFile<T> : IDataSource<string, T>
    {
        protected string filename;
        protected char[] tokenizer;
        protected int keylen;

        protected Encoding enc;

        public AlphabeticFile(string filename, char[] tokenizer, int keylen)
        {
            this.filename = filename;
            this.tokenizer = tokenizer;
            this.keylen = keylen;

            enc = Encoding.Default;

            using (StreamReader reader = new StreamReader(filename, true))
            {
                enc = reader.CurrentEncoding;
                reader.Close();
            }
        }

        public abstract T ReadStringEntry(string line);

        public virtual T ReadStreamEntry(FileStream fs) {
		    StreamReader reader = new StreamReader(fs);
            string line = reader.ReadLine();
            return ReadStringEntry(line);
		}
		
        #region TryGetValue
        /// <summary>
		/// Searches the specified file for the specified keyword
		/// </summary>
		/// <returns>The offset in the file at which the word was found; otherwise 0</returns>
        public bool TryGetValue(string keyword, out T value)
        {
			string key = string.Empty;
            int comparison = 0;
            int lastReadCount = 0;

            FileStream fs = File.OpenRead(filename);

            List<char> tokenlist = new List<char>(tokenizer);

			fs.Seek(0, SeekOrigin.End);
			long top = 0;
			long bottom = fs.Position;
			long mid = (bottom - top) / 2;

            do
			{
                // Go to a random point in the file
				fs.Seek(mid - 1, SeekOrigin.Begin);
                // Go to the end of the line
				if (mid != 1)
					while (fs.ReadByte() != '\n' && fs.Position < fs.Length);

                // Read the start of the line
				byte[] btData = new byte[keylen];
				lastReadCount = fs.Read(btData, 0, btData.Length);

                // Translate into a string
				string readData = enc.GetString(btData);
                if (readData.Length == 0 || tokenlist.Contains(readData[0]))
                {
                    // oops-- too far!
                    bottom = mid;
                    mid = (bottom + top) / 2;
                }
                else
                {
                    if (readData[0] == '#')
                    {
                        // Comment line-- always go forward
                        top = mid;
                        mid = (bottom + top) / 2;
                    }
                    else
                    {
                        // get out the key
                        key = readData.Split(tokenizer)[0];
                        // compare this to what we want
                        comparison = string.Compare(key, keyword, StringComparison.Ordinal);

                        if (comparison != 0)
                        {
                            if (comparison > 0)
                            {
                                bottom = mid;
                                mid = (bottom + top) / 2;
                            }

                            if (comparison < 0)
                            {
                                top = mid;
                                mid = (bottom + top) / 2;
                            }
                        }
                    }
                }
			}
            while (comparison != 0 && top + 2 < bottom);

            if (comparison != 0)
            {
                value = default(T);
				fs.Close();
                return false;
            }

            fs.Seek(fs.Position - lastReadCount, SeekOrigin.Begin);
            value = ReadStreamEntry(fs);
			fs.Close();
            return true;
		}
		#endregion

        #region GetEnumerator
        /// <summary>
        /// Load everything from a file
        /// </summary>
        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            Dictionary<string, T> retVal = new Dictionary<string, T>();
            List<char> tokenlist = new List<char>(tokenizer);

            using (StreamReader sr = new StreamReader(filename, true))
            {
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();

                    if (line.Length == 0 || line[0] == '#' || tokenlist.Contains(line[0]))
                        continue;   // header-- skip it!

                    // get out the key
                    string key = line.Split(tokenizer)[0];

                    retVal[key] = ReadStringEntry(line);
                }

                sr.Close();
            }

            return retVal.GetEnumerator();
        }
        #endregion
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			var me = this as IEnumerable<KeyValuePair<string, T>>;
	        return me.GetEnumerator();
		}
    }
}
