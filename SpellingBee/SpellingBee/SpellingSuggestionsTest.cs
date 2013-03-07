using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace SpellingBee
{
	[TestFixture()]
	public class SpellingSuggestionsTest
	{
		[Test()]
		public void TestCase()
		{
			SpellingSuggestionsHandler handler = new SpellingSuggestionsHandler();
			handler.Prepare("/Users/jrising/projects/virsona/github/data");
			foreach (string sugg in handler.Handle("bufallo"))
				Console.WriteLine(sugg);
			Console.WriteLine("Next");
			foreach (string sugg in handler.Handle("buffalo"))
				Console.WriteLine(sugg);
			handler.Destroy();
		}
	}
}

