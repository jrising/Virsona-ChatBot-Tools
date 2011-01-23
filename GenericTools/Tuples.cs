/******************************************************************\
 *      Class Name:     Tuples
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Containers for several objects
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericTools
{
    /*
     * A set of tuples for 2, 3, and 4 objects
     */
    [Serializable]
    public struct TwoTuple<TOne, TTwo> {
        public TOne one;
        public TTwo two;

        public TwoTuple(TOne one, TTwo two) {
            this.one = one;
            this.two = two;
        }
    }

    [Serializable]
    public struct ThreeTuple<TOne, TTwo, TThree>
    {
        public TOne one;
        public TTwo two;
        public TThree three;

        public ThreeTuple(TOne one, TTwo two, TThree three)
        {
            this.one = one;
            this.two = two;
            this.three = three;
        }
    }

    [Serializable]
    public struct FourTuple<TOne, TTwo, TThree, TFour>
    {
        public TOne one;
        public TTwo two;
        public TThree three;
        public TFour four;

        public FourTuple(TOne one, TTwo two, TThree three, TFour four)
        {
            this.one = one;
            this.two = two;
            this.three = three;
            this.four = four;
        }
    }
}
