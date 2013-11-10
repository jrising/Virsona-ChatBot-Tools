/******************************************************************\
 *      Class Name:     NumberAttribute
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
    public class NumberAttribute : PhraseAttribute, IFastSerializable
    {
        public enum NumberOptions {
            Zero,
            One,
            Many
        }

        protected NumberOptions number;

        public NumberAttribute(string number)
            : base(1, 1)
        {
            if (number == "0")
                this.number = NumberOptions.Zero;
            else if (number == "1")
                this.number = NumberOptions.One;
            else if (number == "2")
                this.number = NumberOptions.Many;
            else
                throw new ArgumentException("No known number " + number);
        }

        public NumberAttribute(NumberOptions number) 
            : base(1, 1)
        {
            this.number = number;
        }

        // deserialization constructor
        public NumberAttribute() { }

        public readonly static NumberAttribute IsZero = new NumberAttribute(NumberOptions.Zero);
        public readonly static NumberAttribute IsOne = new NumberAttribute(NumberOptions.One);
        public readonly static NumberAttribute IsMany = new NumberAttribute(NumberOptions.Many);

        // Guess whether or not a phrase is plural
        public static ProbableStrength SeemsPlural(InformedPhrase phrase)
        {
            ProbableStrength result = new ProbableStrength(0, 0);
            double strengthFactor = result.ImproveStrengthStart();

            // Noun count is well determined by ending in s
            ProbableStrength nounness = PartOSAttribute.SeemsA(phrase, SpeechPart.Noun);
            ProbableStrength nisplural;
            if (phrase.Name.ToLower().EndsWith("s"))
                nisplural = new ProbableStrength(1.0, 0.8);
            else
                nisplural = new ProbableStrength(0.0, 0.5);

            result.ImproveStrength(nounness.Relative(nisplural), ref strengthFactor);

            // Verbs that end in s are probably not plural
            ProbableStrength verbness = PartOSAttribute.SeemsA(phrase, SpeechPart.Verb);
            if (phrase.Name.ToLower().EndsWith("s"))
            {
                ProbableStrength visplural = new ProbableStrength(0.0, 0.8);
                result.ImproveStrength(verbness.Relative(visplural), ref strengthFactor);
            }

            result.ImproveStrengthFinish(strengthFactor);

            return result;
        }

        public override ProbableStrength IsMatch(PhraseAttribute attribute)
        {
            if (attribute is NumberAttribute)
            {
                NumberAttribute numberAttribute = (NumberAttribute)attribute;
                if (numberAttribute.number == number)
                    return ProbableStrength.Full;
                else if (numberAttribute.number == NumberOptions.Zero || number == NumberOptions.Zero)
                    return ProbableStrength.Half;
                else
                    return ProbableStrength.Zero;
            }

            return ProbableStrength.None;
        }

        public override PhraseAttribute Guess(InformedPhrase word)
        {
            ProbableStrength plural = SeemsPlural(word);
            if (plural.strength > .5)
            {
                NumberAttribute result = new NumberAttribute(NumberOptions.Many);
                result.strength.strength = 2.0 * (plural.strength - 0.5);
                result.strength.weight = plural.weight;
                return result;
            }
            else
            {
                NumberAttribute result = new NumberAttribute(NumberOptions.One);
                result.strength.strength = 2.0 * (.5 - plural.strength);
                result.strength.weight = plural.weight;
                return result;
            }
        }

        public override string ToString()
        {
            if (number == NumberOptions.Zero)
                return "0" + base.ToString();
            else if (number == NumberOptions.One)
                return "1" + base.ToString();
            else if (number == NumberOptions.Many)
                return "2" + base.ToString();
            else
                return base.ToString();
        }

        #region IFastSerializable Members

        void IFastSerializable.Deserialize(SerializationReader reader)
        {
            base.Deserialize(reader);
            number = (NumberOptions) reader.ReadObject();
        }

        void IFastSerializable.Serialize(SerializationWriter writer)
        {
            base.Serialize(writer);
            writer.WriteObject(number);
        }

        #endregion
    }
}
