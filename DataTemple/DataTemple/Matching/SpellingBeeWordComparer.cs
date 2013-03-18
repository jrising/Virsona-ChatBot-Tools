using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SpellingBee;

namespace DataTemple.Matching
{
	public class SpellingBeeWordComparer : WordComparer
	{
		protected SpellingSuggestionsHandler handler;
		protected Dictionary<string, List<string>> cache;
		
		
		public SpellingBeeWordComparer(string datadir)
		{
			cache = new Dictionary<string, List<string>>();
			handler = new SpellingSuggestionsHandler();
			handler.Prepare(datadir);
		}
		
		public override bool Match(string input, string pattern) {
			if (base.Match(input, pattern))
				return true;
			
			List<string> corrects = GetCorrects(input);
			foreach (string correct in corrects)
				if (correct == pattern.ToLower())
					return true;
			
			return false;
		}
		
		public override bool MatchAny(string input, List<string> options) {
			if (base.MatchAny(input, options))
				return true;
						
			List<string> corrects = GetCorrects(input);
			foreach (string correct in corrects)
				if (options.Contains(correct))
					return true;
			
			return false;
		}

		public List<string> GetCorrects(string input) {
			List<string> corrects;
			if (!cache.TryGetValue(input, out corrects)) {
				Regex r = new Regex("^[^a-zA-Z0-9]+$");
				if (r.IsMatch(input)) {
					List<string> samecorrects = new List<string>();
					samecorrects.Add(input);

					corrects = samecorrects;
				} else {
					IEnumerable<string> unnormcorrects = handler.Handle(input);
					List<string> lowercorrects = new List<string>();
					foreach (string correct in unnormcorrects)
						lowercorrects.Add(correct.ToLower());

					corrects = lowercorrects;
				}
				
				cache.Add(input, corrects);
			}
			
			return corrects;
		}
	}
}

