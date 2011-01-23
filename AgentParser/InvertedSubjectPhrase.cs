/******************************************************************\
 *      Class Name:     InvertedSubjectPhrase
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates an inverted subject phrase, like Yoda-talk.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class InvertedSubjectPhrase : Phrase
    {
        public InvertedSubjectPhrase()
            : base("SINV")
        {
        }
    }
}