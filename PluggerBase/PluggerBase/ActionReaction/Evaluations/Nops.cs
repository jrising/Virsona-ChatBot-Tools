/******************************************************************\
 *      Class Name:     NopCallable
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * An ICallable, IContinuation, and IFailure that all do nothing
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace PluggerBase.ActionReaction.Evaluations
{
    public class NopCallable : ICallable, IContinuation, IFailure, IEvaluable
    {
        public NopCallable()
        {
        }

        #region ICallable Members

        public int Call(object value, IContinuation succ, IFailure fail)
        {
            return 1;
        }

        #endregion

        #region IContinuation Members

        public int Continue(object value, IFailure fail)
        {
            return 1;
        }

        #endregion

        #region IFailure Members

        public int Fail(string reason, IContinuation succ)
        {
            return 1;
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
		
        #region IEvaluable Members

        public int Evaluate()
        {
            return 1;
        }

        #endregion

    }
	
	
}
