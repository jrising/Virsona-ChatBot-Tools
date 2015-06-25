/******************************************************************\
 *      Class Name:     InvalidRuleException
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Tagger exception
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace HeppleTagger
{
    public class InvalidRuleException : Exception
    {
        // Simple wrapper on a generic exception
        public InvalidRuleException(string msg)
            : base(msg) { }
    }
}
