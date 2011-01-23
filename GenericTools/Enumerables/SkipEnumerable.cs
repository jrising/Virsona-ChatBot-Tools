/******************************************************************\
 *      Class Name:     SkipEnumerable
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Remove one element from an enumeration, lazily evaluated
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericTools.Enumerables
{
    public class SkipEnumerable<T> : IEnumerable<T>
    {
        protected IEnumerable<T> enumerable;
        protected T except;

        public SkipEnumerable(IEnumerable<T> enumerable, T except)
        {
            this.enumerable = enumerable;
            this.except = except;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return new SkipEnumerator(enumerable.GetEnumerator(), except);
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new SkipEnumerator(enumerable.GetEnumerator(), except);
        }

        #endregion

        public class SkipEnumerator : IEnumerator<T>
        {
            protected IEnumerator<T> enumerator;
            protected T except;

            public SkipEnumerator(IEnumerator<T> enumerator, T except)
            {
                this.enumerator = enumerator;
                this.except = except;
            }

            #region IEnumerator<T> Members

            public T Current
            {
                get
                {
                    return enumerator.Current;
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
                    return enumerator.Current;
                }
            }

            public bool MoveNext()
            {
                if (enumerator.MoveNext())
                {
                    if (enumerator.Current.Equals(except))
                        return enumerator.MoveNext();
                    else
                        return true;
                }
                else
                    return false;
            }

            public void Reset()
            {
                enumerator.Reset();
            }

            #endregion
        }
    }
}
