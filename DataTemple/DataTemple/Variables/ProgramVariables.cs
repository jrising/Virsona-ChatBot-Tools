using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.ActionReaction.Evaluations;
using DataTemple.AgentEvaluate;
// Just used for WordChoiceVariables
using LanguageNet.Grammarian;
using DataTemple.Matching;

namespace DataTemple.Variables
{
    public class ProgramVariables
    {
        public static void LoadVariables(Context env, double basesal)
        {
            // @defin0 takes no arguments, but uses the context map of the caller
            env.Map.Add("@definz", new CallAgentWrapper(DefineInNoArgRule, ArgumentMode.RemainderUnevaluated, basesal, 4, 10, basesal));
        	env.Map.Add("@defwc", new CallAgentWrapper(DefineWordChoiceVariable, ArgumentMode.RemainderUnevaluated, basesal, 4, 10));
		}

        public static bool DefineInNoArgRule(Context context, IContinuation succ, IFailure fail, params object[] args)
        {
            double salience = (double) args[0];

            List<IContent> contents = context.Contents;
            string name = contents[0].Name;
            
            Context definition = new Context(context, contents.GetRange(1, contents.Count - 1));

            context.Map.Add(name, new CallAgentWrapper(EvaluateDefinition, ArgumentMode.NoArugments, salience, definition.Size, 10, salience, definition));

            Context empty = new Context(context, new List<IContent>());
            succ.Continue(empty, fail);

            return true;
        }

        public static bool EvaluateDefinition(Context context, IContinuation succ, IFailure fail, params object[] args)
        {
            double salience = (double) args[0];
            Context definition = (Context)args[1];

            Evaluator eval = new Evaluator(salience, ArgumentMode.ManyArguments, succ, new NopCallable(), true);
            eval.Continue(definition, fail);

            return true;
        }
			
		public static bool DefineWordChoiceVariable(Context context, IContinuation succ, IFailure fail, params object[] args) {
            List<IContent> contents = context.Contents;
            string name = contents[0].Name;
            
			List<string> options = new List<string>();
			for (int ii = 1; ii < contents.Count; ii++)
				options.Add(contents[ii].Name);
			
            context.Map.Add(name, new WordChoiceVariable(name, options));

            Context empty = new Context(context, new List<IContent>());
            succ.Continue(empty, fail);

			return true;
		}
    }
	
    public class WordChoiceVariable : Variable
    {
		List<string> options;
		
        public WordChoiceVariable(string name, List<string> options)
            : base(name)
        {
			this.options = options;
        }

        public override bool IsMatch(IParsedPhrase check)
        {
            return options.Contains(check.Text);
        }
    }
}
