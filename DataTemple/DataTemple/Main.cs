using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using PluggerBase;
using PluggerBase.ActionReaction.Actions;
using PluggerBase.ActionReaction.Evaluations;
using LanguageNet.Grammarian;
using DataTemple.Codeland;
using DataTemple.AgentEvaluate;
using DataTemple.Variables;
using DataTemple.Matching;

namespace DataTemple
{
	class MainClass : IMessageReceiver, IContinuation
	{
		protected static string DOCS_URL = "https://github.com/jrising/Virsona-ChatBot-Tools/wiki/DataTemple-Command-Line-Tool";
		protected bool verbose;
		
		public static void Main(string[] args)
		{
			CommandLineArguments parsedArgs = new CommandLineArguments(args);
			MainClass main = new MainClass(parsedArgs);
			if (main.verbose)
				Console.WriteLine("Initializing...");
			
			if (parsedArgs["h"] != null || parsedArgs["help"] != null)
				Console.WriteLine("The documentation is currently at \n" + DOCS_URL);
			if (parsedArgs["c"] == null && parsedArgs["conf"] == null) {
				Console.WriteLine("The -c/-conf argument is required.  See\n" + DOCS_URL);
				return;
			}
			
			string input = "";
			if (parsedArgs["i"] != null)
				input = parsedArgs["i"];
			else if (parsedArgs["if"] != null) {
				StreamReader file = new StreamReader(parsedArgs["if"]);
				input = file.ReadToEnd();
			}	
			
			string template = "";
			if (parsedArgs["t"] != null)
				template = parsedArgs["t"];
			
			string command = "";
			if (parsedArgs["o"] != null)
				command = parsedArgs["o"];
						
			PluginEnvironment plugenv = new PluginEnvironment(main);
			string config = parsedArgs["c"] == null ? parsedArgs["conf"] : parsedArgs["c"];
            plugenv.Initialize(config, new NameValueCollection());
			
			GrammarParser parser = new GrammarParser(plugenv);
			if (main.verbose)
				Console.WriteLine("Parsing input...");
			IParsedPhrase phrase = parser.Parse(input);

			if (main.verbose)
				Console.WriteLine("Matching templates...");

			ZippyCoderack coderack = new ZippyCoderack(main, 10000000);
			Memory memory = new Memory();
			
            Context basectx = new Context(coderack);
			GrammarVariables.LoadVariables(basectx, 100.0, memory, plugenv);
			OutputVariables.LoadVariables(basectx, 100.0, plugenv);
			
            List<PatternTemplateSource> dicta = new List<PatternTemplateSource>();
            DictumMaker maker = new DictumMaker(basectx, "testing");
            dicta.Add(maker.MakeDictum(template, command));
            /*dicta.Add(maker.MakeDictum("%sentence %noun %is %adj", "@know %noun @HasProperty %adj @SubjectTense %is"));
            dicta.Add(maker.MakeDictum("%sentence %event %attime", "@know %event @AtTime %attime"));
            dicta.Add(maker.MakeDictum("%sentence %event %inall", "@know %event @InLocation %inall"));
            dicta.Add(maker.MakeDictum("%sentence %noun %inall", "@know %noun @InLocation %inall"));
            dicta.Add(maker.MakeDictum("%sentence %noun %is a %noun", "@know %noun1 @IsA %noun2 @SubjectTense %is"));
            dicta.Add(maker.MakeDictum("%sentence %noun %will %verb1 * to %verb2 *", "@know %verbx1 @Subject %noun @SubjectTense %will %verb1 @ActiveObjects *1 @know %verbx2 @Subject %noun @SubjectTense %verb2 @ActiveObjects *2 @know %verbx2 @Condition %verbx1"));*/
			
            // Add a codelet for each of these, to match the input
            foreach (PatternTemplateSource dictum in dicta)
                dictum.Generate(coderack, phrase, main, new NopCallable(), 1.0);

            coderack.Execute(1000000, 1.0, true);
		}		
		
		public MainClass(CommandLineArguments parsedArgs) {
			verbose = (parsedArgs["v"] != null || parsedArgs["verbose"] != null);
		}
		
		public MainClass() {
			verbose = false;
		}
		
		public object Clone()
		{
			MainClass clone = new MainClass();
			clone.verbose = verbose;
			return clone;
		}
		
        #region IContinuation Members

        public int Continue(object value, IFailure fail)
        {
			Context context = (Context) value;
            if (!context.IsEmpty)
				Console.WriteLine(context.ContentsCode());
			
			return 1;
        }

        #endregion

		
		/* Formation Code 1
		public static void Main(string[] args)
		{
			TempleMatcher matcher = TempleMatcher.Interpret(plugenv, "a *"); //args[1]);
			List<PatternMatch> matches = matcher.AllMatches(new AmbiguousPhrase("This is a test.")); //args[2]));
			
			foreach (PatternMatch match in matches)
				match.Display(Console.Out);
				
			Console.WriteLine("Complete");			
		}*/
		
		/* Formation Code 2
		public static void Main(string[] args)
		{
			PluginEnvironment plugenv = new PluginEnvironment(new MainClass());
			string plugbase = "/Users/jrising/projects/virsona/github/data";
            plugenv.Initialize(plugbase + "/config.xml", plugbase, new NameValueCollection());

			matcher.Match(args[2], DisplayExtract, DisplayComplete);
		}
		
		// This is a Continuelet
		int DisplayExtract(IArena arena, double salience, object value, IFailure fail, params object[] args)
		{
			PatternMatch match = (PatternMatch) value;
			match.Display(Console.Out);

			arena.Fail(fail, salience, "find the next match", null);
			
			return 1;
		}

		// This is a Faillet
		int DisplayComplete(IArena arena, double salience, string reason, IContinuation skip, params object[] args)
		{
			Console.WriteLine("No more matches: " + reason);
			
			return 1;
		}*/

		// IMessageReceiver
		
		public bool Receive(string message, object reference) {
			if (verbose) {
				if (message == "EvaluateCodelet")
					Console.WriteLine("Codelet " + reference.ToString()); //reference.GetType().Name);
				else
					Console.WriteLine(message);
			}
			return true;
		}

	}
}
