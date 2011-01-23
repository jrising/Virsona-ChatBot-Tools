/******************************************************************\
 *      Class Name:     EnumerableUtilities
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Utilities for common actions on enumeratables
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericTools.Enumerables
{
    public class EnumerableUtilities
    {
        // Produce a string joining all of the elemetns from elts
        public static string JoinEnumerator<T>(string separator, IEnumerable<T> elts)
        {
            StringBuilder result = new StringBuilder();

            foreach (T elt in elts)
            {
                if (result.Length > 0)
                    result.Append(separator);
                result.Append(elt.ToString());
            }

            return result.ToString();
        }

        // Count the number of instances of each word
        public static Dictionary<T, int> CountDuplicates<T>(IEnumerable<T> elts)
        {
            Dictionary<T, int> counts = new Dictionary<T, int>();
            foreach (T elt in elts)
            {
                int count = 0;
                counts.TryGetValue(elt, out count);
                counts[elt] = count + 1;
            }

            return counts;
        }
    }
}
