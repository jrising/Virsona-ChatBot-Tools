/******************************************************************\
 *      Class Name:     TryValues
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Callable to search through available actions for a given
 * argument
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.ActionReaction.Evaluations;

namespace PluggerBase.ActionReaction.Actions
{
    // Call cont on each element in value (an enumerable of T), with the failure being to continue down enumerables
    class TryValues<T> : AgentBase, ICallable
    {
        protected IEnumerator<T> enumerator;

        public TryValues()
        {
        }

        #region ICallable Members

        public int Call(object value, IContinuation succ, IFailure fail)
        {
            enumerator = ((IEnumerable<T>)value).GetEnumerator();
            return 1 + ContinueNext(arena, salience, "no values", succ, succ, fail);
        }

        #endregion

        // faillet: called each time we need to try the next option
        public int ContinueNext(IArena arena, double salience, string reason, IContinuation skip, params object[] args)
        { // params: IContinuation succ, IFailure fail
            IContinuation succ = (IContinuation)args[0];
            IFailure fail = (IFailure)args[1];

            // Try the next value!
            if (enumerator.MoveNext())
                return 2 + arena.Continue(succ, salience, enumerator.Current, new FailletWrapper(ContinueNext, succ, fail));
            else
            {
                // nothing more to try!
                return 2 + arena.Fail(fail, salience, reason, succ);
            }
        }
    }
}
