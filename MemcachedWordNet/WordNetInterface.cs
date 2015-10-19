/******************************************************************\
 *      Class Name:     WordNetInterface
 *      Written By:     James Rising (borrowed from online source)
 *      Copyright:      2011, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * This file is part of MemcachedWordNet and is free software: you
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
 * License along with MemcachedWordNet.  If not, see
 * <http://www.gnu.org/licenses/>.
 *      -----------------------------------------------------------
 * This is the plugin interface for the datasources provided in
 * MemcachedWordNet.  It will load the index files into Memcached
 * if it doesn't see some of the data already there.
\******************************************************************/
using System;
using System.Reflection;
using System.Collections.Generic;
using System.IO;
using ActionReaction;
using PluggerBase;
using ActionReaction.FastSerializer;
using LanguageNet.Grammarian;
using GenericTools.DataSources;
using BeIT.MemCached;

namespace LanguageNet.WordNet
{
    [PluginDisplayName("WordNet")]
	public class WordNetInterface : IPlugin
	{
		protected BackedMemcachedSource<Index> nounIndexSource;
		protected BackedMemcachedSource<Index> verbIndexSource;
		protected BackedMemcachedSource<Index> adjIndexSource;
		protected BackedMemcachedSource<Index> advIndexSource;

		protected MapDataSource<string, Index, long[]> nounOffsetsSource;
		protected MapDataSource<string, Index, long[]> verbOffsetsSource;
		protected MapDataSource<string, Index, long[]> adjOffsetsSource;
		protected MapDataSource<string, Index, long[]> advOffsetsSource;

		protected DefinitionFile nounDefinitionSource;
		protected DefinitionFile verbDefinitionSource;
		protected DefinitionFile adjDefinitionSource;
		protected DefinitionFile advDefinitionSource;

		protected FileWordNetTools fileTools;

		public WordNetInterface()
		{
		}

        #region IPlugin Members

		public uint Version {
			get {
				return 2;
			}
		}

        public InitializeResult Initialize(PluginEnvironment env, Assembly assembly, IMessageReceiver receiver)
        {
			return InitializeLocal(env, receiver);
		}

        public InitializeResult InitializeLocal(PluginEnvironment env, IMessageReceiver receiver)
        {
			// Data files contained in [datadrectory]/wordnet
            string basedir = env.GetConfigDirectory("datadirectory") + Path.DirectorySeparatorChar + "wordnet" + Path.DirectorySeparatorChar;
			MemcachedClient cache = MemcacheSource.DefaultClient();

			nounIndexSource = new BackedMemcachedSource<Index>(new IndexFile(basedir, WordNetAccess.PartOfSpeech.Noun), "WN:I:N:", cache);
			verbIndexSource = new BackedMemcachedSource<Index>(new IndexFile(basedir, WordNetAccess.PartOfSpeech.Verb), "WN:I:V:", cache);
			adjIndexSource = new BackedMemcachedSource<Index>(new IndexFile(basedir, WordNetAccess.PartOfSpeech.Adj), "WN:I:A:", cache);
			advIndexSource = new BackedMemcachedSource<Index>(new IndexFile(basedir, WordNetAccess.PartOfSpeech.Adv), "WN:I:R:", cache);

			if (!advIndexSource.TestMemcached(10, 10)) {
				Console.Out.WriteLine("Loading nouns into Memcached");
				nounIndexSource.LoadIntoMemcached();
				Console.Out.WriteLine("Loading verbs into Memcached");
				verbIndexSource.LoadIntoMemcached();
				Console.Out.WriteLine("Loading adjectives into Memcached");
				adjIndexSource.LoadIntoMemcached();
				Console.Out.WriteLine("Loading adverbs into Memcached");
				advIndexSource.LoadIntoMemcached();
			}

			nounOffsetsSource = new MapDataSource<string, Index, long[]>(nounIndexSource, IndexFile.ExtractOffsets, null);
			verbOffsetsSource = new MapDataSource<string, Index, long[]>(verbIndexSource, IndexFile.ExtractOffsets, null);
			adjOffsetsSource = new MapDataSource<string, Index, long[]>(adjIndexSource, IndexFile.ExtractOffsets, null);
			advOffsetsSource = new MapDataSource<string, Index, long[]>(advIndexSource, IndexFile.ExtractOffsets, null);

            env.SetDataSource<string, long[]>(WordNetAccess.NounIndexSourceName, nounOffsetsSource);
            env.SetDataSource<string, long[]>(WordNetAccess.VerbIndexSourceName, verbOffsetsSource);
            env.SetDataSource<string, long[]>(WordNetAccess.AdjIndexSourceName, adjOffsetsSource);
            env.SetDataSource<string, long[]>(WordNetAccess.AdvIndexSourceName, advOffsetsSource);

			nounDefinitionSource = new DefinitionFile(basedir, WordNetAccess.PartOfSpeech.Noun);
			verbDefinitionSource = new DefinitionFile(basedir, WordNetAccess.PartOfSpeech.Verb);
			adjDefinitionSource = new DefinitionFile(basedir, WordNetAccess.PartOfSpeech.Adv);
			advDefinitionSource = new DefinitionFile(basedir, WordNetAccess.PartOfSpeech.Adv);

            env.SetDataSource<long, WordNetDefinition>(WordNetAccess.NounDefinitionSourceName, nounDefinitionSource);
            env.SetDataSource<long, WordNetDefinition>(WordNetAccess.VerbDefinitionSourceName, verbDefinitionSource);
            env.SetDataSource<long, WordNetDefinition>(WordNetAccess.AdjDefinitionSourceName, adjDefinitionSource);
            env.SetDataSource<long, WordNetDefinition>(WordNetAccess.AdvDefinitionSourceName, advDefinitionSource);

			fileTools = new FileWordNetTools(env.GetConfigDirectory("datadirectory") + Path.DirectorySeparatorChar + "wordnet" + Path.DirectorySeparatorChar);

			return InitializeResult.Success();
        }

