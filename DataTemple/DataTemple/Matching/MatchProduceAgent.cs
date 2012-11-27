/******************************************************************\
 *      Class Name:     MatchProduceAgent
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * A CallAgent which upon calling, calls a match function with
 * a value previously added to the context.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using LanguageNet.Grammarian;
using DataTemple.AgentEvaluate;
using PluggerBase.ActionReaction.Evaluations;

namespace DataTemple.Matching
{
    public class MatchProduceAgent : CallAgent
    {
        protected POSTagger tagger;
		protected GrammarParser parser;
		protected bool breakpointCall;

        public MatchProduceAgent(ArgumentMode argmode, double salience, int space, int time, POSTagger tagger, GrammarParser parser)
            : base(argmode, salience, space, time)
        {
            this.tagger = tagger;
			this.parser = parser;
			this.breakpointCall = false;
        }
		
		public bool BreakpointCall {
			set {
				breakpointCall = value;
			}
		}

        public override int Call(object value, IContinuation succ, IFailure fail)
        {
			if (breakpointCall)
				Console.WriteLine("Breakpoint in MatchProduceAgent");
			Context context = (Context) value;
            bool production = context.LookupDefaulted<bool>("$production", false);
            if (!production)
            {
                object check = context.LookupDefaulted<object>("$check", null);

                if (check == null)
                {
                    List<IContent> contents = new List<IContent>();
                    Context child = new Context(context, contents);
                    // Put us into content stream, for matcher to find
                    contents.Add(new Value(this));
                    // Save this context-- we'll use it later!
                    child.Map["$argctx"] = context;
                    
                    succ.Continue(child, fail);
                    return 4;
                }

                Context argctx = context.LookupDefaulted<Context>("$argctx", context);

                return Match(check, argctx, succ, fail);
            }
            else
                return Produce(context, succ, fail);
        }

        public virtual int Match(object check, Context context, IContinuation succ, IFailure fail)
        {
            return time;
        }

        public virtual int Produce(Context context, IContinuation succ, IFailure fail)
        {
            return time;
        }

        // This will call success either way, but only propogate if context is empty
        public int PropogateOnClear(Context context, IContinuation succ, IFailure fail, params object[] args)
        {
            if (context.IsEmpty || (context.Contents.Count == 1 && context.Contents[0].Name.StartsWith("*")))
            {
                string name = (string) args[0];
                object check = args[1];

                Variable dummy = new Variable(name);
                dummy.Propogate(context, check, context.Weight);
            }

            succ.Continue(context, fail);

            return 10;
        }

        // Called by various Produce functions
        public int ProducePropogated(Context env, string name)
        {
            IParsedPhrase propogated = GetPropogated(env, name);
            if (propogated != null)
                env.Contents.Add(new Value(propogated));

            return 10;
        }

        public IParsedPhrase GetPropogated(Context env, string name)
        {
            Variable dummy = new Variable(name);
            return dummy.Produce(env, tagger, parser);
        }
    }
}
