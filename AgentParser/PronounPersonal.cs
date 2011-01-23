/******************************************************************\
 *      Class Name:     PronounPersonal
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a personal pronoun (like we)
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class PronounPersonal : Noun
    {
        public PronounPersonal(string word)
            : base(word)
        {
            part = "PRP";
        }
    }
}
