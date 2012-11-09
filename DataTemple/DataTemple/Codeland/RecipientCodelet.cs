/******************************************************************\
 *      Class Name:     RecipientCodelet
 *      Written By:     James.R
 *      Copyright:      Virsona Inc
 *      -----------------------------------------------------------
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.ActionReaction.Evaluations;
using PluggerBase.FastSerializer;

namespace DataTemple.Codeland
{
    /// <summary>
    /// Has a "SetResult(T, double) function"
    ///  By default, will add self to coderack when called with SetResult() of a non-zero value
    /// </summary>
    public class RecipientCodelet<T> : Codelet, IResultReceiver<T>
    {
        protected T result;
        protected double weight;

        public RecipientCodelet(Coderack coderack, double sl, int sp, int tm)
            : base(coderack, sl, sp + 8, tm + 1) {
        }

        /// Deserialization constructor
        public RecipientCodelet() {
        }

        public double Weight
        {
            get
            {
                return weight;
            }
        }

        public virtual void SetResult(T result, double weight, string location) {
            this.result = result;
            this.weight = weight;
            if (weight != 0)
                coderack.AddCodelet(this, "SetResult: " + location);
        }

#region IFastSerializable Members

        public override void Deserialize(SerializationReader reader) {
            base.Deserialize(reader);
        }

        public override void Serialize(SerializationWriter writer) {
            base.Serialize(writer);
        }

#endregion

    }
}
