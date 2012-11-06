// A collection of dictionaries, starts and ends

using System;
using System.Collections.Generic;
using System.IO;

namespace DataTemple
{
	public class PatternMatch
	{
		protected Dictionary<string, string> dictionary;
		AmbiguousPhrasePointer start;
		AmbiguousPhrasePointer end;
		
		public PatternMatch()
		{
			dictionary = new Dictionary<string, string>();
			start = new AmbiguousPhrasePointer(0);
			end = new AmbiguousPhrasePointer(0);
		}
		
		public PatternMatch(AmbiguousPhrasePointer startend) {
			dictionary = new Dictionary<string, string>();
			start = end = startend;
		}
		
		public Dictionary<string, string> Dictionary {
			get {
				return dictionary;
			}
		}
		
		public AmbiguousPhrasePointer Start {
			get {
				return start;
			}
		}

		public AmbiguousPhrasePointer End {
			get {
				return end;
			}
		}
		
		public PatternMatch CloneWithEnd(AmbiguousPhrasePointer end) {
			PatternMatch match = new PatternMatch();
			match.dictionary = new Dictionary<string, string>(dictionary);
			match.start = start;
			match.end = end;
			
			return match;
		}
		
		public PatternMatch MatchExtendedBy(int words) {
			return CloneWithEnd(end.PointerFurtheredBy(words));
		}
		
		public void Display(TextWriter console) {
			console.WriteLine("[{0:D} - {1:D}]: ", start.StringTokenIndex, end.StringTokenIndex);
			foreach (KeyValuePair<string, string> kvp in dictionary)
				console.WriteLine("  " + kvp.Key + " -> " + kvp.Value);
		}
	}
}

