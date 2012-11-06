using System;
using System.Collections.Generic;
using System.Text;
using GenericTools;
using LanguageNet.Grammarian;
using LanguageNet.ConceptNet;
using DataTemple.Variables;

namespace DataTemple.Matching
{
    public class Relations
    {
        public enum Relation
        {
            // General Relations
            Means,
            Precedes,
            AtTime,
            HasA,
            InLocation,
            HasProperty,
            IsA,
            Exists,
            Unknown,
            
            // Grammatical Relations
            Subject,
            Object,
            Indirect,
            Tense,
            Condition,  // or Entailment (WordNet) or Prerequisite (ConceptNet)

            // WordNet Relations
            // Noun WordNet Pointer Symbols
            Antonym,
            Hypernym,
            InstanceHypernym,
            InstanceHyponym,
            MemberMeronym,
            SubstanceMeronym,
            PartMeronym,
            DerivedForm,
            SynsetDomainTopic,
            DomainMemberTopic,
            SynsetDomainRegion,
            DomainMemberRegion,
            SynsetDomainUsage,
            DomainMemberUsage,

            // Verb WordNet Pointer Symbols
            Cause,      // really Causes
            AlsoSee,
            VerbGroup,

            // Adjective WordNet Pointer Symbols
            SimilarTo,
            VerbParticiple,
            NounPertainym,

            // ConceptNet Relations
            UsedFor,
            CapableOf,
            CreatedBy,
            HasFirstSubevent,
            HasLastSubevent,
            MotivatedByGoal,
            Desires,
            HasPainCharacter,
            CausesDesire,
            HasSubevent,
            ReceivesAction,
            HasPainIntensity
        }

        public readonly static ReversibleDictionary<Relation, string> RelationMap;

        static Relations()
        {
            RelationMap = new ReversibleDictionary<Relation, string>();

            RelationMap.Add(Relation.AlsoSee, "AlsoSee");
            RelationMap.Add(Relation.Antonym, "Anyonym");
            RelationMap.Add(Relation.AtTime, "AtTime");
            RelationMap.Add(Relation.CapableOf, "CapableOf");
            RelationMap.Add(Relation.Cause, "Cause");
            RelationMap.Add(Relation.CausesDesire, "CausesDesire");
            RelationMap.Add(Relation.Condition, "Condition");
            RelationMap.Add(Relation.CreatedBy, "CreatedBy");
            RelationMap.Add(Relation.DerivedForm, "DerivedForm");
            RelationMap.Add(Relation.Desires, "Desires");
            RelationMap.Add(Relation.DomainMemberRegion, "DomainMemberRegion");
            RelationMap.Add(Relation.DomainMemberTopic, "DomainMemberTopic");
            RelationMap.Add(Relation.DomainMemberUsage, "DomainMemberUsage");
            RelationMap.Add(Relation.Exists, "Exists");
            RelationMap.Add(Relation.HasA, "HasA");
            RelationMap.Add(Relation.HasFirstSubevent, "HasFirstSubevent");
            RelationMap.Add(Relation.HasLastSubevent, "HasLastSubevent");
            RelationMap.Add(Relation.HasPainCharacter, "HasPainCharacter");
            RelationMap.Add(Relation.HasPainIntensity, "HasPainIntensity");
            RelationMap.Add(Relation.HasProperty, "HasProperty");
            RelationMap.Add(Relation.HasSubevent, "HasSubevent");
            RelationMap.Add(Relation.Hypernym, "Hypernym");
            RelationMap.Add(Relation.Indirect, "Indirect");
            RelationMap.Add(Relation.InLocation, "InLocation");
            RelationMap.Add(Relation.InstanceHypernym, "InstanceHypernym");
            RelationMap.Add(Relation.InstanceHyponym, "InstanceHyponym");
            RelationMap.Add(Relation.IsA, "IsA");
            RelationMap.Add(Relation.Means, "Means");
            RelationMap.Add(Relation.MemberMeronym, "MemberMeronym");
            RelationMap.Add(Relation.MotivatedByGoal, "MotivatedByGoal");
            RelationMap.Add(Relation.NounPertainym, "NounPertainym");
            RelationMap.Add(Relation.Object, "Object");
            RelationMap.Add(Relation.PartMeronym, "PartMeronym");
            RelationMap.Add(Relation.Precedes, "Precedes");
            RelationMap.Add(Relation.ReceivesAction, "ReceivesAction");
            RelationMap.Add(Relation.SimilarTo, "SimilarTo");
            RelationMap.Add(Relation.Subject, "Subject");
            RelationMap.Add(Relation.SubstanceMeronym, "SubstanceMeronym");
            RelationMap.Add(Relation.SynsetDomainRegion, "SynsetDomainRegion");
            RelationMap.Add(Relation.SynsetDomainTopic, "SynsetDomainTopic");
            RelationMap.Add(Relation.SynsetDomainUsage, "SynsetDomainUsage");
            RelationMap.Add(Relation.Tense, "Tense");
            RelationMap.Add(Relation.Unknown, "Unknown");
            RelationMap.Add(Relation.UsedFor, "UsedFor");
            RelationMap.Add(Relation.VerbGroup, "VerbGroup");
            RelationMap.Add(Relation.VerbParticiple, "VerbParticiple");
        }

