using System;
using System.Collections.Generic;
using System.Text;
using LanguageNet.Grammarian;
using GenericTools;

namespace DataTemple.Matching
{
    public class Memory
    {
        protected Dictionary<string, List<Concept>> concepts;
        protected Dictionary<Concept, List<Datum>> data;    // have this both directions!

        protected Datum center;

        protected int specialCount;
        public readonly Concept time;
        public readonly Concept now;
        public readonly Concept past;
        public readonly Concept future;
        public readonly Concept here;
        public readonly Concept self;
        public readonly Concept you;
        public readonly Concept they;

        public Memory()
        {
            concepts = new Dictionary<string,List<Concept>>();
            data = new Dictionary<Concept,List<Datum>>();

            specialCount = 0;
            time = SpecialConcept("time", Concept.Kind.Entity);
            now = SpecialConcept("now", Concept.Kind.Entity);
            past = SpecialConcept("past", Concept.Kind.Entity);
            here = SpecialConcept("here", Concept.Kind.Entity);
            self = SpecialConcept("I", Concept.Kind.Entity);
            future = SpecialConcept("future", Concept.Kind.Entity);
            you = SpecialConcept("you", Concept.Kind.Entity);
            they = SpecialConcept("they", Concept.Kind.Entity);

            Know(now, Relations.Relation.IsA, time, 1.0);
            Know(past, Relations.Relation.IsA, time, 1.0);
            Know(past, Relations.Relation.Precedes, now, 1.0);
            Know(now, Relations.Relation.Precedes, future, 1.0);

            Concept dasein = SpecialConcept("[*]", Concept.Kind.Entity);  // the phenomological awareness

            Know(dasein, Relations.Relation.InLocation, here, 1.0);
            Know(dasein, Relations.Relation.AtTime, now, 1.0);
            Know(dasein, Relations.Relation.Subject, self, 1.0);
        }

        public Dictionary<Concept, List<Datum>> Data
        {
            get
            {
                return data;
            }
        }

        public List<Concept> AllConcepts
        {
            get
            {
                List<Concept> allconcepts = new List<Concept>();
                foreach (KeyValuePair<string, List<Concept>> kvp in concepts)
                    allconcepts.AddRange(kvp.Value);
                return allconcepts;
            }
        }

        protected Concept SpecialConcept(string name, Concept.Kind kind)
        {
            Concept concept = NewConcept(name, kind);

            specialCount++;
            concept.Id = -specialCount;

            return concept;
        }
        
        public Concept NewConcept(string name, Concept.Kind kind)
        {
            List<Concept> matches;
            if (!concepts.TryGetValue(name, out matches))
            {
                matches = new List<Concept>();
                concepts.Add(name, matches);
            }

            Concept concept = new Concept(name, kind);
            data.Add(concept, new List<Datum>());
            matches.Add(concept);

            return concept;
        }

        public Concept NewConcept(IParsedPhrase phrase)
        {
            if (phrase.IsLeaf)
            {
                if (phrase.Part.StartsWith("VB"))
                    return NewConcept(phrase.Text, Concept.Kind.Event);
                else if (phrase.Part == "JJ" || phrase.Part == "RB")
                    return NewConcept(phrase.Text, Concept.Kind.Attribute);
                else
                    return NewConcept(phrase.Text, Concept.Kind.Entity);
            }
			
			GroupPhrase groupPhrase = new GroupPhrase(phrase);
            List<IParsedPhrase> constituents = new List<IParsedPhrase>(phrase.Branches);

            // 1. Strip parentheticals
            IParsedPhrase parenthetical = groupPhrase.FindBranch("PRN");
            if (parenthetical != null)
                constituents.Remove(parenthetical);

            // 2. Preposition to contexts, back to front
            /* XXX: Works for "the road of Rome" but not "the city of Rome"
            while (constituents[constituents.Count - 1] is PrepositionalPhrase)
            {
                context = GetConcept(constituents[constituents.Count - 1], context);
                constituents.RemoveAt(constituents.Count - 1);
            }*/

            IParsedPhrase remain = new GroupPhrase("", constituents);

            if (phrase.Part == "PP")
                return NewConcept(remain.Text, Concept.Kind.Attribute);
            else if (phrase.Part == "VB")
                return NewConcept(remain.Text, Concept.Kind.Event);
            else
                return NewConcept(remain.Text, Concept.Kind.Entity);
        }

        public Concept NewUnknown(Concept.Kind kind)
        {
            return NewConcept("", kind);
        }

        public Concept GetConcept(string name, int id, Concept.Kind kind)
        {
            if (name == null)
                Console.WriteLine("But how?");

            List<Concept> matches;
            if (concepts.TryGetValue(name, out matches))
            {
                foreach (Concept match in matches)
                    if (match.Id == id)
                        return match;
            }
            else
            {
                matches = new List<Concept>();
                concepts.Add(name, matches);
            }

            Concept concept = new Concept(name, kind);
            concept.Id = id;
            data.Add(concept, new List<Datum>());
            matches.Add(concept);

            return concept;
        }

        public List<Concept> GetConcepts(string name)
        {
            List<Concept> matches;
            if (concepts.TryGetValue(name, out matches))
                return matches;

            return new List<Concept>();
        }

        public void AddConcept(string name, Concept concept)
        {
            List<Concept> current;
            if (!concepts.TryGetValue(name, out current)) {
                current = new List<Concept>();
                concepts.Add(name, current);
            }

            current.Add(concept);
        }
		
        // Load the concept if it isn't already
        public List<Datum> GetData(Concept concept)
        {
            List<Datum> matches = new List<Datum>();
            data.TryGetValue(concept, out matches);
            return matches;
		}

        public Datum Know(Concept left, Relations.Relation relation, Concept right, double score)
        {
            return Know(new Datum(left, relation, right, score));
        }

        public Datum Know(Concept left, Relations.Relation relation, IEnumerable<Concept> rights, double score)
        {
            Datum last = null;
            foreach (Concept right in rights)
                last = Know(new Datum(left, relation, right, score));

            return last;
        }

        public Datum Know(Concept left, Datum partial)
        {
            return Know(new Datum(left, partial.Relation, partial.Right, partial.Score));
        }

        public Datum Know(Datum datum)
        {
            data[datum.Left].Add(datum);
            data[datum.Right].Add(datum);

            center = datum;
            return datum;
        }

        public Datum IsKnown(Datum datum)
        {
            // Is this datum represented?
            BestScore<Datum> match = new BestScore<Datum>();
            foreach (Datum check in GetData(datum.Left))
                if (datum.Relation == check.Relation)
                    match.Improve(datum.Right.SeemsSimilar(check.Right).ToDouble(1.0), check);

            return match.Payload;
        }

        public Concept BeforeContext(Concept when)
        {
            if (when == now || when == past)
                return past;

            Concept before = NewConcept("before " + when.Name, when.RawKind);
            Know(before, Relations.Relation.Precedes, when, 1.0);

            return before;
        }
    }
}
