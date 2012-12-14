/******************************************************************\
 *      Class Name:     ContinueToCallAgent
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Upon continuation, codelet added, which upon evaluation calls
 * the given CallAgent.
\******************************************************************/
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

        public override bool Evaluate()
        {
            bool done = agent.Call(context, succ, fail);
            return done;
        }
        
        public static ContinueToCallAgent Instantiate(CallAgent agent, Context context, IContinuation succ, IFailure fail)
        {
            ContinueToCallAgent continuer = new ContinueToCallAgent(agent, succ);
            continuer.SetResult(new TwoTuple<Context, IFailure>(context, fail), context.Weight, "ContinueToCallAgent: Instantiate");

            return continuer;
        }
    }
}
