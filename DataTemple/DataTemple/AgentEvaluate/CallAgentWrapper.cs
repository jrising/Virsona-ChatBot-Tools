using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.ActionReaction.Evaluations;

namespace DataTemple.AgentEvaluate
{
    public class CallAgentWrapper : CallAgent
    {
        public delegate bool Agentlet(Context context, IContinuation succ, IFailure fail, params object[] args);

        protected Agentlet agentlet;
        protected ArgumentMode argmode;
        protected object[] args;

        public CallAgentWrapper(Agentlet agentlet, ArgumentMode argmode, double salience, int space, int time, params object[] args)
            : base(argmode, salience, space, time)
        {
            this.agentlet = agentlet;
            this.argmode = argmode;
            this.args = args;
        }

        public override bool Call(object value, IContinuation succ, IFailure fail)
        {
			Context context = (Context) value;

            return agentlet(context, succ, fail, args);
        }

        public static ContinueToCallAgent Instantiate(Agentlet agentlet, ArgumentMode argmode, Context context, IContinuation succ, IFailure fail, double salience, int space, int time, params object[] args)
        {
            CallAgentWrapper wrapper = new CallAgentWrapper(agentlet, argmode, salience, space, time, args);
            return ContinueToCallAgent.Instantiate(wrapper, context, succ, fail);
        }

        public static ContinueToCallAgent MakeContinuation(Agentlet agentlet, IContinuation succ, double salience, int space, int time, params object[] args)
        {
            CallAgentWrapper wrapper = new CallAgentWrapper(agentlet, ArgumentMode.ManyArguments, salience, space, time, args);
            return new ContinueToCallAgent(wrapper, succ);
        }
    }
}
