/******************************************************************\
 *      Class Name:     POSPhrase
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a single word with a known part of speech.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using LanguageNet.Grammarian;

namespace LanguageNet.AgentParser
{
    public class POSPhrase : Phrase
    {
        protected string word;

        public POSPhrase(string kind, string word)
            : base(kind)
        {
            this.word = word;
        }

        public string Word
        {
            get
            {
                return word;
            }
        }
		
		public override bool IsLeaf
		{
			get
			{
				return true;
			}
		}

        public override string ToString()
        {
            return word + "/" + base.ToString();
        }

        public override Phrase Parapharse(Verbs verbs, Nouns nouns, WordNetAccess wordnet, GrammarParser.ParaphraseOptions options, List<Phrase> emphasizes, ref double prob)
        {
            POSPhrase phrase = (POSPhrase) MemberwiseClone();
            if ((options & GrammarParser.ParaphraseOptions.MoveToStart) != GrammarParser.ParaphraseOptions.NoOptions)
                phrase.word = nouns.StartCap(phrase.word);
            else if ((options & GrammarParser.ParaphraseOptions.MoveOffStart) != GrammarParser.ParaphraseOptions.NoOptions)
                phrase.word = nouns.UnStartCap(phrase.word);

            return phrase;
        }

        public Phrase SynonymParaphrase(WordNetAccess.PartOfSpeech part, Verbs verbs, Nouns nouns, WordNetAccess wordnet, GrammarParser.ParaphraseOptions options, List<Phrase> emphasizes, ref double prob) {
            if (word == "not" || word == "non")
                return null;    // we don't replace these!

            // Can we use a synonym?
            List<string> synonyms = wordnet.GetExactSynonyms(word, part);
            if (synonyms != null) {
                synonyms.Remove(word);
                synonyms.Remove(word.ToLower());
                // remove any synonyms more than twice as long, or half as long as the original
                List<string> onlygoods = new List<string>();
                foreach (string synonym in synonyms)
                    if (synonym.Length <= 2 * word.Length && synonym.Length >= word.Length / 2)
                        onlygoods.Add(synonym);
                synonyms = onlygoods;

                if (synonyms.Count > 0 && RemoveUnemphasizedImprobability(.75, emphasizes, this, ref prob))
                {
                    string newword = synonyms[ImprobabilityToInt(synonyms.Count, ref prob)];
                    if (IsStart(options))
                        newword = nouns.StartCap(newword);

                    POSPhrase clone = (POSPhrase) MemberwiseClone();
                    clone.word = newword;

                    return clone;
                }
            }

            return null;
        }
    }
}
