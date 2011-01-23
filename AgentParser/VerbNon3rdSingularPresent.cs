/******************************************************************\
 *      Class Name:     VerbNon3rdSingularPresent
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a present verb that is not in the third person
 * (like run).
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class VerbNon3rdSingularPresent : Verb
    {
        public VerbNon3rdSingularPresent(string word)
            : base(word)
        {
            part = "VBP";
        }
    }
}
