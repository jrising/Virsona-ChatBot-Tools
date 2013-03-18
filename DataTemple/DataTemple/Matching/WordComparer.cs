using System;
using System.Collections.Generic;

namespace DataTemple.Matching
{
	public class WordComparer
	{
		public WordComparer()
		{
		}

		public virtual bool Match(string input, string pattern) {
			return input.ToLower() == pattern.ToLower();
		}
		
		public virtual bool MatchAny(string input, List<string> options) {
			return options.Contains(input.ToLower());
		}
	}
}

