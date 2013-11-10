using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using LanguageNet.Grammarian;

namespace MetricSalad
{
	public class TwitterUtilities
	{
		public static List<string> SplitWords(string line)
		{
			line = Regex.Replace(line, "http://[^\\s]+\\s", "");

			List<string> words = StringUtilities.SplitWords(line, true);
			
			List<string> combos = new List<string>();
			string combo = null;
			foreach (string word in words) {
				if (word == "@" || word == "#") {
					combo = word;
					continue;
				}
				
				if ((word == " @" || word == " #") && combos.Count == 0) {
					combo = word.Substring(1);
					continue;
				}
				
				if (word == " " && combo != null)
					continue;
				
				if ((word == " '" || word == " _") && combos.Count > 0) {
					if (combo == null) {
						combo = combos[combos.Count - 1] + word.Substring(1);
						combos.RemoveAt(combos.Count - 1);
					} else
						combo += word.Substring(1);
						
					continue;
				}

				if (combo != null)
					combo += word;
				else
					combo = word;
									
				combos.Add(combo);
				combo = null;
			}
			
			if (combo != null)
				combos.Add(combo);
			
			return combos;
		}
	}
}

