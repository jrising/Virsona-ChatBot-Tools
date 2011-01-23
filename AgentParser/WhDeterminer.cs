/******************************************************************\
 *      Class Name:     WhDeterminer
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * WHAT and (THE) WHICH are tagged WD when not acting as the head
 * of a wh- noun phrase, and as WPRO otherwise. Note the difference
 * in this regard between wh- words and ordinary determiners, which
 * are tagged as determiners invariably. 
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class WhDeterminer : POSPhrase
    {
        public WhDeterminer(string word)
            : base("WDT", word)
        {
        }

        public override bool Transform(Sentence sentence)
        {
            KeyValuePair<string, string> nks = NeighborKinds(sentence);
            if (nks.Value == "NP" || nks.Value == "VP")
            {
                List<Phrase> consts = new List<Phrase>();
                consts.Add(this);
                consts.Add(sentence.PhraseAfter(this));
                sentence.Combine(consts, new WhNounPhrase());
                return true;
            }

            return false;
        }
    }
}
