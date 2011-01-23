/******************************************************************\
 *      Class Name:     AdverbSuperlative
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a superlative adverb, and how it can enter a larger
 * phrase and be paraphrased.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class AdverbSuperlative : Adverb
    {
        public AdverbSuperlative(string word)
            : base(word)
        {
            part = "RBS";
        }
    }
}