        #endregion

		public FileWordNetTools FileTools {
			get {
				return fileTools;
			}
		}

		#region EncodeWord
		/// <summary>
		/// Converts a compound word/phrase into the recognized format
		/// </summary>
		/// <param name="data">The text to convert</param>
		/// <returns>A normalized string</returns>
		public static string EncodeWord( string data )
		{
			string retVal = string.Empty;
			retVal = data.Replace( ' ', '_' );
			return retVal.ToLower();
		}
		#endregion EncodeWord

        #region GetIndex
        /// <summary>
        /// Returns a list of the Index objects stored in the cache corresponding to the given string and part(s) of speech
        /// </summary>
        /// <param name="word">The string to use as a key</param>
        /// <param name="part">The part of speech limitations</param>
        /// <returns>The Index objects</returns>
        public List<Index> GetIndex(string word, WordNetAccess.PartOfSpeech part)
        {
            if (word.Length == 0)
                return new List<Index>();

            word = EncodeWord(word);

			List<Index> idxres = new List<Index>();

			if (part == WordNetAccess.PartOfSpeech.Adj || part == WordNetAccess.PartOfSpeech.All) {
				Index idxresAdj;
				if (adjIndexSource.TryGetValue(word, out idxresAdj)) {
					idxres.Add(idxresAdj);
					if (part == WordNetAccess.PartOfSpeech.Adj)
						return idxres;
				}
			}
			if (part == WordNetAccess.PartOfSpeech.Adv || part == WordNetAccess.PartOfSpeech.All) {
				Index idxresAdv;
				if (advIndexSource.TryGetValue(word, out idxresAdv)) {
					idxres.Add(idxresAdv);
					if (part == WordNetAccess.PartOfSpeech.Adv)
						return idxres;
				}
			}
			if (part == WordNetAccess.PartOfSpeech.Noun || part == WordNetAccess.PartOfSpeech.All) {
				Index idxresNoun;
				if (nounIndexSource.TryGetValue(word, out idxresNoun)) {
					idxres.Add(idxresNoun);
					if (part == WordNetAccess.PartOfSpeech.Noun)
						return idxres;
				}
			}
			if (part == WordNetAccess.PartOfSpeech.Verb || part == WordNetAccess.PartOfSpeech.All) {
				Index idxresVerb;
				if (verbIndexSource.TryGetValue(word, out idxresVerb)) {
					idxres.Add(idxresVerb);
					if (part == WordNetAccess.PartOfSpeech.Verb)
						return idxres;
				}
			}

			return idxres;
        }
        #endregion GetIndex

        #region IFastSerializable Members

        public void Deserialize(SerializationReader reader)
        {
            // do nothing
        }

        public void Serialize(SerializationWriter writer)
        {
            // do nothing
        }

        #endregion

        #region IMessageReceiver Members

        public bool Receive(string message, object reference)
        {
            return false;
        }

        #endregion
	}
}

