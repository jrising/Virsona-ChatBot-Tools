using System;
using System.Collections.Generic;
namespace DataTemple
{
	public class AnyStartMatcher : TempleMatcher
	{
		public AnyStartMatcher()
		{
		}
		
		public override List<PatternMatch> AllMatches(AmbiguousPhrase subject, PatternMatch before)
		{
			List<PatternMatch> matches = new List<PatternMatch>();
			
			PatternMatch attempt = before;
			while (!attempt.End.Equals(subject.End)) {
				matches.Add(attempt);
				AmbiguousPhrasePointer next = attempt.Start.PointerFurtheredBy(1);
				attempt = new PatternMatch(next);
			}
			
			return matches;
		}
	}
}

