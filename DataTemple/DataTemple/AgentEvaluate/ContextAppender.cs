using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.ActionReaction.Evaluations;

namespace DataTemple.AgentEvaluate
{
    public class ContextAppender : ContinueAgentCodelet
    {
        protected Context before;
        protected int? insert;
		protected string debugName;

        public ContextAppender(double salience, Context before, int? insert, IContinuation succ)
            : base(salience, 2 * 4, 10, succ)
        {
            this.before = before;
            this.insert = insert;
			this.debugName = "";
        }
		
		public string DebugName {
			get {
				return debugName;
			}
			set {
				debugName = value;
			}
		}
		
        public override int Evaluate()
        {
            List<IContent> beforeContentsCopy = new List<IContent>(before.Contents);
			if (insert.HasValue) {
	            if (insert >= 0)
	            {
	                // Replace the consumed region
	                beforeContentsCopy.RemoveRange(insert.Value, beforeContentsCopy.Count - insert.Value);
	                beforeContentsCopy.AddRange(context.Contents);
	            }
	            else
	            {
	                // Remove a certain number from the beginning
	                beforeContentsCopy.RemoveRange(0, -insert.Value);
	                // put in the new context before the old
	                beforeContentsCopy.InsertRange(0, context.Contents);
	            }
			}
			
            Context result = new Context(before, beforeContentsCopy, before.Weight * context.Weight);
            result.AddMappings(context);
			
            succ.Continue(result, fail);
            return time;
        }
    }
}
