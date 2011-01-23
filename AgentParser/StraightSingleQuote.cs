/******************************************************************\
 *      Class Name:     StraightSingleQuote
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a single quote (like ').
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class StraightSingleQuote : Punctuation
    {
        public StraightSingleQuote(string word)
            : base("'", word)
        {
        }
    }
}
