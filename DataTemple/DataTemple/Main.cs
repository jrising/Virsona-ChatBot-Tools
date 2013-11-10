using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.Specialized;
using Gnu.Getopt;
using PluggerBase;
using PluggerBase.ActionReaction.Actions;
using PluggerBase.ActionReaction.Evaluations;
using LanguageNet.Grammarian;
using DataTemple.Codeland;
using DataTemple.AgentEvaluate;
using DataTemple.Variables;
using DataTemple.Matching;
using InOutTools;

namespace DataTemple
{
	class MainClass : IMessageReceiver, IContinuation
	{
		protected static string DOCS_URL = "https://github.com/jrising/Virsona-ChatBot-Tools/wiki/DataTemple-Command-Line-Tool";

		protected bool initialized;		
		protected int verbose;
		protected bool serialmode;
		protected TryToRescueMatch tryToRescueMatch; // succ is a full-match, fail is no-match
		
		protected PluginEnvironment plugenv;
		protected POSTagger tagger;
		protected GrammarParser parser;
		
		protected Coderack coderack;
		protected Memory memory;
		protected Context basectx;
				
		public static void Main(string[] args)
		{
			int c;
			MainClass main = new MainClass();

			LongOpt[] longopts = new LongOpt[] {
				new LongOpt("help", Argument.No, null, 'h'),
				new LongOpt("verbose", Argument.No, null, 'v'),
				new LongOpt("conf", Argument.Required, null, 'c'),
				new LongOpt("tag", Argument.No, null, 2),
				new LongOpt("parse", Argument.No, null, 3),
				new LongOpt("knows", Argument.No, null, 4),
				new LongOpt("spell", Argument.No, null, 'z')
			};
			Getopt g = new Getopt("DataTemple", args, "hvszc:I:P:O:T:i:p:t:", longopts);
			
			bool acted = false;
			List<PatternTemplateSource> dicta = new List<PatternTemplateSource>();

			string input = null;
			string output = null;
			string template = null;
			while ((c = g.getopt()) != -1)
				switch (c) {
				case 1: {
					Console.WriteLine("I see you have return in order set and that " +
						"a non-option argv element was just found " +
						"with the value '" + g.Optarg + "'");
					break;
				}
				case 2: {
					acted = true;
					if (main.tagger == null) {
						Console.WriteLine("Use the -c option before --tag or --parse");
						continue;
					}
					List<KeyValuePair<string, string>> tokens = main.tagger.TagString(input);
					foreach (KeyValuePair<string, string> token in tokens)
						Console.Write(token.Key + "/" + token.Value + " ");
					Console.WriteLine("");
					break;
				}
				case 3: {
					acted = true;
					if (main.parser == null) {
						Console.WriteLine("Use the -c option before --tag or --parse");
						continue;
					}
					Console.WriteLine(main.parser.Parse(input));
					break;
				}
				case 4: {
					DictumMaker maker = new DictumMaker(main.basectx, "testing");
					dicta.Add(maker.MakeDictum("%sentence %noun %is %adj", "@know %noun @HasProperty %adj @SubjectTense %is"));
		            dicta.Add(maker.MakeDictum("%sentence %event %attime", "@know %event @AtTime %attime"));
        		    dicta.Add(maker.MakeDictum("%sentence %event %inall", "@know %event @InLocation %inall"));
            		dicta.Add(maker.MakeDictum("%sentence %noun %inall", "@know %noun @InLocation %inall"));
    		        dicta.Add(maker.MakeDictum("%sentence %noun %is a %noun", "@know %noun1 @IsA %noun2 @SubjectTense %is"));
            		dicta.Add(maker.MakeDictum("%sentence %noun %will %verb1 * to %verb2 *", "@know %verbx1 @Subject %noun @SubjectTense %will %verb1 @ActiveObjects *1 @know %verbx2 @Subject %noun @SubjectTense %verb2 @ActiveObjects *2 @know %verbx2 @Condition %verbx1"));
					break;
				}
				case 'v': {
					main.verbose++;
					break;
				}
				case 'h': {
					Console.WriteLine("The documentation is currently at \n" + DOCS_URL);
					break;
				}
				case 's': {
					main.serialmode = true;
					break;
				}
				case 'z': {
					if (!main.initialized) {
						Console.WriteLine("Use the -c option before -z");
						continue;
					}
					
					SpellingBeeWordComparer wordComparer = new SpellingBeeWordComparer(main.plugenv.GetConfigDirectory("datadirectory"));
					main.basectx.Map["$Compare"] = wordComparer;
					main.tryToRescueMatch = new CorrectSpellingsRescueMatch(main.tryToRescueMatch, wordComparer, main.parser, 100);
					break;
				}
				case 'c': {
					main.Initialize(g.Optarg);
					break;
				}
				case 'I': {
					input = g.Optarg;
					break;
				}
				case 'i': {
					StreamReader file = new StreamReader(g.Optarg);
					input = file.ReadToEnd();
					break;
				}
				case 'P': {
					if (!main.initialized) {
						Console.WriteLine("Use the -c option before -P");
						continue;
					}
					Context context = Interpreter.ParseCommands(main.basectx, g.Optarg);
    	            IContinuation cont = new Evaluator(100.0, ArgumentMode.ManyArguments, main, new NopCallable(), true);
		            cont.Continue(context, new NopCallable());

					main.RunToEnd();
					break;
				}
				case 'p': {
					if (!main.initialized) {
						Console.WriteLine("Use the -c option before -p");
						continue;
					}
					foreach (string line in File.ReadAllLines(g.Optarg)) {
						if (line.Trim().Length == 0 || line.Trim().StartsWith("#"))
							continue;
						Context context = Interpreter.ParseCommands(main.basectx, line);
	    	            IContinuation cont = new Evaluator(100.0, ArgumentMode.ManyArguments, main, new NopCallable(), true);
			            cont.Continue(context, new NopCallable());
	
						main.RunToEnd();
					}
					break;
				}
				case 'T': {
					if (!main.initialized) {
						Console.WriteLine("Use the -c option before -T");
						continue;
					}
					template = g.Optarg;
					if (template != null && output != null) {
			            DictumMaker maker = new DictumMaker(main.basectx, "testing");
						dicta.Add(maker.MakeDictum(template, output));

						template = output = null;
					}
					break;
				}
				case 'O': {
					if (!main.initialized) {
						Console.WriteLine("Use the -c option before -O");
						continue;
					}

					output = g.Optarg;
					if (template != null && output != null) {
			            DictumMaker maker = new DictumMaker(main.basectx, "testing");
						dicta.Add(maker.MakeDictum(template, output));

						template = output = null;
					}
					break;
				}
				case 't': {
					if (!main.initialized) {
						Console.WriteLine("Use the -c option before -t");
						continue;
					}

					bool nextTemplate = true;
					foreach (string line in File.ReadAllLines(g.Optarg)) {
						string trimline = line.Trim();
						if (trimline.Length == 0 || trimline.StartsWith("#"))
							continue;
						
						if (nextTemplate) {
							template = trimline;
							nextTemplate = false;
						} else {
							output = trimline;
				            DictumMaker maker = new DictumMaker(main.basectx, "testing");
							dicta.Add(maker.MakeDictum(template, output));
							nextTemplate = true;
						}
					}
					
					template = output = null;
					break;
				}
			}

			if (dicta.Count != 0)
				main.DoMatching(dicta, input);
			else if (!acted)
				Console.WriteLine("Nothing to do.  Add -tag, -parse, or -t or -T and -O");
		}
		
