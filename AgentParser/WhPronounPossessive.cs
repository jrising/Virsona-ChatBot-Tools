/******************************************************************\
 *      Class Name:     WhPronounPossessive
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a wh-style possessive pronoun (like whose).
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class WhPronounPossessive : POSPhrase
    {
        public WhPronounPossessive(string word)
            : base("WP$", word)
        {
        }
    }
}
