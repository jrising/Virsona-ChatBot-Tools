/******************************************************************\
 *      Class Name:     VerbSimplePast
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a past tense verb (like ran).
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class VerbSimplePast : Verb
    {
        public VerbSimplePast(string word)
            : base(word)
        {
            part = "VBD";
        }
    }
}
