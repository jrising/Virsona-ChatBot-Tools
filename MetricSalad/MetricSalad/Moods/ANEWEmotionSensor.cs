using System;
using System.Collections.Generic;
using System.IO;
using LanguageNet.Grammarian;
using PluggerBase;
using GenericTools;
using MathematicTools.Statistics;
using MathematicTools.Distributions;

namespace MetricSalad.Moods
{
	public class ANEWEmotionSensor
	{
		public enum Emotions {
			Valence = 0,
			Arousal = 1,
			Dominance = 2,
			// Don't mix above and below this line
			Happiness = 3,
			Anger = 4,
			Sadness = 5,
			Fear = 6,
			Disgust = 7,
			COUNT = 8
		};
		
		protected ANEWFileSource source;
		protected PorterStemmer stemmer;
		// We use a minimum distance measure
		protected ContinuousDistribution[,] positiveProduct;
		protected ContinuousDistribution[,] negativeProduct;
				
		public ANEWEmotionSensor(string datadirectory)
		{
            // Data files contained in [datadrectory]/metrics
            string metricsdir = datadirectory + Path.DirectorySeparatorChar + "metrics" + Path.DirectorySeparatorChar;
			source = new ANEWFileSource(metricsdir + "anew.csv");
			stemmer = new PorterStemmer();
			
			// These matrices are used in G emotion = vad
			// positives are mean > 5; negatives are mean < 5
			double[,] positives = new double[,] {
				{.890, -.020, -.110, .116, -.035},
				{.649, .139, -.287, .441, .051},
				{.601, .153, -.305, .125, .042}};
			double[,] positiveTs = new double[,] {
				{45.40, 0.73, 4.24, 4.95, 1.55},
				{19.75, 2.984, 6.57, 11.26, 1.36},
				{16.60, 2.98, 6.34, 2.88, 1.00}};
			double[,] positiveSEs = Matrix.ElementwiseDivide(positives, positiveTs);
			double[,] negatives = new double[,] {
				{.291, -.044, -.515, .020, -.243},
				{.050, .492, -.309, .670, -.042},
				{.136, .369, -.625, -.144, .041}};
			double[,] negativeTs = new double[,] {
				{8.91, 1.27, 13.80, 0.58, 8.27},
				{1.36, 12.59, 7.33, 17.11, 1.27},
				{2.93, 7.49, 11.75, 2.91, 0.98}};
			double[,] negativeSEs = Matrix.ElementwiseDivide(negatives, negativeTs);
			
			ContinuousDistribution[,] randomPositives = RandomMatrix.MakeGaussians(positives, positiveSEs);
			ContinuousDistribution[,] randomNegatives = RandomMatrix.MakeGaussians(negatives, negativeSEs);
			
			positiveProduct = RandomMatrix.Multiply(RandomMatrix.Transpose(randomPositives),
			                                        RandomMatrix.Inverse(RandomMatrix.Multiply(randomPositives, RandomMatrix.Transpose(randomPositives))));
			negativeProduct = RandomMatrix.Multiply(RandomMatrix.Transpose(randomNegatives),
			                                        RandomMatrix.Inverse(RandomMatrix.Multiply(randomNegatives, RandomMatrix.Transpose(randomNegatives))));						
		}
		
		public double[] EstimateEmotions(string text) {
			List<string> words = StringUtilities.SplitWords(text.ToLower(), true);			
			// 3. Look up each word in ANEWFileSource
			double[] numer = new double[(int) Emotions.COUNT], denom = new double[(int) Emotions.COUNT];
			for (int ii = 0; ii < (int) Emotions.COUNT; ii++)
				numer[ii] = denom[ii] = 0;
			
			foreach (string word in words) {
				if (word.StartsWith(" ") || word.Length <= 2)
					continue;
				
				ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution> vad;
				if (!source.TryGetValue(word, out vad)) {
					// try stemmed word
					string stem = stemmer.stemTerm(word);
					if (stem == word || !source.TryGetValue(stem, out vad))
						continue;
				}
				
				numer[(int) Emotions.Valence] += vad.one.Mean / vad.one.Variance;
				denom[(int) Emotions.Valence] += 1 / vad.one.Variance;
				numer[(int) Emotions.Arousal] += vad.two.Mean / vad.two.Variance;
				denom[(int) Emotions.Arousal] += 1 / vad.two.Variance;
				numer[(int) Emotions.Dominance] += vad.three.Mean / vad.three.Variance;
				denom[(int) Emotions.Dominance] += 1 / vad.three.Variance;
			
				// 4. Apply regressions from other paper
				ContinuousDistribution[,] vector = new ContinuousDistribution[,] {
					{vad.one}, {vad.two}, {vad.three}};
				
				ContinuousDistribution[,] emotions;
				if (vad.one.Mean >= .5)
					emotions = RandomMatrix.Multiply(positiveProduct, vector);
				else
					emotions = RandomMatrix.Multiply(negativeProduct, vector);
				
				// 5. Take mean within bounds and sum weighted by variance
				for (int ii = 3; ii < (int) Emotions.COUNT; ii++) {
					ContinuousDistribution clipped = emotions[ii - 3, 0].Transform(0, .1).Clip(0, 1);
					numer[ii] += clipped.Mean / clipped.Variance;
					denom[ii] += 1 / clipped.Variance;
				}
			}
			
			for (int ii = 0; ii < (int) Emotions.COUNT; ii++)
				numer[ii] /= denom[ii];
			
			return numer;
		}
	}
}

