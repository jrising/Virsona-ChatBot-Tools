/******************************************************************\
 *      Class Name:     POSTagger
 *      Written By:     James Rising
 *      Copyright:      2001-2007, The University of Sheffield
 *                      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 * 
 *                      Originally written by Mark Hepple,
 *                      modified by Valentin Tablan and Niraj Aswani
 *                      translated to Python by Jerome Scheuring
 *                      translated to C# by James Rising
 *      -----------------------------------------------------------
 * This file is part of HeppleTagger and is free software: you
 * can redistribute it and/or modify it under the terms of the GNU
 * Lesser General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option)
 * any later version.
 * 
 * Plugger Base is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with HeppleTagger.  If not, see
 * <http://www.gnu.org/licenses/>.
 *      -----------------------------------------------------------
 * Part of Speech Tagger
 *   A quick-and-dirty (but working) C# implementation of Mark
 *   Hepple's Part-of-Speech tagger
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using PluggerBase;
using LanguageNet.Grammarian;

namespace HeppleTagger
{
    public class POSTagger : IDataSource<string, string[]>
    {
        // the "not-a-word" token
        private const string staart = "STAART";

        // will be { staart }
        private string[] staartLex;

        // the default allowed parts of speech for each of these word kinds
        private string[] deflex_NNP = { "NNP" };
        private string[] deflex_JJ  = { "JJ" };
        private string[] deflex_CD  = { "CD" };
        private string[] deflex_NNS = { "NNS" };
        private string[] deflex_RB  = { "RB" };
        private string[] deflex_VBG = { "VBG" };
        private string[] deflex_NN  = { "NN" };

        // the word buffer: the context of seven words we're working on
        public string[] wordBuff = { staart, staart, staart, staart, staart, staart, staart };
        // the tag buffer: the "believed part of speech" for each word in wordBuff
        public string[] tagBuff = { staart, staart, staart, staart, staart, staart, staart };
        // the lexical buffer: the allowed parts of speech for each word in wordBuff
        public string[][] lexBuff = { null, null, null, null, null, null, null };

        protected Lexicon lexicon;
        protected Dictionary<string, List<Rule>> rules;
        protected Encoding encoding;

        // Initializes tagger with lexicon and rules.  Takes about .7 seconds
        public POSTagger(string lexiconFile, string rulesFile, Assembly assembly, Encoding encoding)
        {
            staartLex = new string[1];
            staartLex[0] = staart;

            for (int ii = 0; ii < 7; ii++)
                lexBuff[ii] = staartLex;

            lexicon = new Lexicon(lexiconFile);
            rules = new Dictionary<string, List<Rule>>();
            readRules(rulesFile, assembly);

            this.encoding = encoding;
        }

        // returns key value pairs of word, tag for each sentence
        // Takes about 1.5e-5 seconds per word
        public List<string> tagSentence(IEnumerable<string> sentence)
        {
            List<string> parts = new List<string>();

            foreach (string word in sentence)
                oneStep(word, parts);

            // finished adding all the words from a sentence, add six more
            // staarts to flush all words out of the tagging buffer
            for (int ii = 0; ii < 6; ii++)
                oneStep(staart, parts);

            return parts;
        }

        // Returns the lowest-level constituents, with all ?? parts resolved
        public List<string> ResolveUnknowns(IEnumerable<IParsedPhrase> sentence)
        {
            List<KeyValuePair<string, string>> tokens = PhrasesToTokens(sentence);

            List<string> parts = new List<string>();

            foreach (KeyValuePair<string, string> token in tokens)
            {
                if (token.Value.StartsWith("?"))
                    oneStep(token.Key, parts);
                else
                    oneStep(token.Key, new string[] { token.Value }, parts);
            }
            
            for (int ii = 0; ii < 6; ii++)
                oneStep(staart, parts);

            return parts;
        }

        // Decompose a list of IParsedPhrases to their constituent word-part tokens
        protected List<KeyValuePair<string, string>> PhrasesToTokens(IEnumerable<IParsedPhrase> phrases)
        {
            List<KeyValuePair<string, string>> tokens = new List<KeyValuePair<string, string>>();
            foreach (IParsedPhrase phrase in phrases)
            {
                if (phrase.IsLeaf)
                    tokens.Add(new KeyValuePair<string, string>(phrase.Text, phrase.Part));
                else
                    tokens.AddRange(PhrasesToTokens(phrase.Branches));
            }

            return tokens;
        }

        // Resolve the middle word's part of speech
        protected bool oneStep(string word, List<string> parts)
        {
            return oneStep(word, classifyWord(word), parts);
        }

        // Resolve the middle word's part of speech
        protected bool oneStep(string word, string[] types, List<string> parts)
        {
            // add the new word at the end of the text window
            for (int ii = 1; ii < 7; ii++)
            {
                wordBuff[ii - 1] = wordBuff[ii];
                tagBuff[ii - 1] = tagBuff[ii];
                lexBuff[ii - 1] = lexBuff[ii];
            }

            wordBuff[6] = word;
            lexBuff[6] = types;
            tagBuff[6] = lexBuff[6][0];

            // apply the rules to the word in the middle of the text window
            // Try to fire a rule for the current lexical entry. It may be the case that
            // no rule applies.

            List<Rule> rulesToApply;
            if (rules.TryGetValue(lexBuff[3][0], out rulesToApply) && rulesToApply.Count > 0)
            {
                foreach (Rule rule in rulesToApply)
                {
                    // find the first rule that applies, fire it and stop.
                    if (rule.apply(this))
                        break;
                }
            }

            // save the tagged word from the first position
            string taggedWord = wordBuff[0];
            if (taggedWord != staart)
            {
                parts.Add(tagBuff[0]);
                if (wordBuff[1] == staart)
                {
                    // wordTag[0] was the end of a sentence
                    return true;
                }
            }

            return false;
        }

        // Read all of the rules to initialize the rules array
        public void readRules(string rulesFile, Assembly assembly)
        {
            StreamReader rulesReader;
            if (encoding == null)
                rulesReader = new StreamReader(rulesFile);
            else
                rulesReader = new StreamReader(rulesFile, encoding);

            while (!rulesReader.EndOfStream) {
                string line = rulesReader.ReadLine();

                string[] ruleParts = line.Split(' ');
                if (ruleParts.Length < 3)
                    throw new InvalidRuleException(line);

                Rule newRule = createNewRule(ruleParts[2], assembly);
                newRule.initialize(ruleParts);

                List<Rule> existingRules;
                if (!rules.TryGetValue(newRule.from, out existingRules))
                {
                    existingRules = new List<Rule>();
                    rules.Add(newRule.from, existingRules);
                }
                existingRules.Add(newRule);
            }
        }

        // Create a rule by instantiating the class that handles it
        public Rule createNewRule(string ruleId, Assembly assembly)
        {
            try
            {
                Type ruleType = assembly.GetType("HeppleTagger.Rule_" + ruleId);
                return (Rule)Activator.CreateInstance(ruleType);
            }
            catch (Exception e)
            {
                throw new InvalidRuleException("Could not create rule " + ruleId + "!\n" +
                                               e.ToString());
            }
        }

        // Determine the parts of speech for a word
        private string[] classifyWord(string wd)
        {
            if (wd == staart)
                return staartLex;

            if (wd == " ")
                return new string[] { "``" };

            if (wd[0] == ' ')
                wd = wd.Substring(1);

            string[] categories;
            if (lexicon.TryGetValue(wd, out categories))
                return categories;

            // no lexical entry for the word. Try to guess

            if ('A' <= wd[0] && wd[0] <= 'Z')
                return deflex_NNP;

            for (int ii = 1; ii < wd.Length - 1; ii++)
                if (wd[ii] == '-')
                    return deflex_JJ;

            for (int ii = 0; ii < wd.Length; ii++)
                if ('0' <= wd[ii] && wd[ii] <= '9')
                    return deflex_CD;

            if (wd.EndsWith("ed") || wd.EndsWith("us") ||
                wd.EndsWith("ic") || wd.EndsWith("ble") ||
                wd.EndsWith("ive") || wd.EndsWith("ary") ||
                wd.EndsWith("ful") || wd.EndsWith("ical") || wd.EndsWith("less"))
                return deflex_JJ;

            if (wd.EndsWith("s"))
                return deflex_NNS;

            if (wd.EndsWith("ly"))
                return deflex_RB;

            if (wd.EndsWith("ing"))
                return deflex_VBG;

            return deflex_NN;
        }

        // The data source of possible parts of speech

        #region IDataSource<string,string[]> Members

        public bool TryGetValue(string key, out string[] value)
        {
            return lexicon.TryGetValue(key, out value);
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,string[]>> Members

        public IEnumerator<KeyValuePair<string, string[]>> GetEnumerator()
        {
            return lexicon.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return lexicon.GetEnumerator();
        }

        #endregion
    }
}
