using System;
using System.Collections.Generic;

namespace DataTemple.Matching
{
	public class WordComparer
	{
		public WordComparer()
		{
		}

		public bool Match(string input, string pattern) {
			return input.ToLower() == pattern.ToLower();
		}
		
		public bool MatchAny(string input, List<string> options) {
			return options.Contains(input.ToLower());
		}
	}
}

