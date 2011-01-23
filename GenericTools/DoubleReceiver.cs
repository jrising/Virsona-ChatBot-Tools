/******************************************************************\
 *      Class Name:     DoubleReceiver
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * IMessageReceiver that contains two other recievers
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase;
using PluggerBase.FastSerializer;

namespace GenericTools
{
    /*
     * A higher-order receiver, containing two other receivers
     * It passes all messages to both, and allows internal receivers (in
     * the possible tree of DoubleReceivers below) to be added and removed
     */
    public class DoubleReceiver : IMessageReceiver, IFastSerializable
    {
        protected IMessageReceiver one;
        protected IMessageReceiver two;

        public DoubleReceiver(IMessageReceiver one, IMessageReceiver two)
        {
            this.one = one;
            this.two = two;
        }

        // Deserialization constructor
        public DoubleReceiver() { }

        public bool Receive(string message, object reference)
        {
            if (message == "remove")
            {
                if (reference == one)
                    one = null;
                if (reference == two)
                    two = null;
            }

            bool res1 = false, res2 = false;
            if (one != null)
                res1 = one.Receive(message, reference);
            if (two != null)
                res2 = two.Receive(message, reference);

            return res1 || res2;
        }

        public static IMessageReceiver addReceiver(IMessageReceiver group, IMessageReceiver add) {
            if (group == null)
                return add;
            
            return new DoubleReceiver(add, group);
        }

        public static IMessageReceiver removeReciever(IMessageReceiver group, IMessageReceiver remove)
        {
            if (group == null)
                throw new Exception("Receiver not found!");

            if (remove == group)
                return null;
            else if (group is DoubleReceiver)
            {
                DoubleReceiver dual = (DoubleReceiver)group;
                IMessageReceiver newone = removeReciever(dual.one, remove);
                IMessageReceiver newtwo = removeReciever(dual.two, remove);
                if (newone != null && newtwo != null)
                {
                    dual.one = newone;
                    dual.two = newtwo;
                    return dual;
                }
                else if (newone != null)
                    return newone;
                else if (newtwo != null)
                    return newtwo;
                else
                    return null;
            }
            else
                return group;
        }

        #region IFastSerializable Members

        public void Deserialize(SerializationReader reader)
        {
            one = (IMessageReceiver) reader.ReadPointer();
            two = (IMessageReceiver) reader.ReadPointer();
        }

        public void Serialize(SerializationWriter writer)
        {
            writer.WritePointer(one);
            writer.WritePointer(two);
        }

        #endregion
    }
}
