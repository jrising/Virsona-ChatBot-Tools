/******************************************************************\
 *      Class Name:     ReversibleDictionary
 *      Written By:     James.R
 *      Copyright:      Virsona Inc
 *      -----------------------------------------------------------
 * A dictionary which maintains a mirror copy for value -> key lookups
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericTools
{
    public class ReversibleDictionary<TOne, TTwo> : IDictionary<TOne, TTwo>
    {
        protected Dictionary<TOne, TTwo> forward;
        protected ReversibleDictionary<TTwo, TOne> reverse;

        public ReversibleDictionary() {
            forward = new Dictionary<TOne, TTwo>();
            reverse = new ReversibleDictionary<TTwo, TOne>(this);
        }

        public ReversibleDictionary(ReversibleDictionary<TTwo, TOne> reverse)
        {
            forward = new Dictionary<TOne, TTwo>();
            this.reverse = reverse;
        }

        public IDictionary<TTwo, TOne> Reverse
        {
            get
            {
                return reverse;
            }
        }

        #region IDictionary<TOne,TTwo> Members

        public void Add(TOne key, TTwo value)
        {
            forward.Add(key, value);
            reverse.forward.Add(value, key);
        }

        public bool ContainsKey(TOne key)
        {
            return forward.ContainsKey(key);
        }

        public ICollection<TOne> Keys
        {
            get {
                return forward.Keys;
            }
        }

        public bool Remove(TOne key)
        {
            TTwo value;
            if (forward.TryGetValue(key, out value))
            {
                forward.Remove(key);
                reverse.forward.Remove(value);
                return true;
            }

            return false;
        }

        public bool TryGetValue(TOne key, out TTwo value)
        {
            return forward.TryGetValue(key, out value);
        }

        public ICollection<TTwo> Values
        {
            get {
                return forward.Values;
            }
        }

        public TTwo this[TOne key]
        {
            get
            {
                return forward[key];
            }
            set
            {
                forward[key] = value;
            }
        }

        #endregion

        #region ICollection<KeyValuePair<TOne,TTwo>> Members

        public void Add(KeyValuePair<TOne, TTwo> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            forward.Clear();
            reverse.forward.Clear();
        }

        public bool Contains(KeyValuePair<TOne, TTwo> item)
        {
            TTwo value;
            return (forward.TryGetValue(item.Key, out value) && item.Value.Equals(value));
        }

        public void CopyTo(KeyValuePair<TOne, TTwo>[] array, int arrayIndex)
        {
            foreach (KeyValuePair<TOne, TTwo> kvp in forward)
                array[arrayIndex++] = kvp;
        }

        public int Count
        {
            get {
                return forward.Count;
            }
        }

        public bool IsReadOnly
        {
            get {
                return false;
            }
        }

        public bool Remove(KeyValuePair<TOne, TTwo> item)
        {
            bool foundKey = forward.Remove(item.Key);
            bool foundValue = reverse.forward.Remove(item.Value);
            return foundKey || foundValue;
        }

        #endregion

        #region IEnumerable<KeyValuePair<TOne,TTwo>> Members

        public IEnumerator<KeyValuePair<TOne, TTwo>> GetEnumerator()
        {
            return forward.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return forward.GetEnumerator();
        }

        #endregion
    }
}
