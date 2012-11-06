/******************************************************************\
 *      Class Name:     BestScore
 *      Written By:     James.R
 *      Copyright:      Virsona Inc
 *      -----------------------------------------------------------
 * Keeps track of the best of several options
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using GenericTools;
using GenericTools.Enumerables;

namespace DataTemple.Matching
{
    /*
     * BestScore abstract the logic for finding the "best" of several items, each with a score
     * Use as follows:
     * 
     * BestScore<string> longestString = new BestScore<string>(0, "");
     * foreach (string item in collection)
     *     longestString.Improve(item.Length, item);
     * Console.WriteLine("The longest string is " + longestString.Payload);
     * 
     * BestScore also contains the static function LeftMatch.  LeftMatch takes two
     * collections and a comparison delegate and produces a best-pairs match between
     * them, with one pair for every element of the *first* (left) collection
     * (possibly pairing it to nothing, like a SQL left join).
     */
    public class BestScore<T>
    {
        protected double score;
        protected T payload;

        public BestScore()
        {
            score = 0;
            payload = default(T);
        }

        public BestScore(double lowest, T worst)
        {
            score = lowest;
            payload = worst;
        }

        public T Payload
        {
            get
            {
                return payload;
            }
        }

        public double Score
        {
            get
            {
                return score;
            }
        }

        public KeyValuePair<double, T> Pair
        {
            get
            {
                return new KeyValuePair<double, T>(score, payload);
            }
        }
        
        public bool Improve(double score, T payload)
        {
            if (score > this.score)
            {
                this.score = score;
                this.payload = payload;
                return true;
            }
            else
                return false;
        }

        public bool Improve(KeyValuePair<double, T> attempt)
        {
            return Improve(attempt.Key, attempt.Value);
        }

        /***** KeyValuePair List Search *****/

        public static KeyValuePair<T, double> FindHighestValueWise(IEnumerable<KeyValuePair<T, double>> lst, T def, double min) {
            KeyValuePair<T, double> best = new KeyValuePair<T, double>(def, min);
            foreach (KeyValuePair<T, double> kvp in lst)
                if (kvp.Value > best.Value)
                    best = kvp;
            return best;
        }

        public static KeyValuePair<double, T> FindHighestKeyWise(IEnumerable<KeyValuePair<double, T>> lst, T def, double min)
        {
            KeyValuePair<double, T> best = new KeyValuePair<double, T>(min, def);
            foreach (KeyValuePair<double, T> kvp in lst)
                if (kvp.Key > best.Key)
                    best = kvp;
            return best;
        }

        /***** Pairing Algorithm *****/

        public struct PairResult
        {
            public double score;
            public T one;
            public T two;

            public PairResult(double score, T one, T two)
            {
                this.score = score;
                this.one = one;
                this.two = two;
            }
        }

        public delegate double MatchScorer(T one, T two, object shared);

        // Find the best matches, ensuring one entry for every element in ones
        public static IEnumerable<PairResult> LeftMatch(IEnumerable<T> ones, IEnumerable<T> twos, MatchScorer scorer, object objshr)
        {
            MatchShared shared = new MatchShared();
            shared.objshr = objshr;
            shared.matches = new Memoizer<T, PairResult>(MatchOneRow);
            shared.comparisons = new Memoizer<TwoTuple<T, T>, double>(MatchComparison);
            shared.scorer = scorer;

            shared.ones = ones;
            shared.twos = twos;

            foreach (T one in ones)
                shared.matches.Evaluate(one, shared);

            return shared.matches.Results.Values;
        }

        // MatchShared is a class instead of a struct so it will be passed by reference
        protected class MatchShared
        {
            public object objshr;

            public Memoizer<T, PairResult> matches;
            public Memoizer<TwoTuple<T, T>, double> comparisons;
            public MatchScorer scorer;

            // these changes as other results wittle them down
            public IEnumerable<T> ones;
            public IEnumerable<T> twos;

            public MatchShared() { }
        }

        protected static double MatchComparison(TwoTuple<T, T> onetwo, object objshr)
        {
            MatchShared shared = (MatchShared)objshr;
            return shared.scorer(onetwo.one, onetwo.two, shared.objshr);
        }

        protected static PairResult MatchOneRow(T todo, object objshr)
        {
            MatchShared shared = (MatchShared)objshr;
            BestScore<T> one_to_all = new BestScore<T>();

            // start by looking at all results for one's row
            foreach (T two in shared.twos)
            {
                double score = shared.comparisons.Evaluate(new TwoTuple<T, T>(todo, two), shared);
                one_to_all.Improve(score, two);
            }

            if (one_to_all.payload == null || one_to_all.payload.Equals(default(T)))
            {
                // no more results!  Return empty match.
                shared.ones = new SkipEnumerable<T>(shared.ones, todo);
                return new PairResult(double.NegativeInfinity, todo, default(T));
            }

            // Now look at all other entries in that column
            BestScore<T> match_to_others = new BestScore<T>(one_to_all.score, todo);
            foreach (T one in shared.ones)
            {
                if (!one.Equals(todo))
                {
                    double score = shared.comparisons.Evaluate(new TwoTuple<T, T>(one, one_to_all.payload), shared);
                    match_to_others.Improve(score, one);
                }
            }

            if (match_to_others.payload.Equals(todo))
            {
                // remove this row and column
                shared.ones = new SkipEnumerable<T>(shared.ones, todo);
                shared.twos = new SkipEnumerable<T>(shared.twos, one_to_all.payload);
                return new PairResult(one_to_all.score, todo, one_to_all.payload);
            }
            else
            {
                // First do the row we found!
                shared.matches.Evaluate(match_to_others.payload, shared);
                // Then do this one again!
                return MatchOneRow(todo, shared);
            }
        }
    }
}
