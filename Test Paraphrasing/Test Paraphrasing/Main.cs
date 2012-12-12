using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using PluggerBase;
using LanguageNet.Grammarian;

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
			IParsedPhrase after = parser.Paraphrase(before, null, null, randgen.NextDouble());
			Console.WriteLine(after.Text);
						
			// Test 4: Look up some indices
			WordNetAccess wordnet = new WordNetAccess(plugenv);
			Console.WriteLine("Synonyms: " + string.Join(", ", wordnet.GetExactSynonyms("rug", WordNetAccess.PartOfSpeech.Noun).ToArray()));
		}
		
		public MainClass() {
		}
		
		public bool Receive(string message, object reference) {
			Console.WriteLine(message);
			return true;
		}
	}
}

