/******************************************************************\
 *      Class Name:     Rule
 *      Written By:     James Rising
 *      Copyright:      2001-2007, The University of Sheffield
 *                      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 * 
 *                      Originally written by Mark Hepple,
 *                      modified by Valentin Tablan and Niraj Aswani
 *                      translated to Python by Jerome Scheuring
 *                      translated to C# by James Rising
 *      -----------------------------------------------------------
 * Parent class for all rules
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace HeppleTagger
{
    public class Rule
    {
        public string from;
        public string to;
        public string ruleId;
        public string[] context;

        public Rule()
        {
        }

        // all of the data needed for the rule
        public Rule(string from, string to, string ruleId, string[] context)
        {
            this.from = from;
            this.to = to;
            this.ruleId = ruleId;
            this.context = context;
        }

        // initialize from a single array of all the necessary pieces
        public void initialize(string[] ruleParts)
        {
            from = ruleParts[0];
            to = ruleParts[1];
            ruleId = ruleParts[2];
            int contextSize = ruleParts.Length - 3;
            context = new string[contextSize];
            for (int ii = 0; ii < contextSize; ii++)
                context[ii] = ruleParts[ii + 3];
        }

        public override string ToString()
        {
            return string.Format("<Rule_{0}; {1} -> {2}; [{3}]>", ruleId, from, to, string.Join(",", context));
        }

        // Are we in an appropriate context?
        public virtual bool checkContext(POSTagger tagger)
        {
            return true;
        }

        // Can we apply the part of speech given by our rule?
        public bool hasToTag(POSTagger tagger)
        {
            for (int ii = 0; ii < tagger.lexBuff[3].Length; ii++)
                if (to.Equals(tagger.lexBuff[3][ii]))
                    return true;
            return false;
        }

        // Apply the part of speech to the middle word
        public bool apply(POSTagger tagger)
        {
            if (hasToTag(tagger) && checkContext(tagger))
            {
                tagger.tagBuff[3] = to;
                return true;
            }
            else
                return false;
        }
    }
}
