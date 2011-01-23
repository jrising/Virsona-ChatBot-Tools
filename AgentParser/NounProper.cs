/******************************************************************\
 *      Class Name:     NounProper
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a proper noun, and how it can enter a larger phrase
 * and be paraphrased.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using LanguageNet.Grammarian;

namespace LanguageNet.AgentParser
{
    public class NounProper : Noun
    {
        public NounProper(string word)
            : base(word)
        {
            part = "NNP";
            precedence = 50;
        }

        // Combine with adjacent proper nouns, before rolling into Noun Phrase
        public override bool Transform(Sentence sentence)
        {
            List<Phrase> phrases = new List<Phrase>();
            phrases.Add(this);

            // Look ahead and behind
            Phrase next = sentence.PhraseAfter(this);
            while (next != null && next.Part == "NNP")
            {
                phrases.Add(next);
                next = sentence.PhraseAfter(next);
            }

            Phrase prev = sentence.PhraseBefore(this);
            while (prev != null && prev.Part == "NNP")
            {
                phrases.Insert(0, prev);
                prev = sentence.PhraseBefore(prev);
            }

            sentence.Combine(phrases, new NounPhrase());
            return true;
        }

        public override Phrase Parapharse(Verbs verbs, Nouns nouns, WordNetAccess wordnet, GrammarParser.ParaphraseOptions options, List<Phrase> emphasizes, ref double inprob)
        {
            return (Phrase) MemberwiseClone();
        }
    }
}
