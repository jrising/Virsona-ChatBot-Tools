/******************************************************************\
 *      Class Name:     CallAgent
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * An ICallable with the information of a codelet
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.ActionReaction.Evaluations;

namespace DataTemple.AgentEvaluate
{
    public enum ArgumentMode
    {
        NoArugments,
        SingleArgument,
        ManyArguments,
		DelimitedUnevaluated,
        RemainderUnevaluated
    }

    public class CallAgent : ICallable
    {
        protected ArgumentMode argumentMode;

        protected double salience;
        protected int space;
        protected int time;

        public CallAgent(ArgumentMode argumentMode, double salience, int space, int time)
        {
            this.argumentMode = argumentMode;
            this.salience = salience;
            this.space = space;
            this.time = time;
        }

        public ArgumentMode ArgumentOptions
        {
            get
            {
                return argumentMode;
            }
        }

        public double Salience
        {
            get
            {
                return salience;
            }
        }

        public int Space
        {
            get
            {
                return space;
            }
        }

        public int Time
        {
            get
            {
                return time;
            }
        }

        public virtual bool Call(object value, IContinuation succ, IFailure fail)
        {
            return false;
        }
    }
}
