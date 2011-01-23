/******************************************************************\
 *      Class Name:     Conjunction
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a conjunction (like and) and how it combines
 * things in a sentence.
\******************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class Conjunction : POSPhrase
    {
        public Conjunction(string word)
            : base("CC", word)
        {
            precedence = 20;
        }

        public override bool Transform(Sentence sentence)
        {
            KeyValuePair<string, string> nks = NeighborKinds(sentence);

            if (nks.Key == "NP" && nks.Value == "NP")
            {
                sentence.Combine(Neighborhood(sentence), new NounPhrase());
                return true;
            }
            else if (nks.Key == "VP" && nks.Value == "VP")
            {
                sentence.Combine(Neighborhood(sentence), new VerbPhrase());
                return true;
            }
            else if (nks.Key == "PP" && nks.Value == "PP")
            {
                sentence.Combine(Neighborhood(sentence), new PrepositionalPhrase());
                return true;
            }
            else if (nks.Key == "S" && nks.Value == "S")
            {
                sentence.Combine(Neighborhood(sentence), new SimpleDeclarativePhrase());
                return true;
            }

            if (nks.Key == "PP" && nks.Value == "NP")
            {
                Phrase preposition = sentence.PhraseBefore(this);
                sentence.AbsorbNext(preposition);   // absorb this
                sentence.AbsorbNext(preposition);   // absorb noun
                return true;
            }

            return false;
        }
    }
}
