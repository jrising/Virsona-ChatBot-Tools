/******************************************************************\
 *      Class Name:     DefinitionFile
 *      Written By:     James Rising (borrowed from online source)
 *      Copyright:      2011, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * This file is part of MemcachedWordNet and is free software: you
 * can redistribute it and/or modify it under the terms of the GNU
 * Lesser General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option)
 * any later version.
 * 
 * Plugger Base is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with MemcachedWordNet.  If not, see
 * <http://www.gnu.org/licenses/>.
 *      -----------------------------------------------------------
 * The DefinitionFile provides access (as an IDataSource) to the
 * WordNet definitions in a particular file.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using PluggerBase;
using LanguageNet.Grammarian;
using GenericTools.DataSources;

namespace LanguageNet.WordNet
{
	public class DefinitionFile : IDataSource<long, WordNetDefinition>
	{
		public static long LINE_LEN = 1024 * 25;
		public static char[] Tokenizer = new char[] { ' ', '\n', '\r' };
		
		public static int DONT_KNOW = 0;
		public static int DIRECT_ANT = 1;	/* direct antonyms (cluster head) */
		public static int INDIRECT_ANT = 2;	/* indrect antonyms (similar) */
		public static int PERTAINY = 3;	/* no antonyms or similars (pertainyms) */

		public static int POS_NOUN = 1;
		public static int POS_VERB = 2;
		public static int POS_ADJ = 3;
		public static int POS_ADV = 4;
		
		protected static string lastBaseDir;
		
		protected string filename;
		
		public DefinitionFile(string basedir, WordNetAccess.PartOfSpeech part)
			: this(getFilename(basedir, part)) {
			lastBaseDir = basedir;
		}	

		public DefinitionFile(string filename)
		{
			this.filename = filename;
		}
		
		public static string getFilename(string basedir, WordNetAccess.PartOfSpeech part) {
			return Path.Combine(basedir, string.Format("data.{0}", part.ToString().ToLower()));
		}

        #region IDataSource<long,WordNetDefinition> Members

        public bool TryGetValue(long key, out WordNetDefinition value)
        {
			string data = ReadRecord(key);
			value = ParseDefinition(data, "");
			
			if (value.Position != key)
				throw new ArithmeticException("The stream position is not aligned with the specified offset!");

			return value != null;
        }

        IEnumerator<KeyValuePair<long, WordNetDefinition>> IEnumerable<KeyValuePair<long, WordNetDefinition>>.GetEnumerator()
        {
			Dictionary<long, WordNetDefinition> result = new Dictionary<long, WordNetDefinition>();
			
            StreamReader reader = new StreamReader(filename, true);
			
			do {
				long position = reader.BaseStream.Position;
				string line = reader.ReadLine();
				if (line.Length == 0 || line[0] == ' ')
					continue;
				
				result.Add(position, ParseDefinition(line, ""));
			} while (!reader.EndOfStream);
			
			reader.Close();
			
			return result.GetEnumerator();
        }
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			var me = this as IEnumerable<KeyValuePair<long, WordNetDefinition>>;
	        return me.GetEnumerator();
		}

        #endregion

		/// <summary>
		/// Parses a word definition at the specified offset in the specified file
		/// </summary>
		/// <param name="offset">The offset in the file at which to begin parsing</param>
		/// <param name="dbFileName">The full path of the file to open</param>
		/// <param name="word">The word that will be defined by the parsed definition</param>
		/// <returns>A populated Definition object if successful; otherwise null</returns>
		internal WordNetDefinition ParseDefinition(string data, string exclude)
		{
			WordNetDefinition retVal = null;
			try
			{
				WordNetDefinition tempDef = new WordNetDefinition();;
				if( !string.IsNullOrEmpty( data ) )
				{
					int i = 0;
					bool foundPert = false;
					string[] tokens = data.Split( Tokenizer, StringSplitOptions.RemoveEmptyEntries );

					tempDef.Position = Convert.ToInt64( tokens[ i ] );
					i++;

					tempDef.FileNumber = Convert.ToInt32( tokens[ i ] );
					i++;

					tempDef.PartOfSpeech = tokens[ i ];
					i++;

					if( GetSynSetTypeCode( tempDef.PartOfSpeech ) == WordNetAccess.PartOfSpeech.Satellite )
						tempDef.DefinitionType = INDIRECT_ANT;

					tempDef.WordCount = Convert.ToInt32( tokens[ i ] );
					i++;

					for( int j = 0; j < tempDef.WordCount * 2; j += 2 ) //Step by two for lexid
					{
						string tempWord = tokens[ i + j ];
						if( !string.IsNullOrEmpty( tempWord ) )
							tempDef.Words.Add( DecodeWord( tempWord ) );

						if( tempWord.ToLower() == exclude.ToLower() )
							tempDef.WhichWord = ( i + j );
					}
					i = ( i + ( tempDef.WordCount * 2 ) );

					tempDef.PtrCount = Convert.ToInt32( tokens[ i ] );
					i++;

					for( int j = i; j < ( i + ( tempDef.PtrCount * 4 ) ); j += 4 )
					{
						int pointerIndex = IndexFile.GetPointerTypeIndex( tokens[ j ] );
						long pointerOffset = Convert.ToInt64( tokens[ j + 1 ] );
						int pointerPartOfSpeech = GetPartOfSpeech( Convert.ToChar( tokens[ j + 2 ] ) );
						string lexToFrom = tokens[ j + 3 ];
						int lexFrom = Convert.ToInt32( lexToFrom.Substring( 0, 2 ) );
						int lexTo = Convert.ToInt32( lexToFrom.Substring( 1, 2 ) );

						tempDef.PtrTypes.Add( pointerIndex );
						tempDef.PtrOffsets.Add( pointerOffset );
						tempDef.PtrPartOfSpeech.Add( pointerPartOfSpeech );
						tempDef.PtrFromFields.Add( lexFrom );
						tempDef.PtrToFields.Add( lexTo );

						if( AssertDatabaseType( filename, WordNetAccess.PartOfSpeech.Adj ) && tempDef.DefinitionType == DONT_KNOW )
						{
							if( pointerIndex == PointerTypeConstants.ANTPTR )
							{
								tempDef.DefinitionType = DIRECT_ANT;
							}
							else if( pointerIndex == PointerTypeConstants.PERTPTR )
							{
								foundPert = true;
							}
						}
					}
					i += ( tempDef.PtrCount * 4 );

					if( AssertDatabaseType( filename, WordNetAccess.PartOfSpeech.Adj ) &&
						tempDef.DefinitionType == DONT_KNOW && foundPert )
					{
						tempDef.DefinitionType = PERTAINY;
					}

					if( AssertDatabaseType( filename, WordNetAccess.PartOfSpeech.Verb ) )
					{
						int verbFrames = Convert.ToInt32( tokens[ i ] );
						tempDef.VerbFrameCount = verbFrames;
						i++;

						for( int j = i; j < i + ( tempDef.VerbFrameCount * 3 ); j += 3 )
						{
							int frameId = Convert.ToInt32( tokens[ j + 1 ] );
							int frameTo = Convert.ToInt32( tokens[ j + 2 ] );

							tempDef.FrameIds.Add( frameId );
							tempDef.FrameToFields.Add( frameTo );
						}
						i += ( tempDef.VerbFrameCount * 3 );
					}
					i++;

					string definition = string.Join( " ", tokens, i, tokens.Length - i );
					tempDef.DefinitionText = definition;

					/*tempDef.SenseNumbers = new List<int>( new int[ tempDef.WordCount ] );
					for( int j = 0; j < tempDef.WordCount; j++ )
					{
						tempDef.SenseNumbers[ j ] = GetSearchSense( tempDef, j );
					}*/
				}
				retVal = tempDef;
			}
			catch
			{
				retVal = null;
			}
			return retVal;
		}
		
        #region ReadRecord
        /// <summary>
		/// Reads the record at the specified offset from the specified file
		/// </summary>
		/// <param name="offset">The offset (record id) to read</param>
		/// <param name="dbFileName">The full path of the file to read from</param>
		/// <returns>The record as a string is successfull; otherwise an empty string</returns>
		internal string ReadRecord(long offset)
		{
			string retVal = string.Empty;

            using (StreamReader reader = new StreamReader(filename, true))
			{
				reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                retVal = reader.ReadLine();
				reader.Close();
			}

			return retVal;
        }
        #endregion ReadRecord

		#region GetSynSetTypeCode
		/// <summary>
		/// Converts the text annotation of a speech part into the corosponding POS (Part of Speech)
		/// </summary>
		/// <param name="data">The text to convert</param>
		/// <returns>The corosponding part of spech if successfull; otherwise DbPartOfSpeechType.All</returns>
		public static WordNetAccess.PartOfSpeech GetSynSetTypeCode( string data )
		{
			char pos = data[ 0 ];
			switch( pos )
			{
				case 'n':
					return WordNetAccess.PartOfSpeech.Noun;
				case 'a':
					return WordNetAccess.PartOfSpeech.Adj;
				case 'v':
					return WordNetAccess.PartOfSpeech.Verb;
				case 's':
					return WordNetAccess.PartOfSpeech.Satellite;
				case 'r':
					return WordNetAccess.PartOfSpeech.Adv;
			}
			return WordNetAccess.PartOfSpeech.All;
		}
		#endregion GetSynSetTypeCode
		
		#region GetPartOfSpeech
		/// <summary>
		/// Converts a text annotation of a POS (Part of Speech) to the corosponding numeric value
		/// </summary>
		/// <param name="data">The text to convert</param>
		/// <returns>Any non-negative number is successfull; otherwise -1</returns>
		internal static int GetPartOfSpeech( char data )
		{
			switch( data )
			{
				case 'n':
					return ( POS_NOUN );
				case 'a':
				case 's':
					return ( POS_ADJ );
				case 'v':
					return ( POS_VERB );
				case 'r':
					return ( POS_ADV );
			}
			return -1;
		}
		#endregion GetPartOfSpeech

		#region AssertDatabaseType
		/// <summary>
		/// Determines if the specified database file represents the specified part of speech.
		/// </summary>
		/// <param name="dbFileName">The full path to the database file</param>
		/// <param name="type">The part of speech to check for</param>
		/// <returns>True is the file represents the part of speech; otherwise false</returns>
		private static bool AssertDatabaseType( string dbFileName, WordNetAccess.PartOfSpeech type )
		{
			string strType = Path.GetExtension( dbFileName );
			strType = strType.Substring( 1, strType.Length - 1 );
			return ( strType.ToLower() == type.ToString().ToLower() );
		}
		#endregion AssertDatabaseType
		
		#region PointerTypeConstants
		internal class PointerTypeConstants
		{
			public static int ANTPTR = 1;	/* ! */
			public static int HYPERPTR = 2;	/* @ */
			public static int HYPOPTR = 3;	/* ~ */
			public static int ENTAILPTR = 4;	/* * */
			public static int SIMPTR = 5;	/* & */

			public static int ISMEMBERPTR = 6;	/* #m */
			public static int ISSTUFFPTR = 7;	/* #s */
			public static int ISPARTPTR = 8;	/* #p */

			public static int HASMEMBERPTR = 9;	/* %m */
			public static int HASSTUFFPTR = 10;	/* %s */
			public static int HASPARTPTR = 11;	/* %p */

			public static int MERONYM = 12;	/* % (not valid in lexicographer file) */
			public static int HOLONYM = 13;	/* # (not valid in lexicographer file) */
			public static int CAUSETO = 14;	/* > */
			public static int PPLPTR = 15;	/* < */
			public static int SEEALSOPTR = 16;	/* ^ */
			public static int PERTPTR = 17;	/* \ */
			public static int ATTRIBUTE = 18;	/* = */
			public static int VERBGROUP = 19;	/* $ */
			public static int DERIVATION = 20;	/* + */
			public static int CLASSIFICATION = 21;	/* ; */
			public static int CLASS = 22;	/* - */
		}
		#endregion PointerTypeConstants
		
		#region DecodeWord
		/// <summary>
		/// Converts a normalized string into a presentable format
		/// </summary>
		/// <param name="data">The normalized string</param>
		/// <returns>A presentable format string</returns>
		public static string DecodeWord( string data )
		{
			string retVal = string.Empty;
			retVal = data.Replace( '_', ' ' );
			return retVal;
		}
		#endregion DecodeWord

	}
}

