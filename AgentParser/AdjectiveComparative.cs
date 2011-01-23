/******************************************************************\
 *      Class Name:     AdjectiveComparative
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a comparative adjective, and how it can enter a
 * larger phrase and be paraphrased.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class AdjectiveComparative : Adjective
    {
        public AdjectiveComparative(string word)
            : base(word)
        {
            part = "JJR";
        }
    }
}
