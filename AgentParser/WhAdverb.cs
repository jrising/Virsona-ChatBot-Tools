/******************************************************************\
 *      Class Name:     WhAdverb
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a wh-style adverb (like where).
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class WhAdverb : POSPhrase
    {
        public WhAdverb(string word)
            : base("WRB", word)
        {
        }
    }
}
