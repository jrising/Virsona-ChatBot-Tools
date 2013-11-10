/******************************************************************\
 *      Class Name:     PhraseNumber
 *      Written By:     James.R
 *      Copyright:      Virsona Inc
 *
 *      Modifications:
 *      -----------------------------------------------------------
 *      Date            Author          Modification
 *
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.FastSerializer;
using GenericTools;

namespace LanguageNet.WordLogic
{
    // A part of speech attribute-- encapsulates a SpeechPart object
    public class PartOSAttribute : PhraseAttribute, IFastSerializable
    {
        protected SpeechPart part;

        // Look up the speech part based on its string
        public PartOSAttribute(string name)
            : base(1, 1)
        {
            this.part = SpeechPart.GetPart(name);
        }

        // Encapsulate a given part
        public PartOSAttribute(SpeechPart part)
            : base(1, 1)
        {
            this.part = part;
        }

        // deserialization constructor
        public PartOSAttribute() { }

        // Properties

        public SpeechPart Part
        {
            get
            {
                return part;
            }
        }

        // Does this phrase match a given part of speech?
        public static ProbableStrength SeemsA(InformedPhrase phrase, SpeechPart part)
        {
            return (new PartOSAttribute(part)).Describes(phrase);
        }

        public override ProbableStrength IsMatch(PhraseAttribute attribute)
        {
            if (attribute is PartOSAttribute)
            {
                PartOSAttribute partosAttribute = (PartOSAttribute)attribute;
                return partosAttribute.part.SeemsA(part, false);
            }

            return ProbableStrength.None;
        }

        public override string ToString()
        {
            return part.Name + base.ToString();
        }

        #region IFastSerializable Members

        void IFastSerializable.Deserialize(SerializationReader reader)
        {
            base.Deserialize(reader);
            string name = reader.ReadString();
            part = SpeechPart.GetPart(name);
        }

        void IFastSerializable.Serialize(SerializationWriter writer)
        {
            base.Serialize(writer);
            writer.Write(part.Name);
        }

        #endregion
    }
}
