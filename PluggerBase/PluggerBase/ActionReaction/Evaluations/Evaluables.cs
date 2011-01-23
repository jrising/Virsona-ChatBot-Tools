/******************************************************************\
 *      Class Names:    IEvaluable, EvaluableWrapper
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Basic classes for evaluables
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.FastSerializer;

namespace PluggerBase.ActionReaction.Evaluations
{
    // The Evaluable framework allow for salience-based execuation of delayed operations
    public interface IEvaluable
    {
        int Evaluate();
    }

    public delegate int Evaluatelet(IArena arena, double salience, params object[] args);

    public class EvaluableWrapper : AgentBase, IEvaluable
    {
        // either evaluable or evaluatelet will be provided
        protected IEvaluable evaluable;

        protected Evaluatelet evaluatelet;
        protected object[] args;

        public EvaluableWrapper(IEvaluable evaluable)
        {
            this.evaluable = evaluable;
        }
        
        public EvaluableWrapper(Evaluatelet evaluatelet, params object[] args)
        {
            this.evaluatelet = evaluatelet;
            this.args = args;
        }

        #region IEvaluable Members

        public int Evaluate()
        {
            if (evaluable != null)
                return evaluable.Evaluate();
            else
                return evaluatelet(arena, salience, args);
        }

        #endregion
    }
}
