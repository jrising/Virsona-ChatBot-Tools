/******************************************************************\
 *      Class Name:     EnumerablesTest
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Testing the various kinds of Enumerables
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace GenericTools.Enumerables
{
    [TestFixture]
    public class EnumerablesTest
    {
        [Test]
        public void TestAppendEnumerable()
        {
            int[] odds = new int[] { 3, 5, 7 };
            int[] evens = new int[] { 2, 4, 6 };

            IEnumerable<int> nums = new AppendEnumerable<int>(odds, evens);

            // Find the minimum, maximum, and length
            int min = 10, max = 1, len = 0;
            foreach (int num in nums)
            {
                min = Math.Min(min, num);
                max = Math.Max(max, num);
                len++;
            }

            Assert.AreEqual(6, len);
            Assert.AreEqual(2, min);
            Assert.AreEqual(7, max);
        }

        [Test]
        public void TestMapEnumerable()
        {
            int[] nums = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            IEnumerable<string> exnums = new MapEnumerable<int, string>(nums, ExpressIt, null);

            int check = 1;
            foreach (string exnum in exnums)
                Assert.AreEqual(check++, int.Parse(exnum));
        }

        public string ExpressIt(int num, object shared)
        {
            return num.ToString();
        }

        [Test]
        public void TestSkipEnumerable() {
            int[] originals = new int[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            IEnumerable<int> odds = originals;

            // Remove all evens
            foreach (int original in originals)
                odds = new SkipEnumerable<int>(odds, 2 * original);

            // Do we have any evens?
            foreach (int odd in odds)
                Assert.AreEqual(1, odd % 2);
        }
    }
}
