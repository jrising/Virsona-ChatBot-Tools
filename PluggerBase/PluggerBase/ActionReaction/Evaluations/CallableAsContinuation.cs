/******************************************************************\
 *      Class Names:    CallableAsContinuation
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Class to wrap a callable into a continuation
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace PluggerBase.ActionReaction.Evaluations
{
    class CallableAsContinuation : AgentBase, IContinuation
    {
        protected ICallable callable;
        protected IContinuation succ;

        public CallableAsContinuation(ICallable callable, IContinuation succ)
        {
            this.callable = callable;
            this.succ = succ;
        }

        #region IContinuation Members

        public int Continue(object value, IFailure fail)
        {
            return callable.Call(value, succ, fail);
        }

        public object Clone()
        {
            return new CallableAsContinuation(callable, (IContinuation) succ.Clone());
        }

        #endregion
    }
}
