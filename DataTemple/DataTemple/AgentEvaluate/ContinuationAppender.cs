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
using ExamineTools;
using DataTemple.Codeland;
//using DataTemple.Matching;

namespace DataTemple.AgentEvaluate
{
    public class ContinuationAppender
    {
        protected Context master;
        protected IContinuation succ;
		protected uint indexes;
		
        protected List<uint> callers;
        protected List<Context> completes;

        public ContinuationAppender(Context master, IContinuation succ)
        {
            this.master = master;
            this.succ = succ;
			indexes = 0;
			
			// filled upon continuation
            callers = new List<uint>();
            completes = new List<Context>();
        }
		
		public bool ContinueWithIndex(uint index, object value, IFailure fail) {
			callers.Add(index);
			Context context = (Context) value;
            completes.Add(context);
			
			// Do we have the last element for each index
            List<Context> ordered = new List<Context>();

			for (uint ii = 0; ii <= indexes; ii++) {
				for (int jj = callers.Count - 1; jj >= 0; jj--)
					if (callers[jj] == ii) {
						ordered.Add(completes[jj]);
						break;
					}
				
				if (ordered.Count == ii)
					return true; // still waiting!
			}

            Context result = new Context(master, new List<IContent>());
            foreach (Context complete in ordered)
            {
                result.Contents.AddRange(complete.Contents);
                result.AddMappings(complete);
                result.Weight *= complete.Weight;
            }

            return succ.Continue(result, fail);
        }
		
		public IContinuation AsIndex(uint index) {
			indexes = Math.Max(index, indexes);
			return new IndexedContinuationAppender(this, index);
		}

		public object Clone()
		{
			ContinuationAppender appender = new ContinuationAppender(master, succ);
			appender.callers = callers;
			appender.completes = completes;
			
			return appender;
		}
		
		// ExamineTools.ITraceEvaluation
		
		public IContinuation Success {
			get {
				return succ;
			}
		}
		
		public IFailure Failure {
			get {
				return null;
			}
		}
		
		public string TraceTitle {
			get {
				return "ContinuationAppender";
			}
		}
		
		protected class IndexedContinuationAppender : IContinuation {
			protected ContinuationAppender appender;
			protected uint index;
			
			public IndexedContinuationAppender(ContinuationAppender appender, uint index) {
				this.appender = appender;
				this.index = index;
			}
			
			public bool Continue(object value, IFailure fail)
			{
				return appender.ContinueWithIndex(index, value, fail);
			}
			
			public object Clone()
			{
				return new IndexedContinuationAppender(appender, index);
			}
		}
    }
}
