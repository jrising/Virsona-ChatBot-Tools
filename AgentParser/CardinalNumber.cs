/******************************************************************\
 *      Class Name:     CardinalNumber
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates an cardinal number (like 3).
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class CardinalNumber : POSPhrase
    {
        public CardinalNumber(string word)
            : base("CD", word)
        {
        }
    }
}
