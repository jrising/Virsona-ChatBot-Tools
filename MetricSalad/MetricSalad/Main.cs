using System;
using InOutTools;
using PluggerBase;
using GenericTools;

namespace MetricSalad
{
	class MainClass : IMessageReceiver
	{
		public static void Main (string[] args)
		{
			ToolArguments parsedArgs = new ToolArguments(args, "None", new MainClass());
			
			PorterStemmer stemmer = new PorterStemmer();
			
			if (parsedArgs["stem"] != null)
				Console.WriteLine(parsedArgs["stem"] + " => " + stemmer.stemTerm(parsedArgs["stem"]));
			
			if (parsedArgs["freqrows"] != null) {
				DataReader reader = new DataReader(parsedArgs["f"]);
				for (string[] row = reader.ReadRow(); row != null; row = reader.ReadRow()) {
					TwoTuple<int, int> counts = FrequencyTools.WordCount(parsedArgs["freqrows"], row[1]);
					Console.WriteLine(counts.one + "," + counts.two + ",\"" + row[2] + "\"");
				}
			}
		}
		
		public MainClass() {
		}
		
		public bool Receive(string message, object reference)
		{
			//Console.WriteLine(message);
			return true;
		}
	}
}

