/******************************************************************\
 *      Class Name:     PluginDisplayNameAttribute
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * The basic name attribute implemented by all plugins
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace PluggerBase
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PluginDisplayNameAttribute : Attribute
    {
        private string displayName;

        public PluginDisplayNameAttribute(string displayName)
            : base()
        {
            this.displayName = displayName;
        }

        public override string ToString()
        {
            return displayName;
        }
    }
}
