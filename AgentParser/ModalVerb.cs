/******************************************************************\
 *      Class Name:     ModalVerb
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a modal verb (like would).
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class ModalVerb : POSPhrase
    {
        public ModalVerb(string word)
            : base("MD", word)
        {
            precedence = -6;
        }

        public override bool Transform(Sentence sentence)
        {				
            List<Phrase> phrases = new List<Phrase>();
            phrases.Add(this);
			
			KeyValuePair<string, string> nks = NeighborKinds(sentence);
			if (nks.Value == "VP")
				phrases.Add(sentence.PhraseAfter(this));

            sentence.Combine(phrases, new VerbPhrase());
            return true;
        }
    }
}
