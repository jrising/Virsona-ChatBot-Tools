/******************************************************************\
 *      Class Name:     ForeignWord
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a foreign word (like saudade).
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class ForeignWord : POSPhrase
    {
        public ForeignWord(string word)
            : base("FW", word)
        {
        }
    }
}
