/******************************************************************\
 *      Class Name:     HeppleTaggerInterface
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Main plugin class, hooking up EnumerableTagHandler,
 *   StringTagHandler, and PhraseResolveHandler actions, and
 *   the part of speech lexicon source in POSTagger
\******************************************************************/
using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using PluggerBase;
using PluggerBase.FastSerializer;

namespace HeppleTagger
{
    [PluginDisplayName("HeppleTagger")]
    public class HeppleTaggerInterface : IPlugin
    {
        #region IPlugin Members

        // Add the plugins actions and data source
        public InitializeResult Initialize(PluginEnvironment env, Assembly assembly, IMessageReceiver receiver)
        {
            // Data files contained in [datadrectory]/parser
            string parserdir = env.GetConfigDirectory("datadirectory") + Path.DirectorySeparatorChar + "parser" + Path.DirectorySeparatorChar;
            POSTagger tagger = new POSTagger(parserdir + "lexicon_all", parserdir + "ruleset", assembly, null);

            env.SetDataSource<string, string[]>(LanguageNet.Grammarian.POSTagger.PartsSourceName, tagger);
            env.AddAction(new EnumerableTagHandler(tagger));
            env.AddAction(new PhraseResolveHandler(tagger));
            env.AddAction(new StringTagHandler(tagger));

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
