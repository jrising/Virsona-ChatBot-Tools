using System;
using System.Collections.Generic;
namespace DataTemple
{
	public class SerialMatcher : TempleMatcher
	{
		protected List<TempleMatcher> matchers;
		
		public SerialMatcher(List<TempleMatcher> matchers)
		{
			this.matchers = matchers;
		}
		
		public override List<PatternMatch> AllMatches(AmbiguousPhrase subject, PatternMatch before)
		{
			return RecursiveMatch(subject, before, 0);
		}
			
		protected List<PatternMatch> RecursiveMatch(AmbiguousPhrase subject, PatternMatch before, uint index) {
			List<PatternMatch> matches = matchers[(int) index].AllMatches(subject, before);
			if (matches != null) {
				if (index == matchers.Count - 1)
					return matches;
				
				List<PatternMatch> allMatches = new List<PatternMatch>();
				foreach (PatternMatch match in matches) {
					List<PatternMatch> matchMatches = RecursiveMatch(subject, match, index+1);
					if (matchMatches != null)
						allMatches.AddRange(matchMatches);
				}
				
				return allMatches;
			}
			
			return null;
		}
	}
}

