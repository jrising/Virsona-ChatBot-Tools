/******************************************************************\
 *      Class Names:    ICallable, CallletWrapper, 
 *                      CallableAsEvaluable
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Basic classes for callables
 * Callables are called with continuations
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace PluggerBase.ActionReaction.Evaluations
{
    // The Callable framework allows for ambiguous and halted evaluation
    // Often, ICallables, IContinuations, and IFailures will be IEvaluables

    public interface ICallable
    {
        bool Call(object value, IContinuation succ, IFailure fail);
    }

    public delegate bool Calllet(IArena arena, double salience, object value, IContinuation succ, IFailure fail, params object[] args);

    public class CallletWrapper : AgentBase, ICallable
    {
        protected Calllet calllet;
        protected object[] args;
        
        public CallletWrapper(Calllet calllet, params object[] args)
        {
            this.calllet = calllet;
            this.args = args;
        }

        #region ICallable Members

        public bool Call(object value, IContinuation succ, IFailure fail)
        {
            return calllet(arena, salience, value, succ, fail, args);
        }

        #endregion
    }

    public class CallableAsEvaluable : AgentBase, IEvaluable
    {
        protected ICallable callable;
        protected object value;
        protected IContinuation succ;
        protected IFailure fail;

        public CallableAsEvaluable(ICallable callable, object value, IContinuation succ, IFailure fail)
        {
            this.callable = callable;
            this.value = value;
            this.succ = succ;
            this.fail = fail;
        }

        public override int Initialize(IArena arena, double salience)
        {
            int used = 0;
            if (callable is IAgent)
                used += ((IAgent)callable).Initialize(arena, salience);

            return base.Initialize(arena, salience) + used;
        }

        public override bool Complete()
        {
            if (callable is IAgent)
                return ((IAgent)callable).Complete();
            return base.Complete();
        }

        #region IEvaluable Members

        public bool Evaluate()
        {
            return callable.Call(value, succ, fail);
        }

        #endregion
    }
}
