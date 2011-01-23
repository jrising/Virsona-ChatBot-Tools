/******************************************************************\
 *      Class Name:     BackedMemcachedSource
 *      Written By:     James Rising
 *      Copyright:      2011, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * A datasource which produces the data from a given source,
 * caching the results into a given memcached service.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using BeIT.MemCached;
using PluggerBase;

namespace GenericTools.DataSources
{
    public class BackedMemcachedSource<T> : IDataSource<string, T>
    {
        protected MemcacheSource memcachedSource;
        protected IDataSource<string, T> backing;

        public BackedMemcachedSource(IDataSource<string, T> backing, string prefix, MemcachedClient memcache)
        {
            this.backing = backing;
            memcachedSource = new MemcacheSource(prefix, memcache);
        }
		
		public bool TestMemcached(int num, int skip) {
			int ii = 0;
			foreach (KeyValuePair<string, T> kvp in backing) {
				if (ii % (skip + 1) == 0) {
					object check;
					if (!memcachedSource.TryGetValue(kvp.Key, out check))
						return false;
					if (!kvp.Value.Equals(check))
						return false;
				}
				ii++;
				if (ii / (skip + 1) > (num - 1))
					break;
			}
			
			return true;
		}
		
        public void LoadIntoMemcached()
        {
			foreach (KeyValuePair<string, T> kvp in backing)
				memcachedSource.Set(kvp.Key, kvp.Value);
        }

        #region IDataSource<string,T> Members

        public bool TryGetValue(string key, out T value)
        {
            object objval = null;
            if (memcachedSource.TryGetValue(key, out objval))
            {
                value = (T)objval;
                return true;
            }

            if (backing.TryGetValue(key, out value))
            {
                memcachedSource.Set(key, value);
                return true;
            }

            return false;
        }

        public IEnumerator<KeyValuePair<string, T>> GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			var me = this as IEnumerable<KeyValuePair<string, T>>;
	        return me.GetEnumerator();
		}
	}
}
