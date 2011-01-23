/******************************************************************\
 *      Class Name:     LastFailure
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Utility failure-generator which tracks the reason given by the
 * most recent generated failure called.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.ActionReaction.Evaluations;

namespace PluggerBase.ActionReaction.Actions
{
    public class LastFailure
    {
        protected string reason;
        protected IContinuation skip;

        public LastFailure()
        {
            reason = null;
            skip = new NopCallable();
        }

        public string Reason
        {
            get
            {
                return reason;
            }
        }

        public IContinuation Skip
        {
            get
            {
                return skip;
            }
        }

        public IFailure GetFail()
        {
            return new Failure(this);
        }

        public class Failure : AgentBase, IFailure
        {
            protected LastFailure parent;

            public Failure(LastFailure parent)
            {
                this.parent = parent;
            }

            #region IFailure Members

            public int Fail(string reason, IContinuation skip)
            {
                parent.reason = reason;
                parent.skip = skip;

                return 1;
            }

            public object Clone()
            {
                return new Failure(parent);
            }

            #endregion
        }
    }
}
