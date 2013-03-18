using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase;
using PluggerBase.ActionReaction.Evaluations;
using DataTemple.AgentEvaluate;
// Just used for WordChoiceVariables
using LanguageNet.Grammarian;
using DataTemple.Matching;

namespace DataTemple.Variables
{
    public class ProgramVariables
    {
        public static void LoadVariables(Context env, double basesal, PluginEnvironment plugenv)
        {
            // @defin0 takes no arguments, but uses the context map of the caller
            env.Map.Add("@definz", new CallAgentWrapper(DefineInNoArgRule, ArgumentMode.DelimitedUnevaluated, basesal, 4, 10, basesal));
        	env.Map.Add("@defwc", new CallAgentWrapper(DefineWordChoiceVariable, ArgumentMode.DelimitedUnevaluated, basesal, 4, 10, false, plugenv));
        	env.Map.Add("@defvc", new CallAgentWrapper(DefineWordChoiceVariable, ArgumentMode.DelimitedUnevaluated, basesal, 4, 10, true, plugenv));
			env.Map.Add("@defpc", new CallAgentWrapper(DefinePhraseChoiceVariable, ArgumentMode.DelimitedUnevaluated, basesal, 4, 10, plugenv));
		}

        public static bool DefineInNoArgRule(Context context, IContinuation succ, IFailure fail, params object[] args)
        {
            double salience = (double) args[0];

            List<IContent> contents = context.Contents;
            string name = contents[0].Name;
            
            Context definition = context.ChildRange(1);
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
			bool isVerbChoice = (bool) args[0];
			PluginEnvironment plugenv = (PluginEnvironment) args[1];
            
			List<string> options = new List<string>();
			for (int ii = 1; ii < contents.Count; ii++)
				options.Add(contents[ii].Name.ToLower());
			
			if (isVerbChoice)
	            context.Map.Add(name, new VerbChoiceVariable(name, options, plugenv, (WordComparer) context.LookupSimple("$Compare")));
			else
	            context.Map.Add(name, new WordChoiceVariable(name, options, (WordComparer) context.LookupSimple("$Compare")));

            Context empty = new Context(context, new List<IContent>());
            succ.Continue(empty, fail);

			return true;
		}

		public static bool DefinePhraseChoiceVariable(Context context, IContinuation succ, IFailure fail, params object[] args) {
            List<IContent> contents = context.Contents;
            string name = contents[0].Name;
			PluginEnvironment plugenv = (PluginEnvironment) args[0];
            
			List<List<string>> options = new List<List<string>>();
			List<string> curropt = new List<string>();
			for (int ii = 1; ii < contents.Count; ii++) {
				if (contents[ii] == Special.ArgDelimSpecial) {
					options.Add(curropt);
					curropt = new List<string>();
				} else
					curropt.Add(contents[ii].Name.ToLower());
			}
			options.Add(curropt);
			
            context.Map.Add(name, new PhraseChoiceVariable(name, options, plugenv, (WordComparer) context.LookupSimple("$Compare")));

            Context empty = new Context(context, new List<IContent>());
            succ.Continue(empty, fail);

			return true;
		}
	}
	
    public class WordChoiceVariable : Variable
    {
		List<string> options;
		WordComparer comparer;
		
        public WordChoiceVariable(string name, List<string> options, WordComparer comparer)
            : base(name)
        {
			this.options = options;
			this.comparer = comparer;
        }

        public override bool IsMatch(IParsedPhrase check)
        {
			return comparer.MatchAny(check.Text, options);
        }
    }

    public class VerbChoiceVariable : ProgressiveVariableAgent
    {
		List<string> options;
		List<string> bases;
		Verbs verbs;
		WordComparer comparer;
		
        public VerbChoiceVariable(string name, List<string> options, PluginEnvironment plugenv, WordComparer comparer)
            : base(name, 100.0, new POSTagger(plugenv), new GrammarParser(plugenv))
        {
			this.options = options;
			this.verbs = new Verbs(plugenv);
			this.comparer = comparer;
			
			this.bases = new List<string>();
			foreach (string option in options)
				bases.Add(verbs.InputToBase(option));
        }

        public override bool? IsMatch(IParsedPhrase check)
        {
			string verb;
			if (check.IsLeaf || !check.Text.Contains(" ")) {
				verb = check.Text;
				
				if (Verbs.IsToHave(verb)) {
					if (bases.Contains("have"))
						return true;
					else
						return null;
				}
				if (Verbs.IsToBe(verb)) {
					if (bases.Contains("be"))
						return true;
					else
						return null;
				}
				if (Verbs.IsToDo(verb)) {
					if (bases.Contains("do"))
						return true;
					else
						return null;
				}
				if (verb == "will" || verb == "shall")
					return null; // not sure yet
			} else {
				GroupPhrase groupPhrase = new GroupPhrase(check);
				if (groupPhrase.Count > 2)
					return false;
				
				string helper = groupPhrase.GetBranch(0).Text.ToLower();
				verb = groupPhrase.GetBranch(1).Text.ToLower();
				
				if (!Verbs.IsToHave(helper) && !Verbs.IsToBe(helper) && !Verbs.IsToDo(helper) && 
				    helper != "will" && helper != "shall")
					return false;
			}
			
			string baseverb = verbs.InputToBase(verb);
			return (comparer.MatchAny(verb, options) || comparer.MatchAny(verb, bases) ||
			    comparer.MatchAny(baseverb, bases));
        }
    }

    public class PhraseChoiceVariable : ProgressiveVariableAgent
    {
		List<List<string>> options;
		WordComparer comparer;
		
        public PhraseChoiceVariable(string name, List<List<string>> options, PluginEnvironment plugenv, WordComparer comparer)
            : base(name, 100.0, new POSTagger(plugenv), new GrammarParser(plugenv))
        {
			this.options = options;
			this.comparer = comparer;
        }

        public override bool? IsMatch(IParsedPhrase check)
        {
			List<string> checks = StringUtilities.SplitWords(check.Text.ToLower(), true);
			foreach (List<string> option in options) {
				if (option.Count < checks.Count)
					continue;
				
				bool sofar = true;
				for (int ii = 0; ii < Math.Min(option.Count, checks.Count); ii++)
					if (!comparer.Match(checks[ii], option[ii])) {
						sofar = false;
						break;
					}
				
				if (sofar) {
					if (option.Count == checks.Count)
						return true;
					return null;
				}
			}
			
			return false;
        }
    }
}
