using System;
using System.Collections.Generic;
using PluggerBase.ActionReaction.Evaluations;
using LanguageNet.Grammarian;
using DataTemple.Matching;
using DataTemple.Codeland;

namespace DataTemple
{
	public class CorrectSpellingsRescueMatch :  TryToRescueMatch
	{
		TryToRescueMatch fallback;
		SpellingBeeWordComparer comparer;
		GrammarParser parser;
		double weight;
		
		public CorrectSpellingsRescueMatch(TryToRescueMatch fallback, SpellingBeeWordComparer comparer, GrammarParser parser, double weight)
		{
			this.fallback = fallback;
			this.comparer = comparer;
			this.parser = parser;
			this.weight = weight;
		}
		
		public override bool CallRescue(Coderack coderack, IParsedPhrase input, PatternTemplateSource patternTemplateSource, string reason, IContinuation skip, IContinuation succ, IFailure fail) {
			List<string> words = GroupPhrase.PhraseToTexts(input);

			bool changed = false;
			List<string> corrected = new List<string>();
			foreach (string word in words) {
				string correct = comparer.GetCorrects(word)[0];
				if (correct.ToLower() != word.ToLower())
					changed = true;
				corrected.Add(correct);
			}
			
			if (changed) {
				IParsedPhrase correct = parser.Parse(StringUtilities.JoinWords(corrected));
				IFailure fallfail = fallback.MakeFailure(input, patternTemplateSource, succ, fail, coderack);
				patternTemplateSource.Generate(coderack, correct, succ, fallfail, weight);
				return true;
			} else
				return fallback.CallRescue(coderack, input, patternTemplateSource, reason, skip, succ, fail);
		}
	}
}

