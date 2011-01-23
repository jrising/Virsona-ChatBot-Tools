/******************************************************************\
 *      Class Name:     AgentParserInterface
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Exposes the datasource and handlers for the Agent Parser plugin
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Reflection;
using PluggerBase;
using PluggerBase.FastSerializer;

namespace LanguageNet.AgentParser
{
	[PluginDisplayName("AgentParser")]
	public class AgentParserHandler : IPlugin
	{
		// A data source with the type for a given part of speech name (e.g. NN)
        public const string PartTypeSourceName = "AgentParser:PartTypeSource";
		
        #region IPlugin Members

        // Add the plugins actions and data source
        public InitializeResult Initialize(PluginEnvironment env, Assembly assembly, IMessageReceiver receiver)
        {
			env.SetDataSource(PartTypeSourceName, new Sentence(new List<KeyValuePair<string, string>>()));
            env.AddAction(new StringParseHandler(env));
            env.AddAction(new EnumerableParseHandler());
            env.AddAction(new ParaphraseHandler(env));

            return InitializeResult.Success();
        }

        #endregion

        #region IMessageReceiver Members

        public bool Receive(string message, object reference)
        {
            return false;
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
	}
}

