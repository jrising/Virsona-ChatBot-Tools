/******************************************************************\
 *      Class Name:     WordNetAccess
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Utilities to access WordNet and the plugin functions provided
 * for it.
\******************************************************************/
using System;
using System.Collections.Generic;
using PluggerBase;

namespace LanguageNet.Grammarian
{
	public class WordNetAccess
	{
        public const string NounIndexSourceName = "WordNet:NounIndexSource";
        public const string VerbIndexSourceName = "WordNet:VerbIndexSource";
        public const string AdjIndexSourceName = "WordNet:AdjIndexSource";
        public const string AdvIndexSourceName = "WordNet:AdvIndexSource";
        
		public const string NounDefinitionSourceName = "WordNet:NounDefinitionSource";
        public const string VerbDefinitionSourceName = "WordNet:VerbDefinitionSource";
        public const string AdjDefinitionSourceName = "WordNet:AdjDefinitionSource";
        public const string AdvDefinitionSourceName = "WordNet:AdvDefinitionSource";
		
        public enum PartOfSpeech
        {
            Noun, Verb, Adj, Adv, Satellite, All, AdjSat
		}
		
		protected PluginEnvironment plugenv;
		
		public WordNetAccess(PluginEnvironment plugenv)
		{
			this.plugenv = plugenv;
		}
		
		public string GetIndexSourceName(PartOfSpeech part) {
			if (part == WordNetAccess.PartOfSpeech.Noun)
				return NounIndexSourceName;
			else if (part == WordNetAccess.PartOfSpeech.Verb)
				return VerbIndexSourceName;
			else if (part == WordNetAccess.PartOfSpeech.Adj)
				return AdjIndexSourceName;
			else if (part == WordNetAccess.PartOfSpeech.Adv)
				return AdvIndexSourceName;
			
			return null;
		}
		
		public string GetDefinitionSourceName(PartOfSpeech part) {
			if (part == WordNetAccess.PartOfSpeech.Noun)
				return NounDefinitionSourceName;
			else if (part == WordNetAccess.PartOfSpeech.Verb)
				return VerbDefinitionSourceName;
			else if (part == WordNetAccess.PartOfSpeech.Adj)
				return AdjDefinitionSourceName;
			else if (part == WordNetAccess.PartOfSpeech.Adv)
				return AdvDefinitionSourceName;
			
			return null;
		}

		public List<string> GetExactSynonyms(string word, PartOfSpeech part) {
			if (part == PartOfSpeech.All) {
				List<string> result = new List<string> ();
				List<string> resultAdj = GetExactSynonyms (word, PartOfSpeech.Adj);
				if (resultAdj != null)
					result.AddRange (resultAdj);
				List<string> resultAdv = GetExactSynonyms (word, PartOfSpeech.Adv);
				if (resultAdv != null)
					result.AddRange (resultAdv);
				List<string> resultNoun = GetExactSynonyms (word, PartOfSpeech.Noun);
				if (resultNoun != null)
					result.AddRange (resultNoun);
				List<string> resultVerb = GetExactSynonyms (word, PartOfSpeech.Verb);
				if (resultVerb != null)
					result.AddRange (resultVerb);
				return result;
			}

			long[] indices;
			
			IDataSource<string, long[]> indexSource = plugenv.GetDataSource<string, long[]>(GetIndexSourceName(part));
			if (indexSource == null)
				throw new Exception("Cannot find index for " + GetIndexSourceName (part));
			
			if (!indexSource.TryGetValue(word, out indices) || indices.Length != 1)
				return null; // missing or ambiguous
			
			WordNetDefinition definition;
			
			IDataSource<long, WordNetDefinition> definitionSource = plugenv.GetDataSource<long, WordNetDefinition>(GetDefinitionSourceName(part));
			if (definitionSource == null)
				throw new Exception("Cannot find definitions for " + GetDefinitionSourceName(part));
			if (!definitionSource.TryGetValue(indices[0], out definition))
				return null; // missing
			
			return definition.Words;
		}
	}
	
}

