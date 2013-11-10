using System;
using System.Collections.Generic;
using PluggerBase;

namespace GenericTools
{
	public class MemoizedSource<TKey, TValue> : IDataSource<TKey, TValue>
	{
		protected IDataSource<TKey, TValue> source;
		protected Dictionary<TKey, KeyValuePair<bool, TValue>> pasts;
		
		public MemoizedSource(IDataSource<TKey, TValue> source)
		{
			this.source = source;
			pasts = new Dictionary<TKey, KeyValuePair<bool, TValue>>();
		}
		
        #region IDataSource<TKey,TValue> Members

        public bool TryGetValue(TKey key, out TValue value)
        {
			KeyValuePair<bool, TValue> maybe;
			if (pasts.TryGetValue(key, out maybe)) {
				if (maybe.Key) {
					value = maybe.Value;
					return true;
				} else {
					value = default(TValue);
					return false;
				}
			}

			if (source.TryGetValue(key, out value)) {
				pasts.Add(key, new KeyValuePair<bool, TValue>(true, value));
                return true;
			} else {
				pasts.Add(key, new KeyValuePair<bool, TValue>(false, default(TValue)));
				return false;
			}
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
			return source.GetEnumerator();
        }

        #endregion

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			return source.GetEnumerator();
		}

	}
}

