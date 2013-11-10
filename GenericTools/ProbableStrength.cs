/******************************************************************\
 *      Class Name:     ProbableStrength
 *      Written By:     James.R
 *      Copyright:      Virsona Inc
 *      -----------------------------------------------------------
 * A value with a confidence
\******************************************************************/
using System;
using PluggerBase.FastSerializer;

namespace GenericTools
{
    // Probable strengths are like two probabilities on top of eachother
    // They can be used for progressively calculating a value (including another probability)
    public class ProbableStrength : IFastSerializable
    {
        public double strength; // may be any value-- only used weighted-additively
        public double weight;   // must be 0 to 1

        public readonly static ProbableStrength None = new ProbableStrength(0.0, 0.0);
        public readonly static ProbableStrength Zero = new ProbableStrength(0.0, 1.0);
        public readonly static ProbableStrength Full = new ProbableStrength(1.0, 1.0);
        public readonly static ProbableStrength Half = new ProbableStrength(0.5, 0.5);

        public ProbableStrength(double strength, double weight)
        {
            this.strength = strength;
            this.weight = weight;
        }

        // deserialization constructor
        public ProbableStrength() { }

        // weightpower = 0 for only strength, > 0 to press result toward 0 for low weight
        public double ToDouble(double weightpower)
        {
            if (weightpower == 0)
                return strength;
            if (weightpower == 1)
                return strength * weight;

            return strength * Math.Pow(weight, weightpower);
        }

        public ProbableStrength Combine(ProbableStrength other)
        {
            double newstrength = (strength * weight + other.strength * other.weight) / (weight + other.weight);
            double newweight = weight + other.weight - weight * other.weight;
            return new ProbableStrength(newstrength, newweight);
        }

        public ProbableStrength Improve(ProbableStrength other)
        {
            if (other.strength > strength) {
                double newstrength = strength + (other.strength - strength) * other.weight / (weight + other.weight);
                double newweight = (weight + other.weight) / 2;
                return new ProbableStrength(newstrength, newweight);
            } else
                return this;
        }

        public ProbableStrength Relative(ProbableStrength other)
        {
            double newstrength = other.strength * strength; // <- use instead of RelativeStrength(other.strength, strength), because only used for strength [0, 1]
            double newweight = RelativeWeight(other.weight, weight);
            return new ProbableStrength(newstrength, newweight);
        }

        public ProbableStrength Better(ProbableStrength other)
        {
            if (strength * weight < other.strength * other.weight)
                return other;

            return this;
        }

        public ProbableStrength DownWeight(double factor)
        {
            return new ProbableStrength(strength, weight * factor);
        }

        // this is only if strength is a probability
        public ProbableStrength InverseProbability()
        {
            return new ProbableStrength(1.0 - strength, weight);
        }

        // this is only when strength is a probability
        public bool IsLikely(double min)
        {
            return (strength > min && weight > min);
        }

        // this is only when strength is a probability
        public bool IsPlausible(double min)
        {
            return (strength * weight > min);
        }

        // Progressively calculate strength and weight, with several factors (added in any order)

        public double ImproveStrengthStart()
        {
            strength *= weight;
            return weight;
        }

        public void ImproveStrength(ProbableStrength addStrength, ref double strengthFactor)
        {
            ImproveStrength(addStrength.strength, addStrength.weight, ref strengthFactor);
        }

        public void ImproveStrength(double addStrength, double addWeight, ref double strengthFactor)
        {
            ImproveStrength(addStrength, addWeight, addWeight, ref strengthFactor);
        }

        public void ImproveStrength(double addStrength, double strWeight, double addWeight, ref double strengthFactor)
        {
            //if (addWeight > 1.0 || addWeight < 0.0)
            //    throw new ArgumentOutOfRangeException("Weight must be 0 - 1.");
            if (addWeight > 1.0)
                addWeight = 1.0;
            if (addWeight < 0.0)
                addWeight = 0.0;

            // weighted average
            strength += addStrength * strWeight;
            strengthFactor += strWeight;
            // probabilistic or
            weight = weight + addWeight - weight * addWeight;
        }

        public void ImproveStrengthFinish(double strengthFactor)
        {
            if (strengthFactor == 0)
            {
                strength = 0;
                weight = 0;
            } else
                strength /= strengthFactor;
        }
        
        // Relative ProbableStrengths

        public static double RelativeStrength(double relStrength, double baseStrength)
        {
            double result = relStrength * baseStrength / (relStrength + baseStrength);
            if (double.IsNaN(result))
                return 0;
            else
                return result;
        }

        public static double RelativeWeight(double relWeight, double baseWeight)
        {
            return relWeight * baseWeight;
        }

        public static ProbableStrength Max(ProbableStrength one, ProbableStrength two)
        {
            if (one.strength * one.weight > two.strength * two.weight)
                return one;
            else
                return two;
        }

        #region IFastSerializable Members

        public void Deserialize(SerializationReader reader)
        {
            strength = reader.ReadDouble();
            weight = reader.ReadDouble();
        }

        public void Serialize(SerializationWriter writer)
        {
            writer.Write(strength);
            writer.Write(weight);
        }

        #endregion
    }
}
