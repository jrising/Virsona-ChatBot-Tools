using System;
using System.Collections.Generic;
using LanguageNet.Grammarian;
using GenericTools;

namespace MetricSalad
{
	public class FrequencyTools
	{
		public static TwoTuple<int, int> WordCount(string find, string text) {
			find = find.ToLower();
			text = text.ToLower();
			List<string> words = StringUtilities.SplitWords(text, false);
			int count = 0;
			foreach (string word in words)
				if (word == find)
					count++;
			
			return new TwoTuple<int, int>(count, words.Count);
		}
		
		public static double StemFrequency(string stem, string text) {
			List<string> words = StringUtilities.SplitWords(text, false);
			StemmerInterface stemmer = new PorterStemmer();
			
			uint count = 0;
			foreach (string word in words) {
				if (stem == stemmer.stemTerm(word))
					count++;
			}
			
			return count / (double) words.Count;
		}
	}
}

