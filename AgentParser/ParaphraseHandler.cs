/******************************************************************\
 *      Class Name:     ParaphraseHandler
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Exposes a handler for paraphrasing phrases.
\******************************************************************/
using System;
using System.Collections.Generic;
using PluggerBase;
using ActionReaction;
using ActionReaction.Actions;
using ActionReaction.Interfaces;
using LanguageNet.Grammarian;

namespace LanguageNet.AgentParser
{
	public class ParaphraseHandler : ArgumentsHandler
	{
		protected Nouns nouns;
		protected Verbs verbs;
		protected WordNetAccess wordnet;

        public ParaphraseHandler(PluginEnvironment plugenv)
            : base("Paraphrase an IParsedPhrase",
                   "Construct a new IParsedPhrase which means roughly the same thing.",
			       new string[] { "input", "prob", "opts", "emph" },
				   new string[] { "Phrase Input", "Paraphrase Improbability", "Options", "Words to Emphasize" },
				   new IArgumentType[] { GrammarParser.GrammarParseResultType,
										 new RangedArgumentType<double>(0, 1.0, .75),
										 new SelectableArgumentType(new object[] { GrammarParser.ParaphraseOptions.NoOptions, GrammarParser.ParaphraseOptions.MoveToStart, GrammarParser.ParaphraseOptions.MoveOffStart, GrammarParser.ParaphraseOptions.IsStayingStart }),
										 new EnumerableArgumentType(int.MaxValue, new StringArgumentType(4, ".+", "buffalo")) },
				   new string[] { null, null, null, null },
				   new bool[] { true, true, false, false },
                   LanguageNet.Grammarian.GrammarParser.ParaphrasingResultType, 120)
        {
			nouns = new Nouns(plugenv);
			verbs = new Verbs(plugenv);
			wordnet = new WordNetAccess(plugenv);
        }

        public Phrase Handle(Phrase input, GrammarParser.ParaphraseOptions? opts, List<string> emph, double prob)
        {
			if (opts == null)
				opts = GrammarParser.ParaphraseOptions.NoOptions;

			List<Phrase> emphasizes = new List<Phrase>();
			if (emph != null) {
				foreach (string word in emph)
					emphasizes.AddRange(input.FindPhrases(word));
			}

			return input.Parapharse(verbs, nouns, wordnet, opts.Value, emphasizes, ref prob);
		}

        #region IUnitaryHandler Members

        public override ArgumentTree Handle(ArgumentTree arg)
        {
			object value;
			Phrase input = null;
			GrammarParser.ParaphraseOptions? opts = null;
			List<string> emph = null;
			double prob = 0;

			if (arg.TryGetValue("input", out value))
				input = (Phrase) value;
			if (arg.TryGetValue("opts", out value))
				opts = (GrammarParser.ParaphraseOptions?) value;
			if (arg.TryGetValue("emph", out value))
				emph = (List<string>) value;
			if (arg.TryGetValue("prob", out value))
				prob = (double) value;

            Phrase result = Handle(input, opts, emph, prob);
			return new ArgumentTree(result);
		}

        #endregion
	}
}

