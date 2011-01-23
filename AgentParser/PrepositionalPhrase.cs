/******************************************************************\
 *      Class Name:     PrepositionalPhrase
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a prepositional phrase
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using LanguageNet.Grammarian;

namespace LanguageNet.AgentParser
{
    public class PrepositionalPhrase : Phrase
    {
        public PrepositionalPhrase()
            : base("PP")
        {
            precedence = 19;    // after NP
        }

        public PrepositionalPhrase(params Phrase[] phrases)
            : this()
        {
            LoadPhrases(phrases);
        }


        public override bool Transform(Sentence sentence)
        {
 	         if (!sentence.phrases.Contains(this))
                 return false;

            KeyValuePair<string, string> nks = NeighborKinds(sentence); 

            if (nks.Value == "NP") {
                sentence.AbsorbNext(this);
                return true;
            }

            return false;
        }

        public override Phrase Parapharse(Verbs verbs, Nouns nouns, WordNetAccess wordnet, GrammarParser.ParaphraseOptions options, List<Phrase> emphasizes, ref double prob)
        {
            if (IsComposed(typeof(VerbPhrase), typeof(Conjunction), typeof(VerbPhrase)))
            {
                if (RemoveImprobability(.5, ref prob))
                {
                    Phrase first = constituents[2].Parapharse(verbs, nouns, wordnet, SubMoveToFront(options), emphasizes, ref prob);
                    Phrase and = constituents[1].Parapharse(verbs, nouns, wordnet, SubNotMoved(options), emphasizes, ref prob);
                    Phrase second = constituents[0].Parapharse(verbs, nouns, wordnet, SubMoveOffFront(options), emphasizes, ref prob);

                    return new PrepositionalPhrase(first, and, second);
                }
            }

            return base.Parapharse(verbs, nouns, wordnet, options, emphasizes, ref prob);
        }
    }
}
