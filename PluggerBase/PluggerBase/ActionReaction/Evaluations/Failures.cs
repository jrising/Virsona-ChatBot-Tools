/******************************************************************\
 *      Class Names:    IFailure, FailletWrapper, FailureAsEvaluable
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Basic classes for failures
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace PluggerBase.ActionReaction.Evaluations
{
    public interface IFailure : ICloneable
    {
        bool Fail(string reason, IContinuation skip);
    }

    public delegate bool Faillet(IArena arena, double salience, string reason, IContinuation skip, params object[] args);

    public class FailletWrapper : AgentBase, IFailure
    {
        protected Faillet faillet;
        protected object[] args;

        public FailletWrapper(Faillet faillet, params object[] args)
        {
            this.faillet = faillet;
            this.args = args;
        }

        #region IFailure Members

        public bool Fail(string reason, IContinuation skip)
        {
            return faillet(arena, salience, reason, skip, args);
        }

        public object Clone()
        {
            return new FailletWrapper(faillet, args);
        }

        #endregion
    }

    public class FailureAsEvaluable : AgentBase, IEvaluable
    {
        protected IFailure fail;
        protected string reason;
        protected IContinuation skip;

        public FailureAsEvaluable(IFailure fail, string reason, IContinuation skip)
        {
            this.fail = fail;
            this.reason = reason;
            this.skip = skip;
        }

        public override int Initialize(IArena arena, double salience)
        {
            int used = 0;
            if (fail is IAgent)
                used += ((IAgent)fail).Initialize(arena, salience);

            return base.Initialize(arena, salience) + used;
        }

        public override bool Complete()
        {
            if (fail is IAgent)
                return ((IAgent)fail).Complete();
            return base.Complete();
        }

        #region IEvaluable Members

        public bool Evaluate()
        {
            return fail.Fail(reason, skip);
        }

        #endregion
    }
}
