/******************************************************************\
 *      Class Name:     BaseArgumentTypes
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
 * A collection of classes describing basic types of arguments
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using PluggerBase.ActionReaction.Interfaces.HtmlInterface;

namespace PluggerBase.ActionReaction.Interfaces
{
    public interface IArgumentType
    {
        string Name { get; }
        object Example { get; }
        ArgumentTree IsValid(ArgumentTree value);
    }

    public class AnyArgumentType : IArgumentType
    {
        protected object example;

        public AnyArgumentType(object example)
        {
            this.example = example;
        }

        #region IArgumentType Members

        public object Example
        {
            get { return example; }
        }

        public ArgumentTree IsValid(ArgumentTree value)
        {
            return null;
        }

        public string Name
        {
            get
            {
                return "";
            }
        }

        #endregion
    }

    public class NullArgumentType : IArgumentType {

        public NullArgumentType() {
        }

        #region IArgumentType Members

        public object Example
        {
	        get {
                return null;
            }
        }

        public ArgumentTree IsValid(ArgumentTree value)
        {
            return (value == null || value.Value == null ? null : new ArgumentTree("must be null"));
        }

        public string Name
        {
            get
            {
                return "nil";
            }
        }

        #endregion
    }

    public class UnknownArgumentType : IArgumentType
    {
        public UnknownArgumentType()
        {
        }

        #region IArgumentType Members

        public object Example
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public ArgumentTree IsValid(ArgumentTree value)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public string Name
        {
            get
            {
                throw new Exception("Unknown Argument Type");
            }
        }

        #endregion
    }

    public class AmbiguousArgumentType : IArgumentType
    {
        protected Random randgen;
        protected IArgumentType[] types;

        public AmbiguousArgumentType(IArgumentType[] types)
        {
            this.types = types;
            randgen = new Random();
        }

        public IArgumentType[] Types
        {
            get
            {
                return types;
            }
        }

        #region IArgumentType Members

        public object Example
        {
            get {
                return types[randgen.Next(types.Length)].Example;
            }
        }

        public ArgumentTree IsValid(ArgumentTree value)
        {
            foreach (IArgumentType type in types)
                if (type.IsValid(value) == null)
                    return null;

            return new ArgumentTree("no types matched");
        }

        public string Name
        {
            get
            {
                string[] names = new string[types.Length];

                for (int ii = 0; ii < types.Length; ii++)
                    names[ii] = types[ii].Name;

                return string.Join("|", names);
            }
        }

        #endregion
    }

    public class ManyConstraintArgumentType : IArgumentType
    {
        protected Random randgen;
        protected IArgumentType[] types;
        protected object example;

        public ManyConstraintArgumentType(IArgumentType[] types, object example)
        {
            this.types = types;
            this.example = example;
            randgen = new Random();
        }

        public IArgumentType[] Types
        {
            get
            {
                return types;
            }
        }

        #region IArgumentType Members

        public object Example
        {
            get
            {
                object attempt = types[randgen.Next(types.Length)].Example;
                if (IsValid(new ArgumentTree(attempt)) == null)
                    return attempt;

                return example;
            }
        }

        public ArgumentTree IsValid(ArgumentTree value)
        {
            foreach (IArgumentType type in types)
            {
                ArgumentTree invalidity = type.IsValid(value);
                if (invalidity != null)
                    return invalidity;
            }

            return null;
        }

        public string Name {
            get {
                string[] names = new string[types.Length];

                for (int ii = 0; ii < types.Length; ii++)
                    names[ii] = types[ii].Name;

                return string.Join("&", names);
            }
        }                

        #endregion
    }

    public class BooleanArgumentType : IArgumentType
    {
        protected Random randgen;

        public BooleanArgumentType() {
            this.randgen = new Random();
        }

        #region IArgumentType Members

        public string Name
        {
            get {
                return "bool";
            }
        }

        public object Example
        {
            get {
                return (randgen.Next(2) == 0);
            }
        }

        public ArgumentTree IsValid(ArgumentTree value)
        {
            if (value.Value is bool)
                return null;

            return new ArgumentTree("not boolean");
        }

        #endregion
    }

    public class SelectableArgumentType : IArgumentType
    {
        protected Random randgen;
        protected object[] options;

        public SelectableArgumentType(object[] options)
        {
            this.options = options;
            randgen = new Random();
        }

        public object[] Options
        {
            get
            {
                return options;
            }
        }

        #region IArgumentType Members

        public object Example
        {
            get
            {
                return options[randgen.Next(options.Length)];
            }
        }

        public ArgumentTree IsValid(ArgumentTree value)
        {
            foreach (object option in options)
                if (option.Equals(value.Value))
                    return null;

            return new ArgumentTree("option not found");
        }

        public string Name
        {
            get
            {
                string[] strs = new string[options.Length];

                for (int ii = 0; ii < options.Length; ii++)
                    strs[ii] = options[ii].ToString();

                return string.Join("#", strs);

            }
        }

        #endregion
    }

    public class RangedArgumentType<T> : IArgumentType
        where T : IComparable
    {
        protected T minimum;
        protected T maximum;
        protected T example;

        public RangedArgumentType(T minimum, T maximum, T example)
        {
            this.minimum = minimum;
            this.maximum = maximum;
            this.example = example;
        }

        public T Minimum
        {
            get
            {
                return minimum;
            }
        }

        public T Maximum
        {
            get
            {
                return maximum;
            }
        }

        #region IArgumentType Members

        public object Example
        {
            get
            {
                return example;
            }
        }

        public ArgumentTree IsValid(ArgumentTree value)
        {
            if (value.Value is T)
            {
                if (minimum.CompareTo(value.Value) >= 0 && maximum.CompareTo(value.Value) <= 0)
                    return null;
                else
                    return new ArgumentTree("out of range");
            }

            return new ArgumentTree("not comparable");
        }

        public string Name
        {
            get
            {
                return minimum.ToString() + "-" + maximum.ToString();
            }
        }

        #endregion
    }

    public class TypedArgumentType : IArgumentType
    {
        protected Type type;
        protected object example;

        public TypedArgumentType(Type type, object example)
        {
            this.type = type;
            this.example = example;
        }

        public virtual object Example
        {
            get
            {
                return example;
            }
        }

        public virtual ArgumentTree IsValid(ArgumentTree value)
        {
            if (type.IsInstanceOfType(value.Value))
                return null;

            return new ArgumentTree("incorrect type");
        }

        public virtual string Name
        {
            get
            {
                return type.ToString();
            }
        }
    }

    public class StringArgumentType : TypedArgumentType, IHtmlFormable, IHtmlOutputable
    {
        protected int maxlen;
        protected string validre;
        protected Regex validater;

        public StringArgumentType(int maxlen, string validre, string example)
            : base(typeof(string), example)
        {
            this.maxlen = maxlen;
            this.validre = validre;
            this.example = example;
            validater = new Regex(validre);
        }

        public int MaxLength
        {
            get { return maxlen; }
        }

        public string ValidationRegExp
        {
            get { return validre; }
        }

        public override ArgumentTree IsValid(ArgumentTree value)
        {
            if (value.Value is string) {
                if (validater.IsMatch((string)value.Value))
                    return null;
                else
                    return new ArgumentTree("incorrect format");
            }

            return new ArgumentTree("not string");
        }

        #region IHtmlFormable Members

        public ArgumentTree GetHtmlForm(string name, ArgumentTree args, ArgumentTree invalidity)
        {
            string input = GetHtmlInput(name, args);
            if (invalidity == null)
                return new ArgumentTree(input);
            else
                return new ArgumentTree(input + " " + HtmlArgumentTree.ToHtml(invalidity), "input", input);
        }

        #endregion

        public string GetHtmlInput(string name, ArgumentTree args)
        {
            if (maxlen > 128)
                return HtmlUtilities.TextArea(name, 4, 64, HtmlArgumentTree.GetArgument(args, name, "").ToString());

            return HtmlUtilities.Input("text", name, HtmlArgumentTree.GetArgument(args, name, "").ToString());
        }

        #region IHtmlOutputable Members

        public ArgumentTree GetHtml(object value)
        {
            return new ArgumentTree(value);
        }

        #endregion
    }

    public class EnumerableArgumentType : IArgumentType, IHtmlOutputable
    {
        protected int maxcnt;
        protected IArgumentType argtype;

        public EnumerableArgumentType(int maxcnt, IArgumentType argtype)
        {
            this.maxcnt = maxcnt;
            this.argtype = argtype;
        }

        public int MaxCount
        {
            get { return maxcnt; }
        }

        public IArgumentType ElementType
        {
            get { return argtype; }
        }

        #region IArgumentType Members

        public object Example
        {
            get {
                Random randgen = new Random();
                List<object> list = new List<object>();

                do
                    list.Add(argtype.Example);
                while (list.Count < maxcnt && randgen.Next(2) == 0);

                return list;
            }
        }

        public ArgumentTree IsValid(ArgumentTree value)
        {
            if (!(value.Value is IEnumerable))
                return new ArgumentTree("not enumerable");

            foreach (object elt in (IEnumerable)value.Value) {
                ArgumentTree invalidity = argtype.IsValid(new ArgumentTree(elt));
                if (invalidity != null)
                    return invalidity;
            }

            return null;
        }

        public string Name
        {
            get
            {
                return "[" + argtype.Name + "]";
            }
        }

        #endregion

        #region IHtmlOutputable Members

        public ArgumentTree GetHtml(object value)
        {
            if (argtype is IHtmlOutputable)
            {
                StringBuilder orderedlist = new StringBuilder();

                orderedlist.AppendLine("<ol>");
                foreach (object elt in (IEnumerable)value)
                    orderedlist.AppendLine("<li>" + HtmlArgumentTree.ToHtml(((IHtmlOutputable)argtype).GetHtml(elt)) + "</li>");
                orderedlist.AppendLine("</ol>");

                return new ArgumentTree(orderedlist.ToString());
            } else
                return new ArgumentTree("List.");
        }

        #endregion
    }

    public class DictionaryArgumentType<TKey, TValue> : IArgumentType
    {
        protected IArgumentType keytype;
        protected IArgumentType valuetype;

        public DictionaryArgumentType(IArgumentType keytype, IArgumentType valuetype)
        {
            this.keytype = keytype;
            this.valuetype = valuetype;
        }

        public IArgumentType KeyType
        {
            get { return keytype; }
        }

        public IArgumentType ValueType
        {
            get { return valuetype; }
        }

        #region IArgumentType Members

        public virtual object Example
        {
            get
            {
                Dictionary<TKey, TValue> example = new Dictionary<TKey, TValue>();
                example.Add((TKey) keytype.Example, (TValue) valuetype.Example);
                return example;
            }
        }
        public virtual ArgumentTree IsValid(ArgumentTree value)
        {
            if (value.Value is IDictionary<TKey, TValue>)
            {
                foreach (KeyValuePair<TKey, TValue> kvp in (IDictionary<TKey, TValue>)value.Value)
                {
                    ArgumentTree keyvalid = keytype.IsValid(new ArgumentTree(kvp.Key));
                    if (keyvalid != null)
                        return keyvalid;

                    ArgumentTree valuevalid = valuetype.IsValid(new ArgumentTree(kvp.Value));
                    if (valuevalid != null)
                        return valuevalid;
                }

                return null;
            }

            return new ArgumentTree("not a dictionary");
        }

        public string Name
        {
            get
            {
                return "{" + keytype.Name + "=>" + valuetype.Name + "}";
            }
        }

        #endregion
    }

    public class SeveralArgumentType : IArgumentType
    {
        protected IArgumentType[] types;

        public SeveralArgumentType(IArgumentType[] types)
        {
            this.types = types;
        }

        #region IArgumentType Members

        public virtual object Example
        {
            get {
                object[] examples = new object[types.Length];
                for (int ii = 0; ii < types.Length; ii++)
                    examples[ii] = types[ii].Example;

                return examples;
            }
        }

        public virtual ArgumentTree IsValid(ArgumentTree value)
        {
            if (!(value.Value is object[]))
                return new ArgumentTree("not object array");

            object[] values = (object[])value.Value;
            if (values.Length != types.Length)
                return new ArgumentTree("incorrect length");

            for (int ii = 0; ii < types.Length; ii++)
            {
                ArgumentTree invalidity = types[ii].IsValid(new ArgumentTree(values[ii]));
                if (invalidity != null)
                    return invalidity;
            }

            return null;
        }

        public string Name
        {
            get {
                string[] names = new string[types.Length];
                for (int ii = 0; ii < types.Length; ii++)
                    names[ii] = types[ii].Name;

                return string.Join(",", names);
            }
        }

        #endregion
    }

    // Replaces the Name of an argument type with a given string
    public class NamedArgumentType : IArgumentType, IHtmlOutputable
    {
        protected string name;
        protected IArgumentType inner;

        public NamedArgumentType(string name, IArgumentType inner)
        {
            this.name = name;
            this.inner = inner;
        }

        #region IArgumentType Members

        public string Name
        {
            get {
                return name;
            }
        }

        public object Example
        {
            get {
                return inner.Example;
            }
        }

        public ArgumentTree IsValid(ArgumentTree value)
        {
            return inner.IsValid(value);
        }

        #endregion

        #region IHtmlOutputable Members

        public ArgumentTree GetHtml(object value)
        {
            if (inner is IHtmlOutputable)
                return ((IHtmlOutputable)inner).GetHtml(value);

            return null; // hehe, just kidding
        }

        #endregion
    }

    public class SetArgumentTreeArgumentType : IArgumentType
    {
        protected Random randgen;
        protected string name;
        protected ArgumentTree template;

        public SetArgumentTreeArgumentType(string name, ArgumentTree template)
        {
            this.randgen = new Random();
            this.name = name;
            this.template = template;
        }

        public SetArgumentTreeArgumentType(string name, IArgumentType valuetype)
        {
            this.randgen = new Random();
            this.name = name;
            this.template = new ArgumentTree(valuetype);
        }

        public SetArgumentTreeArgumentType(string name, IArgumentType valuetype, string[] names, IArgumentType[] types)
        {
            this.randgen = new Random();
            this.name = name;

            Dictionary<string, object> children = new Dictionary<string,object>();
            for (int ii = 0; ii < names.Length; ii++)
                children[names[ii]] = types[ii];

            this.template = new ArgumentTree(valuetype, children);
        }

        #region IArgumentType Members

        public string Name
        {
            get {
                if (name != null)
                    return name;

                StringBuilder output = new StringBuilder();
                
                if (template.Value != null)
                    output.Append(((IArgumentType)template.Value).Name);

                if (template.Children.Count > 0)
                {
                    output.Append("<");
                    foreach (KeyValuePair<string, ArgumentTree> child in template.Children)
                    {
                        output.Append(child.Key + ":");
                        output.Append((new SetArgumentTreeArgumentType(null, child.Value)).Name);
                    }
                    output.Append(">");
                }

                return output.ToString();
            }
        }

        public object Example
        {
            get {
                ArgumentTree output = new ArgumentTree();

                if (template.Value != null)
                    output.Value = ((IArgumentType)template.Value).Example;

                if (template.Children.Count > 0)
                {
                    foreach (KeyValuePair<string, ArgumentTree> child in template.Children)
                    {
                        if (randgen.Next(2) == 1)
                            continue;
                        output.Children[child.Key] = (ArgumentTree) (new SetArgumentTreeArgumentType(null, child.Value)).Example;
                    }
                }

                return output;
            }
        }

        public ArgumentTree IsValid(ArgumentTree value)
        {
            ArgumentTree output = null;
            if (template.Value != null)
                output = ((IArgumentType)template.Value).IsValid(value);
            if (output == null)
                output = new ArgumentTree();

            if (template.Children.Count > 0)
            {
                foreach (KeyValuePair<string, ArgumentTree> child in template.Children) {
                    if (value.ContainsKey(child.Key)) {
                        ArgumentTree invalidity = (new SetArgumentTreeArgumentType(null, child.Value)).IsValid(value.Children[child.Key]);
                        if (invalidity != null)
                            output.Children[child.Key] = invalidity;
                    }
                }
            }

            if (output.Children.Count > 0 || output.Value != null)
                return output;
            else
                return null;
        }

        #endregion
    }
}
