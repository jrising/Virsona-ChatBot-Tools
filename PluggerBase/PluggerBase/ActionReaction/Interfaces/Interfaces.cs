/******************************************************************\
 *      Class Name:     Interfaces
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * The basic interfaces to deliniate the interface system
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace PluggerBase.ActionReaction.Interfaces
{
    public interface IHtmlFormable
    {
        ArgumentTree GetHtmlForm(string name, ArgumentTree args, ArgumentTree invalidity);
    }

    public interface IHtmlOutputable
    {
        ArgumentTree GetHtml(object value);
    }

    public class HtmlOutputArgumentType : NamedArgumentType
    {
        public HtmlOutputArgumentType()
            : base("text/html", new StringArgumentType(int.MaxValue, ".+", "<span></span>"))
        {
        }
    }

    public interface IXmlOutput
    {
        string GetXml(object value);
    }

    public class XmlOutputArgumentType : NamedArgumentType
    {
        public XmlOutputArgumentType()
            : base("text/xml", new StringArgumentType(int.MaxValue, ".+", "<foo bar=\"baz\" />"))
        {
        }
    }

    public interface IWindowsForm
    {
    }

    public interface IWindowsDisplay
    {
    }
}
