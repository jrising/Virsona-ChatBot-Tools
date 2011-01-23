/******************************************************************\
 *      Class Name:     Verbs
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * This file is part of Grammarian and is free software: you can
 * redistribute it and/or modify it under the terms of the GNU
 * Lesser General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option)
 * any later version.
 * 
 * Grammarian is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with Grammarian.  If not, see
 * <http://www.gnu.org/licenses/>.
 *      -----------------------------------------------------------
 * Interface to the Verb manipulator and information object
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase;
using PluggerBase.ActionReaction.Interfaces;

namespace LanguageNet.Grammarian
{
    public class Verbs
    {
        // The allowed argument to a conjugation command: a <string, conjugation-conversion> pair
        public static IArgumentType ConjugationArgumentType =
            new KeyValueArgumentType<string, Convert>(new StringArgumentType(int.MaxValue, ".+", "buffalo"),
                new SelectableArgumentType(new object[] { Convert.ext_V, Convert.ext_Vs, Convert.ext_Ving, Convert.ext_Ved, Convert.ext_Ven }));
        // The result of a conjugation command: a string
        public static IArgumentType ConjugationResultType =
            new NamedArgumentType("Morphology:Conjugate", new StringArgumentType(int.MaxValue, ".+", "buffalos"));

		// The allowed argument to a transitive-test command: a <strnig, boolean> pair
        // the boolean is true to check transitivity; false to check intransitivity
        public static IArgumentType TransitiveArgumentType =
            new KeyValueArgumentType<string, bool>(new StringArgumentType(
                int.MaxValue, ".+", "buffalo"), new BooleanArgumentType());
		// the result of a transitive-test command: a boolean
        public static IArgumentType TransitiveResultType =
            new NamedArgumentType("Syntax:Transitivity", new BooleanArgumentType());
        // The result of an inflection-test command: an inflection
        public static IArgumentType InflectionResultType =
            new SelectableArgumentType(new object[] { Convert.ext_V, Convert.ext_Vs, Convert.ext_Ving, Convert.ext_Ved, Convert.ext_Ven });

        // Contains noun type information (e.g., proper noun, count noun) (and can guess it)
        public const string VerbTypeSourceName = "Verbs:VerbTypeSource";

		// The various conjugations
        public enum Convert
        {
            ext_V = 1,      // base form
            ext_Vs = 2,     // present tense
            ext_Ving = 3,   // present participle (gerund)
            ext_Ved = 4,    // past tense
            ext_Ven = 5     // past participle
        };

        // The person of a conjugation
        public enum Person
        {
            FirstSingle,
            ThirdSingle,
            FirstPlural,
            ThirdPlural,
            Second
        }

        // The type of noun
        public enum VerbType
        {
            NotVerb,
            VI,         // intransitive
            VT,         // transitive
            VE,         // either
            VE_ING,     // present participles
            VI_ING,
            VT_ING,
            VE_EN,      // past participles
            VI_EN,
            VT_EN
        };

        /***** Static Methods *****/

        // is this a form of the verb to be
        public static bool IsToBe(string word)
        {
            word = word.ToLower();
            return (word == "was" || word == "were" || word == "am" || word == "is" || word == "are" || word == "being" || word == "been" || word == "be");
        }

        //  compse a conjugation of the verb to be
        public static string ComposeToBe(Verbs.Person person, Verbs.Convert convert)
        {
            if (convert == Verbs.Convert.ext_Ved)
                return (person == Verbs.Person.FirstSingle || person == Verbs.Person.ThirdSingle) ? "was" : "were";
            if (convert == Verbs.Convert.ext_Vs)
                return person == Verbs.Person.FirstSingle ? "am" : (person == Verbs.Person.ThirdSingle ? "is" : "are");
            if (convert == Verbs.Convert.ext_Ving)
                return "being";
            if (convert == Verbs.Convert.ext_Ven)
                return "been";

            return "be";
        }

        // is this a form of the verb to do
        public static bool IsToDo(string word)
        {
            word = word.ToLower();
            return (word == "did" || word == "do" || word == "does" || word == "doing" || word == "done");
        }

        // compose a conjugation of the verb to do
        public static string ComposeToDo(Verbs.Person person, Verbs.Convert convert)
        {
            if (convert == Verbs.Convert.ext_Ved)
                return "did";
            if (convert == Verbs.Convert.ext_Vs)
                return person == Verbs.Person.ThirdSingle ? "does" : "do";
            if (convert == Verbs.Convert.ext_Ving)
                return "doing";
            if (convert == Verbs.Convert.ext_Ven)
                return "done";

            return "do";
        }

        // is this a form of the verb to have
        public static bool IsToHave(string word)
        {
            word = word.ToLower();
            return (word == "had" || word == "have" || word == "has" || word == "having");
        }

        // compase a conjugation of the verb to have
        public static string ComposeToHave(Verbs.Person person, Verbs.Convert convert)
        {
            if (convert == Verbs.Convert.ext_Ved)
                return "had";
            if (convert == Verbs.Convert.ext_Vs)
                return (person == Verbs.Person.ThirdSingle) ? "has" : "have";
            if (convert == Verbs.Convert.ext_Ving)
                return "having";
            if (convert == Verbs.Convert.ext_Ven)
                return "had";

            return "have";
        }

        // is this a modal or modal-like word?
        public static bool IsModal(string words)
        {
            return (words == "will" || words == "would" || words == "shall" || words == "should" ||
                words == "may" || words == "might" || words == "can" || words == "could" ||
                words == "mote" || words == "must" || words == "ought" || words == "ought to" ||
                words == "had better" || words == "used" || words == "used to" || words == "need" ||
                words == "need to" || words == "have" || words == "have to" || words == "date");
        }

        /**** Instatiated Methods, for plugin interaction *****/
        
        protected PluginEnvironment plugenv;

        public Verbs(PluginEnvironment plugenv)
        {
            this.plugenv = plugenv;
        }

        // form the infinitive base form from any conjugation
        public string InputToBase(string verb)
        {
            if (plugenv != null)
            {
                return (string)plugenv.ImmediateConvertTo(new KeyValuePair<string, Convert>(verb, Convert.ext_V),
                    ConjugationResultType, 2, 1000);
            }

            verb = verb.ToLower();
            if (verb.EndsWith("s"))
                return verb.Substring(0, verb.Length - 1);
            if (verb.EndsWith("ed") || verb.EndsWith("en"))
                return verb.Substring(0, verb.Length - 2);
            if (verb.EndsWith("ing"))
                return verb.Substring(0, verb.Length - 3);
            return verb;
        }

        // form the present tense
        public string ComposePresent(string verb)
        {
            if (plugenv != null)
            {
                return (string)plugenv.ImmediateConvertTo(new KeyValuePair<string, Convert>(verb, Convert.ext_Vs),
                    ConjugationResultType, 2, 1000);
            }

            return verb + "s";
        }

        // form the past tense
        public string ComposePast(string verb)
        {
            if (plugenv != null)
            {
                return (string)plugenv.ImmediateConvertTo(new KeyValuePair<string, Convert>(verb, Convert.ext_Ved),
                    ConjugationResultType, 2, 1000);
            }

            return verb + "ed";
        }

        // compose the present participle
        public string ComposePrespart(string verb)
        {
            if (plugenv != null)
            {
                return (string)plugenv.ImmediateConvertTo(new KeyValuePair<string, Convert>(verb, Convert.ext_Ving),
                    ConjugationResultType, 2, 1000);
            }

            return verb + "ing";
        }

        // compose the past participle
        public string ComposePastpart(string verb)
        {
            if (plugenv != null)
            {
                return (string)plugenv.ImmediateConvertTo(new KeyValuePair<string, Convert>(verb, Convert.ext_Ven),
                    ConjugationResultType, 2, 1000);
            }

            return verb + "ing";
        }

        // inflect according to the given conversion
        public string InflectVerb(string verb, Convert convert)
        {
            if (plugenv != null)
            {
                return (string)plugenv.ImmediateConvertTo(new KeyValuePair<string, Convert>(verb, convert),
                    ConjugationResultType, 2, 1000);
            }

            return verb;
        }

        // inflect a verb, appropriate to a given person for irregulars
        public string ComposePersonable(string verb, Verbs.Person person, Verbs.Convert convert)
        {
            if (Verbs.IsToBe(verb))
                return Verbs.ComposeToBe(person, convert);
            if (Verbs.IsToDo(verb))
                return Verbs.ComposeToDo(person, convert);
            if (Verbs.IsToHave(verb))
                return Verbs.ComposeToHave(person, convert);

            if (convert == Verbs.Convert.ext_Vs && person != Verbs.Person.ThirdSingle)
                return InflectVerb(verb, Verbs.Convert.ext_V);
            else
                return InflectVerb(verb, convert);
        }

        // Check if a verb is (can be) transitive
        public bool IsTransitive(string verb)
        {
            if (plugenv != null)
            {
                return (bool)plugenv.ImmediateConvertTo(new KeyValuePair<string, bool>(verb, true),
                    TransitiveResultType, 2, 200);
            }

            return false;
        }

        // Check if a verb is (can be) intransitive
        public bool IsIntransitive(string verb)
        {
            if (plugenv != null)
            {
                return (bool)plugenv.ImmediateConvertTo(new KeyValuePair<string, bool>(verb, false),
                    TransitiveResultType, 2, 200);
            }

            return false;
        }

        // Determine the inflection of a verb
        public Convert GetInflection(string verb)
        {
            if (plugenv != null)
                return (Convert)plugenv.ImmediateConvertTo(verb, InflectionResultType, 2, 500);

            verb = verb.ToLower();
            if (verb.EndsWith("s"))
                return Convert.ext_Vs;
            if (verb.EndsWith("en"))
                return Convert.ext_Ven;
            if (verb.EndsWith("ing"))
                return Convert.ext_Ving;
            if (verb.EndsWith("ed"))
                return Convert.ext_Ved;
            return Convert.ext_V;
        }
    }
}
