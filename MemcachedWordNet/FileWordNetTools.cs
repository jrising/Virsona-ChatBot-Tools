using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LanguageNet.Grammarian;

namespace LanguageNet.WordNet
{
	public class FileWordNetTools
	{
		public static long KEY_LEN = ( 1024 );
		public static long LINE_LEN = ( 1024 * 25 );

		protected string dictdir;
		
		public FileWordNetTools(string dictdir)
		{
			this.dictdir = dictdir;
		}
		
		#region DbType
		/// <summary>
		/// Denotes the a type of database
		/// </summary>
		internal enum DbType
		{
			Index = 1,
			Data = 2
		}
		#endregion DbType
		
		#region GetIndexForType
		/// <summary>
		/// Builds a list of dictionary index files for the specified Part of Speech (POS)
		/// </summary>
		/// <param name="type">The POS to return the index of</param>
		/// <returns>A list of dictionary file paths is successfull; otherwise an empty list</returns>
		public List<string> GetIndexForType( WordNetAccess.PartOfSpeech type )
		{
			List<string> retVal = new List<string>();

			if ( type == WordNetAccess.PartOfSpeech.All )
			{
				retVal.Add( GetDbIndexPath( WordNetAccess.PartOfSpeech.Adj ) );
				retVal.Add( GetDbIndexPath( WordNetAccess.PartOfSpeech.Adv ) );
				retVal.Add( GetDbIndexPath( WordNetAccess.PartOfSpeech.Noun ) );
				retVal.Add( GetDbIndexPath( WordNetAccess.PartOfSpeech.Verb ) );
			}
			else
			{
				retVal.Add( GetDbIndexPath( type ) );
			}

			return retVal;
		}
		#endregion GetIndexForType


		#region GetDBaseForType
		/// <summary>
		/// Builds a list of dictionary data files for the specified Part of Speech (POS)
		/// </summary>
		/// <param name="type">The POS to return the data of</param>
		/// <returns>A list of dictionary file paths is successfull; otherwise an empty list</returns>
		public List<string> GetDBaseForType(WordNetAccess.PartOfSpeech type)
		{
			List<string> retVal = new List<string>();

			if ( type == WordNetAccess.PartOfSpeech.All )
			{
				retVal.Add( GetDbDataPath( WordNetAccess.PartOfSpeech.Adj ) );
				retVal.Add( GetDbDataPath( WordNetAccess.PartOfSpeech.Adv ) );
				retVal.Add( GetDbDataPath( WordNetAccess.PartOfSpeech.Noun ) );
				retVal.Add( GetDbDataPath( WordNetAccess.PartOfSpeech.Verb ) );
			}
			else
			{
				retVal.Add( GetDbDataPath( type ) );
			}

			return retVal;
		}
		#endregion GetDBaseForType
		
		#region GetDbDataPath
		/// <summary>
		/// Builds the expected dictionary data file for the specified Part of Speech (POS)
		/// </summary>
		/// <param name="type">The POS to build the path for</param>
		/// <returns>The expected dictionary data file</returns>
		private string GetDbDataPath( WordNetAccess.PartOfSpeech type )
		{
			return GetDbFilePath( DbType.Data, type );
		}
		#endregion GetDbDataPath

		#region GetDbIndexPath
		/// <summary>
		/// Builds the expected dictionary index file for the specified Part of Speech (POS)
		/// </summary>
		/// <param name="type">The POS to build the path for</param>
		/// <returns>The expected dictionary index file</returns>
		private string GetDbIndexPath( WordNetAccess.PartOfSpeech type )
		{
			return GetDbFilePath( DbType.Index, type );
		}
		#endregion GetDbIndexPath

		#region GetDbFilePath
		/// <summary>
		/// Builds the expected database file path
		/// </summary>
		/// <param name="db">The type of database file</param>
		/// <param name="pos">The part of speech</param>
		/// <returns>The expected database file path</returns>
		private string GetDbFilePath( DbType db, WordNetAccess.PartOfSpeech pos )
		{
            string lcdb = db.ToString().ToLower();
            string lcpos = pos.ToString().ToLower();
			return Path.Combine( dictdir, string.Format( "{0}.{1}", lcdb, lcpos ) );
		}
		#endregion GetDbFilePath
		
