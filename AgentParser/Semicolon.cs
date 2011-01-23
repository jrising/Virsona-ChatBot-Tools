/******************************************************************\
 *      Class Name:     Semicolon
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a semicolon (like ;)
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class Semicolon : Punctuation
    {
        public Semicolon(string word)
            : base(";", word)
        {
        }

        public override bool Transform(Sentence sentence)
        {
            KeyValuePair<string, string> nks = NeighborKinds(sentence);
            if (nks.Key == "S" && nks.Value == "S")
            {
                sentence.Combine(Neighborhood(sentence), new SimpleDeclarativePhrase());
                return true;
            }

            return false;
        }
    }
}
