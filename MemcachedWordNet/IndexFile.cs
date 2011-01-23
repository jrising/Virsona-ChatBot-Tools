/******************************************************************\
 *      Class Name:     IndexFile
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
 * The IndexFile provides access (as an IDataSource) to the
 * WordNet pointers in a particular index file.
\******************************************************************/
using System;
using System.IO;
using System.Collections.Generic;
using PluggerBase;
using GenericTools.Enumerables;
using LanguageNet.Grammarian;
using GenericTools.DataSources;

namespace LanguageNet.WordNet
{
	public class IndexFile : AlphabeticFile<Index>, IDataSource<string, long[]>
	{
		public static int KEY_LEN = 1024;
		public static char[] Tokenizer = new char[] { ' ', '\n', '\r' };
		
		public IndexFile(string basedir, WordNetAccess.PartOfSpeech part)
			: this(getFilename(basedir, part)) {
		}	
		
		public IndexFile(string filename)
			: base(filename, Tokenizer, KEY_LEN)
		{
		}
		
		public static string getFilename(string basedir, WordNetAccess.PartOfSpeech part) {
			return Path.Combine(basedir, string.Format("index.{0}", part.ToString().ToLower()));
		}
		
        public override Index ReadStringEntry(string line)
        {
			return ParseIndex(line);
        }
		
        #region IDataSource<string,long[]> Members

        public bool TryGetValue(string key, out long[] value)
        {
			Index index;
			if (!TryGetValue(key, out index)) {
				value = null;
				return false;
			}
			
			value = index.SynSetsOffsets.ToArray();
			return true;
        }

        IEnumerator<KeyValuePair<string, long[]>> IEnumerable<KeyValuePair<string, long[]>>.GetEnumerator()
        {
			return (new MapEnumerable<KeyValuePair<string, Index>, KeyValuePair<string, long[]>>(this, ExtractOffsets, null)).GetEnumerator();
        }

		public static KeyValuePair<string, long[]> ExtractOffsets(KeyValuePair<string, Index> index, object shared)
        {
			return new KeyValuePair<string, long[]>(index.Key, index.Value.SynSetsOffsets.ToArray());
        }

        #endregion
		
		public static long[] ExtractOffsets(Index index, object shared) {
			return index.SynSetsOffsets.ToArray();
		}
		
        /// <summary>
        /// Parses an index structure in a string
        /// </summary>
        /// <param name="data">The string containing the index</param>
        /// <returns>A populated index structure in successful; otherwise an empty index structure</returns>
		public static Index ParseIndex(string data) {
            Index retVal = Index.Empty;
            retVal.IdxOffset = 0;
            retVal.OffsetCount = 0;
            retVal.PartOfSpech = string.Empty;
            retVal.PointersUsed = new List<int>();
            retVal.PointersUsedCount = 0;
            retVal.SenseCount = 0;
            retVal.SynSetsOffsets = new List<long>();
            retVal.TaggedSensesCount = 0;
            retVal.Word = string.Empty;

            if (!string.IsNullOrEmpty(data))
            {
                int i = 0;
                string[] tokens = data.Split(Tokenizer, StringSplitOptions.RemoveEmptyEntries);

                retVal.IdxOffset = 0;

                retVal.Word = tokens[i];
                i++;

                retVal.PartOfSpech = tokens[i];
                i++;

                retVal.SenseCount = Convert.ToInt32(tokens[i]);
                i++;

                retVal.PointersUsedCount = Convert.ToInt32(tokens[i]);
                i++;

                for (int j = 0; j < retVal.PointersUsedCount; j++)
                {
                    int pointerIndex = GetPointerTypeIndex(tokens[i + j]);
                    retVal.PointersUsed.Add(pointerIndex);
                }
                i = (i + retVal.PointersUsedCount);

                retVal.OffsetCount = Convert.ToInt32(tokens[i]);
                i++;

                retVal.TaggedSensesCount = Convert.ToInt32(tokens[i]);
                i++;

                for (int j = 0; j < retVal.OffsetCount; j++)
                {
                    long synSetOffset = Convert.ToInt64(tokens[i + j]);
                    retVal.SynSetsOffsets.Add(synSetOffset);
                }
            }

            return retVal;
		}

		#region GetPointerTypeIndex
		/// <summary>
		/// Converts a text annotation of a marker into its numeric equivelant
		/// </summary>
		/// <param name="pointerMark">The marker text to convert</param>
		/// <returns>Any non-negative numer if successfull; otherwise -1</returns>
		internal static int GetPointerTypeIndex( string pointerMark )
		{
			int retVal = -1;
			for( int i = 0; i < PointerTypes.Length; i++ )
			{
				string pointer = PointerTypes[ i ];
				if( pointer.ToLower().Trim() == pointerMark.ToLower().Trim() )
				{
					retVal = i;
					break;
				}
			}
			return retVal;
		}
		#endregion GetPointerTypeIndex
		
		#region PointerTypes
		public static string[] PointerTypes = new string[]
			{
				"",				/* 0 not used */
				"!",			/* 1 ANTPTR */
				"@",			/* 2 HYPERPTR */
				"~",			/* 3 HYPOPTR */
				"*",			/* 4 ENTAILPTR */
				"&",			/* 5 SIMPTR */
				"#m",			/* 6 ISMEMBERPTR */
				"#s",			/* 7 ISSTUFFPTR */
				"#p",			/* 8 ISPARTPTR */
				"%m",			/* 9 HASMEMBERPTR */
				"%s",			/* 10 HASSTUFFPTR */
				"%p",			/* 11 HASPARTPTR */
				"%",			/* 12 MERONYM */
				"#",			/* 13 HOLONYM */
				">",			/* 14 CAUSETO */
				"<",			/* 15 PPLPTR */
				"^",			/* 16 SEEALSO */
				"\\",			/* 17 PERTPTR */
				"=",			/* 18 ATTRIBUTE */
				"$",			/* 19 VERBGROUP */
				"+",		        /* 20 NOMINALIZATIONS */
				";",			/* 21 CLASSIFICATION */
				"-",			/* 22 CLASS */
			/* additional searches, but not pointers.  */
				"",				/* SYNS */
				"",				/* FREQ */
				"+",			/* FRAMES */
				"",				/* COORDS */
				"",				/* RELATIVES */
				"",				/* HMERONYM */
				"",				/* HHOLONYM */
				"",				/* WNGREP */
				"",				/* OVERVIEW */
				";c",			/* CLASSIF_CATEGORY */
				";u",			/* CLASSIF_USAGE */
				";r",			/* CLASSIF_REGIONAL */
				"-c",			/* CLASS_CATEGORY */
				"-u",			/* CLASS_USAGE */
				"-r",			/* CLASS_REGIONAL */
				"@i",			/* 38 INSTANCE */
				"~i"			/* 39 INSTANCES */
			};
		#endregion PointerTypes

	}
}

