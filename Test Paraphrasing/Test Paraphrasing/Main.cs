using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using ActionReaction;
using PluggerBase;
using LanguageNet.Grammarian;
using LanguageNet.AgentParser; // XXX

namespace TestParaphrasing
{
	class MainClass : IMessageReceiver
	{
		public static void Main (string[] args)
		{
			PluginEnvironment plugenv = new PluginEnvironment(new MainClass());
			string plugbase = "/Users/jrising/projects/virsona/github";
            plugenv.Initialize(plugbase + "/config.xml", new NameValueCollection());
			
			// Test 1: POS Tagging
			POSTagger tagger = new POSTagger(plugenv);
			List<KeyValuePair<string, string>> tagged = tagger.TagList(StringUtilities.SplitWords("This is a test.", false));
			foreach (KeyValuePair<string, string> kvp in tagged)
				Console.WriteLine(kvp.Key + ": " + kvp.Value);
			
			// Test 2: Grammar parsing
			GrammarParser parser = new GrammarParser(plugenv);
			IParsedPhrase before = parser.Parse("This is a rug and a keyboard.");
			Console.WriteLine(before.ToString());
			
			// Test 5: Pluralize nouns and conjugate verbs
			Nouns nouns = new Nouns(plugenv);
			Console.WriteLine("person becomes " + nouns.Pluralize("person"));
			Verbs verbs = new Verbs(plugenv);
			Console.WriteLine("goes becomes " + verbs.ComposePast("goes"));
			Console.WriteLine("eats becomes " + verbs.ComposePrespart(verbs.InputToBase("eats")));

			// Test 3: Paraphrasing
			Random randgen = new Random();
			try {
				IParsedPhrase after = parser.Paraphrase(before, null, null, randgen.NextDouble());
				Console.WriteLine(after.Text);
			} catch (Exception ex) {
				Console.WriteLine ("Error: " + ex.Message);
			}
						
			// Test 4: Look up some indices
			WordNetAccess wordnet = new WordNetAccess(plugenv);
			List<string> synonyms = null;
			try {
				synonyms = wordnet.GetExactSynonyms("rug", WordNetAccess.PartOfSpeech.Noun);
			} catch (Exception ex) {
				Console.WriteLine ("Error: " + ex.Message);
			}
			if (synonyms == null)
				Console.WriteLine("Could not find a synonym for 'rug'.  Is Memcached installed?");
			else
				Console.WriteLine("Synonyms: " + string.Join(", ", synonyms.ToArray()));
		}
		
		public MainClass() {
		}
		
		public bool Receive(string message, object reference) {
			Console.WriteLine(message);
			return true;
		}
	}
}

