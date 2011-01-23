/******************************************************************\
 *      Class Name:     HtmlInterface
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * This file is part of Plugger Base and is free software: you can
 * redistribute it and/or modify it under the terms of the GNU
 * Lesser General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option)
 * any later version.
 * 
 * Plugger Base is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with Plugger Base.  If not, see
 * <http://www.gnu.org/licenses/>.
 *      -----------------------------------------------------------
 * Wrapper class for producing and handling forms for IHandlers
 *   with a collection of IHtmlFormable argument types
\******************************************************************/
using System;
using System.Text;
using PluggerBase.ActionReaction.Actions;

namespace PluggerBase.ActionReaction.Interfaces.HtmlInterface
{
    public class HtmlInterface
    {
        protected PluginEnvironment plugenv;
        protected IAction action;

        public HtmlInterface(PluginEnvironment plugenv, IAction action)
        {
            this.plugenv = plugenv;
            this.action = action;
        }

        public string GetForm(string name, ArgumentTree args, ArgumentTree invalidity)
        {
            if (action is IHtmlFormable)
                return ((IHtmlFormable) action).GetHtmlForm(name, args, invalidity).Value.ToString();

            IArgumentType type = action.Input;

            StringBuilder output = new StringBuilder();

            output.AppendLine("<table>");
            if (type is IHtmlFormable)
                output.AppendLine(((IHtmlFormable)type).GetHtmlForm(name, args, invalidity).Value.ToString());
            output.AppendLine("</table>");

            output.AppendLine(HtmlUtilities.Input("submit", "submit", "Process"));

            return output.ToString();
        }

        public ArgumentTree TestForm(string name, ArgumentTree args)
        {
            // Are all inputs valid?
            return action.Input.IsValid(args.Children[name]);
        }

        public string ProcessForm(string name, ArgumentTree args) {
            object result = QueueArena.CallResult(action, args.Children[name].Value, int.MaxValue, double.MaxValue);

            if (action is IHtmlOutputable)
            {
                ArgumentTree output = ((IHtmlOutputable)action).GetHtml(result);
                if (output != null && output.Value != null)
                    return output.Value.ToString();
            }
            if (action.Output is IHtmlOutputable)
            {
                ArgumentTree output = ((IHtmlOutputable)action.Output).GetHtml(result);
                if (output != null && output.Value != null)
                    return output.Value.ToString();
            }

            // Try to convert to something that is outputable
            object output3 = plugenv.ImmediateConvertTo(result, new HtmlOutputArgumentType(), 2, 200);
            if (output3 != null)
                return output3.ToString();
            else
                return "Done.";
        }
    }
}
