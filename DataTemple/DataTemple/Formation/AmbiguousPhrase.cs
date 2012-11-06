// Contains many parsings of a phrase, including the original text

using System;
using System.Collections.Generic;
using LanguageNet.Grammarian;

namespace DataTemple
{
	public class AmbiguousPhrase
	{
		protected string text;
		List<string> stringTokens;
		
		public AmbiguousPhrase(string text)
		{
			stringTokens = StringUtilities.SplitWords(text, true);
		}

		public AmbiguousPhrase(List<string> stringTokens)
		{
			this.stringTokens = stringTokens;
		}
		
		public string StringAt(AmbiguousPhrasePointer pointer) {
			int index = pointer.StringTokenIndex;
			
			if (index >= stringTokens.Count)
				return null;
			
			return stringTokens[index];
		}
		
		public AmbiguousPhrasePointer End {
			get {
				return new AmbiguousPhrasePointer(stringTokens.Count);
			}
		}
	}
}

