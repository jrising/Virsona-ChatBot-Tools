/******************************************************************\
 *      Class Name:     SpeechPart
 *      Written By:     James.R
 *      Copyright:      Virsona Inc
 *      -----------------------------------------------------------
 * Encapsulate a part of speech
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using GenericTools;
using LanguageNet.Grammarian;

namespace LanguageNet.WordLogic
{
    public class SpeechPart
    {
        // All known parts of speech (add others here!)
        // characters are ordered-- put the top hierarchy level first
        public static readonly SpeechPart Unknown = new SpeechPart("u");
        public static readonly SpeechPart Punctuation = new SpeechPart(".");
        public static readonly SpeechPart Sentence = new SpeechPart("S");

        public static readonly SpeechPart ConjunctionCoordinating = new SpeechPart("CC");
        public static readonly SpeechPart CardinalNumber = new SpeechPart("CD");
        public static readonly SpeechPart Article = new SpeechPart("DT");
        public static readonly SpeechPart DefiniteArticle = new SpeechPart("DTd");     // the
        public static readonly SpeechPart IndefiniteArticle = new SpeechPart("DTi");   // an
        public static readonly SpeechPart ArticulatePronoun = new SpeechPart("DTip");  // its, that
        public static readonly SpeechPart ExistencialThere = new SpeechPart("EX");
        public static readonly SpeechPart ForeignWord = new SpeechPart("FW");
        public static readonly SpeechPart Preposition = new SpeechPart("IN");
        public static readonly SpeechPart Adjective = new SpeechPart("JJ");
        public static readonly SpeechPart AdjectiveComparative = new SpeechPart("JJR");
        public static readonly SpeechPart AdjectiveSuperlative = new SpeechPart("JJS");
        public static readonly SpeechPart DescriptivePronoun = new SpeechPart("JJp"); // sixth
        public static readonly SpeechPart ListItemMarker = new SpeechPart("LS");
        public static readonly SpeechPart ModalVerb = new SpeechPart("MD");
        public static readonly SpeechPart Noun = new SpeechPart("NN");
        public static readonly SpeechPart ProperNoun = new SpeechPart("NNP");
        public static readonly SpeechPart PersonalPronoun = new SpeechPart("NNp");     // it, she, everyone
        public static readonly SpeechPart ObjectPronoun = new SpeechPart("NNpo");      // him
        public static readonly SpeechPart ReflexivePronoun = new SpeechPart("NNpor");  // themselves
        public static readonly SpeechPart PossessivePronoun = new SpeechPart("NNpop"); // its, hers
        public static readonly SpeechPart DeclarativePronoun = new SpeechPart("NNpe"); // one, any, those
        public static readonly SpeechPart WhPronoun = new SpeechPart("NNWP");
        public static readonly SpeechPart PossiveWhPronoun = new SpeechPart("NNWP$");
        public static readonly SpeechPart Predeterminer = new SpeechPart("PDT");
        public static readonly SpeechPart PossesiveEnding = new SpeechPart("POS");
        public static readonly SpeechPart Adverb = new SpeechPart("RB");
        public static readonly SpeechPart AdverbComparative = new SpeechPart("RBR");
        public static readonly SpeechPart AdverbSuperlative = new SpeechPart("RBS");
        public static readonly SpeechPart Particle = new SpeechPart("RP");
        public static readonly SpeechPart Symbol = new SpeechPart("SYM");
        public static readonly SpeechPart To = new SpeechPart("TO");
        public static readonly SpeechPart Interjection = new SpeechPart("UH");
        public static readonly SpeechPart Verb = new SpeechPart("VB");
        public static readonly SpeechPart VerbPresentNotThird = new SpeechPart("VBP");
        public static readonly SpeechPart VerbPresentThird = new SpeechPart("VBZ");
        public static readonly SpeechPart VerbPastTense = new SpeechPart("VBD");
        public static readonly SpeechPart GerundOrPresentParticiple = new SpeechPart("VBG");
        public static readonly SpeechPart PastParticiple = new SpeechPart("VBN");
        public static readonly SpeechPart WhDeterminer = new SpeechPart("WDT");
        public static readonly SpeechPart WhAdverb = new SpeechPart("WRB");
        public static readonly SpeechPart NounPhrase = new SpeechPart("NN_P");
        public static readonly SpeechPart VerbPhrase = new SpeechPart("VB_P");
        public static readonly SpeechPart AdjectivePhrase = new SpeechPart("JJ_P");
        public static readonly SpeechPart AdverbPhrase = new SpeechPart("RB_P");
        public static readonly SpeechPart PrepositionalPhrase = new SpeechPart("IN_P");
        public static readonly SpeechPart ToPhrase = new SpeechPart("TO_P");
        // Note: attributes of pronouns used as articles apply to articulate noun
        //       attributes of pronouns used as nouns or adjectives apply to referent
        //   Some words (e.g. its, those) have both attributes (on different senses)

        // The catalog of all parts of speech
        protected static Dictionary<string, SpeechPart> catalog;

        protected string name;

        // protected creator, for our static parts of speech
        protected SpeechPart(string name)
        {
            this.name = name;
            if (catalog == null)
                catalog = new Dictionary<string, SpeechPart>();
            // add to catalog
            catalog.Add(name, this);
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        // Find the known part of speech
        public static SpeechPart GetPart(string name)
        {
            if (catalog.ContainsKey(name))
            {
                return catalog[name];
            }
            string underscoreName = name.Insert( name.Length - 1, "_");
            if (catalog.ContainsKey(underscoreName))
            {
                return catalog[underscoreName];
            }
            catalog[name] = new SpeechPart(name);
            return catalog[name];
        }

        // Is this an anaphora-type part of speech
        public ProbableStrength SeemsAnaphora()
        {
            ProbableStrength itis = SeemsA(PersonalPronoun, true).
                Improve(SeemsA(DescriptivePronoun, true)).
                Improve(SeemsA(WhPronoun, true)).
                Improve(SeemsA(DefiniteArticle, true).DownWeight(.5));
            if (itis.strength == 0)
                return itis;

            ProbableStrength itsnot = SeemsA(ProperNoun, true);

            return new ProbableStrength(itis.strength * itis.strength / (itis.strength + itsnot.strength), (itis.weight + itsnot.weight) / 2.0);
        }

        // Is this part of speech like ours?
        public ProbableStrength SeemsA(SpeechPart part, bool allSubs) {
            if (name == part.name || (allSubs && name.StartsWith(part.name)))
                return ProbableStrength.Full;   // exact match!
            else
            {
                // Look for a partial match
                int totalLength;
                if (allSubs)
                    totalLength = part.name.Length;
                else
                    totalLength = Math.Max(name.Length, part.name.Length);

                double strength = 0;
                // while there are more characters in each
                for (int ii = 0; ii < name.Length && ii < part.name.Length; ii++)
                {
                    if (name[ii] == part.name[ii])
                        strength += 1.0 / (2 << ii);    // add more score
                    else
                        break;
                }

                double maxScore = 1.0 - 1.0 / (2 << totalLength);

                if (strength / maxScore > .5)
                    return new ProbableStrength(strength / maxScore - .5, strength / maxScore);
                else
                    return new ProbableStrength(0, 0.5);
            }
        }

        // Find the phrase part of speech characteristic of this part
        public static SpeechPart PhraseOf(SpeechPart part)
        {
            if (part.name.EndsWith("_P"))
                return part;
            else
                return GetPart(part.name.Substring(0, 2) + "_P");
        }

        public static List<PhraseAttribute> TreebankToAttributes(string part, IWordLookup informer, string word, bool simplified)
        {
            List<PhraseAttribute> attributes = new List<PhraseAttribute>();
            InformedPhrase informed = (informer == null || word == null || word.Contains(" ")) ? null : informer.GetInformed(word, false);

            if (part == "NN")
            {
                attributes.Add(new PartOSAttribute(Noun));
                attributes.Add(new NumberAttribute(NumberAttribute.NumberOptions.One));
                PhraseSense sense = informed != null ? informed.FindSense(SpeechPart.Noun, true) : null;
                return OverrideAttributes(sense, attributes);
            }

            if (part == "NNS")
            {
                attributes.Add(new PartOSAttribute(Noun));
                attributes.Add(new NumberAttribute(NumberAttribute.NumberOptions.Many));
                PhraseSense sense = informed != null ? informed.FindSense(SpeechPart.Noun, false) : null;
                return OverrideAttributes(sense, attributes);
            }

            if (part == "NNP")
            {
                attributes.Add(new PartOSAttribute(ProperNoun));
                attributes.Add(new NumberAttribute(NumberAttribute.NumberOptions.One));
                return attributes;
            }

            if (part == "NNPS")
            {
                attributes.Add(new PartOSAttribute(ProperNoun));
                attributes.Add(new NumberAttribute(NumberAttribute.NumberOptions.Many));
                return attributes;
            }

            if (part == "PRP" || part == "PRP$" || part == "WDT" || part == "WP" || part == "WP$" || part == "WDT" || part == "WRB")
            {
                if (informer == null || word == null || informed == null)
                {
                    if (part == "PRP")
                        attributes.Add(new PartOSAttribute(PersonalPronoun));
                    else if (part == "PRP$")
                        attributes.Add(new PartOSAttribute(PossessivePronoun));
                    else if (part == "WP")
                        attributes.Add(new PartOSAttribute(WhPronoun));
                    else if (part == "WP$")
                        attributes.Add(new PartOSAttribute(PossiveWhPronoun));
                    else
                        attributes.Add(new PartOSAttribute(part));
                    return attributes;
                }

                PhraseSense sense = informed.FindSense(PersonalPronoun, true);
                if (sense == null)
                    sense = informed.FindSense(ArticulatePronoun, true);
                if (sense == null)
                    sense = informed.Senses[0].Key;

                return sense.Attributes;
            }

            // Convert phrases
            if (part.StartsWith("NP"))
                part = "NN_P" + part.Substring(2);
            else if (part.StartsWith("VP"))
                part = "VB_P" + part.Substring(2);
            else if (part.StartsWith("PP"))
                part = "IN_P" + part.Substring(2);

            if (simplified)
            {
                int dash = part.IndexOf('-');
                if (dash > 0)
                    part = part.Substring(0, dash);
            }

            if (!catalog.ContainsKey(part))
                attributes.Add(new PartOSAttribute(new SpeechPart(part)));
            else
                attributes.Add(new PartOSAttribute(part));
            return attributes;
        }

        public static List<PhraseAttribute> OverrideAttributes(PhraseSense sense, List<PhraseAttribute> attrs)
        {
            if (sense == null)
                return attrs;

            foreach (PhraseAttribute attr in attrs)
            {
                PhraseAttribute already = sense.FindAttribute(attr.GetType());
                sense.Attributes.Remove(already);
                sense.Attributes.Add(attr);
            }

            return sense.Attributes;
        }
		
        // Convert from WordNet speech part
        public static SpeechPart WordNetPartToSpeechPart(WordNetAccess.PartOfSpeech type)
        {
            if (type == WordNetAccess.PartOfSpeech.Adj)
                return Adjective;
            if (type == WordNetAccess.PartOfSpeech.AdjSat || type == WordNetAccess.PartOfSpeech.All || type == WordNetAccess.PartOfSpeech.Satellite)
                return Unknown;
            if (type == WordNetAccess.PartOfSpeech.Adv)
                return Adverb;
            if (type == WordNetAccess.PartOfSpeech.Noun)
                return Noun;
            if (type == WordNetAccess.PartOfSpeech.Verb)
                return Verb;

            return Unknown;
        }

        // Convert a list of wordnet speech parts
        public static List<SpeechPart> WordNetPartToSpeechPart(List<WordNetAccess.PartOfSpeech> types)
        {
            List<SpeechPart> parts = new List<SpeechPart>();
            foreach (WordNetAccess.PartOfSpeech type in types)
                parts.Add(WordNetPartToSpeechPart(type));

            return parts;
        }
    }
}
