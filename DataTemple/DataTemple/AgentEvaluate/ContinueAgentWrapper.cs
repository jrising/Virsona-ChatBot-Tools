using System;
using PluggerBase.ActionReaction.Evaluations;

namespace DataTemple.AgentEvaluate
{
	public class ContinueAgentWrapper : IContinuation
	{
        public delegate void Agentlet(Context context, IContinuation succ, IFailure fail, params object[] args);
		
        protected Agentlet agentlet;
		protected IContinuation succ;
        protected object[] args;

        public ContinueAgentWrapper(Agentlet agentlet, IContinuation succ, params object[] args)
        {
            this.agentlet = agentlet;
			this.succ = succ;
            this.args = args;
        }
		
		public int Continue(object value, IFailure fail)
		{
			agentlet((Context) value, succ, fail, args);
			return 1;
		}
		
		public object Clone()
		{
			return new ContinueAgentWrapper(agentlet, succ, args);
		}
	}
}

