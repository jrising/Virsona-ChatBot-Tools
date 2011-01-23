/******************************************************************\
 *      Class Name:     To
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates the word to.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class To : POSPhrase
    {
        public To(string word)
            : base("TO", word)
        {
        }

        public override bool Transform(Sentence sentence)
        {
            KeyValuePair<string, string> nks = NeighborKinds(sentence);

            if (nks.Value == "NP")
            {
                List<Phrase> us = new List<Phrase>();
                us.Add(this);
                us.Add(sentence.PhraseAfter(this));
                sentence.Combine(us, new PrepositionalPhrase());
                return true;
            }

            return false;
        }
    }
}
