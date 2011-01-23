/******************************************************************\
 *      Class Name:     BestContinuation
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Continuation-creating helper, which keeps track of the best
 * result value given to any of its created children.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.ActionReaction.Evaluations;

namespace PluggerBase.ActionReaction.Actions
{
    public class BestContinuation
    {
        protected double salience;
        protected object value;
        protected IFailure fail;

        public BestContinuation()
        {
            salience = 0;
            value = null;
            fail = new NopCallable();
        }

        public double Salience
        {
            get
            {
                return salience;
            }
        }

        public object Value
        {
            get
            {
                return value;
            }
        }

        public IFailure Failure
        {
            get
            {
                return fail;
            }
        }

        public IContinuation GetContinue()
        {
            return new Continuation(this);
        }

        public class Continuation : AgentBase, IContinuation
        {
            protected BestContinuation parent;

            public Continuation(BestContinuation parent)
            {
                this.parent = parent;
            }

            #region IContinuation Members

            public int Continue(object value, IFailure fail)
            {
                if (parent.salience <= salience)
                {
                    parent.salience = salience;
                    parent.value = value;
                    parent.fail = fail;
                }

                return 1;
            }

            public object Clone()
            {
                return new Continuation(parent);
            }

            #endregion
        }
    }
}
