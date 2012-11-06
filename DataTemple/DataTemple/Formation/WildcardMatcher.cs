using System;
using System.Text.RegularExpressions;

namespace DataTemple
{
	public class WildcardMatcher
	{
		protected int min;
		protected int max;
		protected string name;
		
		protected static Regex star = new Regex(@"^\*:?(\w*)$", RegexOptions.Compiled);
		protected static Regex plus = new Regex(@"^+:?(\w*)$", RegexOptions.Compiled);
		protected static Regex sing = new Regex(@"^@:?(\w*)$", RegexOptions.Compiled);
		
		public WildcardMatcher(int min, int max, string name)
		{
			this.min = min;
			this.max = max;
			this.name = name;
		}
		
		public static WildcardMatcher Interpret(string item) {
			if (item.StartsWith("*")) {
				MatchCollection matches = star.Matches(item);
				string name = matches[0].Groups[0].Value;
				if (name.Length == 0)
					name = "*";
				return new WildcardMatcher(0, -1, name);
			}
			
			if (item.StartsWith("+")) {
				MatchCollection matches = plus.Matches(item);
				string name = matches[0].Groups[0].Value;
				if (name.Length == 0)
					name = "+";
				return new WildcardMatcher(1, -1, name);
			}
	
			if (item.StartsWith("@")) {
				MatchCollection matches = plus.Matches(item);
				string name = matches[0].Groups[0].Value;
				if (name.Length == 0)
					name = "@";
				return new WildcardMatcher(1, 1, name);
			}
			
			return null;
		}
		
		
	}
}

