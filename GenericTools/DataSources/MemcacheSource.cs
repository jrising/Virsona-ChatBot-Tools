/******************************************************************\
 *      Class Name:     MemcachedSource
 *      Written By:     James Rising
 *      Copyright:      2011, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * A source stores values in the local Memcached service
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using BeIT.MemCached;
using PluggerBase;

namespace GenericTools.DataSources
{
    public class MemcacheSource : IDataSource<string, object>
    {
        protected string prefix;
        protected MemcachedClient memcache;

        protected const string defaultCacheName = "MyCache";

        public static MemcachedClient defaultInstance = null;

        public MemcacheSource(string prefix, MemcachedClient memcache)
        {
            this.prefix = prefix;
            this.memcache = memcache;
        }

        public static MemcachedClient DefaultClient()
        {
            if (defaultInstance == null)
            {
                MemcachedClient.Setup(defaultCacheName, new string[] { "localhost" });
                defaultInstance = MemcachedClient.GetInstance(defaultCacheName);
            }

            return defaultInstance;
        }

        #region IDataSource<string,object> Members

        public bool TryGetValue(string key, out object value)
        {
            value = memcache.Get(prefix + key);
            return (value != null);
        }

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
			var me = this as IEnumerable<KeyValuePair<string, object>>;
	        return me.GetEnumerator();
		}

        public void Set(string key, object value)
        {
            memcache.Set(prefix + key, value);
        }
	}
}
