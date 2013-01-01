using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.ActionReaction.Evaluations;
using LanguageNet.Grammarian;
using DataTemple.AgentEvaluate;
using GenericTools;

namespace DataTemple.Matching
{
    public class ProgressiveVariableAgent : MatchProduceAgent
    {
		protected string name;
		
        public ProgressiveVariableAgent(string name, double salience, POSTagger tagger, GrammarParser parser)
            : base(ArgumentMode.NoArugments, salience, 10, 10, tagger, parser)
        {
			this.name = name;
        }
	
        public override bool Match(object check, Context context, IContinuation succ, IFailure fail)
        {
			if (!(check is IParsedPhrase)) {
				fail.Fail("Cannot match a " + check.GetType(), succ);
				return true;
			}
			
			// Set up main check
			IParsedPhrase full = (IParsedPhrase) check;
			
			GroupPhrase sofar = context.LookupDefaulted<GroupPhrase>("$active$" + name, null);
			if (sofar != null)
				full = sofar.AddBranch(full);
				
			bool? isMatch = IsMatch(full);

			if (!isMatch.HasValue) {
				List<IContent> contents = new List<IContent>();
				contents.Add(new Value(this));
				Context tryagain = new Context(context, contents);
				tryagain.Map["$active$" + name] = new GroupPhrase(full);
				// Continue with same context
				succ.Continue(tryagain, fail);
			} else {
				if (isMatch.Value) {
	                Propogate(context, full, 1.0);
					context.Map[StarUtilities.NextStarName(context, name)] = full.Text;
					
					succ.Continue(context.ChildRange(1), fail);
				} else {
					fail.Fail("Does not match " + full.Text, succ);
				}
            }
			
			return true;
		}

        public virtual void Propogate(Context env, object matched, double strength)
        {
            //object myMatched = env.LookupDefaulted<object>("$p$" + name, null);
            double myStrength = env.LookupDefaulted<double>("$s$" + name, 0.0);

            if (myStrength < strength)
            {
                env.Map["$p$" + name] = matched;
                env.Map["$s$" + name] = strength;
            }
        }

        public virtual bool? IsMatch(IParsedPhrase check)
        {
            return false;
        }

		public override bool Produce(Context context, IContinuation succ, IFailure fail)
        {
            object var = context.LookupDefaulted<object>("$p$" + name, null);

            if (var is IParsedPhrase)
                succ.Continue((IParsedPhrase)var, fail);
			
			succ.Continue(null, fail);
			
			return true;
        }
    }
}
