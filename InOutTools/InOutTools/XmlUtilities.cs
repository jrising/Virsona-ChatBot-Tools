using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace InOutTools
{
    public class XmlUtilities
    {
        public static string GetAttribute(XmlNode node, string name)
        {
            foreach (XmlAttribute attr in node.Attributes)
                if (attr.Name.ToLower() == name)
                    return attr.Value;

            return null;
        }

        /// <summary> 
        /// Given a name will try to find a node named "name" in the childnodes or return null 
        /// </summary> 
        /// <param name="name">The name of the node</param> 
        /// <param name="node">The node whose children need searching</param> 
        /// <returns>The node (or null)</returns> 
        public static XmlNode GetNode(string name, XmlNode parent)
        {
            foreach (XmlNode child in parent.ChildNodes)
                if (child.Name == name)
                    return child;

            return null;
        }

    }
}
