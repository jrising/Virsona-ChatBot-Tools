/******************************************************************\
 *      Class Name:     UnknownPart
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a single word of unknown part of speech.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class UnknownPart : POSPhrase
    {
        public UnknownPart(string word)
            : base("??", word)
        {
        }
    }
}
