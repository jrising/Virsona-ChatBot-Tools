/******************************************************************\
 *      Class Name:     ImmediateArena
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * A very simple arena which immediately performs whatever action
 * is asked of it (at the risk of infinite loops)
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace PluggerBase.ActionReaction.Evaluations
{
    public class ImmediateArena : IArena
    {
        #region IArena Members

        public int Evaluate(IEvaluable evaluable, double salience)
        {
            if (evaluable is IAgent)
                ((IAgent)evaluable).Initialize(this, salience);
            if (salience > 0)
                return evaluable.Evaluate();
            return 1;
        }

        public int Call(ICallable callable, double salience, object value, IContinuation succ, IFailure fail)
        {
            if (callable is IAgent)
                ((IAgent)callable).Initialize(this, salience);
            if (salience > 0)
                return callable.Call(value, succ, fail);
            return 1;
        }

        public int Continue(IContinuation cont, double salience, object value, IFailure fail)
        {
            // Clone it!
            cont = (IContinuation)cont.Clone();
            if (cont is IAgent)
                ((IAgent)cont).Initialize(this, salience);
            if (salience > 0)
                return cont.Continue(value, fail);
            return 1;
        }

        public int Fail(IFailure fail, double salience, string reason, IContinuation skip)
        {
            // Clone it!
            fail = (IFailure)fail.Clone();
            if (fail is IAgent)
                ((IAgent)fail).Initialize(this, salience);
            if (salience > 0)
                return fail.Fail(reason, skip);
            return 1;
        }

        #endregion

        #region IFastSerializable Members

        public void Deserialize(PluggerBase.FastSerializer.SerializationReader reader)
        {
            // nothing to do
        }

        public void Serialize(PluggerBase.FastSerializer.SerializationWriter writer)
        {
            // nothing to do
        }

        #endregion
    }
}
