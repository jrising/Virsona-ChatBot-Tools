using System;
using System.Collections.Generic;
using LanguageNet.Grammarian;
using PluggerBase;
using PluggerBase.ActionReaction.Evaluations;
using PluggerBase.ActionReaction.Interfaces;
using PluggerBase.ActionReaction.Actions;

namespace DataTemple
{
	public class TempleMatcher
	{
		public static IArgumentType TempleMatcherResultType =
			new TypedArgumentType(typeof(TempleMatcher), new StringTokenMatcher("bufallo", false));

		public TempleMatcher()
		{
		}

		public static TempleMatcher Interpret(PluginEnvironment plugenv, string template)
		{
			List<string> tokens = StringUtilities.SplitWords(template, true);

			List<TempleMatcher> matchers = new List<TempleMatcher>();
			matchers.Add(new AnyStartMatcher());
			foreach (string token in tokens) {
				object value = plugenv.ImmediateConvertTo(token, TempleMatcherResultType, 10, 1000);
				if (value == null || !(value is TempleMatcher))
					matchers.Add(new StringTokenMatcher(token, false));
				else
					matchers.Add((TempleMatcher) value);
			}
			//matchers.Add(new AnyEndMatcher());

			return new SerialMatcher(matchers);
		}

		public PatternMatch Match(AmbiguousPhrase phrase, IContinuation succ, IFailure fail) {
			//IArena arena = new QueueArena();
			//arena.Call();
			
			return null;
		}
		
		public List<PatternMatch> AllMatches(AmbiguousPhrase subject) {
			PatternMatch before = new PatternMatch();
			return AllMatches(subject, before);	
		}
		
		public List<PatternMatch> AllMatches(AmbiguousPhrase subject, List<PatternMatch> before) {
			List<PatternMatch> after = new List<PatternMatch>();
			foreach (PatternMatch beforeMatch in before) {
				List<PatternMatch> afterMatch = AllMatches(subject, beforeMatch);
				if (afterMatch != null)
					after.AddRange(afterMatch);
			}
			
			return after;
		}

		public virtual List<PatternMatch> AllMatches(AmbiguousPhrase subject, PatternMatch before) {
			return null;
		}
	}
}

