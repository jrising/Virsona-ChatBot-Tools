/******************************************************************\
 *      Class Name:     ComboSource
 *      Written By:     James Rising
 *      Copyright:      2011, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * A source that produces the contents of two other sources
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase;
using GenericTools.Enumerables;

namespace GenericTools.DataSources
{
    public class ComboSource<TKey, TValue> : IDataSource<TKey, TValue>
    {
        protected IDataSource<TKey, TValue> first;
        protected IDataSource<TKey, TValue> second;

        public ComboSource(IDataSource<TKey, TValue> first, IDataSource<TKey, TValue> second)
        {
            this.first = first;
            this.second = second;
        }

        #region IDataSource<TKey,TValue> Members

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (first.TryGetValue(key, out value))
                return true;

            return second.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
			return (new AppendEnumerable<KeyValuePair<TKey, TValue>>(first, second)).GetEnumerator();
			
			/* For a replacing enumerator (like a dictionary)		
            IEnumerator<KeyValuePair<TKey, TValue>> dict1 = first.GetEnumerator();
            IEnumerator<KeyValuePair<TKey, TValue>> dict2 = second.GetEnumerator();

            Dictionary<TKey, TValue> combined = new Dictionary<TKey, TValue>();
            foreach (KeyValuePair<TKey, TValue> kvp in dict1)
                combined.Add(kvp.Key, kvp.Value);
            foreach (KeyValuePair<TKey, TValue> kvp in dict2)
                if (!combined.ContainsKey(kvp.Key))
                    combined.Add(kvp.Key, kvp.Value);

            return combined.GetEnumerator();
     		*/
        }

        #endregion

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			var me = this as IEnumerable<KeyValuePair<TKey, TValue>>;
	        return me.GetEnumerator();
		}
    }
}
