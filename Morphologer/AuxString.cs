/******************************************************************\
 *      Class Name:     AuxString
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 * 
 * 						Modified from DoMuchMore by David Levy
 *      -----------------------------------------------------------
 * A word-aware and English-aware string
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.Morphologer
{
    public class AuxString
    {
        // Static elements

        public static bool IsConsonant(string input, int ii)
        {
            if (ii < input.Length)
            {
                char letter = char.ToLower(input[ii]);
                if ((letter != 'a') &&
                    (letter != 'e') &&
                    (letter != 'i') &&
                    (letter != 'o') &&
                    (letter != 'u'))
                    return true;
            }

            return false;
        }

        public static bool IsVowel(string input, int ii)
        {
            if (ii < input.Length) {
                char letter = char.ToLower(input[ii]);
                if ((letter == 'a') ||
                    (letter == 'e') ||
                    (letter == 'i') ||
                    (letter == 'o') ||
                    (letter == 'u'))
                    return true;
            }

            return false;
        }

        // Instance elements
        
        protected string str;

        public AuxString(string str)
        {
            this.str = str;
        }

        public char Letter(int ii)
        {
            if (ii < str.Length)
                return str[ii];
            else
                return '\0';
        }

        public bool InWord(int ii) {
            return (ii < str.Length) &&
                ((str[ii] >= 'a' && str[ii] <= 'z') ||
                 (str[ii] >= 'A' && str[ii] <= 'Z') ||
                 (str[ii] >= '0' && str[ii] <= '9') ||
                 str[ii] == '-' || str[ii] == '\'');
        }

        // returns the word which, returns "" if no such word
        public string Word(uint which)
        {
            StringBuilder theword = new StringBuilder();
            int from = WordStart(which);
            if (from >= 0) {
                for (int ii = from; ii < str.Length && InWord(ii); ii++)
                    theword.Append(str[ii]);
            }

            return theword.ToString();
        }

        // returns < 0 if we can't find the word
        public int WordStart(uint which) {
            uint no = 0;
            for (int ii = 0; ii < str.Length; ii++)
            {
                for (; ii < str.Length && !InWord(ii); ii++) ;
                if (ii < str.Length)
                {
                    if (no == which)
                        return (int) ii;
                    no++;
                }
                for (; ii < str.Length && InWord(ii); ii++) ;
            }

            return -1;
        }

        public int WordCount()
        {
            int no = 0;
            for(int ii = 0; ii < str.Length; ii++) {
                for (; ii < str.Length && !InWord(ii); ii++) ;
                if (ii < str.Length)
                    no++;
                for (; ii < str.Length && InWord(ii); ii++) ;
            }

            return no;
        }
    }
}
