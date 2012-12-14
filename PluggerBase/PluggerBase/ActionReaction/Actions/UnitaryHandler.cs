/******************************************************************\
 *      Class Name:     UnitaryHandler
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * UnitaryHandler is a base class for ease of coding IActions that
 * work as a single function, taking a single argument and return-
 * ing a single result.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.ActionReaction.Evaluations;
using PluggerBase.ActionReaction.Interfaces;
using PluggerBase.ActionReaction.Interfaces.HtmlInterface;

namespace PluggerBase.ActionReaction.Actions
{
    public abstract class UnitaryHandler : FormableAction
    {
        protected IArgumentType arguments;
        protected IArgumentType results;
        protected int time;

        public UnitaryHandler(string title, string description, IArgumentType arguments, IArgumentType results, int time)
            : base(title, description)
        {
            this.arguments = arguments;
            this.results = results;
            this.time = time;
        }

        public abstract object Handle(object args);

        #region IAction Members

        public override IArgumentType Input
        {
            get {
                return arguments;
            }
        }

        public override IArgumentType Output
        {
            get {
                return results;
            }
        }

        #endregion

        #region ICallable Members

        public override bool Call(object value, IContinuation succ, IFailure fail)
        {
            try
            {
                object results = Handle(value);
                return arena.Continue(succ, salience, results, fail);
            }
            catch (Exception ex)
            {
                return arena.Fail(fail, salience, ex.Message, succ);
            }
        }

        #endregion
    }
}
