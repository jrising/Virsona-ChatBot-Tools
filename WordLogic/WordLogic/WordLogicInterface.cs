/******************************************************************\
 *      Class Name:     WordLogicInterface
 *      Written By:     James Rising
 *      Copyright:      2013
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
\******************************************************************/

using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using PluggerBase;
using PluggerBase.FastSerializer;
using BeIT.MemCached;
using GenericTools.DataSources;

namespace LanguageNet.WordLogic
{
    [PluginDisplayName("WordLogic")]
    public class WordLogicInterface : IPlugin
    {
        #region IPlugin Members

        // Add the plugins actions and data source
        public InitializeResult Initialize(PluginEnvironment env, Assembly assembly, IMessageReceiver receiver)
        {
            // Data files contained in [datadrectory]/parser
            string morphodir = env.GetConfigDirectory("datadirectory") + Path.DirectorySeparatorChar + "morpho" + Path.DirectorySeparatorChar;
			MemcachedClient cache = MemcacheSource.DefaultClient();

            GivenNames names = new GivenNames(morphodir, cache);

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
