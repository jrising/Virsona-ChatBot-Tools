/******************************************************************\
 *      Class Name:     MapDataSource
 *      Written By:     James Rising
 *      Copyright:      2011, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * A source that produces data from another source, mapped through
 * a conversion function
\******************************************************************/
using System;
using System.Collections.Generic;
using PluggerBase;
using GenericTools.Enumerables;

namespace GenericTools.DataSources
{
	public class MapDataSource<TKey, TValueFrom, TValueTo> : IDataSource<TKey, TValueTo>
	{
        public delegate TValueTo Converter(TValueFrom elt, object shared);

        protected IDataSource<TKey, TValueFrom> source;
        protected Converter converter;
        protected object shared;

		public MapDataSource(IDataSource<TKey, TValueFrom> source, Converter converter, object shared)
        {
            this.source = source;
            this.converter = converter;
            this.shared = shared;
        }

        #region IDataSource<TKey,TValue> Members

        public bool TryGetValue(TKey key, out TValueTo value)
        {
			TValueFrom fromValue;
			if (!source.TryGetValue(key, out fromValue)) {
				value = default(TValueTo);
				return false;
			}
			
			value = converter(fromValue, shared);
			return true;
        }

        public IEnumerator<KeyValuePair<TKey, TValueTo>> GetEnumerator()
        {
			return (new MapEnumerable<KeyValuePair<TKey, TValueFrom>, KeyValuePair<TKey, TValueTo>>(source, PairConverter, shared)).GetEnumerator();
        }

        #endregion
		
		public KeyValuePair<TKey, TValueTo> PairConverter(KeyValuePair<TKey, TValueFrom> kvp, object shared) {
			return new KeyValuePair<TKey, TValueTo>(kvp.Key, converter(kvp.Value, shared));
		}
		
		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			var me = this as IEnumerable<KeyValuePair<TKey, TValueTo>>;
	        return me.GetEnumerator();
		}
	}
}

