/******************************************************************\
 *      Class Name:     InflectionTestHandler
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 * 						Modified from DoMuchMore by David Levy
 *      -----------------------------------------------------------
 * This file is part of Morphologer and is free software: you
 * can redistribute it and/or modify it under the terms of the GNU
 * Lesser General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option)
 * any later version.
 *
 * Plugger Base is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with Morphologer.  If not, see
 * <http://www.gnu.org/licenses/>.
 *      -----------------------------------------------------------
 * Determine the inflection of a verb
\******************************************************************/
using System;
using System.Collections.Generic;
using LanguageNet.Grammarian;
using ActionReaction.Actions;
using ActionReaction.Interfaces;
using GenericTools;

namespace LanguageNet.Morphologer
{
	public class InflectionTestHandler : UnitaryHandler
	{
		protected VerbsData verbsData;
		protected ConjugateHandler conjugator;

		public InflectionTestHandler(VerbsData verbsData, ConjugateHandler conjugator)
			: base("Inflection Tester",
			       "Determine the inflection of a verb",
			       new StringArgumentType(int.MaxValue, ".+", "buffalo"),
			       Verbs.InflectionResultType, 120)
		{
			this.verbsData = verbsData;
			this.conjugator = conjugator;
		}

		public Verbs.Convert Handle(string input)
        {
			if (IsBaseForm(input))
				return Verbs.Convert.ext_V;
            if (IsPastTense(input))
                return Verbs.Convert.ext_Ved;
            if (IsPastParticiple(input))
                return Verbs.Convert.ext_Ven;
            if (IsPresentParticiple(input))
                return Verbs.Convert.ext_Ving;

            Verbs.Convert convert;
            CheckVerb(ref input, out convert);
            return convert;
        }

        // input is converted to base form
        Verbs.VerbType CheckVerb(ref string input, out Verbs.Convert convert)
        {
            // look it up in the verb table first
            TwoTuple<string, string> convinfo;

            if (verbsData.base_past.TryGetValue(input, out convinfo))
            {
                convert = Verbs.Convert.ext_Vs;
                if (convinfo.two == "I")
                    return Verbs.VerbType.VI;
                else if (convinfo.two == "T")
                    return Verbs.VerbType.VT;
                else
                    return Verbs.VerbType.VE;
            }

            if (input.EndsWith("s"))
            {
                convert = Verbs.Convert.ext_Vs;
                if (verbsData.verb_S.TryGetValue(input, out convinfo))
                {
                    input = convinfo.one;
                    if (convinfo.two == "I")
                        return Verbs.VerbType.VI;
                    else if (convinfo.two == "T")
                        return Verbs.VerbType.VT;
                    else
                        return Verbs.VerbType.VE;
                }
                else
                {
                    input = conjugator.SItBase(input);
                    if (verbsData.intransitive.ContainsKey(input))
                        return Verbs.VerbType.VI;
                    else if (verbsData.transitive.ContainsKey(input))
                        return Verbs.VerbType.VT;
                    else if (verbsData.either.ContainsKey(input))
                        return Verbs.VerbType.VE;
                }
            }
            else if (input.EndsWith("ing"))
            {
                convert = Verbs.Convert.ext_Ving;
                if (verbsData.verb_ING.TryGetValue(input, out convinfo))
                {
                    input = convinfo.one;
                    if (convinfo.two == "I")
                        return Verbs.VerbType.VI_ING;
                    else if (convinfo.two == "T")
                        return Verbs.VerbType.VT_ING;
                    else
                        return Verbs.VerbType.VE_ING;
                }
                else
                {
                    input = conjugator.GItBase(input);
                    if (verbsData.intransitive.ContainsKey(input))
                        return Verbs.VerbType.VI_ING;
                    else if (verbsData.transitive.ContainsKey(input))
                        return Verbs.VerbType.VT_ING;
                    else if (verbsData.either.ContainsKey(input))
                        return Verbs.VerbType.VE_ING;
                }
            }
            else
            {
                if (verbsData.past_base.TryGetValue(input, out convinfo))
                {
                    convert = Verbs.Convert.ext_Ven;
                    input = convinfo.one;
                    if (convinfo.two == "I")
                        return Verbs.VerbType.VI_EN;
                    else if (convinfo.two == "T")
                        return Verbs.VerbType.VT_EN;
                    else
                        return Verbs.VerbType.VE_EN;
                }

                if (verbsData.verb_ED.TryGetValue(input, out convinfo))
                {
                    convert = Verbs.Convert.ext_Ven;
                    input = convinfo.one;

                    if (convinfo.two == "I")
                        return Verbs.VerbType.VI_EN;
                    else if (convinfo.two == "T")
                        return Verbs.VerbType.VT_EN;
                    else
                        return Verbs.VerbType.VE_EN;
                }
                else
                {
                    if (input.EndsWith("ed") && input != "need")
                    {
                        string alt = "";
                        CheckEDVerb(ref input, out alt);

                        convert = Verbs.Convert.ext_Ven;
                        if (verbsData.intransitive.ContainsKey(input))
                            return Verbs.VerbType.VI_EN;
                        else if (verbsData.transitive.ContainsKey(input))
                            return Verbs.VerbType.VT_EN;
                        else if (verbsData.either.ContainsKey(input))
                            return Verbs.VerbType.VE_EN;

                        // first one didn't work but do we have an alternative?
                        if (alt.Length > 0)
                        {
                            if (verbsData.intransitive.ContainsKey(input))
                            {
                                input = alt;
                                return Verbs.VerbType.VI_EN;
                            }
                            else if (verbsData.transitive.ContainsKey(input))
                            {
                                input = alt;
                                return Verbs.VerbType.VT_EN;
                            }
                            else if (verbsData.either.ContainsKey(input))
                            {
                                input = alt;
                                return Verbs.VerbType.VE_EN;
                            }
                        }
                    }
                    else
                    {
                        convert = Verbs.Convert.ext_Vs;

                        if (verbsData.intransitive.ContainsKey(input))
                            return Verbs.VerbType.VI;
                        else if (verbsData.transitive.ContainsKey(input))
                            return Verbs.VerbType.VT;
                        else if (verbsData.either.ContainsKey(input))
                            return Verbs.VerbType.VE;
                    }
                }
            }

            return Verbs.VerbType.NotVerb;
        }

