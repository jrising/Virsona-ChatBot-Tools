/******************************************************************\
 *      Class Name:     IPlugin
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * The basic interface implemented by all plugins
\******************************************************************/
using System;
using System.Reflection;
using PluggerBase.FastSerializer;

namespace PluggerBase
{
    public interface IPlugin : IMessageReceiver, IFastSerializable
    {
        InitializeResult Initialize(PluginEnvironment env, Assembly assembly, IMessageReceiver receiver);
    }
}
