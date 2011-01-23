/******************************************************************\
 *      Class Name:     AppendEnumerable
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * An enumerable representing two enumerables appended
 *   This uses delayed operations, similar to the
 *     System.Linq Enumerable.Concat method
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericTools.Enumerables
{
    public class AppendEnumerable<T> : IEnumerable<T>
    {
        protected IEnumerable<T> enum1;
        protected IEnumerable<T> enum2;

        public AppendEnumerable(IEnumerable<T> enum1, IEnumerable<T> enum2)
        {
            this.enum1 = enum1;
            this.enum2 = enum2;
        }

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return new AppendEnumerator(enum1.GetEnumerator(), enum2.GetEnumerator());
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new AppendEnumerator(enum1.GetEnumerator(), enum2.GetEnumerator());
        }

        #endregion

        public class AppendEnumerator : IEnumerator<T>
        {
            protected bool isFirst;
            protected IEnumerator<T> enum1;
            protected IEnumerator<T> enum2;

            public AppendEnumerator(IEnumerator<T> enum1, IEnumerator<T> enum2)
            {
                isFirst = true;
                this.enum1 = enum1;
                this.enum2 = enum2;
            }

            #region IEnumerator<T> Members

            public T Current
            {
                get
                {
                    if (isFirst)
                        return enum1.Current;
                    else
                        return enum2.Current;
                }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                enum1.Dispose();
                enum2.Dispose();
            }

            #endregion

            #region IEnumerator Members

            object System.Collections.IEnumerator.Current
            {
                get
                {
                    if (isFirst)
                        return enum1.Current;
                    else
                        return enum2.Current;
                }
            }

            public bool MoveNext()
            {
                bool moveNext = (isFirst ? enum1 : enum2).MoveNext();
                if (!moveNext && isFirst)
                {
                    isFirst = false;
                    return enum2.MoveNext();
                }

                return moveNext;
            }

            public void Reset()
            {
                enum1.Reset();
                enum2.Reset();
                isFirst = true;
            }

            #endregion
        }
    }
}
