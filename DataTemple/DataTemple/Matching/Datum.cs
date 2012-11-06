using System;
using System.Collections.Generic;
using System.Text;
using LanguageNet.Grammarian;

namespace DataTemple.Matching
{
    public class Datum
    {        
        protected Concept left;
        protected Relations.Relation relation;
        protected Concept right;
        protected double score;

        public Datum(Concept left, Relations.Relation relation, Concept right, double score)
        {
            this.left = left;
            this.relation = relation;
            this.right = right;
            this.score = score;
        }

        public Datum(Datum parent)
        {
            left = parent.left;
            relation = parent.relation;
            right = parent.right;
            score = parent.score;
        }

        public Concept Left
        {
            get
            {
                return left;
            }
        }

        public Relations.Relation Relation
        {
            get
            {
                return relation;
            }
        }

        public Concept Right
        {
            get
            {
                return right;
            }
        }

        public double Score
        {
            get
            {
                return score;
            }
        }

        public double IsSameUnder(Datum datum, Concept before, Concept after)
        {
            if ((left == datum.left || (left == before && datum.left == after)) &&
                (right == datum.right || (right == before && datum.right == after)) &&
                (relation == datum.relation))
                return 1.0 - Math.Abs(score - datum.score) / (score + datum.score);

            return 0.0;
        }

        public void ChangeLeft(Concept left, Memory memory)
        {
            memory.GetData(left).Remove(this);
            this.left = left;
            memory.GetData(left).Add(this);
        }

        public void ChangeRight(Concept right, Memory memory)
        {
            memory.GetData(right).Remove(this);
            this.right = right;
            memory.GetData(right).Add(this);
        }

        public void ChangeScore(double score)
        {
            this.score = score;
        }

		// Merge and remove old
        public void Merge(Datum old, Memory memory)
        {
            if (old.score > score)
                ChangeScore(old.score);

            List<Datum> dataLeft = memory.GetData(old.left);
            if (dataLeft != null)
                dataLeft.Remove(old);
            List<Datum> dataRight = memory.GetData(old.right);
            if (dataRight != null)
                dataRight.Remove(old);
            // Make this datum reflect other (in case we're in the middle of a save)
            old.left = left;
            old.right = right;
        }

        public string Represent(Memory memory, Concept within, Verbs verbs, Nouns nouns) {
            string[] sentence = new string[] { };
            switch (relation)
            {
                case Relations.Relation.InLocation:
                    sentence = new string[] { left.Name, RepresentToBe(memory, left, within, verbs, nouns), "in", right.Name, " ." };
                    break;
                case Relations.Relation.HasA:
                    sentence = new string[] { left.Name, RepresentToHave(memory, left, within, verbs, nouns), right.Name, " ." };
                    break;
                case Relations.Relation.HasProperty:
                    sentence = new string[] { left.Name, RepresentToBe(memory, left, within, verbs, nouns), right.Name, " ." };
                    break;
                case Relations.Relation.IsA:
                    sentence = new string[] { left.Name, RepresentToBe(memory, left, within, verbs, nouns), right.Name, " ." };
                    break;
            }

            return StringUtilities.JoinWords(new List<string>(sentence));
        }

        public static string RepresentToBe(Memory memory, Concept concept, Concept within, Verbs verbs, Nouns nouns)
        {
            return Verbs.ComposeToBe(nouns.GetPerson(concept.Name), GetTense(concept, within, memory));
        }

        public static string RepresentToHave(Memory memory, Concept concept, Concept within, Verbs verbs, Nouns nouns)
        {
            return Verbs.ComposeToHave(nouns.GetPerson(concept.Name), GetTense(concept, within, memory));
        }

        public static Verbs.Convert GetTense(Concept context, Concept within, Memory memory)
        {
            if (context.Precedes(within, memory))   // past < past, past < now
                return Verbs.Convert.ext_Ved;

            /*if (context.DerivesFrom(memory.now, memory, dbquery) && within.DerivesFrom(memory.now, memory, dbquery)) // now == now
                return Verbs.Convert.ext_Vs;*/

            /*if (within.Precedes(context, memory, dbquery))   // past > past, now < past
                return Verbs.Convert.ext_V; // will...*/

            return Verbs.Convert.ext_Vs;
        }

        public override string ToString()
        {
            if (left == null)
                return "[?] " + Relations.RelationMap[relation] + " " + right.ToString();
            else
                return left.ToString() + " " + Relations.RelationMap[relation] + " " + right.ToString();
        }
    }
}
