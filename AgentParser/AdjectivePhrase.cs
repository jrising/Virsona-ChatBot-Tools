/******************************************************************\
 *      Class Name:     AdjectivePhrase
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates an adjective phrase, and how it can enter a larger
 * phrase and be paraphrased.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using LanguageNet.Grammarian;

namespace LanguageNet.AgentParser
{
    public class AdjectivePhrase : Phrase
    {
        public AdjectivePhrase()
            : base("ADJP")
        {
        }

        public AdjectivePhrase(params Phrase[] phrases)
            : this()
        {
            LoadPhrases(phrases);
        }


        public override bool Transform(Sentence sentence) {
            KeyValuePair<string, string> nks = NeighborKinds(sentence);

            if (nks.Value == "ADJP") {
                sentence.MergeNext(this);
                return true;
            }

            if (nks.Key == "ADVP") {
                sentence.AbsorbPrevious(this);
                return true;
            }

            return false;
        }

        public override Phrase Parapharse(Verbs verbs, Nouns nouns, WordNetAccess wordnet, GrammarParser.ParaphraseOptions options, List<Phrase> emphasizes, ref double prob)
        {
            if (IsComposed(typeof(AdjectivePhrase), typeof(Conjunction), typeof(AdjectivePhrase)))
            {
                if (RemoveImprobability(.5, ref prob))
                {
                    Phrase first = constituents[2].Parapharse(verbs, nouns, wordnet, SubMoveToFront(options), emphasizes, ref prob);
                    Phrase and = constituents[1].Parapharse(verbs, nouns, wordnet, SubNotMoved(options), emphasizes, ref prob);
                    Phrase second = constituents[0].Parapharse(verbs, nouns, wordnet, SubMoveOffFront(options), emphasizes, ref prob);

                    return new AdjectivePhrase(first, and, second);
                }
            }

            return base.Parapharse(verbs, nouns, wordnet, options, emphasizes, ref prob);
        }
    }
}
