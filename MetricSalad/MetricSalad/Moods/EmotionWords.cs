using System;
using System.Collections.Generic;
using System.IO;

namespace MetricSalad
{
	public class EmotionWords
	{
		Dictionary<string, string[]> equivalents;
		
		public EmotionWords(string filename)
		{
			StreamReader file = new StreamReader(filename);
			equivalents = ReadEquivalents(file.ReadToEnd());
		}
		
		public static Dictionary<string, string[]> ReadEquivalents(string text) {
			Dictionary<string, string[]> equivalence = new Dictionary<string, string[]>();
			
			string[] lines = text.Split('\n');
		    foreach (string line in lines) {
				if (line.Trim() == "")
					continue;
				
				string[] words = line.Split(' ');
				string[] moods = new string[words.Length-1];
				Array.Copy(words, 1, moods, 0, words.Length-1);
				equivalence.Add(words[0], moods);
			}
			
			return equivalence;
		}
		
		public Dictionary<string, string[]> Equivalents {
			get {
				return equivalents;
			}
		}
	}
}