        public static Datum ConceptNetForwardTranslate(Memory memory, Verbs verbs, Concept left, Assertion assertion)
        {
            double score = 1.0 - 1.0 / (assertion.Score + 1);

            if (assertion.Type == Associator.AtLocation)
                return new Datum(left, Relation.InLocation, ConceptNetGetConcept(memory, verbs, assertion.Right, Concept.Kind.Entity), score);
            if (assertion.Type == Associator.CapableOf)
                return new Datum(left, Relation.CapableOf, ConceptNetGetConcept(memory, verbs, assertion.Right, Concept.Kind.Event), score);
            if (assertion.Type == Associator.Causes)
                return new Datum(left, Relation.Cause, ConceptNetGetConcept(memory, verbs, assertion.Right, Concept.Kind.Event), score);
            if (assertion.Type == Associator.CausesDesire)
                return new Datum(left, Relation.CausesDesire, ConceptNetGetConcept(memory, verbs, assertion.Right, Concept.Kind.Event), score);
            if (assertion.Type == Associator.CreatedBy)
                return new Datum(left, Relation.CreatedBy, ConceptNetGetConcept(memory, verbs, assertion.Right, Concept.Kind.Entity), score);
            if (assertion.Type == Associator.DefinedAs)
                return new Datum(left, Relation.Means, ConceptNetGetConcept(memory, verbs, assertion.Right, left.RawKind), score);
            if (assertion.Type == Associator.Desires)
                return new Datum(left, Relation.Desires, ConceptNetGetConcept(memory, verbs, assertion.Right, Concept.Kind.Entity), score);
            if (assertion.Type == Associator.HasFirstSubevent)
                return new Datum(left, Relation.HasFirstSubevent, ConceptNetGetConcept(memory, verbs, assertion.Right, Concept.Kind.Event), score);
            if (assertion.Type == Associator.HasLastSubevent)
                return new Datum(left, Relation.HasLastSubevent, ConceptNetGetConcept(memory, verbs, assertion.Right, Concept.Kind.Event), score);
            if (assertion.Type == Associator.HasPainCharacter)
                return new Datum(left, Relation.HasPainCharacter, ConceptNetGetConcept(memory, verbs, assertion.Right, Concept.Kind.Attribute), score);
            if (assertion.Type == Associator.HasPainIntensity)
                return new Datum(left, Relation.HasPainIntensity, ConceptNetGetConcept(memory, verbs, assertion.Right, Concept.Kind.Attribute), score);
            if (assertion.Type == Associator.HasPrerequisite)
                return new Datum(left, Relation.Condition, ConceptNetGetConcept(memory, verbs, assertion.Right, Concept.Kind.Entity), score);
            if (assertion.Type == Associator.HasProperty)
                return new Datum(left, Relation.HasProperty, ConceptNetGetConcept(memory, verbs, assertion.Right, Concept.Kind.Attribute), score);
            if (assertion.Type == Associator.HasSubevent)
                return new Datum(left, Relation.HasSubevent, ConceptNetGetConcept(memory, verbs, assertion.Right, Concept.Kind.Event), score);
            if (assertion.Type == Associator.IsA)
                return new Datum(left, Relation.IsA, ConceptNetGetConcept(memory, verbs, assertion.Right, left.RawKind), score);
            if (assertion.Type == Associator.MadeOf)
                return new Datum(left, Relation.SubstanceMeronym, ConceptNetGetConcept(memory, verbs, assertion.Right, Concept.Kind.Entity), score);
            if (assertion.Type == Associator.MotivatedByGoal)
                return new Datum(left, Relation.MotivatedByGoal, ConceptNetGetConcept(memory, verbs, assertion.Right, Concept.Kind.Entity), score);
            if (assertion.Type == Associator.PartOf)
                return new Datum(ConceptNetGetConcept(memory, verbs, assertion.Right, Concept.Kind.Entity), Relation.PartMeronym, left, score);
            if (assertion.Type == Associator.ReceivesAction)
                return new Datum(left, Relation.ReceivesAction, ConceptNetGetConcept(memory, verbs, assertion.Right, Concept.Kind.Event), score);
            if (assertion.Type == Associator.UsedFor)
                return new Datum(left, Relation.UsedFor, ConceptNetGetConcept(memory, verbs, assertion.Right, Concept.Kind.Event), score);

            //throw new ArgumentException("Unknown assertion type: " + assertion.Type.ToString());
            return new Datum(left, Relation.Unknown, ConceptNetGetConcept(memory, verbs, assertion.Right, Concept.Kind.Entity), score);
        }

