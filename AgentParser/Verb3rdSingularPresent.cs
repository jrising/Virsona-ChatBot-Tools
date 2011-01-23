/******************************************************************\
 *      Class Name:     Verb3rdSingularPresent
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a verb in the 3rd-person singular present (like 
 * runs).
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class Verb3rdSingularPresent : Verb
    {
        public Verb3rdSingularPresent(string word)
            : base(word)
        {
            part = "VBZ";
        }
    }
}