		public MainClass() {
			verbose = 0;
			initialized = false;
			serialmode = false;
			
			tryToRescueMatch = new TryToRescueMatch();
		}
					
		public void Initialize(string config) {
			plugenv = new PluginEnvironment(this);
			if (!File.Exists(config)) {
				Console.WriteLine("Cannot find configuration file at " + config);
				return;
			}
			
            plugenv.Initialize(config, new NameValueCollection());
				
			tagger = new POSTagger(plugenv);			
			parser = new GrammarParser(plugenv);
			
			coderack = new ZippyCoderack(this);
			memory = new Memory();
			
            basectx = new Context(coderack);
			GrammarVariables.LoadVariables(basectx, 100.0, memory, plugenv);
			OutputVariables.LoadVariables(basectx, 100.0, plugenv);
			ProgramVariables.LoadVariables(basectx, 100.0, plugenv);
			basectx.Map.Add("$Compare", new WordComparer());
			
			initialized = true;
		}
		
		public void RunToEnd() {
            coderack.Execute(1000000, false);
			Unilog.DropBelow(Unilog.levelRecoverable);
			if (Unilog.HasEntries)
				Console.WriteLine(Unilog.FlushToStringShort());			
		}
		
		void DoMatching(List<PatternTemplateSource> dicta, string input) {
			if (verbose > 0)
				Console.WriteLine("Parsing input...");
			IParsedPhrase phrase = parser.Parse(input);

			if (verbose > 0)
				Console.WriteLine("Matching templates...");
						
            // Add a codelet for each of these, to match the input
			if (!serialmode) {
	            foreach (PatternTemplateSource dictum in dicta) {
					IFailure fail = tryToRescueMatch.MakeFailure(phrase, dictum, this, new NopCallable(), coderack);
    	            dictum.Generate(coderack, phrase, this, fail, 1.0);
				}
			} else {
				SerialTemplateMatcher matcher = new SerialTemplateMatcher(this, this, coderack, tryToRescueMatch, phrase, dicta, 1.0);
				matcher.MatchNextSentence();
			}
			
			RunToEnd();
		}
				
		public object Clone()
		{
			MainClass clone = new MainClass();
			clone.verbose = verbose;
			return clone;
		}
		
        #region IContinuation Members

        public bool Continue(object value, IFailure fail)
        {
			Context context = (Context) value;
            if (!context.IsEmpty)
				Console.WriteLine(context.ContentsCode());
			
			// update context
			basectx = context;
			
			return true;
        }

        #endregion

		// IMessageReceiver
		
		public bool Receive(string message, object reference) {
			if (verbose > 0) {
				if (message == "EvaluateCodelet") {
					if (Unilog.HasEntries)
						Console.WriteLine(Unilog.FlushToStringShort());
					Console.WriteLine("Codelet " + reference.ToString()); //reference.GetType().Name);
				}
				else
					Console.WriteLine(message);
			}
			return true;
		}

	}
}
