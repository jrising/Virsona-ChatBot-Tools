using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using ActionReaction;
using PluggerBase;
using LanguageNet.Grammarian;

namespace Commander
{
	class MainClass : IMessageReceiver
	{
		WordNetAccess wordnet;

		public static void Main (string[] args)
		{
			if (args.Length < 1) {
				Console.WriteLine ("Call with the path to config.xml.");
				return;
			}
			MainClass instance = new MainClass ();
			instance.Run (args[0]);
		}

		public void Run(string configpath) {
			PluginEnvironment plugenv = new PluginEnvironment(this);

			plugenv.Initialize(configpath, null);
			Console.WriteLine (plugenv.GetConfigDirectory ("datadirectory"));

			Console.WriteLine ("Welcome to the Virsona Commander!");
			bool running = true;
			while (running) {
				Console.Write ("(: ");
				string command = Console.ReadLine ();
				string[] argv = Regex.Split (command, "\\s+");

				switch(argv[0]) {
				case "hepple":
					{
						string sentence = string.Join (" ", new ArraySegment<string> (argv, 1, argv.Length - 1));
						object result = plugenv.ImmediateConvertTo (sentence,
							                LanguageNet.Grammarian.POSTagger.TagEnumerationResultType, 1, 1000);
						if (result is Exception) {
							Console.WriteLine (((Exception)result).Message);
							Console.WriteLine (((Exception)result).StackTrace);
							continue;
						}
						Console.WriteLine (result);
						string output = string.Join (" ", (IEnumerable<string>)result);
						Console.WriteLine (output);
						break;
					}
				case "synonyms":
					{
						if (wordnet == null)
							wordnet = new WordNetAccess (plugenv);
						List<string> synonyms = wordnet.GetExactSynonyms (argv [1], WordNetAccess.PartOfSpeech.All);
						if (synonyms.Count == 0)
							Console.WriteLine ("None.");
						else {
							string output = string.Join (" ", synonyms);
							Console.WriteLine (output);
						}
						break;
					}
				case "quit":
					Console.WriteLine ("Goodbye!");
					running = false;
					break;
				default:
					Console.WriteLine ("Unknown command: " + argv [0]);
					break;
				} 
			}
		}

		public bool Receive(string message, object reference) {
			Console.WriteLine ("[" + message + "]");
			return true;
		}
	}
}
