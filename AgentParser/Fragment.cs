/******************************************************************\
 *      Class Name:     Fragment
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a fragment of a sentence-- a full expression which
 * can not be called a sentence or a question.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class Fragment : Phrase
    {
        public Fragment()
            : base("FRAG")
        {
        }

        public Fragment(List<Phrase> constituents)
            : base("FRAG", constituents)
        {
        }
		
		public override bool IsWhole {
			get {
				return true;
			}
		}
    }
}
