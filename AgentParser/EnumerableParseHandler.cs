/******************************************************************\
 *      Class Name:     EnumerablePhraseHandler
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Parse a part-tagged words enumeration into a parsed phrase
 * structure.
\******************************************************************/
using System;
using System.Collections.Generic;
using ActionReaction.Actions;
using LanguageNet.Grammarian;

namespace LanguageNet.AgentParser
{
	public class EnumerableParseHandler : UnitaryHandler
	{
        public EnumerableParseHandler()
            : base("Grammar Parser of Lists of Tokens",
                "Construct an IParsedPhrase from a list of tokens",
                POSTagger.TagEnumerationResultType,
                GrammarParser.GrammarParseResultType, 120)
        {
        }

        public IParsedPhrase Handle(IEnumerable<KeyValuePair<string, string>> tokens)
        {
			Sentence sentence = new Sentence(tokens);
			return sentence.Parse();
		}

        #region IUnitaryHandler Members

        public override object Handle(object arg)
        {
            return Handle((IEnumerable<KeyValuePair<string, string>>)arg);
        }

        #endregion
	}
}

