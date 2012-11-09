using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.ActionReaction.Evaluations;
using GenericTools;

namespace DataTemple.AgentEvaluate
{
    public class ContinueToCallAgent : ContinueAgentCodelet
    {
        protected CallAgent agent;

        public ContinueToCallAgent(CallAgent agent, IContinuation succ)
            : base(agent.Salience, 2 * 4 + agent.Space, agent.Time, succ)
        {
            this.agent = agent;
        }

        public override int Evaluate()
        {
            int used = agent.Call(context, succ, fail);
            AdjustTime(used - time);
            return used;
        }
        
        public static ContinueToCallAgent Instantiate(CallAgent agent, Context context, IContinuation succ, IFailure fail)
        {
            ContinueToCallAgent continuer = new ContinueToCallAgent(agent, succ);
            continuer.SetResult(new TwoTuple<Context, IFailure>(context, fail), context.Weight, "ContinueToCallAgent: Instantiate");

            return continuer;
        }
    }
}
