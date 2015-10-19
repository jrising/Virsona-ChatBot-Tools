/******************************************************************\
 *      Class Name:     TransitiveTestHandler
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 * 						Modified from DoMuchMore by David Levy
 *      -----------------------------------------------------------
 * Determine whether a verb or verbal phrase is transitive or
 * intransitive
\******************************************************************/
using System;
using System.Collections.Generic;
using LanguageNet.Grammarian;
using ActionReaction.Actions;

namespace LanguageNet.Morphologer
{
	public class TransitiveTestHandler : UnitaryHandler
	{
		protected VerbsData verbsData;
		protected ConjugateHandler conjugator;

		public TransitiveTestHandler(VerbsData verbsData, ConjugateHandler conjugator)
			: base("Transitivity Tester",
			       "Check if a verb is transitive",
			       Verbs.TransitiveArgumentType, Verbs.TransitiveResultType, 120)
		{
			this.verbsData = verbsData;
			this.conjugator = conjugator;
		}

		public bool Handle(string verb, bool checkTransitivity) {
			if (checkTransitivity)
				return IsTransitive(verb) || verbsData.phrVerbT.ContainsKey(verb) || verbsData.phrVerbE.ContainsKey(verb);
			else
				return IsIntransitive(verb) || verbsData.phrVerbI.ContainsKey(verb) || verbsData.phrVerbE.ContainsKey(verb);
		}

        // something is transitive if it is not in the intransitive list
        public bool IsTransitive(string input) {
            string verb = conjugator.InputToBase(input);
            return !verbsData.intransitive.ContainsKey(verb);
        }

        // something is intransitive if it is not in the transitive list
        public bool IsIntransitive(string input) {
            string verb = conjugator.InputToBase(input);
            return !verbsData.transitive.ContainsKey(verb);
        }

		#region IUnitaryHandler Members

        public override object Handle(object arg)
        {
			KeyValuePair<string, bool> kvp = (KeyValuePair<string, bool>) arg;
            return Handle(kvp.Key, kvp.Value);
        }

        #endregion

	}
}

