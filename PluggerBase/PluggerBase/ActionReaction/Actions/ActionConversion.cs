/******************************************************************\
 *      Class Name:     ActionConversion
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Searches through available actions to find an applicable
 * conversion for the given arguments.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.ActionReaction.Evaluations;
using PluggerBase.ActionReaction.Interfaces;

namespace PluggerBase.ActionReaction.Actions
{
    public class ActionConversion : AgentBase, ICallable
    {
        protected PluginEnvironment plugenv;
        protected IArgumentType resultType;
        protected Dictionary<IAction, int> searched;
        protected Aborter aborter;

        // makes copy of searched
        public ActionConversion(PluginEnvironment plugenv, IArgumentType resultType, Dictionary<IAction, int> searched, Aborter aborter)
        {
            this.plugenv = plugenv;
            this.resultType = resultType;
            this.searched = new Dictionary<IAction, int>(searched);
            this.aborter = aborter;
        }

        #region ICallable Members

        public bool Call(object value, IContinuation succ, IFailure fail)
        {
            if (aborter.IsAborted)
                return true;   // abort!

            List<IAction> namedacts = plugenv.GetNamedActions(resultType.Name);
            // Remove all actions that have already been used
            int ii = 0;
            while (ii < namedacts.Count)
            {
                if (searched.ContainsKey(namedacts[ii]))
                    namedacts.RemoveAt(ii);
                else
                {
                    searched.Add(namedacts[ii], 1);
                    ii++;
                }
            }

            if (namedacts.Count == 0)
                return arena.Fail(fail, salience, "no matching acts", succ);

            IContinuation next = new ContinueletWrapper(CheckAction, value, succ);
            IFailure more = new FailletWrapper(RecurseConversionAttempt, value, namedacts, succ);

            if (namedacts.Count == 1)
                return arena.Continue(next, salience, namedacts[0], more);

            TryValues<IAction> tryeach = new TryValues<IAction>();
            return arena.Call(tryeach, salience * .9, namedacts, next, more);
        }

        #endregion

        public bool CheckAction(IArena arena, double salience, object value, IFailure fail, params object[] args)
        { // params: object oldval, IContinuation succ
            if (aborter.IsAborted)
                return true;   // abort!

            ArgumentTree argtree = new ArgumentTree(args[0]);
            IContinuation succ = (IContinuation)args[1];
            IAction action = (IAction)value;

            ArgumentTree result = action.Input.IsValid(argtree);
            if (result == null)
            {
                // abort the search-- we found an appropriate action!
                aborter.Abort();

                return arena.Call(action, salience, args[0], succ, fail);
            }
            else
                return arena.Fail(fail, salience, "input type doesn't match", succ);
        }

        // faillet: Look for ways to get to these inputs, to get that result type
        public bool RecurseConversionAttempt(IArena arena, double salience, string reason, IContinuation skip, params object[] args)
        { // params: object value, IEnumerable<IAction> actions, IContinuation succ
            if (aborter.IsAborted)
                return true;   // abort!

            // Can we go down another level?
            Aborter next = new Aborter(aborter);
            if (next.IsAborted)
                return true;

            object value = args[0];
            IEnumerable<IAction> actions = (IEnumerable<IAction>)args[1];
            IContinuation succ = (IContinuation)args[2];

            foreach (IAction action in actions)
                arena.Call(new ActionConversion(plugenv, action.Input, searched, next), salience * .9, value,
                    new CallableAsContinuation(action, succ), new NopCallable());

            return true;
        }
    }
}