        #region GetIndex
        /// <summary>
        /// Returns a list of the Index objects stored in the cache corresponding to the given string and part(s) of speech
        /// </summary>
        /// <param name="word">The string to use as a key</param>
        /// <param name="part">The part of speech limitations</param>
        /// <returns>The Index objects</returns>
        public List<Index> GetIndex(string word, WordNetAccess.PartOfSpeech part)
        {
            if (word.Length == 0)
                return new List<Index>();

            word = WordNetInterface.EncodeWord(word);

            // search in the file
            List<Index> result = new List<Index>();
            List<string> fileListIndex = GetIndexForType(part);
            for (int i = 0; i < fileListIndex.Count; i++)
            {
                long offset = FastSearch(word, fileListIndex[i], IndexFile.Tokenizer);
                if (offset > 0)
                {
                    Index index = ParseIndexAt(offset, fileListIndex[i]);
                    result.Add(index);
                }
            }
            return result;
        }
        #endregion GetIndex

        #region GetDefinition
        /// <summary>
		/// Finds definitions for a specified word
		/// </summary>
		/// <param name="word">The word to define</param>
		/// <returns>A dictionary of wrods and definition pertenant to the specified word</returns>
		public Dictionary<string, List<WordNetDefinition>> GetDefinition( string word )
		{
			Dictionary<string, List<WordNetDefinition>> retVal = new Dictionary<string, List<WordNetDefinition>>();
			List<string> fileListIndex = GetIndexForType( WordNetAccess.PartOfSpeech.All );
			List<string> fileListData = GetDBaseForType( WordNetAccess.PartOfSpeech.All );

			for( int i = 0; i < fileListIndex.Count; i++ )
			{
				long offset = FastSearch(word.ToLower(), fileListIndex[i], IndexFile.Tokenizer);
				if( offset > 0 )
				{
					Index idx = ParseIndexAt( offset, fileListIndex[ i ] );
					foreach( long synSetOffset in idx.SynSetsOffsets )
					{
						try
						{
							WordNetDefinition def = ParseDefinitionAt( synSetOffset, fileListData[ i ], word );
							string wordKey = string.Join( ", ", def.Words.ToArray() );
							if( !retVal.ContainsKey( wordKey ) )
								retVal.Add( wordKey, new List<WordNetDefinition>() );

							retVal[ wordKey ].Add( def );
						}
						catch( Exception ex )
						{
							string message = ex.Message;
						}
					}
				}
			}

			return retVal;
		}
        #endregion GetDefinition


        #region ReadRecord
        /// <summary>
		/// Reads the record at the specified offset from the specified file
		/// </summary>
		/// <param name="offset">The offset (record id) to read</param>
		/// <param name="dbFileName">The full path of the file to read from</param>
		/// <returns>The record as a string is successfull; otherwise an empty string</returns>
		internal static string ReadRecord(long offset, string dbFileName)
		{
			string retVal = string.Empty;

            using( StreamReader reader = new StreamReader( dbFileName, true ) )
			{
				reader.BaseStream.Seek( offset, SeekOrigin.Begin );
                retVal = reader.ReadLine();
				reader.Close();
			}

			return retVal;
        }
        #endregion ReadRecord

        #region ReadPartialRecord
        /// <summary>
        /// Reads the start of the record at the specified offset from the specified file,
        ///   (and possibly some of whatever is after it)
        /// </summary>
        /// <param name="offset">The offset (record id) to read</param>
        /// <param name="dbFileName">The full path of the file to read from</param>
        /// <param name="count">How many characters to read</param>
        /// <returns>The start of the record as a string is successfull; otherwise an empty string</returns>
        internal static string ReadPartialRecord(long offset, string dbFileName, int count)
        {
            char[] buffer = new char[count];

            using (StreamReader reader = new StreamReader(dbFileName, true))
            {
                reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                reader.Read(buffer, 0, count);
                reader.Close();
            }

            return new string(buffer);
        }
        #endregion ReadPartialRecord
		
