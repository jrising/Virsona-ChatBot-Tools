using System;
using InOutTools;
using PluggerBase;
using GenericTools;
using MetricSalad.Moods;

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
			
			if (parsedArgs["emotion"] != null) {
				ANEWEmotionSensor sensor = new ANEWEmotionSensor("/Users/jrising/projects/virsona/github/data");
				double[] emotions = sensor.EstimateEmotions(parsedArgs["emotion"]);
				for (int ii = 0; ii < (int) ANEWEmotionSensor.Emotions.COUNT; ii++)
					Console.WriteLine(((ANEWEmotionSensor.Emotions) ii).ToString() + ": " + emotions[ii]);
			}
			
			if (parsedArgs["emorows"] != null) {
				int rows = 0, valids = 0;
				ANEWEmotionSensor sensor = new ANEWEmotionSensor("/Users/jrising/projects/virsona/github/data");
				DataReader reader = new DataReader(parsedArgs["f"]);
				for (string[] row = reader.ReadRow(); row != null; row = reader.ReadRow()) {
					rows++;
					double[] emotions = sensor.EstimateEmotions(row[1]);
					Console.WriteLine("\"" + row[0] + "\"," + emotions[0] + "," + emotions[1] + "," + emotions[2] + "," + emotions[3] + "," + emotions[4] + "," + emotions[5] + "," + emotions[6] + "," + emotions[7] + ",\"" + row[2] + "\"");
					if (!double.IsNaN(emotions[0]))
						valids++;
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

