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
            Noun, Verb, Adj, Adv, Satellite, All
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
			long[] indices;
			
			IDataSource<string, long[]> indexSource = plugenv.GetDataSource<string, long[]>(GetIndexSourceName(part));
			if (indexSource == null || !indexSource.TryGetValue(word, out indices) || indices.Length != 1)
				return null; // missing or ambiguous
			
			WordNetDefinition definition;
			
			IDataSource<long, WordNetDefinition> definitionSource = plugenv.GetDataSource<long, WordNetDefinition>(GetDefinitionSourceName(part));
			if (definitionSource == null || !definitionSource.TryGetValue(indices[0], out definition))
				return null; // missing
			
			return definition.Words;
		}
	}
	
}