        #region ParseIndexAt
        /// <summary>
		/// Parses an index structure from the specified file at the specified offset
		/// </summary>
		/// <param name="offset">The offset in the file at which the index exists</param>
		/// <param name="dbFileName">The full path to the database file to open</param>
		/// <returns>A populated index structure in successful; otherwise an empty index structure</returns>
		internal static Index ParseIndexAt(long offset, string dbFileName)
		{
			string data = ReadRecord(offset, dbFileName);
            Index idx = IndexFile.ParseIndex(data);
            idx.IdxOffset = offset;

            return idx;
		}
		#endregion ParseIndexAt
		
        #region ParseDefinitionAt
        /// <summary>
		/// Parses an definition structure from the specified file at the specified offset
		/// </summary>
		/// <param name="offset">The offset in the file at which the index exists</param>
		/// <param name="dbFileName">The full path to the database file to open</param>
		/// <returns>A populated index structure in successful; otherwise an empty index structure</returns>
		internal WordNetDefinition ParseDefinitionAt(long offset, string dbFileName, string word)
		{
			string data = ReadRecord(offset, dbFileName);
            WordNetDefinition def = DefinitionFile.ParseDefinition(data, word, dbFileName);
           	def.Position = offset;

            return def;
		}
		#endregion ParseIndexAt

		#region FastSearch
        /// <summary>
		/// Searches the specified file for the specified keyword
		/// </summary>
		/// <param name="keyword">The keyword to find</param>
		/// <param name="dbFileName">The full path to the file to search in</param>
		/// <returns>The offset in the file at which the word was found; otherwise 0</returns>
		internal static long FastSearch(string keyword, string dbFileName, char[] tokenizer)
		{
			long retVal = 0L;
			string key = string.Empty;
			Encoding enc = Encoding.Default;

			using( StreamReader reader = new StreamReader( dbFileName, true ) )
			{
				enc = reader.CurrentEncoding;
				reader.Close();
			}

			using( FileStream fs = File.OpenRead( dbFileName ) )
			{
				long diff = 666;
				string line = string.Empty;

				fs.Seek( 0, SeekOrigin.End );
				long top = 0;
				long bottom = fs.Position;
				long mid = ( bottom - top ) / 2;

				do
				{
					fs.Seek( mid - 1, SeekOrigin.Begin );
					if( mid != 1 )
					{
						while( fs.ReadByte() != '\n' && fs.Position < fs.Length )
						{
							retVal = fs.Position;
						}
					}

					byte[] btData = new byte[ KEY_LEN ];
					int count = fs.Read( btData, 0, btData.Length );
					fs.Seek( fs.Position - count, SeekOrigin.Begin );

					string readData = enc.GetString( btData );
                    if (readData[0] == ' ')
                    {
                        // oops-- too far!
                        bottom = mid;
                        diff = (bottom - top) / 2;
                        mid = top + diff;
                    }
                    else
                    {
                        key = readData.TrimStart(tokenizer).Split(tokenizer)[0];
                        int comparison = string.Compare(key, keyword, StringComparison.Ordinal);

                        if (comparison != 0)
                        {
                            if (comparison > 0)
                            {
                                bottom = mid;
                                diff = (bottom - top) / 2;
                                mid = top + diff;
                            }

                            if (comparison < 0)
                            {
                                top = mid;
                                diff = (bottom - top) / 2;
                                mid = top + diff;
                            }
                        }
                    }
				}
                while (string.Compare(key, keyword, StringComparison.Ordinal) != 0 && diff != 0);
			}

            if (string.Compare(key, keyword, StringComparison.Ordinal) != 0)
				retVal = 0L;
			else
				retVal++;

			return retVal;
		}
		#endregion FastSearch

	}
}