        public static Datum ConceptNetReverseTranslate(Memory memory, Verbs verbs, Concept right, Assertion assertion)
        {
            double score = 1.0 - 1.0 / (assertion.Score + 1);

            if (assertion.Type == Associator.AtLocation)
                return new Datum(ConceptNetGetConcept(memory, verbs, assertion.Left, Concept.Kind.Entity), Relation.InLocation, right, score);
            if (assertion.Type == Associator.CapableOf)
                return new Datum(ConceptNetGetConcept(memory, verbs, assertion.Left, Concept.Kind.Entity), Relation.CapableOf, right, score);
            if (assertion.Type == Associator.Causes)
                return new Datum(ConceptNetGetConcept(memory, verbs, assertion.Left, Concept.Kind.Event), Relation.Cause, right, score);
            if (assertion.Type == Associator.CausesDesire)
                return new Datum(ConceptNetGetConcept(memory, verbs, assertion.Left, Concept.Kind.Event), Relation.CausesDesire, right, score);
            if (assertion.Type == Associator.CreatedBy)
                return new Datum(ConceptNetGetConcept(memory, verbs, assertion.Left, Concept.Kind.Entity), Relation.CreatedBy, right, score);
            if (assertion.Type == Associator.DefinedAs)
                return new Datum(ConceptNetGetConcept(memory, verbs, assertion.Left, right.RawKind), Relation.Means, right, score);
            if (assertion.Type == Associator.Desires)
                return new Datum(ConceptNetGetConcept(memory, verbs, assertion.Left, Concept.Kind.Entity), Relation.Desires, right, score);
            if (assertion.Type == Associator.HasFirstSubevent)
                return new Datum(ConceptNetGetConcept(memory, verbs, assertion.Left, Concept.Kind.Event), Relation.HasFirstSubevent, right, score);
            if (assertion.Type == Associator.HasLastSubevent)
                return new Datum(ConceptNetGetConcept(memory, verbs, assertion.Left, Concept.Kind.Event), Relation.HasLastSubevent, right, score);
            if (assertion.Type == Associator.HasPainCharacter)
                return new Datum(ConceptNetGetConcept(memory, verbs, assertion.Left, Concept.Kind.Event), Relation.HasPainCharacter, right, score);
            if (assertion.Type == Associator.HasPainIntensity)
                return new Datum(ConceptNetGetConcept(memory, verbs, assertion.Left, Concept.Kind.Event), Relation.HasPainIntensity, right, score);
            if (assertion.Type == Associator.HasPrerequisite)
                return new Datum(ConceptNetGetConcept(memory, verbs, assertion.Left, Concept.Kind.Event), Relation.Condition, right, score);
            if (assertion.Type == Associator.HasProperty)
                return new Datum(ConceptNetGetConcept(memory, verbs, assertion.Left, Concept.Kind.Entity), Relation.HasProperty, right, score);
            if (assertion.Type == Associator.HasSubevent)
                return new Datum(ConceptNetGetConcept(memory, verbs, assertion.Left, Concept.Kind.Event), Relation.HasSubevent, right, score);
            if (assertion.Type == Associator.IsA)
                return new Datum(ConceptNetGetConcept(memory, verbs, assertion.Left, right.RawKind), Relation.IsA, right, score);
            if (assertion.Type == Associator.MadeOf)
                return new Datum(ConceptNetGetConcept(memory, verbs, assertion.Left, Concept.Kind.Entity), Relation.SubstanceMeronym, right, score);
            if (assertion.Type == Associator.MotivatedByGoal)
                return new Datum(ConceptNetGetConcept(memory, verbs, assertion.Left, Concept.Kind.Entity), Relation.MotivatedByGoal, right, score);
            if (assertion.Type == Associator.PartOf)
                return new Datum(ConceptNetGetConcept(memory, verbs, assertion.Left, Concept.Kind.Entity), Relation.PartMeronym, right, score);
            if (assertion.Type == Associator.ReceivesAction)
                return new Datum(ConceptNetGetConcept(memory, verbs, assertion.Left, Concept.Kind.Entity), Relation.ReceivesAction, right, score);
            if (assertion.Type == Associator.UsedFor)
                return new Datum(ConceptNetGetConcept(memory, verbs, assertion.Left, Concept.Kind.Entity), Relation.UsedFor, right, score);

            //throw new ArgumentException("Unknown assertion type: " + assertion.Type.ToString());
            return new Datum(ConceptNetGetConcept(memory, verbs, assertion.Left, Concept.Kind.Entity), Relation.Unknown, right, score);
        }

