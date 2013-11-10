/******************************************************************\
 *      Class Name:     PhraseSense
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
    // A particular meaning/usage of a word
    public class PhraseSense : IFastSerializable
    {
        protected string definition;
        protected List<InformedPhrase> phrases;
        protected List<PhraseAttribute> attributes;

        // Ctor for a terminal node with set of attributes
        public PhraseSense(string definition, List<PhraseAttribute> attributes)
        {
            this.definition = definition;
            phrases = new List<InformedPhrase>();
            this.attributes = attributes;
        }

        // Ctor just based on a speech part
        public PhraseSense(string definition, SpeechPart part)
        {
            this.definition = definition;
            phrases = new List<InformedPhrase>();
            attributes = new List<PhraseAttribute>();
            attributes.Add(new PartOSAttribute(part));
        }

        // Ctor of a non-terminal node
        public PhraseSense(string definition, SpeechPart part, List<InformedPhrase> phrases)
        {
            this.definition = definition;
            this.phrases = phrases;
            attributes = new List<PhraseAttribute>();
            attributes.Add(new PartOSAttribute(part));
        }

        // Ctor of a non-terminal node
        public PhraseSense(string definition, List<InformedPhrase> phrases, List<PhraseAttribute> attributes)
        {
            this.definition = definition;
            this.phrases = phrases;
            this.attributes = attributes;
        }

        // deserialization constructor
        public PhraseSense() { }

        // Properties

        public string Definition
        {
            get
            {
                return definition;
            }
        }

        public List<InformedPhrase> Phrases
        {
            get
            {
                return phrases;
            }
        }

        public List<PhraseAttribute> Attributes
        {
            get
            {
                return attributes;
            }
        }

        // Construct a name by putting together subphrases
        public string Name()
        {
            if (phrases.Count == 0)
                return definition;

            StringBuilder builder = new StringBuilder();
            foreach (InformedPhrase phrase in phrases)
            {
                if (builder.Length > 0)
                    builder.Append(" ");
                builder.Append(phrase.Name);
            }
            return builder.ToString();
        }

        public int CountWords()
        {
            int total = 0;
            foreach (InformedPhrase phrase in phrases)
            {
                if (phrase.IsTerminal)
                    total++;
                else
                    total += phrase.Senses[0].Key.CountWords();
            }

            return total;
        }

        // Is this sense identical to us?
        public bool IsIdentical(PhraseSense other)
        {
            if (definition != other.definition ||
                attributes.Count != other.attributes.Count ||
                phrases.Count != other.phrases.Count)
                return false;

            // Check if the attributes all produce perfect matches
            // Only one needs to match for each
            foreach (PhraseAttribute attribute in attributes)
            {
                bool found = false;
                foreach (PhraseAttribute otherattribute in other.attributes)
                {
                    ProbableStrength match = attribute.Match(otherattribute);
                    if (match.strength == 1.0)
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    return false;
            }

            // Apply IsIdentical to each subphrase-- only one must match for each
            foreach (InformedPhrase phrase in phrases)
            {
                bool found = false;
                foreach (InformedPhrase otherphrase in other.phrases)
                    if (phrase.IsIdentical(otherphrase))
                    {
                        found = true;
                        break;
                    }

                if (!found)
                    return false;
            }

            return true;
        }

        public bool IsContained(PhraseSense sense)
        {
            if (IsIdentical(sense))
                return true;    // a kind of contains
            
            foreach (InformedPhrase subphrase in phrases)
                if (subphrase.IsContained(sense))
                    return true;

            return false;
        }

        public bool IsContained(InformedPhrase phrase)
        {
            foreach (InformedPhrase subphrase in phrases)
                if (subphrase.IsContained(phrase))
                    return true;

            return false;
        }

        // Find an attribute based on its class type
        public PhraseAttribute FindAttribute(Type type)
        {
            foreach (PhraseAttribute attribute in attributes)
            {
                if (attribute.GetType().Equals(type))
                    return attribute;
            }

            return null;    // not found
        }

        // Add (returns true) or update (returns false) an attribute
        public bool AddOrUpdateAttribute(PhraseAttribute attr)
        {
            for (int ii = 0; ii < attributes.Count; ii++)
                if (attributes[ii].GetType() == attr.GetType()) {
                    attributes[ii] = attr;
                    return false;
                }

            attributes.Add(attr);
            return true;
        }

        // Find the SpeechPart attribute
        public SpeechPart SpeechPart()
        {
            foreach (PhraseAttribute attribute in attributes)
                if (attribute is PartOSAttribute)
                    return ((PartOSAttribute)attribute).Part;

            return null;    // not found
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();

            if (phrases.Count > 0)
            {
                // output the subphrases
                result.Append("[");
                foreach (InformedPhrase phrase in phrases)
                {
                    if (result.Length > 1)
                        result.Append(" ");
                    result.Append(phrase.ToString());
                }
                result.Append("]/");
            }
            else
            {
                result.Append(definition);
                result.Append("/");
            }

            // Just produce the attributes
            foreach (PhraseAttribute attribute in attributes)
                result.Append(attribute.ToString());

            return result.ToString();
        }

        #region IFastSerializable Members

        public void Deserialize(SerializationReader reader)
        {
            definition = reader.ReadString();
            phrases = reader.ReadList<InformedPhrase>();
            attributes = reader.ReadList<PhraseAttribute>();
        }

        public void Serialize(SerializationWriter writer)
        {
            writer.Write(definition);
            writer.WriteList(phrases);
            writer.WriteList(attributes);
        }

        #endregion
    }
}
