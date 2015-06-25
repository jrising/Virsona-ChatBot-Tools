using System;
using System.IO;
using InOutTools;
using PluggerBase;
using GenericTools;
using MetricSalad.Moods;

using System.Collections.Generic;
using MathematicTools.Distributions;

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
			
			/*ANEWEmotionSensor sensor2 = new ANEWEmotionSensor("/Users/jrising/projects/virsona/github/data");
			for (int rr = 0; rr < sensor2.positiveMatrix.GetLength(0); rr++) {
				for (int cc = 0; cc < sensor2.positiveMatrix.GetLength(1); cc++)
					Console.Write(sensor2.positiveMatrix[rr, cc] + ", ");
				Console.WriteLine(" - ");
			}
			for (int rr = 0; rr < sensor2.negativeMatrix.GetLength(0); rr++) {
				for (int cc = 0; cc < sensor2.negativeMatrix.GetLength(1); cc++)
					Console.Write(sensor2.negativeMatrix[rr, cc] + ", ");
				Console.WriteLine(" - ");
			}
			return;*/
			
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
			
			if (parsedArgs["eimpute"] != null) {
				ANEWEmotionSensor sensor = new ANEWEmotionSensor("/Users/jrising/projects/virsona/github/data");
				
				// DIAGNOSTIC
				/*List<List<string>> rows = new List<List<string>>();
				rows.Add(TwitterUtilities.SplitWords("happy aaaa cccc"));
				rows.Add(TwitterUtilities.SplitWords("sad bbbb cccc"));
				
				IDataSource<string, ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution>> inputed = sensor.ImputeEmotionalContent(rows, 1000);
				foreach (KeyValuePair<string, ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution>> kvp in inputed)
					Console.WriteLine(kvp.Key + ": " + kvp.Value.one.Mean + ", " + kvp.Value.two.Mean + ", " + kvp.Value.three.Mean);*/
				
				bool smallFile = false;
				if (smallFile) {
					DataReader reader = new DataReader(parsedArgs["f"]);
					List<List<string>> rows = new List<List<string>>();
					for (string[] row = reader.ReadRow(); row != null; row = reader.ReadRow()) {
						Console.WriteLine(row);
						rows.Add(TwitterUtilities.SplitWords(row[10].ToLower()));
					}
					reader.Close();
				
					/*IDataSource<string, ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution>> inputed = sensor.ImputeEmotionalContent(rows, 10);
					double minv = 1, maxv = 0;
					foreach (KeyValuePair<string, ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution>> kvp in inputed) {
						minv = Math.Min(minv, kvp.Value.one.Mean);
						maxv = Math.Max(maxv, kvp.Value.one.Mean);
						Console.WriteLine(kvp.Key + ": " + kvp.Value.one.Mean + " x " + kvp.Value.one.Variance + ", " + kvp.Value.two.Mean + ", " + kvp.Value.three.Mean);
					}
				
					Console.WriteLine("Min: " + minv + ", Max: " + maxv);*/
				
					sensor.ImputeEmotionalContent(rows, 10, parsedArgs["f"] + "imputed");
				} else {
					sensor.ImputeEmotionalContentFromFile(parsedArgs["f"], 11, 0, parsedArgs["f"].Substring(0, parsedArgs["f"].Length - 4) + "imputed.csv");
				}
				
				uint jj = 0;
				using (var stream = File.CreateText(parsedArgs["f"] + "result")) {
					jj++;
					if (jj % 1000 == 0)
						Console.WriteLine("#" + jj);

					DataReader reader = new DataReader(parsedArgs["f"]);
					for (string[] row = reader.ReadRow(); row != null; row = reader.ReadRow()) {
						double[] emotions = sensor.EstimateEmotions(row[11]);
						for (int ii = 0; ii < 11; ii++)
							stream.Write(row[ii] + ",");
						stream.WriteLine(emotions[0] + "," + emotions[1] + "," + emotions[2] + "," + emotions[3] + "," + emotions[4] + "," + emotions[5] + "," + emotions[6] + "," + emotions[7]);
					}
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

