/******************************************************************\
 *      Class Name:     SynonymSource
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Produce both the pointers and definitions for dummy synonyms
\******************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using PluggerBase;
using LanguageNet.Grammarian;

namespace DummyWordNet
{
	public class SynonymSource : IDataSource<string, long[]>, IDataSource<long, WordNetDefinition>
	{
		protected Dictionary<string, long[]> indices;
		protected Dictionary<long, string[]> synonyms;
		
		public SynonymSource()
		{
			indices = new Dictionary<string, long[]>();
			synonyms = new Dictionary<long, string[]>();
		}
		
		public void Add(string word, string[] words) {
			indices.Add(word, new long[] { indices.Count });
			synonyms.Add(indices.Count - 1, words);
		}
		
        #region IDataSource<string,long[]> Members

        public bool TryGetValue(string key, out long[] value)
        {
            return indices.TryGetValue(key, out value);
        }

        IEnumerator<KeyValuePair<string, long[]>> IEnumerable<KeyValuePair<string, long[]>>.GetEnumerator()
        {
            return indices.GetEnumerator();
        }

        #endregion

        #region IDataSource<long,string[]> Members

        public bool TryGetValue(long key, out WordNetDefinition value)
        {
			string[] words;
			if (!synonyms.TryGetValue(key, out words)) {
				value = null;
				return false;
			}
			
			value = new WordNetDefinition();
			value.Words = new List<string>(words);
			return true;
        }

        IEnumerator<KeyValuePair<long, WordNetDefinition>> IEnumerable<KeyValuePair<long, WordNetDefinition>>.GetEnumerator()
        {
			List<KeyValuePair<long, WordNetDefinition>> all = new List<KeyValuePair<long, WordNetDefinition>>();
			foreach (KeyValuePair<long, string[]> words in synonyms) {
				WordNetDefinition one = new WordNetDefinition();
				one.Words = new List<string>(words.Value);
				all.Add(new KeyValuePair<long, WordNetDefinition>(words.Key, one));
			}
			
			return (IEnumerator<KeyValuePair<long, WordNetDefinition>>) all;
        }

        #endregion
		
		IEnumerator IEnumerable.GetEnumerator() {
			var me = this as IEnumerable<KeyValuePair<string, long>>;
	        return me.GetEnumerator();
		}
	}
}

