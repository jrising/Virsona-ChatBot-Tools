/******************************************************************\
 *      Class Name:     DummyWordNetInterface
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Create and expose synonym sources
\******************************************************************/
using System;
using System.Reflection;
using System.Collections.Generic;
using PluggerBase;
using ActionReaction;
using ActionReaction.FastSerializer;
using LanguageNet.Grammarian;

namespace DummyWordNet
{
    [PluginDisplayName("DummyWordNet")]
	public class DummyWordNetInterface : IPlugin
	{
		protected SynonymSource nounSynonyms;
		protected SynonymSource verbSynonyms;
		protected SynonymSource adjSynonyms;
		protected SynonymSource advSynonyms;

		public DummyWordNetInterface()
		{
			nounSynonyms = new SynonymSource();
			verbSynonyms = new SynonymSource();
			adjSynonyms = new SynonymSource();
			advSynonyms = new SynonymSource();

			nounSynonyms.Add("test", new string[] {"exam", "quiz"});
		}

        #region IPlugin Members

		public uint Version {
			get {
				return 1;
			}
		}

        public InitializeResult Initialize(PluginEnvironment env, Assembly assembly, IMessageReceiver receiver)
        {
            // register data sources for WordNet
            env.SetDataSource<string, long[]>(WordNetAccess.NounIndexSourceName, nounSynonyms);
            env.SetDataSource<string, long[]>(WordNetAccess.VerbIndexSourceName, verbSynonyms);
            env.SetDataSource<string, long[]>(WordNetAccess.AdjIndexSourceName, adjSynonyms);
            env.SetDataSource<string, long[]>(WordNetAccess.AdvIndexSourceName, advSynonyms);
            env.SetDataSource<long, WordNetDefinition>(WordNetAccess.NounDefinitionSourceName, nounSynonyms);
            env.SetDataSource<long, WordNetDefinition>(WordNetAccess.VerbDefinitionSourceName, verbSynonyms);
            env.SetDataSource<long, WordNetDefinition>(WordNetAccess.AdjDefinitionSourceName, adjSynonyms);
            env.SetDataSource<long, WordNetDefinition>(WordNetAccess.AdvDefinitionSourceName, advSynonyms);

			return InitializeResult.Success();
        }

        #endregion

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

