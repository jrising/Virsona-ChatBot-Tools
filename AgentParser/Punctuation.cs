/******************************************************************\
 *      Class Name:     Punctuation
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Parent class for all forms of punctuation.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class Punctuation : POSPhrase
    {
        public Punctuation(string kind, string word)
            : base(kind, word)
        {
        }
    }
}
