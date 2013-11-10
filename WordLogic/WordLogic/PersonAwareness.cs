using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.WordLogic
{
    public class PersonAwareness
    {
        public static double PersonScore(List<string> words1, List<string> words2)
        {
            bool first1 = RefersToFirst(words1), first2 = RefersToFirst(words2);
            bool second1 = RefersToSecond(words1), second2 = RefersToSecond(words2);

            if (first1 == first2 && second1 == second2) {
                if (first1 != second1)
                    return 1.0; // perfect!
                else
                    return 0.5; // no indication-- all have or all not
            }
            if (first1 == first2 || second1 == second2)
                return 0.75; // halfway good
            
            return 0.0; // nothing matches
        }

        public static bool RefersToFirst(List<string> words)
        {
            return (words.Contains("I") || words.Contains("ME") || words.Contains("MYSELF") || words.Contains("MY") || words.Contains("MINE"));
        }

        public static bool RefersToSecond(List<string> words)
        {
            return (words.Contains("YOU") || words.Contains("YOURSELF") || words.Contains("YOUR") || words.Contains("YOURS"));
        }
    }
}
