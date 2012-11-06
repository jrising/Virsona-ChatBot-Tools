/******************************************************************\
 *      Class Name:     ContinuationAppender
 *      Written By:     James.R
 *      Copyright:      Virsona Inc
 *      -----------------------------------------------------------
 * Expects to be called twice, at which point it will call its succ
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.ActionReaction.Evaluations;
using DataTemple.Codeland;
using DataTemple.Matching;

namespace DataTemple.AgentEvaluate
{
    public class ContinuationAppender : IContinuation
    {
        protected Context master;
        protected IContinuation succ;

        protected List<string> callers;
        protected List<Context> completes;

        public ContinuationAppender(Context master, IContinuation succ)
        {
            this.master = master;
            this.succ = succ;

            callers = new List<string>();
            completes = new List<Context>();
        }

        public void RegisterCaller(string caller)
        {
            callers.Add(caller);
        }

        #region IContinuation Members

        public int Continue(object value, IFailure fail)
        {
			Context context = (Context) value;
            completes.Add(context);

            if (completes.Count < callers.Count)
                return 1; // still waiting!
            if (completes.Count > callers.Count)
            {
                // remove the closest complete to the most recent
                completes.RemoveAt(completes.Count - 2);
            }

            List<int> completeIndices = new List<int>();
            for (int ii = 0; ii < completes.Count; ii++)
                completeIndices.Add(ii);
            List<int> callerIndices = new List<int>(completeIndices);

            IEnumerable<BestScore<int>.PairResult> pairresults = BestScore<int>.LeftMatch(completeIndices, callerIndices, MatchCompleteCaller, null);
            
            List<Context> unusedCompletes = new List<Context>();
            foreach (BestScore<int>.PairResult pairresult in pairresults)
                if (pairresult.score < 0)
                    unusedCompletes.Add(completes[pairresult.one]);

            // For each caller, find the context that's closest
            List<Context> ordered = new List<Context>();

            for (int ii = 0; ii < callers.Count; ii++) {
                // find the matching complete
                bool found = false;
                foreach (BestScore<int>.PairResult pairresult in pairresults)
                    if (pairresult.score > 0 && pairresult.two == ii)
                    {
                        ordered.Add(completes[pairresult.one]);
                        found = true;
                    }

                if (!found)
                {
                    // use the first unused
                    ordered.Add(unusedCompletes[0]);
                    unusedCompletes.RemoveAt(0);
                }
            }

            Context result = new Context(master, new List<IContent>());
            foreach (Context complete in ordered)
            {
                result.Contents.AddRange(complete.Contents);
                result.AddMappings(complete);
                result.Weight *= complete.Weight;
            }

            return succ.Continue(result, fail) + 10;
        }

        protected double MatchCompleteCaller(int completeIndex, int callerIndex, object shared)
        {
            List<Codelet> sequence = completes[completeIndex].FullSequence;

            for (int kk = sequence.Count - 1; kk >= 0; kk--)
            {
                if (!(sequence[kk] is ContinueAgentCodelet))
                    continue;

                if (((ContinueAgentCodelet)sequence[kk]).Lineage == callers[callerIndex])
                    return 1.0 / (sequence.Count - kk);
            }

            return 0;   // no match
        }

        #endregion
		
		public object Clone()
		{
			ContinuationAppender appender = new ContinuationAppender(master, succ);
			appender.callers = callers;
			appender.completes = completes;
			
			return appender;
		}
    }
}
