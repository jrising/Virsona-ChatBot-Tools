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
using System.Text.RegularExpressions;

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
		
		        public delegate string StringConverter(string input);

        public static string Standardize(string input)
        {
            return input.ToUpper();
        }

        public static string StripInvalids(string input)
        {
            Regex invalids = new Regex("[^0-9a-zA-Z-]", RegexOptions.IgnorePatternWhitespace);
            return invalids.Replace(input, " ");
        }

        public static string ReplaceCI(string original, string pattern, string replacement)
        {
            int count, position0, position1;
            count = position0 = position1 = 0;
            string upperString = original.ToUpper();
            string upperPattern = pattern.ToUpper();
            int inc = (original.Length / pattern.Length) *
                      (replacement.Length - pattern.Length);
            char[] chars = new char[original.Length + Math.Max(0, inc)];
            while ((position1 = upperString.IndexOf(upperPattern,
                                              position0)) != -1)
            {
                for (int i = position0; i < position1; ++i)
                    chars[count++] = original[i];
                for (int i = 0; i < replacement.Length; ++i)
                    chars[count++] = replacement[i];
                position0 = position1 + pattern.Length;
            }
            if (position0 == 0) return original;
            for (int i = position0; i < original.Length; ++i)
                chars[count++] = original[i];
            return new string(chars, 0, count);
        }

        // This both does temporary replacements and case-insensitivity
        public static string ReplaceCI(string original, Dictionary<string, string> replaces)
        {
            List<string> matches = new List<string>();

            string upperString = original.ToUpper();
            string result = original;

            foreach (KeyValuePair<string, string> replace in replaces)
            {
                int count, position0, position1;
                count = position0 = position1 = 0;

                if (replace.Key.Trim() == "")
                {
                    result.Replace(replace.Key, replace.Value);
                    continue;
                }
                string upperPattern = replace.Key.ToUpper();
                int inc = (result.Length / replace.Key.Length) *
                          ((replace.Value.Length - replace.Value.Trim().Length + 6) - replace.Key.Length);
                char[] chars = new char[result.Length + Math.Max(0, inc)];
                while ((position1 = upperString.IndexOf(upperPattern,
                                                  position0)) != -1)
                {
                    for (int i = position0; i < position1; ++i)
                        chars[count++] = result[i];
                    string replacement = "__R" + (matches.Count + 1);
                    // Add spaces based on original
                    string untrimmed = replace.Value;
                    string trimStart = untrimmed.TrimStart();
                    string trimEnd = untrimmed.TrimEnd();
                    replacement = untrimmed.Substring(0, untrimmed.Length - trimStart.Length)
                        + replacement + untrimmed.Substring(trimEnd.Length);

                    for (int i = 0; i < replacement.Length; ++i)
                        chars[count++] = replacement[i];
                    position0 = position1 + replace.Key.Length;
                }
                if (position0 != 0)
                {
                    matches.Add(replace.Value.Trim());
                    for (int i = position0; i < result.Length; ++i)
                        chars[count++] = result[i];
                    result = new string(chars, 0, count);
                    upperString = result.ToUpper();
                }
            }

            for (int ii = 0; ii < matches.Count; ii++)
                result = result.Replace("__R" + (ii + 1), matches[ii]);

            return result;
        }
    }
}
