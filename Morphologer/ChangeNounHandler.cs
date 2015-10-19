/******************************************************************\
 *      Class Name:     ChangeNounHandler
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Change the number of a noun
\******************************************************************/
using System;
using System.Collections.Generic;
using LanguageNet.Grammarian;
using ActionReaction.Actions;
using ActionReaction.Interfaces;

namespace LanguageNet.Morphologer
{
	public class ChangeNounHandler : UnitaryHandler
	{
		protected Nouns.Number changeTo;
		protected Dictionary<string, string> toMap;

		public ChangeNounHandler(Nouns.Number changeTo, Dictionary<string, string> toMap)
			: base("Number of Noun Changer",
			       "Change the Number of a Noun (plural or singular)",
			       new StringArgumentType(int.MaxValue, ".+", "buffalo"),
			       changeTo == Nouns.Number.Singular ? LanguageNet.Grammarian.Nouns.SingularNounResultType :
			       LanguageNet.Grammarian.Nouns.PluralNounResultType, 120)
		{
			this.changeTo = changeTo;
			this.toMap = toMap;

		}

        public string Handle(string input)
        {
			if (toMap.ContainsKey(input))
				return toMap[input];

			if (changeTo == Nouns.Number.Singular) {
				if (input.EndsWith("ses"))
					return input.Substring(0, input.Length - 2);
				else if (input.EndsWith("s"))
					return input.Substring(0, input.Length - 1);
			} else {
				if (input.EndsWith("s"))
					return input + "es";
				else
					return input + "s";
			}

			return input;
        }

        #region IUnitaryHandler Members

        public override object Handle(object arg)
        {
            return Handle((string)arg);
        }

        #endregion

	}
}

