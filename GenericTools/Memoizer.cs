/******************************************************************\
 *      Class Name:     Memoizer
 *      Written By:     James.R
 *      Copyright:      Virsona Inc
 *      -----------------------------------------------------------
 * Remember past results of a function, and return them immediately
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace GenericTools
{
    /*
     * The memoizer wraps a function to store its results and return them on subsequent calls
     */
    public class Memoizer<TInput, TOutput>
    {
        public delegate TOutput Evaluator(TInput input, object shared);

        protected Dictionary<TInput, TOutput> results;
        protected bool hasnull;
        protected TOutput nullresult;
        protected Evaluator evaluator;

        public Memoizer(Evaluator evaluator)
        {
            this.results = new Dictionary<TInput, TOutput>();
            hasnull = false;
            this.evaluator = evaluator;
        }

        public Dictionary<TInput, TOutput> Results
        {
            get
            {
                return results;
            }
        }

        public TOutput Evaluate(TInput input, object shared)
        {
            if (input == null)
            {
                if (!hasnull)
                {
                    nullresult = evaluator(input, shared);
                    hasnull = true;
                }

                return nullresult;
            }
            
            TOutput output;
            if (results.TryGetValue(input, out output))
                return output;

            output = evaluator(input, shared);
            results.Add(input, output);
            return output;
        }
    }
}
