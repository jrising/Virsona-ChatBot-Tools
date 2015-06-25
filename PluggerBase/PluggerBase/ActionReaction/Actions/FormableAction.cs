/******************************************************************\
 *      Class Name:     FormableAction
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Ease-of-coding base class for providing an Html interface based
 * on the IAction's argument types.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.ActionReaction.Evaluations;
using PluggerBase.ActionReaction.Interfaces;
using PluggerBase.ActionReaction.Interfaces.HtmlInterface;

namespace PluggerBase.ActionReaction.Actions
{
    public abstract class FormableAction : AgentBase, IAction, IHtmlFormable
    {
        protected string title;
        protected string description;

        public FormableAction(string title, string description)
        {
            this.title = title;
            this.description = description;
        }

        public string Title
        {
            get
            {
                return title;
            }
        }

        public string Description
        {
            get
            {
                return description;
            }
        }

        #region IHtmlFormable Members

        public virtual ArgumentTree GetHtmlForm(string name, ArgumentTree args, ArgumentTree invalidity)
        {
            StringBuilder output = new StringBuilder();

            output.AppendLine(HtmlUtilities.SpnCl("handler", title));
            output.AppendLine(HtmlUtilities.DivCl("desc", description));

            if (Input is IHtmlFormable)
                output.AppendLine(((IHtmlFormable)Input).GetHtmlForm(name, args, invalidity).Value.ToString());

            output.AppendLine(HtmlUtilities.Input("submit", "submit", "Process"));

            return new ArgumentTree(output.ToString());
        }

        #endregion

        #region IAction Members

        public abstract IArgumentType Input { get; }
        public abstract IArgumentType Output { get; }
        public abstract bool Call(object value, IContinuation succ, IFailure fail);

        #endregion
    }
}
