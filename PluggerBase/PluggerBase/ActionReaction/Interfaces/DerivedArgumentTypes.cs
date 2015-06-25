/******************************************************************\
 *      Class Name:     DerivedArgumentTypes
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
 * A collection of classes of argument types built on other types
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace PluggerBase.ActionReaction.Interfaces
{
    public class SetStringDictionaryArgumentType : DictionaryArgumentType<string, object>
    {
        protected Random randgen;

        protected string[] options;
        protected IArgumentType[] values;

        protected Dictionary<string, int> argmap;

        public SetStringDictionaryArgumentType(string[] options, IArgumentType[] values) :
            base(new SelectableArgumentType(options), new AnyArgumentType(null))
        {
            this.options = options;
            this.values = values;

            argmap = new Dictionary<string, int>();
            for (int ii = 0; ii < options.Length; ii++)
                argmap[options[ii]] = ii;
            
            randgen = new Random();
        }

        public IArgumentType ArgumentType(string name) {
            return values[argmap[name]];
        }

        public override object Example
        {
            get {
                Dictionary<string, object> example = new Dictionary<string, object>();
                for (int ii = 0; ii < options.Length; ii++)
                    if (randgen.Next(2) == 0)
                        example[options[ii]] = values[ii].Example;

                return example;
            }
        }

        public override ArgumentTree IsValid(ArgumentTree value)
        {
            if (!(value.Value is IDictionary<string, object>))
                return new ArgumentTree("not argument dictionary");

            IDictionary<string, object> dict = (IDictionary<string, object>)value.Value;
            foreach (KeyValuePair<string, object> kvp in dict)
            {
                ArgumentTree invalidity = values[argmap[kvp.Key]].IsValid(new ArgumentTree(kvp.Value));
                if (invalidity != null)
                    return invalidity;
            }

            return null;
        }
    }
    
    public class AnyStringDictionaryArgumentType : DictionaryArgumentType<string, object>
    {
        public AnyStringDictionaryArgumentType(int maxkeylen, object valuexpl)
            : base(new StringArgumentType(maxkeylen, ".+", "value"), new AnyArgumentType(valuexpl))
        {
        }

        #region IArgumentType Members

        public override object Example
        {
            get {
                Dictionary<string, object> example = new Dictionary<string, object>();
                example[(string) keytype.Example] = valuetype.Example;

                return example;
            }
        }

        public override ArgumentTree IsValid(ArgumentTree value)
        {
            return (value.Value is IDictionary<string, object> ? null : new ArgumentTree("not argument dictionary"));
        }

        #endregion
    }

    public class LabelledArgumentType : IArgumentType, IHtmlFormable
    {
        protected string label;
        protected IArgumentType inner;

        public LabelledArgumentType(string label, IArgumentType inner)
        {
            this.label = label;
            this.inner = inner;
        }

        public IArgumentType Inner
        {
            get
            {
                return inner;
            }
        }

        #region IArgumentType Members

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

        public string Name
        {
            get
            {
                return "*" + inner.Name;
            }
        }

        #endregion

        #region IHtmlFormable Members

        public ArgumentTree GetHtmlForm(string name, ArgumentTree args, ArgumentTree invalidity)
        {
            string innerhtml = "";
            if (inner is IHtmlFormable)
                innerhtml = ((IHtmlFormable)inner).GetHtmlForm(name, args, invalidity).Value.ToString();

            return new ArgumentTree("<th>" + label + "</th><td>" + innerhtml + "</td>");
        }

        #endregion
    }

    public class KeyValueArgumentType<TKey, TValue> : SeveralArgumentType
    {
        public KeyValueArgumentType(IArgumentType keytype, IArgumentType valuetype)
            : base(new IArgumentType[] {keytype, valuetype})
        {
        }

        public override object Example
        {
            get
            {
                return new KeyValuePair<TKey, TValue>((TKey) types[0].Example, (TValue) types[1].Example);
            }
        }

        public override ArgumentTree IsValid(ArgumentTree value)
        {
            if (!(value.Value is KeyValuePair<TKey, TValue>))
                return new ArgumentTree("not key-value pair");

            return null;
        }
    }
}
