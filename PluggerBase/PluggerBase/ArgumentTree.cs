/******************************************************************\
 *      Class Name:     ArgumentTree
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
 * An ArgumentTree is a hierarchy of named data; it lends itself
 *   naturally to XML/HTML configuration files and hierarchical HTTP
 *   Post data.
 * In the Plugger Base, it is used to store a live configuration,
 *   and for named arguments and results from interfaces
 * 
 * IClonable maintains all of the values, but produces a new hierarchy
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Collections.Specialized;
using System.Collections;

namespace PluggerBase
{
    // A Argument node somewhat conforms to the data that can be in an xml document except:
    // each tag and attribute name is allowed only once, so everything is accessible in dictionaries
    public class ArgumentTree : ICloneable, IDictionary<string, object>
    {
        protected object value;
        protected Dictionary<string, ArgumentTree> children;

        public ArgumentTree()
        {
            value = null;
            children = new Dictionary<string, ArgumentTree>();
        }

        public ArgumentTree(object value)
        {
            if (value is ArgumentTree)
            {
                ArgumentTree clone = (ArgumentTree) ((ArgumentTree)value).Clone();
                value = clone.value;
                children = clone.children;
            }
            else
            {
                this.value = value;
                children = new Dictionary<string, ArgumentTree>();
            }
        }

        public ArgumentTree(ArgumentTree copy)
        {
            if (copy == null)
            {
                // treat as ArgumentTree(object)
                this.value = null;
                children = new Dictionary<string, ArgumentTree>();
            }
            else
            {
                ArgumentTree clone = (ArgumentTree)copy.Clone();
                value = clone.value;
                children = clone.Children;
            }
        }

        public ArgumentTree(object value, Dictionary<string, ArgumentTree> children)
        {
            this.value = value;
            this.children = children;
        }

        public ArgumentTree(object value, params object[] children)
        {
            this.value = value;
            this.children = new Dictionary<string, ArgumentTree>();

            for (int ii = 0; ii < children.Length; ii += 2)
                this.children[children[ii].ToString()] = new ArgumentTree(children[ii + 1]);
        }

        public object Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
            }
        }

        public Dictionary<string, ArgumentTree> Children
        {
            get
            {
                return children;
            }
        }

        public static ArgumentTree LoadFromXml(XmlNode root)
        {
            // Find the value of this node
            object value = GetAttribute(root, "value");
            if (value == null)
            {
                // Use the inner text (that isn't included in other nodes) as the value
                StringBuilder text = new StringBuilder();
                foreach (XmlNode child in root.ChildNodes)
                {
                    if (child is XmlText)
                        text.Append(((XmlText)child).Data);
                    else if (child is XmlWhitespace)
                        text.Append(" ");
                }

                string strtext = text.ToString().Trim();
                if (strtext.Length > 0)
                    value = strtext;
            }

            ArgumentTree node = new ArgumentTree(value);

            foreach (XmlAttribute attr in root.Attributes)
                if (attr.Name.ToLower() != "value")
                    node.children[attr.Name] = new ArgumentTree(attr.Value);

            foreach (XmlNode child in root.ChildNodes)
                if (child is XmlElement)
                    node.children[child.Name] = LoadFromXml(child);

            return node;
        }

        protected static string GetAttribute(XmlNode node, string name)
        {
            foreach (XmlAttribute attr in node.Attributes)
                if (attr.Name.ToLower() == name)
                    return attr.Value;

            return null;
        }

        #region ICloneable Members

        public object Clone()
        {
            ArgumentTree copy = (ArgumentTree) MemberwiseClone();
            copy.children = new Dictionary<string, ArgumentTree>();

            foreach (KeyValuePair<string, ArgumentTree> child in children)
                copy.children[child.Key] = (ArgumentTree) child.Value.Clone();

            return copy;
        }

        #endregion

        #region IDictionary<string,object> Members

        public void Add(string key, object value)
        {
            children.Add(key, new ArgumentTree(value));
        }

        public bool ContainsKey(string key)
        {
            return children.ContainsKey(key);
        }

        public ICollection<string> Keys
        {
            get {
                return children.Keys;
            }
        }

        public bool Remove(string key)
        {
            return children.Remove(key);
        }

        public bool TryGetValue(string key, out object value)
        {
            ArgumentTree child = null;
            if (children.TryGetValue(key, out child))
            {
                value = child.value;
                return true;
            }

            value = null;
            return false;
        }

        public ICollection<object> Values
        {
            get {
                List<object> values = new List<object>();
                foreach (KeyValuePair<string, ArgumentTree> child in children)
                    values.Add(child.Value.value);

                return values;
            }
        }

        public object this[string key]
        {
            get
            {
                ArgumentTree child = null;
                if (children.TryGetValue(key, out child))
                    return child.value;

                return null;
            }
            set
            {
                ArgumentTree child = null;
                if (children.TryGetValue(key, out child))
                    child.value = value;
                else
                    children[key] = new ArgumentTree(value);
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,object>> Members

        public void Add(KeyValuePair<string, object> item)
        {
            children.Add(item.Key, new ArgumentTree(item.Value));
        }

        public void Clear()
        {
            children.Clear();
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            ArgumentTree child = null;
            if (children.TryGetValue(item.Key, out child))
                return child.value == item.Value;

            return false;
        }

        public void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            foreach (KeyValuePair<string, ArgumentTree> child in children)
                array[arrayIndex++] = new KeyValuePair<string,object>(child.Key, child.Value.value);
        }

        public int Count
        {
            get {
                return children.Count;
            }
        }

        public bool IsReadOnly
        {
            get {
                return false;
            }
        }

        public bool Remove(KeyValuePair<string, object> item)
        {
            if (Contains(item))
                return Remove(item.Key);

            return false;
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,object>> Members

        public IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return new ArgumentTreeEnumerator(this);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ArgumentTreeEnumerator(this);
        }

        #endregion

        public class ArgumentTreeEnumerator : IEnumerator<KeyValuePair<string, object>>
        {
            protected IEnumerator<KeyValuePair<string, ArgumentTree>> elements;

            public ArgumentTreeEnumerator(ArgumentTree tree)
            {
                elements = tree.children.GetEnumerator();
            }

            #region IEnumerator<KeyValuePair<string,object>> Members

            public KeyValuePair<string, object> Current
            {
                get {
                    return new KeyValuePair<string, object>(elements.Current.Key, elements.Current.Value.value);
                }
            }

            #endregion

            #region IDisposable Members

            public void Dispose()
            {
                elements.Dispose();
            }

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current
            {
                get {
                    return elements.Current.Value.value;
                }
            }

            public bool MoveNext()
            {
                return elements.MoveNext();
            }

            public void Reset()
            {
                elements.Reset();
            }

            #endregion
        }
    }
}
