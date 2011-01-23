/******************************************************************\
 *      Class Name:     POSTagger
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Interface to the plugin-based part of speech tagging methods
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase;
using PluggerBase.ActionReaction.Interfaces;

namespace LanguageNet.Grammarian
{
    public class POSTagger
    {
        // The result of a tagging operation: a list of part of speech types
        public static IArgumentType TagEnumerationResultType = new NamedArgumentType("SpeechPartTag:TagEnumeration",
            new EnumerableArgumentType(int.MaxValue, new StringArgumentType(4, ".+", "??")));
        // A data source with the possible parts of speech for words
        public const string PartsSourceName = "SpeechPartTag:PartsSource";

        protected IDataSource<string, string[]> partsSource;
        protected PluginEnvironment plugenv;

        public POSTagger(PluginEnvironment plugenv)
        {
            partsSource = plugenv.GetDataSource<string, string[]>(PartsSourceName);
            this.plugenv = plugenv;
        }
		
		public List<KeyValuePair<string, string>> TagString(string words) {
			return TagList(StringUtilities.SplitWords(words, false));
		}
		
        // Tag every element from a grammatical list of words
        public List<KeyValuePair<string, string>> TagList(List<string> words)
        {
            if (plugenv != null)
            {
                IEnumerable<string> parts = (IEnumerable<string>) plugenv.ImmediateConvertTo(words, TagEnumerationResultType, 2, 200 * words.Count);

                List<KeyValuePair<string, string>> tokens = new List<KeyValuePair<string,string>>();
                int ii = 0;
                foreach (string part in parts)
                    tokens.Add(new KeyValuePair<string,string>(words[ii++], part));

                return tokens;
            }

            // Without a plugin environment, just return a list of unknowns
            List<KeyValuePair<string, string>> result = new List<KeyValuePair<string, string>>();
            foreach (string word in words)
                result.Add(new KeyValuePair<string, string>(word, "??"));

            return result;
        }

        // Resolve all unknown (i.e., ??) part of speech typed words in the parsed phrases
        public List<KeyValuePair<string, string>> ResolveUnknowns(List<IParsedPhrase> phrases)
        {
            if (plugenv != null)
            {
                IEnumerable<string> parts = (IEnumerable<string>)plugenv.ImmediateConvertTo(phrases, TagEnumerationResultType, 2, 1000 * phrases.Count);

                List<string> words = new List<string>();
                foreach (IParsedPhrase phrase in phrases)
                    words.AddRange(GrammarParser.GetWords(phrase));

                List<KeyValuePair<string, string>> tokens = new List<KeyValuePair<string, string>>();
                int ii = 0;
                foreach (string part in parts)
                    tokens.Add(new KeyValuePair<string, string>(words[ii++], part));

                return tokens;
            }

            // no plugin environment?  do nothing
            return GetLowestLevel(phrases);
        }

        // Get a collection of <word, part of speech> tokens at the bottom of these phrases
        public List<KeyValuePair<string, string>> GetLowestLevel(IEnumerable<IParsedPhrase> phrases)
        {
            List<KeyValuePair<string, string>> tokens = new List<KeyValuePair<string,string>>();
            foreach (IParsedPhrase phrase in phrases)
                if (phrase.IsLeaf)
                    tokens.Add(new KeyValuePair<string, string>(phrase.Text, phrase.Part));
                else
                    tokens.AddRange(GetLowestLevel(phrase.Branches));

            return tokens;
        }

        // Get the allowed parts of speech for a given word
        public string[] GetPossibleParts(string word)
        {
            if (partsSource == null)
                return null;

            string[] parts = null;
            if (!partsSource.TryGetValue(word, out parts))
                return null;    // we don't know

            return parts;
        }
    }
}
