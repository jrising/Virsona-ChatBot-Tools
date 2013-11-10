using System;
using PluggerBase.ActionReaction.Evaluations;

namespace DataTemple.AgentEvaluate
{
	public class EvaluateAll : ContinueAgentCodelet
	{
        protected ArgumentMode argumentMode;
		protected bool isUserInput;

		public EvaluateAll(double salience, ArgumentMode argumentMode, IContinuation succ, bool isUserInput)
            : base(salience, 2 * 4, 100, succ)
        {
            this.argumentMode = argumentMode;
			this.isUserInput = isUserInput;
		}
		
        public override bool Evaluate()
        {
			if (context.Contents.Count == 0)
				return succ.Continue(context, fail);
			
			ContinuationAppender appender = new ContinuationAppender(context, succ);
			EvaluateAll evalall = new EvaluateAll(salience, argumentMode, appender.AsIndex(1), isUserInput);
			Evaluator eval = new Evaluator(salience, argumentMode, appender.AsIndex(0), evalall, isUserInput);
			                               
            return eval.Continue(context, fail);
		}
	}
}