        public static Concept ConceptNetGetConcept(Memory memory, Verbs verbs, Notion notion, Concept.Kind kind)
        {
            if (kind == Concept.Kind.Event)
                return memory.NewConcept(verbs.InputToBase(notion.Canonical), kind);
            else
                return memory.NewConcept(notion.Canonical, kind);
        }

        public static string ConjugateToTense(Memory memory, string verb, Verbs.Person person, Concept right, Verbs verbs)
        {
            if (right == memory.past)
                return verbs.ComposePersonable(verb, person, Verbs.Convert.ext_Ved);
            if (right == memory.now)
                return verbs.ComposePersonable(verb, person, Verbs.Convert.ext_Vs);
            if (right == memory.future)
                return "will " + verbs.ComposePersonable(verb, person, Verbs.Convert.ext_V);

            List<string> parts = StringUtilities.SplitWords(right.Name, true);
            bool usedverb = false;
            for (int ii = 0; ii < parts.Count; ii++)
            {
                if (parts[ii] == "en") {
                    parts[ii] = verbs.ComposePersonable(verb, person, Verbs.Convert.ext_Ven);
                    usedverb = true;
                }
                else if (parts[ii] == "ing") {
                    parts[ii] = verbs.ComposePersonable(verb, person, Verbs.Convert.ext_Ving);
                    usedverb = true;
                }
            }
            if (!usedverb)
                parts.Add(verbs.InputToBase(verb));
            return StringUtilities.JoinWords(parts);
        }

        public static IParsedPhrase ConjugateToPhrase(Memory memory, IrregularVerbVariable.ConjugateVerb conjugate, Verbs.Person person, Concept right) {
            if (right == memory.past)
                return new WordPhrase(conjugate(person, Verbs.Convert.ext_Ved), "VBD");
            else if (right == memory.now)
                return new WordPhrase(conjugate(person, Verbs.Convert.ext_Vs), "VBZ");
            else if (right == memory.future)
                return new WordPhrase("will " + conjugate(person, Verbs.Convert.ext_V), "VB");

            List<string> parts = StringUtilities.SplitWords(right.Name, true);
            bool usedverb = false;
            for (int ii = 0; ii < parts.Count; ii++)
            {
                if (parts[ii] == "en")
                {
                    parts[ii] = conjugate(person, Verbs.Convert.ext_Ven);
                    usedverb = true;
                }
                else if (parts[ii] == "ing")
                {
                    parts[ii] = conjugate(person, Verbs.Convert.ext_Ving);
                    usedverb = true;
                }
            }
            if (!usedverb)
                parts.Add(conjugate(person, Verbs.Convert.ext_V));
			
			List<IParsedPhrase> branches = new List<IParsedPhrase>();
			branches.Add(new WordPhrase(StringUtilities.JoinWords(parts), "VB"));
            return new GroupPhrase("VP", branches);
        }
    }
}
