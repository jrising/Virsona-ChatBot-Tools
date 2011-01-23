/******************************************************************\
 *      Class Name:     ArgumentsHandler
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Ease-of-coding class for any Action that can be described as
 * a single function which takes a ArgumentTree argument and
 * returns and ArgumentTree result.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.ActionReaction.Evaluations;
using PluggerBase.ActionReaction.Interfaces;
using PluggerBase.ActionReaction.Interfaces.HtmlInterface;

namespace PluggerBase.ActionReaction.Actions
{
    public abstract class ArgumentsHandler : FormableAction
    {
        protected string[] argnames;
        protected string[] argtitles;
        protected IArgumentType[] argtypes;
        protected string[] argdescs;
        protected bool[] argreqs;

        protected SetArgumentTreeArgumentType resulttype;

        protected int time;

        protected Dictionary<string, int> argmap;

        public ArgumentsHandler(string title, string description, string[] argnames, string[] argtitles, IArgumentType[] argtypes, string[] argdescs, bool[] argreqs, SetArgumentTreeArgumentType resulttype, int time)
            : base(title, description)
        {
            this.argnames = argnames;
            this.argtitles = argtitles;
            this.argtypes = argtypes;
            this.argdescs = argdescs;
            this.argreqs = argreqs;
            this.resulttype = resulttype;

            this.time = time;

            argmap = new Dictionary<string, int>();
            for (int ii = 0; ii < argnames.Length; ii++)
                argmap[argnames[ii]] = ii;
        }

        public string[] ArgumentNames
        {
            get
            {
                return argnames;
            }
        }

        public string GetArgumentTitle(string name)
        {
            return argtitles[argmap[name]];
        }

        public IArgumentType GetArgumentType(string name)
        {
            return argtypes[argmap[name]];
        }

        public string GetArgumentDescription(string name)
        {
            return argdescs[argmap[name]];
        }

        bool IsArgumentRequired(string name)
        {
            return argreqs[argmap[name]];
        }

        public abstract ArgumentTree Handle(ArgumentTree args);

        #region IHtmlFormable Members

        public override ArgumentTree GetHtmlForm(string name, ArgumentTree args, ArgumentTree invalidity)
        {
            StringBuilder output = new StringBuilder();

            output.AppendLine(HtmlUtilities.SpnCl("handler", title));
            output.AppendLine(HtmlUtilities.DivCl("desc", description));

            output.AppendLine("<table>");
            for (int ii = 0; ii < argnames.Length; ii++)
            {
                if (argtypes[ii] is IHtmlFormable)
                {
                    if (argreqs[ii])
                        output.AppendLine("<th>" + argtitles[ii] + "<font color=\"red\">*</font></th><td>" + ((IHtmlFormable)argtypes[ii]).GetHtmlForm(HtmlArgumentTree.AppendKey(name, argnames[ii]), args, invalidity) + "</td>");
                    else
                        output.AppendLine("<th>" + argtitles[ii] + "</th><td>" + ((IHtmlFormable)argtypes[ii]).GetHtmlForm(HtmlArgumentTree.AppendKey(name, argnames[ii]), args, invalidity) + "</td>");
                }
                output.AppendLine("<td colspan=\"2\">" + argdescs[ii] + "</td>");
            }
            output.AppendLine("</table>");

            output.AppendLine(HtmlUtilities.Input("submit", "submit", "Process"));

            return new ArgumentTree(output.ToString());
        }

        #endregion

        #region IAction Members

        public override IArgumentType Input
        {
            get {
                // turn argnames and argtypes into ArgumentTree
                ArgumentTree argtree = new ArgumentTree();
                for (int ii = 0; ii < argnames.Length; ii++)
                    argtree[argnames[ii]] = new ArgumentTree(argtypes[ii]);

                return new SetArgumentTreeArgumentType(title + ":Arguments", argtree);
            }
        }

        public override IArgumentType Output
        {
            get {
                return resulttype;
            }
        }

        #endregion

        #region ICallable Members

        public override int Call(object value, IContinuation succ, IFailure fail)
        {
            try
            {
                ArgumentTree result = Handle((ArgumentTree)value);
                return time + arena.Continue(succ, salience, result, fail);
            }
            catch (Exception ex)
            {
                return time + 2 + arena.Fail(fail, salience, ex.Message, succ);
            }
        }

        #endregion
    }
}
