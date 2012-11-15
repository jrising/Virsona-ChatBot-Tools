using System;
using System.IO;
using PluggerBase;

namespace InOutTools
{
	public class ToolArguments : CommandLineArguments
	{
		protected PluginEnvironment plugenv;
		protected TextReader input;
		
		public ToolArguments(string[] args, string docsUrl, IMessageReceiver receiver)
			: base(args) {
			if (Parameters["h"] != null || Parameters["help"] != null)
				Console.WriteLine("The documentation is currently at \n" + docsUrl);
			if (Parameters["c"] == null && Parameters["conf"] == null) {
				Console.WriteLine("The -c/-conf argument is required.  See\n" + docsUrl);
				return;
			}
			
			plugenv = new PluginEnvironment(receiver);
			string config = Parameters["c"] == null ? Parameters["conf"] : Parameters["c"];
			if (!File.Exists(config)) {
				Console.WriteLine("Cannot find configuration file at " + config);
				return;
			}
			
            plugenv.Initialize(config, null);

			if (Parameters["i"] != null)
				input = new StringReader(Parameters["i"]);
			else if (Parameters["if"] != null)
				input = new StreamReader(Parameters["if"]);
		}
		
		public PluginEnvironment PlugEnv {
			get {
				return plugenv;
			}
		}
		
		public TextReader Input {
			get {
				return input;
			}
		}
	}
}