        public bool IsPastTense(string input) {
	        /* special case were to avoid complications. The past of be is was OR were
	        and handling two types gets complicated so we special case it here */
            if (input == "were" || input == "was")
                return true;

            if (verbsData.past_base.ContainsKey(input) || verbsData.pastpart_base.ContainsKey(input))
                return true;
            else {
                string verb = conjugator.InputToBase(input);
                string past = conjugator.ComposePast(verb);
                if (past == input)
                    return true;
                else {
                    past = conjugator.ComposePastpart(verb);
                    if (past == input)
                        return true;
                }
            }

            return false;
        }

        public bool IsPresentTense(string input)
        {
            /* special case were to avoid complications. The past of be is was OR were
            and handling two types gets complicated so we special case it here */
            if (input == "are" || input == "is" || input == "am")
                return true;

            string verb = conjugator.InputToBase(input);
            string present = conjugator.ComposePresent(verb);
            if (present == input)
                return true;

            return false;
        }

        public bool IsPastParticiple(string input) {
            if (verbsData.pastpart_base.ContainsKey(input))
                return true;

            if (input.EndsWith("ed"))
            {
                string baseverb = conjugator.DItBase(input);
                return verbsData.transitive.ContainsKey(baseverb) || verbsData.intransitive.ContainsKey(baseverb) ||
                    verbsData.either.ContainsKey(baseverb) || verbsData.todouble.ContainsKey(baseverb);
            }
            else
                return false;
        }

        public bool IsPresentParticiple(string input) {
            if (input.EndsWith("ing")) {
                string baseverb = conjugator.GItBase(input);
                return verbsData.transitive.ContainsKey(baseverb) || verbsData.intransitive.ContainsKey(baseverb) ||
                    verbsData.either.ContainsKey(baseverb) || verbsData.todouble.ContainsKey(baseverb);
            } else
                return false;
        }

        // Get the base (and possible alternative base) of a verb ending in ed
	    // assumes all special cases (e.g belied, etc) have been removed */
        public void CheckEDVerb(ref string input, out string alt) {
            alt = "";		// initialise possible alternative

            int ilen = input.Length;

	        // if input ends ed and has at least one other vowel
            if (input.EndsWith("ed") && conjugator.CountVowels(input) > 1) {
		        // If input ends ied change ied to y
                if (input.EndsWith("ied"))
                    input = input.Substring(0, ilen - 3) + "y";
                else if (input.EndsWith("cked") || input.EndsWith("ffed") || input.EndsWith("lled") ||
                        input.EndsWith("ooed") || input.EndsWith("ssed") || input.EndsWith("wed") ||
                        input.EndsWith("yed") || input.EndsWith("xed") || input.EndsWith("zzed")) {
                    input = input.Substring(0, ilen - 2);   // remove ed
                } else if (ilen > 4 && input[ilen - 3] == input[ilen - 4] && AuxString.IsConsonant(input, ilen - 3)) {
			        // word ends in two identical consonats + ed
                    input = input.Substring(0, ilen - 3);
                } else if (input.EndsWith("ced") || input.EndsWith("sed") || input.EndsWith("ued") ||
                        input.EndsWith("ved") || input.EndsWith("ized")) {
                    input = input.Substring(0, ilen - 1);
                } else if (ilen > 5 && AuxString.IsConsonant(input, ilen - 5) &&
                        AuxString.IsVowel(input, ilen - 4) && AuxString.IsConsonant(input, ilen - 3) && conjugator.CountVowels(input) == 2) {
    			    // word consists of one or more consanants + single vowel + consonant + ed
                    input = input.Substring(0, ilen - 1);   // remove finial 'd'
                } else if (ilen > 3 && AuxString.IsConsonant(input, ilen - 3)) {
			        // word ends in consonant + ed
                    input = input.Substring(0, ilen - 1);   // remove final d
                    alt = input;                            // possible alternative
                    input = input.Substring(0, ilen - 2);   // remove e as well
                } else
                    input = input.Substring(0, ilen - 1);   // remove final d
            }
        }

        public bool IsBaseForm(string input)
        {
            return input == conjugator.InputToBase(input);
        }

		#region IUnitaryHandler Members

        public override object Handle(object arg)
        {
            return Handle((string) arg);
        }

        #endregion

	}
}

