/******************************************************************\
 *      Class Names:    IContinuation, ContinueletWrapper,
 *                      ContinuationAsEvaluable
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Basic classes for continuations
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace PluggerBase.ActionReaction.Evaluations
{
    public interface IContinuation : ICloneable
    {
        bool Continue(object value, IFailure fail);
    }

    public delegate bool Continuelet(IArena arena, double salience, object value, IFailure fail, params object[] args);

    public class ContinueletWrapper : AgentBase, IContinuation
    {
        protected Continuelet continuelet;
        protected object[] args;

        public ContinueletWrapper(Continuelet continuelet, params object[] args)
        {
            this.continuelet = continuelet;
            this.args = args;
        }

        #region IContinuation Members

        public bool Continue(object value, IFailure fail)
        {
            return continuelet(arena, salience, value, fail, args);
        }

        public object Clone()
        {
            return new ContinueletWrapper(continuelet, args);
        }

        #endregion
    }

    public class ContinuationAsEvaluable : AgentBase, IEvaluable
    {
        protected IContinuation continuation;
        protected object value;
        protected IFailure fail;

        public ContinuationAsEvaluable(IContinuation continuation, object value, IFailure fail)
        {
            this.continuation = continuation;
            this.value = value;
            this.fail = fail;
        }

        public override int Initialize(IArena arena, double salience)
        {
            int used = 0;
            if (continuation is IAgent)
                used += ((IAgent)continuation).Initialize(arena, salience);

            return base.Initialize(arena, salience) + used;
        }

        public override bool Complete()
        {
            if (continuation is IAgent)
                return ((IAgent)continuation).Complete();
            return base.Complete();
        }

        #region IEvaluable Members

        public bool Evaluate()
        {
            return continuation.Continue(value, fail);
        }

        #endregion
    }
}
