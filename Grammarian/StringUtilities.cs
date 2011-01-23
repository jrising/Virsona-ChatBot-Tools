/******************************************************************\
 *      Class Name:     StringUtilities
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Simple utilities for parsing and concatenating strings
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.Grammarian
{
    public class StringUtilities
    {
        // Concatenation that works with SplitWords (space-prefixed words are concatenated without space)
        public static string JoinWords(List<string> words)
        {
            StringBuilder result = new StringBuilder();
            bool nowhite = true;
            foreach (string word in words)
            {
                if (word == " ")
                    nowhite = true;
                else
                {
                    if (word.StartsWith(" "))
                        result.Append(word.TrimStart());
                    else
                    {
                        if (!nowhite)
                            result.Append(" ");
                        result.Append(word);
                    }
                    nowhite = false;
                }
            }

            return result.ToString();
        }

        // Works with JoinWords
        // if spacer is true, include a space before any place where parsing wasn't based on spaces
        // note, without this, we cannot reproduce the input
        public static List<string> SplitWords(string line, bool spacer)
        {
            List<string> words = new List<string>();
            StringBuilder word = new StringBuilder();
            bool gotWhiteSpace = false, lastPunct = false;
            for (int ii = 0; ii < line.Length; ii++)
            {
                if (Char.IsLetterOrDigit(line[ii]))
                {
                    if (lastPunct && spacer)
                        words.Add(" "); // no whitespace!
                    word.Append(line[ii]);
                    gotWhiteSpace = false;
                    lastPunct = false;
                }
                else
                {
                    if (word.Length > 0)
                    {
                        words.Add(word.ToString());
                        word = new StringBuilder();
                    }

                    if (!char.IsWhiteSpace(line[ii]))
                    {
                        if (gotWhiteSpace || !spacer)
                            words.Add(line[ii].ToString());
                        else
                            words.Add(" " + line[ii]); // means "no space"
                        lastPunct = true;
                    }
                    else
                        lastPunct = false;

                    gotWhiteSpace = Char.IsWhiteSpace(line[ii]);
                }
            }

            if (word.Length > 0)
                words.Add(word.ToString());

            return words;
        }
    }
}
