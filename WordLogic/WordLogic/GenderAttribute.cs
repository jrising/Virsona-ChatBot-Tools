/******************************************************************\
 *      Class Name:     GenderAttribute
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
    public class GenderAttribute : PhraseAttribute, IFastSerializable
    {
        public enum GenderOptions
        {
            Male,   // in English, for animals who identify as male
            Female, // in English, for animals who identify as female
            Human,  // in English, for humans
            OneOfUs, // Either the first or second person
            Neuter  // in English, for objects without consciousness
        }

        protected GenderOptions gender;

        // Populate structure from string
        public GenderAttribute(string gender)
            : base(1, 1)
        {
            if (gender == "m")
                this.gender = GenderOptions.Male;
            else if (gender == "f")
                this.gender = GenderOptions.Female;
            else if (gender == "h")
                this.gender = GenderOptions.Human;
            else if (gender == "u")
                this.gender = GenderOptions.OneOfUs;
            else if (gender == "n")
                this.gender = GenderOptions.Neuter;
            else
                throw new ArgumentException("Unknown gender: " + gender);
        }

        // Copy constructor
        public GenderAttribute(GenderOptions gender)
            : base(1, 1)
        {
            this.gender = gender;
        }

        // deserialization constructor
        public GenderAttribute() { }

        // Helper objects, for checking gender (note gender field is protected)
        public readonly static GenderAttribute IsMale = new GenderAttribute(GenderOptions.Male);
        public readonly static GenderAttribute IsFemale = new GenderAttribute(GenderOptions.Female);
        public readonly static GenderAttribute IsHuman = new GenderAttribute(GenderOptions.Human);
        public readonly static GenderAttribute IsOneOfUs = new GenderAttribute(GenderOptions.OneOfUs);
        public readonly static GenderAttribute IsNeuter = new GenderAttribute(GenderOptions.Neuter);

        public GenderOptions Gender {
            get
            {
                return gender;
            }
        }

        // Does this attribute conform to ours?
        public override ProbableStrength IsMatch(PhraseAttribute attribute)
        {
            if (attribute is GenderAttribute)
            {
                // It's a gender attribute-- it might!
                GenderAttribute genderAttribute = (GenderAttribute)attribute;
                if (genderAttribute.gender == gender)
                    return ProbableStrength.Full;   // perfect match
                else if (gender == GenderOptions.OneOfUs || genderAttribute.gender == GenderOptions.OneOfUs)
                    return ProbableStrength.Zero;   // nothing matches us! (for now)
                else if ((genderAttribute.gender == GenderOptions.Male && gender == GenderOptions.Female) ||
                         (genderAttribute.gender == GenderOptions.Female && gender == GenderOptions.Male))
                    return ProbableStrength.Zero;   // male != female
                else if ((genderAttribute.gender == GenderOptions.Human && (gender == GenderOptions.Male || gender == GenderOptions.Female)) ||
                         (gender == GenderOptions.Human && (genderAttribute.gender == GenderOptions.Male || genderAttribute.gender == GenderOptions.Female)))
                    return ProbableStrength.Full;   // human matches male or female
                else if ((genderAttribute.gender == GenderOptions.Human && gender == GenderOptions.Neuter) ||
                         (gender == GenderOptions.Human && genderAttribute.gender == GenderOptions.Neuter))
                    return ProbableStrength.Zero;   // human doesn't match neuter
                else
                    // one or the other is neuter
                    return ProbableStrength.Half;   // neuter attribute is applied only as a suggestion
            }

            return ProbableStrength.None;
        }

        // This is specific for when we know we want to recognize one of us
        public static ProbableStrength DescribesOneOfUs(PhraseSense sense)
        {
            PhraseAttribute attr = sense.FindAttribute(typeof(GenderAttribute));
            if (attr != null && ((GenderAttribute)attr).gender == GenderOptions.OneOfUs)
                return ProbableStrength.Full;
            return ProbableStrength.Zero;
        }
        
        public override PhraseAttribute Guess(InformedPhrase word)
        {
            List<KeyValuePair<SpeechPart, double>> kvparts = word.SpeechParts();
            if (kvparts.Count != 1)
            {
                GenderAttribute attr = new GenderAttribute(GenderOptions.Neuter);
                attr.strength = new ProbableStrength(0.0, 0.0); // we don't know anything
                return attr;
            }

            // If it's a proper noun, we guess it might be a human
            SpeechPart part = kvparts[0].Key;
            if (part == SpeechPart.ProperNoun)
            {
                GenderAttribute attr = new GenderAttribute(GenderOptions.Human);
                attr.strength = new ProbableStrength(1.0, 0.5);
                return attr;
            }
            else
            {
                GenderAttribute attr = new GenderAttribute(GenderOptions.Neuter);
                attr.strength = new ProbableStrength(1.0, 0.1);
                return attr;
            }
        }

        public override string ToString()
        {
            if (gender == GenderOptions.Male)
                return "m" + base.ToString();
            else if (gender == GenderOptions.Female)
                return "f" + base.ToString();
            else if (gender == GenderOptions.Human)
                return "h" + base.ToString();
            else if (gender == GenderOptions.OneOfUs)
                return "u" + base.ToString();
            else if (gender == GenderOptions.Neuter)
                return "n" + base.ToString();
            else
                return base.ToString();
        }

        #region IFastSerializable Members

        void IFastSerializable.Deserialize(SerializationReader reader)
        {
            base.Deserialize(reader);
            gender = (GenderOptions) reader.ReadObject();
        }

        void IFastSerializable.Serialize(SerializationWriter writer)
        {
            base.Serialize(writer);
            writer.WriteObject(gender);
        }

        #endregion
    }
}
