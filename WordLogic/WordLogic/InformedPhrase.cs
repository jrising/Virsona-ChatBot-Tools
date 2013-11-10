/******************************************************************\
 *      Class Name:     InformedPhrase
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
using System.Runtime.Serialization;
using PluggerBase.FastSerializer;
using InOutTools;
using LanguageNet.Grammarian;
using GenericTools;

namespace LanguageNet.WordLogic
{
    // An informed phrase has a collection of possible senses
    [Serializable]
    public class InformedPhrase : IFastSerializable, ISerializable
    {
        protected string name;              // (potentially composite) text representation
        protected List<KeyValuePair<PhraseSense, double>> senses; // list of possible senses and weights

        // Single-word phrase, with a set of parts of speech
        public InformedPhrase(string name, List<KeyValuePair<SpeechPart, double>> kvparts)
        {
            this.name = name;
            senses = new List<KeyValuePair<PhraseSense, double>>();
            foreach (KeyValuePair<SpeechPart, double> kvp in kvparts)
                senses.Add(new KeyValuePair<PhraseSense, double>(new PhraseSense(name, kvp.Key), kvp.Value));
        }

        // Single-word phrase, with a set of senses
        public InformedPhrase(string name, List<KeyValuePair<PhraseSense, double>> senses)
        {
            this.name = name;
            this.senses = senses;
        }

        // Single word phrase, with a single part of speech and definition
        public InformedPhrase(string name, string definition, SpeechPart part)
        {
            this.name = name;
            this.senses = new List<KeyValuePair<PhraseSense, double>>();
            KeyValuePair<PhraseSense, double> sense = new KeyValuePair<PhraseSense, double>(new PhraseSense(definition, part), 1.0);
            senses.Add(sense);
        }

        // Non-terminal phrase, with a single sense
        public InformedPhrase(string definition, SpeechPart part, List<InformedPhrase> phrases)
        {
            senses = new List<KeyValuePair<PhraseSense, double>>();
            KeyValuePair<PhraseSense, double> sense = new KeyValuePair<PhraseSense, double>(new PhraseSense(definition, part, phrases), 1.0);
            this.name = sense.Key.Name();
            senses.Add(sense);
        }

        // deserialization constructors
        public InformedPhrase()
        {
        }

        protected InformedPhrase(SerializationInfo info, StreamingContext context)
        {
            SerialToFast.FromObjectData(this, info);
        }

        // Properties
        
        public string Name
        {
            get
            {
                return name;
            }
        }

        public List<KeyValuePair<PhraseSense, double>> Senses
        {
            get
            {
                return senses;
            }
        }

        public bool IsTerminal
        {
            get
            {
                return senses[0].Key.Phrases.Count == 0;
            }
        }

        // Is this phrase identical to ours?
        public bool IsIdentical(InformedPhrase other)
        {
            if (name != other.name || senses.Count != other.senses.Count)
                return false;

            // Each sense must match one in the other
            foreach (KeyValuePair<PhraseSense, double> sense in senses)
            {
                bool found = false;
                foreach (KeyValuePair<PhraseSense, double> othersense in other.senses)
                    if (sense.Key.IsIdentical(othersense.Key))
                    {
                        found = true;
                        break;
                    }

                if (!found)
                    return false;
            }

            return true;
        }

        // Is this phrase a subphrase of us?
        public bool IsContained(InformedPhrase phrase)
        {
            if (IsIdentical(phrase))
                return true;    // a kind of contains

            // Look through each phrase
            foreach (KeyValuePair<PhraseSense, double> sense in senses)
                if (sense.Key.IsContained(phrase))
                    return true;

            return false;
        }

        public bool IsContained(PhraseSense sense)
        {
            // Look through each phrase
            foreach (KeyValuePair<PhraseSense, double> mysense in senses)
                if (mysense.Key.IsContained(sense))
                    return true;

            return false;
        }            

        // Is this actual object in our structure?
        public bool IsObjectContained(InformedPhrase phrase)
        {
            if (Equals(phrase))
                return true;    // a kind of contains

            // Look through each phrase
            foreach (KeyValuePair<PhraseSense, double> sense in senses)
                foreach (InformedPhrase subphrase in sense.Key.Phrases)
                    if (subphrase.IsObjectContained(phrase))
                        return true;

            return false;
        }
        
        // Look through phrases, finding all speech parts
        public List<KeyValuePair<SpeechPart, double>> SpeechParts()
        {
            List<KeyValuePair<SpeechPart, double>> kvparts = new List<KeyValuePair<SpeechPart, double>>();

            foreach (KeyValuePair<PhraseSense, double> sense in senses)
            {
                PartOSAttribute partAttribute = (PartOSAttribute)sense.Key.FindAttribute(typeof(PartOSAttribute));
                kvparts.Add(new KeyValuePair<SpeechPart, double>(partAttribute.Part, sense.Value));
            }

            return kvparts;
        }

        // Find a sense of the given speech part
        public PhraseSense FindSense(SpeechPart part, bool allSubs)
        {
            foreach (KeyValuePair<PhraseSense, double> sense in senses)
            {
                PartOSAttribute partAttribute = (PartOSAttribute)sense.Key.FindAttribute(typeof(PartOSAttribute));
                if (partAttribute.Part.SeemsA(part, allSubs).IsLikely(.5))
                    return sense.Key;
            }

            return null;    // not found
        }

        // Find all phrases with the given speech part
        // Phrases returned may overlap lots
        public List<InformedPhrase> FindPhrases(SpeechPart part, bool allSubs)
        {
            List<InformedPhrase> phrases = new List<InformedPhrase>();

            // Look through each sense
            foreach (KeyValuePair<PhraseSense, double> sense in senses)
            {
                PartOSAttribute partAttribute = (PartOSAttribute)sense.Key.FindAttribute(typeof(PartOSAttribute));
                if (partAttribute.Part.SeemsA(part, allSubs).IsLikely(.5))
                {
                    if (sense.Key.Phrases.Count == 1)
                    {
                        // just add the sub piece, if it matches too
                        List<InformedPhrase> submatches = sense.Key.Phrases[0].FindPhrases(part, allSubs);
                        if (submatches.Count > 0)
                            phrases.AddRange(submatches);
                        else if (!phrases.Contains(this))
                            phrases.Add(this);
                    }
                    else if (!phrases.Contains(this))
                        phrases.Add(this);
                }

                if (sense.Key.Phrases.Count > 1)
                {
                    // Recurse on each phrase
                    foreach (InformedPhrase subphrase in sense.Key.Phrases)
                        phrases.AddRange(subphrase.FindPhrases(part, allSubs));
                }
            }

            return phrases;
        }

        // Merge new sense structures into existing ones
        public InformedPhrase Merge(InformedPhrase other)
        {
            if (this == other || other == null)
                return this;

            InformedPhrase merged = new InformedPhrase(name, senses);

            // Add each sense
            foreach (KeyValuePair<PhraseSense, double> othersense in other.senses)
            {
                // Does this sense already exist in our list?
                bool found = false;
                foreach (KeyValuePair<PhraseSense, double> sense in senses)
                    if (sense.Key.IsIdentical(othersense.Key))
                        if (sense.Value == othersense.Value)
                        {
                            found = true;
                            break;
                        }

                if (!found)
                    merged.senses.Add(othersense);
            }

            return merged;
        }

        public InformedPhrase ReplaceByName(string before, InformedPhrase after)
        {
            if (name == before)
                return after;

            List<KeyValuePair<PhraseSense, double>> aftersenses = new List<KeyValuePair<PhraseSense, double>>();
            foreach (KeyValuePair<PhraseSense, double> sense in senses)
            {
                // Recurse on each subphrase
                List<InformedPhrase> afterphrases = new List<InformedPhrase>();
                foreach (InformedPhrase subphrase in sense.Key.Phrases)
                    afterphrases.Add(subphrase.ReplaceByName(before, after));

                PhraseSense aftersense = new PhraseSense(sense.Key.Definition, afterphrases, sense.Key.Attributes);
                aftersenses.Add(new KeyValuePair<PhraseSense, double>(aftersense, sense.Value));
            }

            string aftername = " " + name + " ";
            aftername = aftername.Replace(" " + before + " ", " " + after.name + " ").Trim();
            return new InformedPhrase(aftername, aftersenses);
        }

        // Generate a readable list from this phrase
        public List<string> Generate()
        {
            bool found = true;
            return GenerateWithReplacement(null, null, ref found);
        }

        // If the replacement is found in any of the senses, that's used
        public List<string> GenerateWithReplacement(InformedPhrase before, List<string> after, ref bool found)
        {
            // Replace!
            if (this == before)
                return after;

            // Just plug in the word
            if (IsTerminal)
            {
                List<string> single = new List<string>();
                single.Add(name);
                return single;
            }

            List<string> result = new List<string>();
            // Try this on each sense, looking for the replacement
            foreach (KeyValuePair<PhraseSense, double> sense in senses)
            {
                // Recurse on each subphrase
                result = new List<string>();
                foreach (InformedPhrase subphrase in sense.Key.Phrases)
                    result.AddRange(subphrase.GenerateWithReplacement(before, after, ref found));
                if (found || before == null)
                    return result;  // if the replacement was found, use this result
            }

            return result;
        }

        public string Normalized(StringUtilities.StringConverter wordconv)
        {
            if (IsTerminal)
                return wordconv(name);

            // choose max-probability sense
            BestScore<PhraseSense> strongest = new BestScore<PhraseSense>();
            foreach (KeyValuePair<PhraseSense, double> sense in senses)
                strongest.Improve(sense.Value, sense.Key);

            StringBuilder result = new StringBuilder();
            foreach (InformedPhrase subphr in strongest.Payload.Phrases)
            {
                string normal = subphr.Normalized(wordconv);
                if (normal == "")
                    continue;   // skip it
                if (result.Length > 0)
                    result.Append(" ");
                result.Append(normal);
            }

            return result.ToString();
        }

        public string Simple()
        {
            return StringUtilities.JoinWords(Generate());
        }

        public override string ToString()
        {
            StringBuilder sensestr = new StringBuilder();
            foreach (KeyValuePair<PhraseSense, double> sense in senses)
            {
                if (sensestr.Length > 0)
                    sensestr.Append("|");
                sensestr.Append(sense.Key.ToString());
                // add on a strength character
                if (sense.Value <= 0.5)
                    sensestr.Append("?");
            }
            return sensestr.ToString();
        }

        #region IFastSerializable Members

        public void Deserialize(SerializationReader reader)
        {
            name = reader.ReadString();
            int count = reader.ReadInt32();
            senses = new List<KeyValuePair<PhraseSense,double>>(count);
            for (int ii = 0; ii < count; ii++)
            {
                PhraseSense sense = (PhraseSense)reader.ReadPointer();
                double weight = reader.ReadDouble();
                senses.Add(new KeyValuePair<PhraseSense, double>(sense, weight));
            }
        }

        public void Serialize(SerializationWriter writer)
        {
            writer.Write(name);
            writer.Write(senses.Count);
            foreach (KeyValuePair<PhraseSense, double> sense in senses)
            {
                writer.WritePointer(sense.Key);
                writer.Write(sense.Value);
            }
        }

        #endregion

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            SerialToFast.ToObjectData(this, info);
        }

        #endregion
    }
}
