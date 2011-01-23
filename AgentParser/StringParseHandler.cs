/******************************************************************\
 *      Class Name:     StringParseHandler
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Parsing a string into a phrase structure.
\******************************************************************/
using System;
using System.Collections.Generic;
using PluggerBase;
using PluggerBase.ActionReaction.Actions;
using PluggerBase.ActionReaction.Interfaces;
using LanguageNet.Grammarian;

namespace LanguageNet.AgentParser
{
	public class StringParseHandler : UnitaryHandler
	{
		protected POSTagger tagger;
		
        public StringParseHandler(PluginEnvironment plugenv)
            : base("Grammar Parser of Strings",
                "Construct an IParsedPhrase for a given string",
                new StringArgumentType(int.MaxValue, ".+", "buffalo buffalo"),
                GrammarParser.GrammarParseResultType, 120)
        {
			tagger = new POSTagger(plugenv);
        }

        public IParsedPhrase Handle(string input)
        {
			List<KeyValuePair<string, string>> tokens = tagger.TagString(input);
			Sentence sentence = new Sentence(tokens);
			return sentence.Parse();
		}

        #region IUnitaryHandler Members

        public override object Handle(object arg)
        {
            return Handle((string)arg);
        }

        #endregion
	}
}

