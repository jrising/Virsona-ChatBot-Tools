using System;
using System.Collections.Generic;
using System.Text;
using LanguageNet.Grammarian;

namespace DataTemple.Matching
{
    public class KnowledgeUtilities
    {
        public static double ScoreConnectedStructure(List<Datum> structure)
        {
            double scoreTotal = 0;
            int scoreMax = 0;

            foreach (Datum datum in structure)
            {
                if (!datum.Left.IsSpecial && !datum.Left.IsUnknown)
                {
                    scoreTotal += 1.0 / StringUtilities.SplitWords(datum.Left.Name, true).Count;
                    scoreMax++;
                }

                if (!datum.Right.IsSpecial && !datum.Right.IsUnknown)
                {
                    scoreTotal += 1.0 / StringUtilities.SplitWords(datum.Right.Name, true).Count;
                    scoreMax++;
                }
            }

            return scoreTotal / scoreMax;
        }

        public static List<Datum> GetConnectedStructure(Memory memory, Datum start)
        {
            List<Datum> structure = new List<Datum>();
            structure.Add(start);

            // these are concepts we need to (or did) look up the data for
            List<Concept> processed = new List<Concept>();
            Queue<Concept> pending = new Queue<Concept>();
            pending.Enqueue(start.Left);
            pending.Enqueue(start.Right);

            while (pending.Count > 0)
            {
                Concept concept = pending.Dequeue();

                if (concept.IsSpecial || processed.Contains(concept))
                    continue;

                processed.Add(concept);
                List<Datum> data = memory.GetData(concept);
                foreach (Datum datum in data)
                {
                    if (!structure.Contains(datum))
                    {
                        structure.Add(datum);

                        pending.Enqueue(datum.Left);
                        pending.Enqueue(datum.Right);
                    }
                }
            }

            return structure;
        }

        public static Datum GetClosestDatum(Memory memory, Datum start, Relations.Relation relation)
        {
            // these are concepts we need to (or did) look up the data for
            List<Concept> processed = new List<Concept>();
            Queue<Concept> pending = new Queue<Concept>();
            pending.Enqueue(start.Left);
            pending.Enqueue(start.Right);

            while (pending.Count > 0)
            {
                Concept concept = pending.Dequeue();

                if (concept.IsSpecial || processed.Contains(concept))
                    continue;

                processed.Add(concept);
                List<Datum> data = memory.GetData(concept);
                foreach (Datum datum in data)
                {
                    if (datum.Relation == relation)
                        return datum;

                    pending.Enqueue(datum.Left);
                    pending.Enqueue(datum.Right);
                }
            }

            return null;
        }
    }
}
