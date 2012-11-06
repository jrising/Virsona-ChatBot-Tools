using System;
using System.Collections.Generic;
using System.Text;
using DataTemple.AgentEvaluate;
using LanguageNet.Grammarian;

namespace DataTemple.Matching
{
    public class Concept : IContent
    {
        public enum Kind
        {
            Entity, // concept is noun phrase
            Event,  // concept is verb
            Attribute   // concept is adjective or adverb
        };

        protected int id;
        protected string name;

        protected Kind kind;

        protected bool loaded;

        public Concept(string name, Kind kind)
        {
            id = 0;
            this.name = name;

            this.kind = kind;

            loaded = false;
        }

        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
        }

        public bool IsLoaded
        {
            get
            {
                return loaded;
            }
            set
            {
                loaded = value;
            }
        }

        // refine should not be equality-- we could go in circles
        public double HasRelationTo(Memory memory, Concept find, Relations.Relation relation, Relations.Relation refine)
        {
            List<Datum> data = memory.GetData(this);
            BestScore<Datum> bestConnect = new BestScore<Datum>(0.0, null);

            foreach (Datum datum in data)
            {
                if (datum.Relation == relation && datum.Right == find)
                    return datum.Score;
                if (datum.Relation == relation || datum.Relation == refine)
                    bestConnect.Improve(datum.Right.HasRelationTo(memory, find, relation, refine) * datum.Score, datum);
            }

            return bestConnect.Score;
        }

        public bool Precedes(Concept other, Memory memory)
        {
            return HasRelationTo(memory, other, Relations.Relation.Precedes, Relations.Relation.AtTime) > 0.0;
        }

        public bool Contains(Concept other, Memory memory)
        {
            return HasRelationTo(memory, other, Relations.Relation.HasA, Relations.Relation.HasA) > 0.0 ||
                HasRelationTo(memory, other, Relations.Relation.InLocation, Relations.Relation.InLocation) > 0.0;
        }

        public Kind RawKind
        {
            get
            {
                return kind;
            }
        }

        public bool IsSpecial
        {
            get
            {
                return id < 0;
            }
        }

        public bool IsEntity
        {
            get
            {
                return kind == Kind.Entity;
            }
        }

        public bool IsEvent
        {
            get
            {
                return kind == Kind.Event;
            }
        }

        public bool IsAttribute
        {
            get
            {
                return kind == Kind.Attribute;
            }
        }

        public bool IsUnknown
        {
            get
            {
                return string.IsNullOrEmpty(name);
            }
        }

        public bool IsSame(Concept other, Memory memory)
        {
            return true;    // for now, always same concept
            /*
            if (datum != null)
                if (datum.InDatabase(memory, dbquery))
                    datumId = datum.Id;
            if (other.datum != null)
                if (other.datum.InDatabase(memory, dbquery))
                    other.datumId = other.datum.Id;

            if (datumId != 0 || other.datumId != 0)
                return datumId == other.datumId;

            List<Datum> data = memory.GetData(this, null);  // don't use the db
            List<Datum> otherData = memory.GetData(other, dbquery);

            foreach (Datum ddatum in data)
            {
                bool found = false;
                foreach (Datum otherDatum in otherData)
                    if (ddatum.IsSameUnder(otherDatum, this, other) > 0.0)
                    {
                        found = true;
                        break;
                    }

                if (!found)
                    return false;
            }

            // All the data were found!  These two are identical.
            return true;*/
        }

        public ProbableStrength SeemsSimilar(Concept other)
        {
            if (other.kind != kind)
                return ProbableStrength.Zero;

            if (other.name == name)
                return ProbableStrength.Full;

            // for now, could be...
            return ProbableStrength.Half;
        }

        public IParsedPhrase ToPhrase(POSTagger tagger, GrammarParser parser)
        {
            if (name.Contains(" "))
            {
                List<IParsedPhrase> phrases = new List<IParsedPhrase>();
                List<string> words = StringUtilities.SplitWords(name, true);
                foreach (string word in words)
                    phrases.Add(new WordPhrase(word, "??"));

                List<KeyValuePair<string, string>> tokens = tagger.ResolveUnknowns(phrases);
				
                return parser.Parse(tokens);
            }
            else
            {
                if (kind == Kind.Event)
                    return new WordPhrase(name, "VB");
                if (kind == Kind.Attribute)
                    return new WordPhrase(name, "JJ");
                // entity
                return new WordPhrase(name, "NN");
            }
        }

        public override string ToString()
        {
            if (name != null)
            {
                if (id == 0)
                    return "[" + name + "]";
                else
                    return "[" + name + " (" + id + ")]";
            }
            else
                return "[?]";
        }
    }
}
