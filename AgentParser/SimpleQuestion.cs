/******************************************************************\
 *      Class Name:     SimpleQuestion
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a question and how it can be formed.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class SimpleQuestion : Phrase
    {
        public SimpleQuestion()
            : base("SBARQ")
        {
        }
		
		public override bool IsWhole {
			get {
				return true;
			}
		}

        public override bool Transform(Sentence sentence)
        {
            if (sentence.Complete())
                return true;
 	         
            bool success = false;
        
            KeyValuePair<string, string> nks = NeighborKinds(sentence);

            if (nks.Value == ".") {
                sentence.AbsorbNext(this);
                success = true;
            }

            if (!success && !sentence.Complete()) {
                string last = constituents[constituents.Count - 1].Part;
                if (nks.Key == "" && last == ".")
                {
                    sentence.AddFirstToCompletes();
                    success = true;
                }
                else
                {
                    sentence.Separate(this);
                    success = true;
                }
            }

            return success;
        }
    }
}
