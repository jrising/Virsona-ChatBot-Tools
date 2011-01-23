/******************************************************************\
 *      Class Name:     PluginEnvironment
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * This file is part of Plugger Base and is free software: you can
 * redistribute it and/or modify it under the terms of the GNU
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
 * License along with Plugger Base.  If not, see
 * <http://www.gnu.org/licenses/>.
 *      -----------------------------------------------------------
 * The shared state available to all plugins
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Xml;
using PluggerBase.ActionReaction.Actions;
using PluggerBase.ActionReaction.Interfaces;
using PluggerBase.ActionReaction.Evaluations;

namespace PluggerBase
{
    // The plugin environment combines a globally accessible environment
    public class PluginEnvironment : ICloneable
    {
        protected IMessageReceiver receiver;

        protected List<IPlugin> plugins;
        protected ArgumentTree config;

        // tree of sources
        protected Dictionary<string, IDataSource> sources;
        // ICallables and IHandlers, keyed by named result types (or "" for unnamed)
        protected Dictionary<string, List<IAction>> actions;

        protected Dictionary<string, List<IMessageReceiver>> hooks;

        public PluginEnvironment(IMessageReceiver receiver)
        {
            this.receiver = receiver;

            plugins = new List<IPlugin>();
            config = new ArgumentTree(null);
            sources = new Dictionary<string, IDataSource>();
            actions = new Dictionary<string, List<IAction>>();
            actions[""] = new List<IAction>();

            hooks = new Dictionary<string, List<IMessageReceiver>>();
        }

        public List<IPlugin> Plugins
        {
            get
            {
                return plugins;
            }
        }

        public ArgumentTree Configuration
        {
            get
            {
                return config;
            }
        }

        public IDictionary<string, IDataSource> Sources
        {
            get
            {
                return sources;
            }
        }

        public IDictionary<string, List<IAction>> Actions
        {
            get
            {
                return actions;
            }
        }

        public void Initialize(string conffile, string plugindir, NameValueCollection args)
        {
            LoadConfigurationFile(conffile);

            // overwrite it with supplied arguments
            if (args != null)
                foreach (string key in args.AllKeys)
                    SetConfig(key, args[key]);

            // load all the plugins
            LoadPlugins(plugindir);

        }

        public object GetConfig(string key)
        {
            ArgumentTree node = null;
            if (!config.Children.TryGetValue(key, out node))
                return null;
            else
                return node.Value;
        }

        public void SetConfig(string key, object value)
        {
            ArgumentTree node = null;
            if (!config.Children.TryGetValue(key, out node))
                config.Children[key] = new ArgumentTree(value);
            else
                node.Value = value;
        }

        protected void LoadConfigurationFile(string conffile)
        {
            // first load the config file
            XmlDocument doc = new XmlDocument();
            doc.Load(conffile);
            config = ArgumentTree.LoadFromXml(doc.DocumentElement);
        }

        protected void LoadPlugins(string plugindir)
        {
            // First load all dlls
            string[] dlls = Directory.GetFiles(plugindir, "*.dll");

            foreach (string dll in dlls)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(dll);
                }
                catch (Exception e)
                {
                    receiver.Receive(e.Message, this);
                }
            }

            Queue<KeyValuePair<IPlugin, Assembly>> toloads = new Queue<KeyValuePair<IPlugin, Assembly>>();
            
            string[] files = Directory.GetFiles(plugindir, "*.vip");

            foreach (string file in files)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFrom(file);
                    Type[] types = assembly.GetTypes();

                    foreach (Type type in types)
                    {
                        if (type.GetInterface("IPlugin") != null)
                        {
                            if (type.GetCustomAttributes(typeof(PluginDisplayNameAttribute), false).Length != 1)
                                throw new PluginNotValidException(type, "PluginDisplayNameAttribute not supported");

                            toloads.Enqueue(new KeyValuePair<IPlugin, Assembly>((IPlugin)Activator.CreateInstance(type), assembly));
                        }
                    }
                }
                catch (Exception e)
                {
                    receiver.Receive(e.Message, this);
                }
            }

            // Now load all the plugins
            int sinceSuccess = 0;
            while (toloads.Count > sinceSuccess / 2)
            {
                KeyValuePair<IPlugin, Assembly> toload = toloads.Dequeue();
                IPlugin plugin = toload.Key;
                InitializeResult result = plugin.Initialize(this, toload.Value, receiver);
                if (result.IsSuccess)
                {
                    plugins.Add(plugin);
                    sinceSuccess = 0;
                }
                else if (result.IsTryAgain)
                {
                    toloads.Enqueue(toload);
                    sinceSuccess++;
                }
            }
        }

        public void SetDataSource<TKey, TValue>(string name, IDataSource<TKey, TValue> source)
        {
            sources[name] = source;
        }

        public IDataSource<TKey, TValue> GetDataSource<TKey, TValue>(string name)
        {
            IDataSource source = null;
            if (sources.TryGetValue(name, out source))
                return (IDataSource<TKey, TValue>) source;

            return null;
        }

        public void AddAction(IAction action)
        {
            string resultname = action.Output.Name;
            // Add this to the possible actions to produce this
            GetNamedActions(resultname).Add(action);
        }

        public object ImmediateConvertTo(object args, IArgumentType resultType, int maxlevel, int time)
        {
            return QueueArena.CallResult(CallableConvertTo(resultType, maxlevel), args, time, 0.0);
        }

        public ICallable CallableConvertTo(IArgumentType resultType, int maxlevel)
        {
            return new ActionConversion(this, resultType, new Dictionary<IAction,int>(), Aborter.NewAborter(maxlevel));
        }

        public List<IAction> GetNamedActions(string name)
        {
            if (string.IsNullOrEmpty(name))
                return actions[""];
            else
            {
                List<IAction> namedacts = null;
                if (!actions.TryGetValue(name, out namedacts))
                    actions[name] = namedacts = new List<IAction>();

                return namedacts;
            }
        }

        public void CallHooks(string key, bool forcehandle, string message, object reference)
        {
            List<IMessageReceiver> hookrecs = GetHooks(key);
            foreach (IMessageReceiver hookrec in hookrecs)
                if (hookrec.Receive(message, reference))
                    forcehandle = false;

            if (forcehandle)
                receiver.Receive(message, reference);
        }

        protected List<IMessageReceiver> GetHooks(string key)
        {
            List<IMessageReceiver> receivers = null;
            if (hooks.TryGetValue(key, out receivers))
                return receivers;

            return new List<IMessageReceiver>();
        }

        public void AddHook(string key, IMessageReceiver receiver)
        {
            List<IMessageReceiver> receivers = null;
            if (!hooks.TryGetValue(key, out receivers))
                receivers = hooks[key] = new List<IMessageReceiver>();

            receivers.Add(receiver);
        }

        public void RemoveHook(string key, IMessageReceiver receiver)
        {
            List<IMessageReceiver> receivers = null;
            if (!hooks.TryGetValue(key, out receivers))
                return;

            receivers.Remove(receiver);
        }

        #region ICloneable Members

        public object Clone()
        {
            PluginEnvironment copy = (PluginEnvironment)MemberwiseClone();
            copy.config = (ArgumentTree) config.Clone();
            copy.sources = new Dictionary<string, IDataSource>(sources);

            copy.actions = new Dictionary<string, List<IAction>>();
            foreach (KeyValuePair<string, List<IAction>> namedacts in actions)
                copy.actions[namedacts.Key] = new List<IAction>(namedacts.Value);

            copy.hooks = new Dictionary<string, List<IMessageReceiver>>();
            foreach (KeyValuePair<string, List<IMessageReceiver>> hook in hooks)
                copy.hooks[hook.Key] = new List<IMessageReceiver>(hook.Value);

            return copy;
        }

        #endregion
    }
}
