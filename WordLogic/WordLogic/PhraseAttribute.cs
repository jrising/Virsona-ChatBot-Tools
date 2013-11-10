/******************************************************************\
 *      Class Name:     PhraseAttribute
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
    // An attribute is something that we can use to predict something about a phrase
    public class PhraseAttribute : IFastSerializable
    {
        protected ProbableStrength strength;

        // Base constructor: just stores "how strong" the attribute is
        public PhraseAttribute(double strength, double weight)
        {
            this.strength = new ProbableStrength(strength, weight);
        }

        // deserialization constructor
        public PhraseAttribute() { }

        public ProbableStrength Strength
        {
            get
            {
                return strength;
            }
            set
            {
                strength = value;
            }
        }

        public virtual ProbableStrength Describes(PhraseSense sense)
        {
            ProbableStrength total = new ProbableStrength(0, 0);
            double strengthFactor = total.ImproveStrengthStart();

            foreach (PhraseAttribute attribute in sense.Attributes)
                total.ImproveStrength(Match(attribute), ref strengthFactor);

            if (strengthFactor == 0)
            {
                // do we match subphrases then?
                foreach (InformedPhrase subphrase in sense.Phrases)
                    total.ImproveStrength(Describes(subphrase).DownWeight(1.0 / sense.Phrases.Count), ref strengthFactor);
            }

            if (strengthFactor == 0)
            {
                // We never found an appropriate attribute-- so guess!
                List<KeyValuePair<PhraseSense, double>> senses = new List<KeyValuePair<PhraseSense, double>>();
                senses.Add(new KeyValuePair<PhraseSense,double>(sense, 1.0));
                InformedPhrase dummy = new InformedPhrase(sense.Name(), senses);
                ProbableStrength result = Match(Guess(dummy));
                total.ImproveStrength(result, ref strengthFactor);
            }

            total.ImproveStrengthFinish(strengthFactor);
            return total;
        }

        // Does this attribute describe the word?  Consider overriding this
        public ProbableStrength Describes(InformedPhrase word)
        {
            ProbableStrength total = new ProbableStrength(0, 0);
            double strengthFactor = total.ImproveStrengthStart();

            foreach (KeyValuePair<PhraseSense, double> sense in word.Senses)
                total.ImproveStrength(Describes(sense.Key).DownWeight(sense.Value), ref strengthFactor);

            if (strengthFactor == 0)
            {
                // We never found an appropriate attribute-- so guess!
                ProbableStrength result = Match(Guess(word));
                total.ImproveStrength(result, ref strengthFactor);
            }

            total.ImproveStrengthFinish(strengthFactor);
            return total;
        }

        // Wrapper on IsMatch to apply the weighting
        public ProbableStrength Match(PhraseAttribute attribute)
        {
            ProbableStrength match = IsMatch(attribute);
            return new ProbableStrength(match.strength, match.weight * (strength.weight + attribute.strength.weight) / 2.0);
        }

        // Does this attribute match the other?  Override this!
        public virtual ProbableStrength IsMatch(PhraseAttribute attribute)
        {
            return ProbableStrength.None;
        }

        // What does it look like the attribute for this phrase is?  Consider overriding this.
        public virtual PhraseAttribute Guess(InformedPhrase word)
        {
            return new PhraseAttribute(0, 0);
        }

        public override string ToString()
        {
            if (strength.strength <= 0.5 || strength.weight <= 0.5)
                return "?";
            return "";
        }

        #region IFastSerializable Members

        public void Deserialize(SerializationReader reader)
        {
            strength = (ProbableStrength) reader.ReadPointer();
        }

        public void Serialize(SerializationWriter writer)
        {
            writer.WritePointer(strength);
        }

        #endregion
    }
}
