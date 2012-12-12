using System;
using System.Collections.Generic;
using System.Text;

namespace GenericTools
{
    public class ExpiringDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        protected TimeSpan defaultLifetime = new TimeSpan(1, 0, 0);
        protected Dictionary<TKey, KeyValuePair<DateTime, TValue>> data;

        public ExpiringDictionary()
        {
            data = new Dictionary<TKey, KeyValuePair<DateTime, TValue>>();
        }

        public ExpiringDictionary(TimeSpan defaultLifetime)
        {
            this.defaultLifetime = defaultLifetime;
            data = new Dictionary<TKey, KeyValuePair<DateTime, TValue>>();            
        }

        // Call periodically if you're concerned about memory usage
        public void Clean()
        {
            List<KeyValuePair<TKey, KeyValuePair<DateTime, TValue>>> alldata = new List<KeyValuePair<TKey, KeyValuePair<DateTime, TValue>>>(data);
            foreach (KeyValuePair<TKey, KeyValuePair<DateTime, TValue>> kvp in alldata)
                if (kvp.Value.Key.CompareTo(DateTime.Now) < 0)
                {
                    // remove it!
                    data.Remove(kvp.Key);
                }
        }
        
        // Returns KVP if the key still has a value
        protected KeyValuePair<DateTime, TValue>? CheckExpiry(TKey key)
        {
            KeyValuePair<DateTime, TValue> datum;
            if (data.TryGetValue(key, out datum))
                if (datum.Key.CompareTo(DateTime.Now) < 0)
                {
                    // remove it!
                    data.Remove(key);
                }
                else
                {
                    // yes, it's still here
                    return datum;
                }

            return null;
        }

        // returns true if it found the entry to update
        public bool UpdateExpiry(TKey key, DateTime expires)
        {
            KeyValuePair<DateTime, TValue> datum;
            if (data.TryGetValue(key, out datum))
            {
                data[key] = new KeyValuePair<DateTime, TValue>(expires, datum.Value);
                return true;
            }

            return false;
        }

        #region IDictionary<TKey,TValue> Members

        public void Add(TKey key, TValue value)
        {
            Add(key, value, DateTime.Now.Add(defaultLifetime));
        }

        public void Add(TKey key, TValue value, DateTime expires) {
            CheckExpiry(key);
            data.Add(key, new KeyValuePair<DateTime, TValue>(expires, value));
        }

        public bool ContainsKey(TKey key)
        {
            return CheckExpiry(key).HasValue;
        }

        public ICollection<TKey> Keys
        {
            get {
                // Collect all keys now (rather than lazily)
                List<TKey> keys = new List<TKey>();
                foreach (KeyValuePair<TKey, KeyValuePair<DateTime, TValue>> kvp in data)
                {
                    if (kvp.Value.Key.CompareTo(DateTime.Now) > 0)
                        keys.Add(kvp.Key);
                }

                return keys;
            }
        }

        public bool Remove(TKey key)
        {
            return data.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            KeyValuePair<DateTime, TValue>? kvp = CheckExpiry(key);
            if (kvp.HasValue)
            {
                value = kvp.Value.Value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        public ICollection<TValue> Values
        {
            get {
                // Collect all values now (rather than lazily)
                List<TValue> values = new List<TValue>();
                foreach (KeyValuePair<TKey, KeyValuePair<DateTime, TValue>> kvp in data)
                {
                    if (kvp.Value.Key.CompareTo(DateTime.Now) > 0)
                        values.Add(kvp.Value.Value);
                }

                return values;
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                KeyValuePair<DateTime, TValue>? kvp = CheckExpiry(key);
                if (!kvp.HasValue)
                    throw new ArgumentOutOfRangeException("key not found or expired");

                return kvp.Value.Value;
            }
            set
            {
                data[key] = new KeyValuePair<DateTime, TValue>(DateTime.Now.Add(defaultLifetime), value);
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TKey,TValue>> Members

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            data.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            KeyValuePair<DateTime, TValue>? kvp = CheckExpiry(item.Key);
            if (kvp.HasValue && kvp.Value.Value.Equals(item.Value))
                return true;

            return false;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            foreach (KeyValuePair<TKey, KeyValuePair<DateTime, TValue>> kvp in data)
            {
                if (kvp.Value.Key.CompareTo(DateTime.Now) > 0)
                    array[arrayIndex++] = new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value.Value);
            }
        }

        public int Count
        {
            get {
                return Keys.Count;
            }
        }

        public bool IsReadOnly
        {
            get {
                return false;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            KeyValuePair<DateTime, TValue>? kvp = CheckExpiry(item.Key);
            if (kvp.HasValue)
            {
                if (kvp.Value.Value.Equals(item.Value))
                {
                    Remove(item.Key);
                    return true;
                }
            }

            return false;
        }

        #endregion

        #region IEnumerable<KeyValuePair<TKey,TValue>> Members

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            // Collect everything now (rather than lazily)
            List<KeyValuePair<TKey, TValue>> kvps = new List<KeyValuePair<TKey, TValue>>();
            foreach (KeyValuePair<TKey, KeyValuePair<DateTime, TValue>> kvp in data)
            {
                if (kvp.Value.Key.CompareTo(DateTime.Now) > 0)
                    kvps.Add(new KeyValuePair<TKey, TValue>(kvp.Key, kvp.Value.Value));
            }

            return (IEnumerator<KeyValuePair<TKey, TValue>>) kvps.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
