/******************************************************************\
 *      Class Name:     EnumerablesExample
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Examples of the uses of various Enumerables
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace GenericTools.Enumerables
{
    [TestFixture]
    public class EnumerablesExample
    {
        [Test]
        public void AppendEnumerableExample()
        {
            // Append the following two lists
            int[] one2three = new int[] { 1, 2, 3 };
            int[] four2six = new int[] { 4, 5, 6 };

            IEnumerable<int> appended = new AppendEnumerable<int>(one2three, four2six);

            // Writes: 1, 2, 3, 4, 5, 6
            Console.WriteLine(EnumerableUtilities.JoinEnumerator(", ", appended));
        }

        [Test]
        public void MapEnumerableExample()
        {
            // Square every element of a lst
            int[] nums = new int[] { 1, 2, 3 };

            IEnumerable<int> squared = new MapEnumerable<int, int>(nums, SquareIt, null);

            // Writes: 1, 4, 9
            Console.WriteLine(EnumerableUtilities.JoinEnumerator(", ", squared));
        }

        public int SquareIt(int num, object shared)
        {
            return num * num;
        }

        [Test]
        public void SkipEnumerableExample()
        {
            // Remove all the square numbers
            int[] nums = new int[] {1, 2, 3, 4, 5, 6, 7, 8, 9};
            IEnumerable<int> squareless = nums;

            foreach (int num in nums)
            {
                double sqrt = Math.Sqrt(num);
                if (sqrt == Math.Round(sqrt))
                    squareless = new SkipEnumerable<int>(squareless, num);
            }

            // Writes: 2, 3, 5, 6, 7, 8
            Console.WriteLine(EnumerableUtilities.JoinEnumerator(", ", squareless));
        }
    }
}
