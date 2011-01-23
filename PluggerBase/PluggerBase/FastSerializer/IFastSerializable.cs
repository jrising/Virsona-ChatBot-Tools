/******************************************************************\
 *      Class Name:     IFastSerializable
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Interface for things that know how to FastSerialize
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.IO;

namespace PluggerBase.FastSerializer
{
    /*
     * This interface allows FastSerialization to recurse throughout classes
     */
    public interface IFastSerializable
    {
        void Deserialize(SerializationReader reader);
        void Serialize(SerializationWriter writer);
    }

    // Connector between ISerializable and IFastSerializable
    public class SerialToFast
    {
        public static void ToObjectData(IFastSerializable obj, SerializationInfo info)
        {
            SerializationWriter writer = new SerializationWriter();

            obj.Serialize(writer);

            byte[] result = writer.ToArray();
            info.AddValue("fast$", Convert.ToBase64String(result));
        }

        public static void FromObjectData(IFastSerializable obj, SerializationInfo info)
        {
            string serial = info.GetString("fast$");

            byte[] binary = Convert.FromBase64String(serial);
            MemoryStream stream = new MemoryStream(binary);
            SerializationReader reader = new SerializationReader(stream);

            obj.Deserialize(reader);
        }
    }

    public class FastSerializableString : IFastSerializable, IDisposable
    {
        protected string s;

        public FastSerializableString(string s)
        {
            this.s = s;
        }

        public FastSerializableString() { }

        public override string ToString()
        {
            return s;
        }

        #region IFastSerializable Members

        public virtual void Deserialize(SerializationReader reader)
        {
            s = reader.ReadString();
        }

        public virtual void Serialize(SerializationWriter writer)
        {
            writer.Write(s);
        }

        #endregion

        #region IDisposable Members

        public virtual void Dispose()
        {
            s = String.Empty;
        }

        #endregion
    }

}
