/******************************************************************\
 *      Class Name:     Aborter
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Helper class for watching the recursion level of many children,
 * and allowing them all to abort when one succeeds.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace PluggerBase.ActionReaction.Actions
{
    public class Aborter
    {
        public static Aborter NewAborter(int maxlevel)
        {
            Aborter common = new Aborter(maxlevel);
            Aborter child = new Aborter(common);
            child.level = 1;

            return child;
        }

        protected Aborter common;
        protected int level;

        protected Aborter(int maxlevel)
        {
            this.level = maxlevel;
            this.common = this;
        }

        public Aborter(Aborter parent)
        {
            this.level = parent.level + 1;
            this.common = parent.common;
        }

        public bool IsAborted
        {
            get
            {
                return level > common.level;
            }
        }

        public void Abort()
        {
            common.level = 0;
        }
    }
}
