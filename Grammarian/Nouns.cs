/******************************************************************\
 *      Class Name:     Nouns
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
 * Interface to the Noun manipulator and information object
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase;
using ActionReaction;
using ActionReaction.Interfaces;

namespace LanguageNet.Grammarian
{
    public class Nouns
    {
        // Contains number information for nouns (and can guess it)
        public const string NumberSourceName = "Nouns:NumberSource";
        // Contains gender information for nouns (and can guess it)
        public const string GenderSourceName = "Nouns:GenderSource";
        // Contains noun type information (e.g., proper noun, count noun) (and can guess it)
        public const string NounTypeSourceName = "Nouns:NounTypeSource";
        // The result of a noun change from singular to plural
        public static IArgumentType PluralNounResultType = new NamedArgumentType("Nouns:ChangeToPlural",
            new StringArgumentType(int.MaxValue, ".+", "buffalos"));
        // The result of a noun change from plural to singular
        public static IArgumentType SingularNounResultType = new NamedArgumentType("Nouns:ChangeToSingular",
            new StringArgumentType(int.MaxValue, ".+", "buffalo"));
		

        public enum Number
        {
            Singular,
            Plural
        };

        public enum Gender
        {
            Neuter,
            Female,
            Male,
            Either
        };

        public enum NounType
        {
            NotNoun = 0,
            Unknown = 1,
            Mass = 2,
            Count = 3,
            Animate = 4,
            The = 5,
            ProperFemale = 8,
            ProperMale = 16,
            ProperEither = 24,
            ProperCity = 32,
            ProperCountry = 64,
            ProperProvince = 128,
            ProperMask = 248
        };

        /***** Static Utility Methods *****/

        // What does this noun look like as an object?  The same, unless it's a pronoun
        public static string AsObject(string input)
        {
            string lower = input.ToLower();
            if (lower == "i" || lower == "me" || lower == "my" || lower == "mine" || lower == "myself")
                return "me";
            if (lower == "we" || lower == "us" || lower == "our" || lower == "ours" || lower == "ourselves")
                return "us";
            if (lower == "you" || lower == "your" || lower == "yours" || lower == "yourself")
                return "you";
            if (lower == "she" || lower == "her" || lower == "hers" || lower == "herself")
                return "her";
            if (lower == "he" || lower == "him" || lower == "his" || lower == "himself")
                return "him";
            if (lower == "it" || lower == "its" || lower == "itself")
                return "it";
            if (lower == "they" || lower == "them" || lower == "their" || lower == "theirs" || lower == "themselves")
                return "them";

            return input;
        }

        // What does this noun look like as an subject?  The same, unless it's a pronoun
        public static string AsSubject(string input)
        {
            string lower = input.ToLower();
            if (lower == "i" || lower == "me" || lower == "my" || lower == "mine" || lower == "myself")
                return "I";
            if (lower == "we" || lower == "us" || lower == "our" || lower == "ours" || lower == "ourselves")
                return "we";
            if (lower == "you" || lower == "your" || lower == "yours" || lower == "yourself")
                return "you";
            if (lower == "she" || lower == "her" || lower == "hers" || lower == "herself")
                return "she";
            if (lower == "he" || lower == "him" || lower == "his" || lower == "himself")
                return "he";
            if (lower == "it" || lower == "its" || lower == "itself")
                return "it";
            if (lower == "they" || lower == "them" || lower == "their" || lower == "theirs" || lower == "themselves")
                return "they";

            return input;
        }

        // Is this probably an anaphora (pronoun)?  0 means no, 1.0 means yes
        public static double IsAnaphora(string word)
        {
            string lower = word.ToLower();
            if (lower == "she" ||
                lower == "he" || lower == "him" ||
                lower == "it" || lower == "they" || lower == "them" ||
                lower == "that")
                return 1.0;

            if (lower == "his" || lower == "her" || lower == "hers" || lower == "its" || lower == "their" || lower == "theirs")
                return 0.5;

            return 0.0;
        }

        // Count "how much" anaphora-stuff there is in a collection fo words
        public static double CountAnaphora(string[] words)
        {
            for (int ii = 0; ii < words.Length; ii++)
                words[ii] = words[ii].ToLower();

            double count = 0;

            for (int ii = 0; ii < words.Length; ii++)
            {
                if (ii < words.Length - 1 && words[ii] == "it" && words[ii + 1] == "that")
                    ii++;
                else
                    count += IsAnaphora(words[ii]);
            }

            return count;
        }

        /***** Instance Methods, for plugin interaction *****/

        protected PluginEnvironment plugenv;

        public Nouns(PluginEnvironment plugenv)
        {
            this.plugenv = plugenv;
        }

        // Capitalize the first letter, if it would be grammatically appropriate
        public string StartCap(string input)
        {
            return input[0].ToString().ToUpper() + input.Substring(1);
        }

        // Uncapitalize the first letter, if would be grammatically appropriate
        public string UnStartCap(string input)
        {
            // Is this a proper noun?
            if (plugenv != null)
            {
                NounType type;

                if (plugenv.GetDataSource<string, Nouns.NounType>(NounTypeSourceName).TryGetValue(input, out type))
                    if ((type & NounType.ProperMask) != NounType.NotNoun)
                        return input;
            }

            return input[0].ToString().ToLower() + input.Substring(1);
        }

        // Get the number of a noun
        public Nouns.Number GetNumber(string noun)
        {
            if (plugenv != null)
            {
                Nouns.Number number;
                if (plugenv.GetDataSource<string, Nouns.Number>(NumberSourceName).TryGetValue(noun, out number))
                    return number;
            }

            if (noun.ToLower().EndsWith("s"))
                return Number.Plural;
            else
                return Number.Singular;
        }

        // Get the verb conjugation person of an input-- third person unless it's a pronoun
        public Verbs.Person GetPerson(string input)
        {
            string lower = input.ToLower();
            if (lower == "i" || lower == "me" || lower == "my" || lower == "mine" || lower == "myself")
                return Verbs.Person.FirstSingle;
            if (lower == "we" || lower == "us" || lower == "our" || lower == "ours" || lower == "ourselves")
                return Verbs.Person.FirstPlural;
            if (lower == "you" || lower == "your" || lower == "yours" || lower == "yourself")
                return Verbs.Person.Second;
            if (lower == "she" || lower == "her" || lower == "hers" || lower == "herself" ||
                lower == "he" || lower == "him" || lower == "his" || lower == "himself" ||
                lower == "it" || lower == "its" || lower == "itself")
                return Verbs.Person.ThirdSingle;
            if (lower == "they" || lower == "them" || lower == "their" || lower == "theirs" || lower == "themselves")
                return Verbs.Person.ThirdPlural;

            return (GetNumber(input) == Nouns.Number.Singular ? Verbs.Person.ThirdSingle : Verbs.Person.ThirdPlural);
        }

        // Change a noun to plural
        public string Pluralize(string noun)
        {
            if (plugenv != null) {
				object output = plugenv.ImmediateConvertTo(noun, PluralNounResultType, 2, 1000);
                return (string)output;
			}
			
			if (noun.EndsWith("s"))
				return noun + "es";
			else
	            return noun + "s";
        }

        // Change a noun to singular
        public string Singularize(string nouns)
        {
            if (plugenv != null)
                return (string)plugenv.ImmediateConvertTo(nouns, SingularNounResultType, 2, 1000);
			
			if (nouns.EndsWith("es"))
				return nouns.Substring(0, nouns.Length - 2);
			else
	            return nouns.Substring(0, nouns.Length - 1);
        }
	}
}
