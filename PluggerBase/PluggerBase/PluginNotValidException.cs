/******************************************************************\
 *      Class Name:     PluginNotValidException
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * The exception thrown when a plugin is not valid or fails to
 *   initialize
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace PluggerBase
{
    public class PluginNotValidException : Exception
    {
        protected Type type;

        // wrapper on a generic exception, with a plugin class type
        public PluginNotValidException(Type type, string message)
            : base(message)
        {
            this.type = type;
        }
    }
}
