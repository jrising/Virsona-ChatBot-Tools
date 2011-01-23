/******************************************************************\
 *      Class Name:     AdverbPhrase
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates an adverb phrase, and how it can enter a larger
 * phrase and be paraphrased.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class AdverbPhrase : Phrase
    {
        public AdverbPhrase()
            : base("ADVP")
        {
        }

        public override bool Transform(Sentence sentence)
        {
            KeyValuePair<string, string> nks = NeighborKinds(sentence);

            if (nks.Value == "ADVP")
            {
                sentence.MergeNext(this);
                return true;
            }

            return false;
        }
    }
}
