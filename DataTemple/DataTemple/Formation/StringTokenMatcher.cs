using System;
using System.Collections.Generic;

namespace DataTemple
{
	public class StringTokenMatcher : TempleMatcher
	{
		protected string pattern;
		protected bool caseSensitive;
		
		public StringTokenMatcher(string pattern, bool caseSensitive)
		{
			this.caseSensitive = caseSensitive;
			if (caseSensitive)
				this.pattern = pattern;
			else
				this.pattern = pattern.ToLower();
		}
		
		public override List<PatternMatch> AllMatches(AmbiguousPhrase subject, PatternMatch before) {
			List<PatternMatch> result = new List<PatternMatch>();
			if (caseSensitive) {
				if (pattern == subject.StringAt(before.End))
					result.Add(before.MatchExtendedBy(1));
			} else
				if (pattern == subject.StringAt(before.End).ToLower())
					result.Add(before.MatchExtendedBy(1));
			
			return result;
		}
		
		
	}
}

