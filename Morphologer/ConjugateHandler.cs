/******************************************************************\
 *      Class Name:     ConjugateHandler
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
 * Functions for intelligently conjugating and changing the
 * conjugation of a verb.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using LanguageNet.Grammarian;
using PluggerBase.ActionReaction.Actions;
using GenericTools;

namespace LanguageNet.Morphologer
{
	public class ConjugateHandler : UnitaryHandler
	{
		protected VerbsData verbsData;
		
		public ConjugateHandler(VerbsData verbsData)
			: base("Verb Conjugator",
			       "Change the conjugation of a verb",
			       Verbs.ConjugationArgumentType, Verbs.ConjugationResultType, 120)
		{
			this.verbsData = verbsData;
		}

        public string Handle(string verb, Verbs.Convert convert)
        {
            verb = InputToBase(verb);

            if (verb == "shall" || verb == "should" || verb == "will" ||
                verb == "would" || verb == "may" || verb == "might" ||
                verb == "can" || verb == "could" || verb == "must")
                return verb;

            switch (convert)
            {
                case Verbs.Convert.ext_V:
                    return verb;

                case Verbs.Convert.ext_Vs:
                    return ComposePresent(verb);

                case Verbs.Convert.ext_Ving:
                    return ComposePrespart(verb);

                case Verbs.Convert.ext_Ved:
                    return ComposePast(verb);

                case Verbs.Convert.ext_Ven:
                    return ComposePastpart(verb);
            }

            return verb;

        }

        public string InputToBase(string verb)
        {
            string found1 = "";
            TwoTuple<string, string> found2;
            
            if (verbsData.verb_tobase.TryGetValue(verb, out found1))
                return found1;

            if (verbsData.base_past.ContainsKey(verb))
                return verb;

            if (verbsData.past_base.TryGetValue(verb, out found2))
                return found2.one;

            if (verbsData.pastpart_base.TryGetValue(verb, out found1))
                return found1;

            //input not in any of the verb tables 
            char last = char.ToLower(verb[verb.Length - 1]);
            if (last == 's')
                return SItBase(verb);
            else if (last == 'g')
                return GItBase(verb);
            else if (last == 'd')
                return DItBase(verb);

            return verb;
        }

        //extract base from verbs ending in s
        public string SItBase(string input) {
            if (input.EndsWith("ies")) {
                // check for ies exceptions
                if ((input == "belies") || (input == "dies") || (input == "lies")
                    || (input == "ties") || (input == "vies")) {
                    //remove final s
                    return input.Substring(0, input.Length - 1);
                } else {
                    return input.Substring(0, input.Length - 3) + "y";
                }
            } else if ((input.EndsWith("ces")) || (input.EndsWith("ches")) ||
                       (input.EndsWith("oes")) || (input.EndsWith("shes")) ||
                       (input.EndsWith("sses")) || (input.EndsWith("xes")) ||
                       (input.EndsWith("zzes"))) {
                //check for exceptions
                if ((input == "axes") || (input == "annexes") || (input == "finesses") ||
                    (input == "hoes") || (input == "shoes") || (input == "toes")) {
                    //remove final s
                    return input.Substring(0, input.Length - 1);
                } else if ((input == "busses") || (input == "gasses")) {
                    // couple more exceptions
                    return input.Substring(0, input.Length - 3);
                } else {
                    //remove final es
                    return input.Substring(0, input.Length - 2);
                }
            // if letter-other-than-s + s   we already know last letter is s
            // therefore if second to last letter is not s
            } else if (char.ToLower(input[input.Length - 2]) != 's') {
                //check for exceptions
                if (input == "has") {
                    return "have";
                } else if (input == "summonses") {
                    return "summons";
                } else if (input != "bus" && input != "gas") {
                    return input.Substring(0, input.Length - 1);
                }
            }

            return input;
        }

        //extract base from verbs ending in g
        public string GItBase(string input) {
            int ilen = input.Length;

            if ((input.EndsWith("cking")) || (input.EndsWith("ffing")) ||
                (input.EndsWith("wing")) ||
                (input.EndsWith("ssing")) || (input.EndsWith("ying")) ||
                (input.EndsWith("xing")) || (input.EndsWith("zzing"))) {
                // check for exceptions
                if ((input == "belying") || (input == "dying") || (input == "lying") ||
                    (input == "tying") || (input == "vying")) {
                    //change ying to ie
                    return input.Substring(0, ilen - 4) + "ie";
                } else if ((input == "axing") || (input == "annexing") ||
                           (input == "finessing") || (input=="eying")) {
                    return input.Substring(0, ilen - 3) + "e";
                } else if((input == "gassing")    || (input=="bussing") ||
                          (input == "panicking")  || (input=="mimicking") ||
                          (input == "frolicking") || (input=="shellacking") ||
                          (input == "tarmacking")  || (input == "trafficking")) {
                    // remove sing or king
                    return input.Substring(0, ilen - 4);
                } else {
                    // remove ing
                    return input.Substring(0, ilen - 3);
                }
            //ends ing & no other vowels
            } else if ((input.EndsWith("ing")) && (CountVowels(input) == 1)) {
                if (input == "typing")
                    return "type";
                // else leave unchanged
                else
                    return input;
            } else if ((input.EndsWith("lling")) && (CountVowels(input) == 2)) {
                //remove ing
                return input.Substring(0, ilen - 3);
                // two identical consonants + 'ing'
            } else if ((input.EndsWith("ing")) && (ilen > 4) &&
                       (input[ilen - 4] == input[ilen - 5]) &&
                       AuxString.IsConsonant(input, ilen - 4)) {
                if ((input=="boycotting") ||
                    (input=="blackballing") ||
                    (input=="recalling") ||
                    (input=="overcalling") ||
                    (input=="befalling") ||
                    (input=="mis-spelling") ||
                    (input=="retelling") ||
                    (input=="reselling") ||
                    (input=="overselling") ||
                    (input=="underselling") ||
                    (input=="installing") ||
                    (input=="unrolling") ||
                    (input=="overfilling") ||
                    (input=="whirring") ||
                    (input=="purring") ||
                    (input=="erring")) {
                    return input.Substring(0, ilen - 3);
                } else {
                    // remove last ing and changed double const. to single
                    string result = input.Substring(0, ilen - 4);
                    if (!verbsData.todouble.ContainsKey(result))
                        result += result[ilen - 1];
                    return result;
                }
            }
            else if (input.EndsWith("cing") ||
                input.EndsWith("fing") ||
                input.EndsWith("ling") ||
                input.EndsWith("sing") ||
                input.EndsWith("uing") ||
                input.EndsWith("ving"))
            {
                return input.Substring(0, ilen - 3) + "e";
            } else if ((ilen > 5) && input.EndsWith("ing") &&
                AuxString.IsConsonant(input, ilen - 6) &&
                AuxString.IsVowel(input, ilen - 5) &&
                CountVowels(input) == 2 &&
                AuxString.IsConsonant(input, ilen - 4)) {
                return input.Substring(0, ilen - 3) + "e";
            } else if ((ilen > 3) && input.EndsWith("ing") &&
                       AuxString.IsConsonant(input, ilen - 4)) {
                // check in transtive list to tell us what to do
                string result = input.Substring(0, ilen - 3);
                if (!verbsData.transitive.ContainsKey(input) && !verbsData.intransitive.ContainsKey(input) && !verbsData.either.ContainsKey(input))
                    result += "e";
                return result;
            } else if(input.EndsWith("ing")) {
                return input.Substring(0, ilen - 3);
            }

            return input;
        }

        //extract base from verbs ending in d
        public string DItBase(string input) {
            int ilen = input.Length;

            if (input.EndsWith("ed")) {        //check for input ending 'ed'
                // ends in 'eed'
                if ((ilen > 3) && char.ToLower(input[ilen - 3]) == 'e') {
                    // leave unchanged
                    return input;
                } else if ((ilen > 3) && char.ToLower(input[ilen - 3]) == 'i') {
                    // check exceptions
                    if ((input == "belied") || (input == "died") || (input == "lied") ||
                        (input == "tied") || (input == "vied")) {
                        return input.Substring(0, ilen - 1); // remove final d
                    } else if (input == "taxied") {
                        return "taxi";
                    } else {
                        //change 'ied' to 'y'
                        return input.Substring(0, ilen - 3) + "y";
                    }
                }
                else if ((input.EndsWith("cked")) || (input.EndsWith("ffed")) ||
                     (input.EndsWith("ooed")) || (input.EndsWith("lled")) ||
                     (input.EndsWith("wed")) ||
                     (input.EndsWith("ssed")) || (input.EndsWith("yed")) ||
                    (input.EndsWith("xed")) || (input.EndsWith("zzed")))
                {
                    //exceptions
                    if ((input == "axed") || (input == "annexed") ||
                        (input == "finessed") || (input == "dyed") ||
                        (input == "eyed")) {
                        //change ed to e 
                        return input.Substring(0, ilen - 1);
                    } else if ((input == "gassed") || (input == "bussed") ||
                               (input == "trafficked") || (input == "panicked") ||
                               (input == "mimicked") || (input == "frolicked") ||
                               (input == "shellacked") || (input == "tarmacked")) {
                        // remove sed or ked
                        return input.Substring(0, ilen - 3);
                    } else {
                        // remove ed
                        return input.Substring(0, ilen - 2);
                    }
                } else if (CountVowels(input) == 1) {
                    if (input=="typed")
                        return "type";
                    // leave unchanged
                    return input;
                } else if ((CountVowels(input)==2) && input.EndsWith("lled")) {
                    return input.Substring(0, ilen - 2);
                } else if ((ilen > 4) && (input[ilen - 3] == input[ilen - 4]) &&
                           AuxString.IsConsonant(input, ilen-3)) {
                    if ((input=="boycotted") ||
                        (input=="blackballed") || (input=="recalled") ||
                        (input=="overcalled") || (input=="mis-spelled") ||
                        (input=="installed") || (input=="unrolled") ||
                        (input=="overfilled") || (input=="whirred") ||
		                (input=="putted") ||
                        (input=="purred") || (input=="erred")) {
                        // remove ed
                        return input.Substring(0, ilen - 2);
                    } else {
                        // remove last ed and changed double const. to single
                        string result = input.Substring(0, ilen - 3);
                        if (!verbsData.todouble.ContainsKey(result))
                            result += input[result.Length - 1];
                        return result;
                    }
                } else if (input.EndsWith("ced") ||
                    input.EndsWith("fed") ||
                    input.EndsWith("led") ||
                    input.EndsWith("sed") ||
                    input.EndsWith("ued") ||
                    input.EndsWith("ized") ||
                    input.EndsWith("ved"))
                {

                    if (input == "summonsed")
                        return "summons";

                    return input.Substring(0, ilen - 1);
                // one-or-more-consonants + single-vowel + single-consonant + ed
                } else if ((ilen > 5) && 
                    AuxString.IsConsonant(input, ilen - 5) &&
                    AuxString.IsVowel(input, ilen - 4) &&
                    AuxString.IsConsonant(input, ilen - 3)) {
                    return input.Substring(0, ilen - 1); // remove final 'd'
                //ends  consonant + ed  
                } else if ((ilen > 3) && AuxString.IsConsonant(input, ilen - 3)) {
                    if (input == "singed")
                        return "singe";
                    else if (input == "swinged")
                        return "swinge";
                    else {
                        string result = input.Substring(0, ilen - 1); // try removing 'd'
                        if (!(verbsData.transitive.ContainsKey(result) && verbsData.intransitive.ContainsKey(result) &&
                              verbsData.either.ContainsKey(result) && verbsData.todouble.ContainsKey(result)))   {        
                            return result.Substring(0, result.Length - 1);
                        }

                        return result;
                    }
                } else {
                    // else change 'ed' to 'e'
                    return input.Substring(0, ilen - 1);
                }
            }

            return input;
        }

        public string ComposePresent(string verb) {
            if (verb == "have")
                return "has";
            else if (verb == "go")
                return "goes";
            else if (verb == "do")
                return "does";
            else {
                int ilen = verb.Length;
                char last = char.ToLower(verb[ilen - 1]);
    
                //end consonant + y
                if ((ilen > 2) && (last == 'y') && 
                    (AuxString.IsConsonant(verb, ilen - 2))) {
                    // replace y with ies
                    return verb.Substring(0, ilen - 1) + "ies";
                } else if ((ilen > 1) && ((last == 's') ||
                                          (last == 'z') || (last == 'x') ||
                                          (verb.EndsWith("ch")) || (verb.EndsWith("sh")))) {
                    return verb + "es";    // add 'es'
                } else {
                    return verb + "s";
                }
            }
        }

        public string ComposePrespart(string verb) {
            int ilen = verb.Length;

            if (ilen > 2) {
                char last = char.ToLower(verb[ilen - 1]);

                if ((last == 'e') && ((verb[ilen - 2] == 'u') || (AuxString.IsConsonant(verb, ilen - 2)))) {
                    //check exceptions
                    if ((verb == "dye") || (verb == "singe") ||
                        (verb == "age") || (verb == "eye") ||
                        (verb == "swinge") || (verb == "whinge")) {
                        return verb + "ing";          // add ing
                    } else {     // change e to ing
                        return verb.Substring(0, ilen - 1) + "ing";
                    }
                } else if (verb.EndsWith("ie")) {     // replace 'ie' with 'ying'
                    return verb.Substring(0, ilen - 2) + "ying";
                //base ends (consonant [or qu] + vowel + consonant)
                } else if (AuxString.IsConsonant(verb, ilen - 1) && AuxString.IsVowel(verb, ilen - 2) &&
                    (AuxString.IsConsonant(verb, ilen - 3) ||
                     (ilen > 3 && verb[ilen - 4] == 'q' && verb[ilen - 3] == 'u'))) {
                    if (last == 'c')
                        return verb + "king";
                    else if ((last == 'x') || (last == 'y') || (last=='w'))
                        return verb + "ing";
                    else if (last == 'g')
                        return verb + "ging";
                    else if (last == 'l')
                        return verb + "ling";
                    else if (verbsData.todouble.ContainsKey(verb))
                        return verb + last + "ing";
                    else
                        return verb + "ing";
                } else {
                    return verb + "ing";
                }
            } else {
                return verb + "ing";
            }
        }

        public string ComposePast(string verb) {
	        int ilen = verb.Length;
            char last = char.ToLower(verb[ilen - 1]);

            TwoTuple<string, string> found;
            if (verbsData.base_past.TryGetValue(verb, out found)) // if verb is in irregular verb table
                return found.one;
            else {
                if (last == 'e') {
                    return verb + "d";
                } else if ((last == 'y') && AuxString.IsConsonant(verb, ilen - 2)) {
                    return verb.Substring(0, ilen - 1) + "ied";
                //base ends (consonant [or qu] + vowel + consonant)
                } else if ((AuxString.IsConsonant(verb, ilen - 1)) && (AuxString.IsVowel(verb, ilen - 2)) &&
                           ((AuxString.IsConsonant(verb, ilen - 3)) ||
                            ((verb[ilen - 4] == 'q') && (verb[ilen - 3] == 'u')))) {
                    if (last == 'c')
                        return verb + "ked";
                    else if ((last == 'x') || (last == 'y') || (last=='w'))
                        return verb + "ed";
                    else if (last == 'g')
                        return verb = "ged";
                    else if (last == 'l')
                        return verb + "led";
                    else if (verbsData.todouble.ContainsKey(verb))
                        return verb + last + "ed";
                    else
                        return verb + "ed";
                } else {
                    return verb + "ed";
                }
            }
        }

        public string ComposePastpart(string verb) {
            string found = "";
            if (verbsData.base_pastpart.TryGetValue(verb, out found))
                return found;
            else
                return ComposePast(verb);
        }

        public int CountVowels(string input)
        {
            int vct = 0;

            for (int cct = 0; cct < input.Length; cct++)
            {
                if (AuxString.IsVowel(input, cct))
                    vct++;
            }

            return vct;
        }

		#region IUnitaryHandler Members

        public override object Handle(object arg)
        {
			KeyValuePair<string, Verbs.Convert> kvp = (KeyValuePair<string, Verbs.Convert>) arg;
            return Handle(kvp.Key, kvp.Value);
        }

        #endregion
	}
}

