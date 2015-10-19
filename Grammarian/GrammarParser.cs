/******************************************************************\
 *      Class Name:     GrammarParser
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Interface to grammar parser plugins
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase;
using ActionReaction;
using ActionReaction.Interfaces;

namespace LanguageNet.Grammarian
{
    public class GrammarParser
    {
        // The result of a parsing option: an IParsedPhrase
        public static IArgumentType GrammarParseResultType =
            new TypedArgumentType(typeof(IParsedPhrase), new WordPhrase("bufallo", "NN"));
        // The result of a paraphrasing option: an IParsedPhrase
        public static SetArgumentTreeArgumentType ParaphrasingResultType =
            new SetArgumentTreeArgumentType("GrammarParse:Paraphrase", GrammarParseResultType);

        // Recursive options for paraphrasing
        public enum ParaphraseOptions
        {
            NoOptions = 0,
            MoveToStart = 2,    // This phrase will newly be the start of a sentence; capitalize as needed
            MoveOffStart = 4,   // This phrase will no longer be the start of a sentence: drop capitalization if applicable
            IsStayingStart = 8  // This phrase is and will remain the start of a sentence: don't second-guess capitalization
        }

        /***** Static Utility Functions *****/

        // Get the elemental words contained in a parsed phrase
        public static List<string> GetWords(IParsedPhrase phrase)
        {
            List<string> words = new List<string>();
            if (phrase.IsLeaf)
                words.Add(phrase.Text);
            else
            {
                foreach (IParsedPhrase constituent in phrase.Branches)
                    words.AddRange(GetWords(constituent));
            }

            return words;
        }

        // Get a part of a parsed phrase-- drop parents when possible, create a new surrounding group when needed
        // start can be negative: count from end
        // count can be negative: use that many less than all the elements
        public static IParsedPhrase GetSubphrase(IParsedPhrase phrase, int start, int count)
        {
            IEnumerable<IParsedPhrase> branches = phrase.Branches;

            List<IParsedPhrase> included = new List<IParsedPhrase>();

            int ii = 0;
            foreach (IParsedPhrase branch in branches)
                if (ii++ >= start)
                    included.Add(branch);

            // start can be negative: count from end
            if (start < 0)
                included = included.GetRange(included.Count + start, -start);
            if (count > 0)
                included = included.GetRange(0, count);
            // count can be negative: less than max elts
            if (count < 0)
                included = included.GetRange(0, included.Count + count);

            if (included.Count == 0)
                return null;    // count <= start!
            if (included.Count == 1)
                return included[0];

            return new GroupPhrase(phrase.Part, included);
        }
        
        public static IParsedPhrase GetSubphrase(IParsedPhrase phrase, int start)
        {
            return GetSubphrase(phrase, start, 0);
        }

        /***** Instantiated Class, for Plugin Interaction *****/

        protected PluginEnvironment plugenv;

        public GrammarParser(PluginEnvironment plugenv)
        {
            this.plugenv = plugenv;
        }

        // Parse a string into an IParsedPhrase
        public IParsedPhrase Parse(string phrase)
        {
            if (plugenv != null)
            {
                object result = plugenv.ImmediateConvertTo(phrase, GrammarParseResultType, 2, 100 * phrase.Length);
                if (result is Exception)
                    throw (Exception)result;
                else
                    return (IParsedPhrase)result;
            }

            return null;
        }

        // Parse a sequence of <word, part of speech> tokens into an IParsedPhrase
        public IParsedPhrase Parse(List<KeyValuePair<string, string>> tokens)
        {
            if (plugenv != null) {
                object result = plugenv.ImmediateConvertTo(tokens, GrammarParseResultType, 2, 1200 * tokens.Count);
                if (result is Exception)
                    throw (Exception)result;
                else
                    return (IParsedPhrase)result;
            }

            return null;
        }

        // Parapharse a phrase, with a given set of options
        public IParsedPhrase Paraphrase(IParsedPhrase phrase, ParaphraseOptions? options, List<string> emphasizes, double prob)
        {
            ArgumentTree args = new ArgumentTree();
            args["input"] = phrase;
            args["prob"] = prob;
            if (options.HasValue)
                args["opts"] = options.Value;
            if (emphasizes != null)
                args["emph"] = emphasizes;

            ArgumentTree result = (ArgumentTree)plugenv.ImmediateConvertTo(args, GrammarParser.ParaphrasingResultType, 2, 1000 + 100 * phrase.Text.Length);
            return (IParsedPhrase) result.Value;
        }

        // Extract the first complete phrase from a string
        public string SingleClause(string input)
        {
            IParsedPhrase phrase = Parse(input);
            if (phrase.Part == "=P") // paragraph
                return GetSubphrase(phrase, 0, 1).Text;

            if (phrase.Part == "S" || phrase.Part == "SBARQ" ||
                phrase.Part == "SINV" || phrase.Part == "SBAR")
                return phrase.Text;

            // Otherwise, go to first period
            List<IParsedPhrase> phrases = new List<IParsedPhrase>();
            foreach (IParsedPhrase branch in phrase.Branches)
            {
                phrases.Add(branch);
                if (phrases.Count == 1)
                    continue;   // always add one element

                if (branch.Part == "?" || branch.Part == "!" ||
                    (branch.Part == "." && phrases[phrases.Count - 1].Text.Length > 1))  // watch out for initials!
                {
                    IParsedPhrase first = new GroupPhrase("S", phrases);
                    return first.Text;
                }
            }

            return phrase.Text;
        }
    }
}
