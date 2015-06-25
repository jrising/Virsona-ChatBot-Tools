/******************************************************************\
 *      Class Name:     HtmlArgumentTree
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
 * A collection of static functions for handling ArgumentTrees in
 *   an HTML environment
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;

namespace PluggerBase.ActionReaction.Interfaces.HtmlInterface
{
    public class HtmlArgumentTree
    {
        public static ArgumentTree LoadFromParams(NameValueCollection form)
        {
            Dictionary<string, ArgumentTree> children = new Dictionary<string, ArgumentTree>();

            foreach (string key in form.AllKeys)
            {
                string value = form[key];
                string prefix = key;
                Dictionary<string, ArgumentTree> addto = children;

                while (true)
                {
                    string stripped;
                    prefix = StripPrefix(prefix, out stripped);
                    if (string.IsNullOrEmpty(stripped))
                        break;

                    ArgumentTree grandchildren;
                    if (!addto.TryGetValue(prefix, out grandchildren))
                        addto[prefix] = grandchildren = new ArgumentTree();

                    addto = grandchildren.Children;
                    prefix = stripped;
                }

                addto[prefix] = new ArgumentTree(value);
            }

            return new ArgumentTree(null, children);
        }
        
        public static object GetArgument(ArgumentTree tree, string name, object defval)
        {
            if (string.IsNullOrEmpty(name))
                return tree.Value;

            string stripped;
            string key = StripPrefix(name, out stripped);

            ArgumentTree child;
            if (!tree.Children.TryGetValue(key, out child))
                return defval;

            return HtmlArgumentTree.GetArgument(child, stripped, defval);
        }

        public static string ToHtml(ArgumentTree tree)
        {
            if (tree.Value == null)
                return "";
            else
                return tree.Value.ToString();
        }

        public static string AllToHtml(ArgumentTree tree)
        {
            if (tree.Children.Count == 0)
                return ToHtml(tree);

            StringBuilder output = new StringBuilder();
            output.Append(tree.Value);
            output.Append("<ul>");
            foreach (KeyValuePair<string, ArgumentTree> kvp in tree.Children)
                output.Append("<li>" + kvp.Key + ": " + HtmlArgumentTree.AllToHtml(kvp.Value) + "</li>");
            output.Append("</ul>");

            return output.ToString();
        }

        public static string AppendKey(string prefix, string key)
        {
            if (string.IsNullOrEmpty(prefix))
                return key;
            else
                return prefix + "[" + key + "]";
        }

        public static string StripPrefix(string name, out string stripped)
        {
            if (!name.Contains("["))
            {
                stripped = "";
                return name;
            }
            else
            {
                int preflen = name.IndexOf('[');
                string prefix = name.Substring(0, preflen);
                stripped = name.Substring(preflen + 1, name.IndexOf(']') - preflen - 1) + name.Substring(name.IndexOf(']') + 1);

                return prefix;
            }
        }
    }
}
