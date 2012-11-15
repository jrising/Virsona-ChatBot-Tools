using System;
using System.Collections.Generic;
using LanguageNet.Grammarian;

namespace MetricSalad
{
	public class LexicalHistogram
	{
		protected Dictionary<string, double> freqs;
		protected double total;
		
		public LexicalHistogram()
		{
			freqs = new Dictionary<string, double>();
			total = 0;
		}
		
		public void AddTerm(string word) {
			double freq;
			if (freqs.TryGetValue(word, out freq))
				freqs[word] = freq + 1;
			else
				freqs[word] = 1;
			total++;
		}
		
		// This is a poor measure of lexical diversity, comparable only for documents of similar size
		public double LexicalDiversity() {
			return freqs.Count / total;
		}
	}
}

