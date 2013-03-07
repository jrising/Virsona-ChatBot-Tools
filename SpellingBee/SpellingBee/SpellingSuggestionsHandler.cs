/******************************************************************\
 *      Class Name:     SpellingSuggestionsHandler
 *      Written By:     James Rising
 *                      based on work by Matthew Gordon, Virsona
 *      Copyright:      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Returns a list of possible correct spellings for a given word
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using PluggerBase.ActionReaction.Actions;
using PluggerBase.ActionReaction.Interfaces;
using System.Diagnostics;

namespace SpellingBee
{
    public class SpellingSuggestionsHandler : UnitaryHandler
    {
        public static IArgumentType StringSuggestionsResultType = new NamedArgumentType("SpellingSuggestions:String",
            new EnumerableArgumentType(int.MaxValue, new StringArgumentType(int.MaxValue, ".+", "buffalo")));
		
		protected Process hunspell;
		
        public SpellingSuggestionsHandler()
            : base("Spelling Suggestions",
                "Provide a list of spelling suggestions for a word",
                new StringArgumentType(int.MaxValue, "\\w+", "bufallo"),
                StringSuggestionsResultType, 120)
        {
        }
				
        public IEnumerable<string> Handle(string input)
        {
            List<string> suggestions = new List<string>();
			
			hunspell.StandardInput.WriteLine(input);
			string result = hunspell.StandardOutput.ReadLine().Trim();
			hunspell.StandardOutput.ReadLine();
			
			if (result == "*")
				suggestions.Add(input);
			else if (result.StartsWith("&")) {
				string[] parts = Regex.Split(result.Substring(result.IndexOf(':') + 2), ", ");
				foreach (string part in parts)
					suggestions.Add(part);
			}

            return suggestions;
		}

        #region IUnitaryHandler Members

        public override object Handle(object arg)
        {
            return Handle((string)arg);
        }

        #endregion
		
        public void Prepare(string datadir)
        {
			ProcessStartInfo startInfo = new ProcessStartInfo {
		        FileName = "hunspell",
		        Arguments = String.Format("-d {0}/hunspell/en_US", datadir),
				RedirectStandardInput = true,
				RedirectStandardOutput = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};
			hunspell = Process.Start(startInfo);
			hunspell.StandardOutput.ReadLine();
		}

        public void Destroy()
        {
			hunspell.Kill();
        }
    }
}
