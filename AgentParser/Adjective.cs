/******************************************************************\
 *      Class Name:     Adjective
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates an adjective, and how it can enter a larger phrase
 * and be paraphrased.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using LanguageNet.Grammarian;
namespace LanguageNet.AgentParser
{
    public class Adjective : POSPhrase
    {
        public Adjective(string word)
            : base("JJ", word)
        {
        }

        public override bool Transform(Sentence sentence)
        {
            List<Phrase> phrases = new List<Phrase>();
            phrases.Add(this);
            sentence.Combine(phrases, new AdjectivePhrase());
            return true;
        }

        public override Phrase Parapharse(Verbs verbs, Nouns nouns, WordNetAccess wordnet, GrammarParser.ParaphraseOptions options, List<Phrase> emphasizes, ref double prob)
        {
            Phrase synonym = SynonymParaphrase(WordNetAccess.PartOfSpeech.Adj, verbs, nouns, wordnet, options, emphasizes, ref prob);
            if (synonym == null)
                return base.Parapharse(verbs, nouns, wordnet, options, emphasizes, ref prob);
            else
                return synonym;
        }
    }
}
