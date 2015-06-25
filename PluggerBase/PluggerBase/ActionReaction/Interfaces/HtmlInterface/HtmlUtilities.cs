/******************************************************************\
 *      Class Name:     HtmlUtilities
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * A collection of static functions for producing HTML
\******************************************************************/
using System;

namespace PluggerBase.ActionReaction.Interfaces.HtmlInterface
{
    public class HtmlUtilities
    {
        public static string Input(string type, string name, string value) {
            return string.Format("<input type=\"{0}\" name=\"{1}\" value=\"{2}\" />", type, name, value);
        }

        public static string TextArea(string name, int rows, int cols, string value)
        {
            return string.Format("<textarea name=\"{0}\" rows=\"{1}\" cols=\"{2}\">{3}</textarea>", name, rows, cols, value);
        }

        public static string SpnCl(string cls, string contents)
        {
            return string.Format("<span class=\"{0}\">{1}</span>", cls, contents);
        }

        public static string DivCl(string cls, string contents)
        {
            return string.Format("<div class=\"{0}\">{1}</div>", cls, contents);
        }
    }
}
