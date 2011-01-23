/******************************************************************\
 *      Class Name:     MapEnumerable
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Map a function over an enumeration, lazily evaluated
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericTools.Enumerables
{
    public class MapEnumerable<TFrom, TTo> : IEnumerable<TTo>
    {
        public delegate TTo Converter(TFrom elt, object shared);

        protected IEnumerable<TFrom> enumerable;
        protected Converter converter;
        protected object shared;

        public MapEnumerable(IEnumerable<TFrom> enumerable, Converter converter, object shared)
        {
            this.enumerable = enumerable;
            this.converter = converter;
            this.shared = shared;
        }

        #region IEnumerable<TTo> Members

        public IEnumerator<TTo> GetEnumerator()
        {
            return new MapEnumerator(enumerable.GetEnumerator(), converter, shared);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new MapEnumerator(enumerable.GetEnumerator(), converter, shared);
        }

        #endregion

        public class MapEnumerator : IEnumerator<TTo>
        {
            protected IEnumerator<TFrom> enumerator;
            protected Converter converter;
            protected object shared;
            protected TTo current;

            public MapEnumerator(IEnumerator<TFrom> enumerator, Converter converter, object shared)
            {
                this.enumerator = enumerator;
                this.converter = converter;
                this.shared = shared;
            }

            #region IEnumerator<TTo> Members

            public TTo Current
            {
                get
                {
                    return current;
                }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                enumerator.Dispose();
            }

            #endregion

            #region IEnumerator Members

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return current;
                }
            }

            public bool MoveNext()
            {
                if (enumerator.MoveNext())
                {
                    current = converter(enumerator.Current, shared);
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                enumerator.Reset();
            }

            #endregion
        }
    }

    public class MapHelper
    {
        public static IEnumerable<TOne> EachTwoTupleOne<TOne, TTwo>(IEnumerable<TwoTuple<TOne, TTwo>> enumerable)
        {
            return new MapEnumerable<TwoTuple<TOne, TTwo>, TOne>(enumerable, TwoTupleOner<TOne, TTwo>, null);
        }

        protected static TOne TwoTupleOner<TOne, TTwo>(TwoTuple<TOne, TTwo> elt, object shared)
        {
            return elt.one;
        }

        public static IEnumerable<TTwo> EachTwoTupleTwo<TOne, TTwo>(IEnumerable<TwoTuple<TOne, TTwo>> enumerable)
        {
            return new MapEnumerable<TwoTuple<TOne, TTwo>, TTwo>(enumerable, TwoTupleTwor<TOne, TTwo>, null);
        }

        protected static TTwo TwoTupleTwor<TOne, TTwo>(TwoTuple<TOne, TTwo> elt, object shared)
        {
            return elt.two;
        }

        public static IEnumerable<TKey> EachKey<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
        {
            return new MapEnumerable<KeyValuePair<TKey, TValue>, TKey>(enumerable, EachKeyer<TKey, TValue>, null);
        }

        protected static TKey EachKeyer<TKey, TValue>(KeyValuePair<TKey, TValue> kvp, object shared)
        {
            return kvp.Key;
        }

        public static IEnumerable<TValue> EachValue<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> enumerable)
        {
            return new MapEnumerable<KeyValuePair<TKey, TValue>, TValue>(enumerable, EachValuer<TKey, TValue>, null);
        }

        protected static TValue EachValuer<TKey, TValue>(KeyValuePair<TKey, TValue> kvp, object shared)
        {
            return kvp.Value;
        }
    }
}