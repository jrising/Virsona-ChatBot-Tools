/******************************************************************\
 *      Class Name:     NounProperPlural
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a plural proper noun, and how it can enter a larger
 * phrase and be paraphrased.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using LanguageNet.Grammarian;

namespace LanguageNet.AgentParser
{
    public class NounProperPlural : Noun
    {
        public NounProperPlural(string word)
            : base(word)
        {
            part = "NNPS";
        }

        public override Phrase Parapharse(Verbs verbs, Nouns nouns, WordNetAccess wordnet, GrammarParser.ParaphraseOptions options, List<Phrase> emphasizes, ref double inprob)
        {
            return (Phrase) MemberwiseClone();
        }
    }
}
