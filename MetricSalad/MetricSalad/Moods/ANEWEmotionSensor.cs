using System;
using System.Collections.Generic;
using System.IO;
using LanguageNet.Grammarian;
using PluggerBase;
using GenericTools;
using GenericTools.DataSources;
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
		
		protected IDataSource<string, ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution>> source;
		protected PorterStemmer stemmer;
		// We use a minimum distance measure
		protected ContinuousDistribution[,] positiveProduct;
		protected ContinuousDistribution[,] negativeProduct;
		
		public double[,] positiveMatrix;
		public double[,] negativeMatrix;
				
		public ANEWEmotionSensor(string datadirectory)
		{
            // Data files contained in [datadrectory]/metrics
            string metricsdir = datadirectory + Path.DirectorySeparatorChar + "metrics" + Path.DirectorySeparatorChar;
			source = new MemoizedSource<string, ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution>>(new ANEWFileSource(metricsdir + "anew.csv"));
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
			
			positiveMatrix = Matrix.Multiply(Matrix.Transpose(positives), Matrix.Inverse(Matrix.Multiply(positives, Matrix.Transpose(positives))));
			negativeMatrix = Matrix.Multiply(Matrix.Transpose(negatives), Matrix.Inverse(Matrix.Multiply(negatives, Matrix.Transpose(negatives))));			
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
				if (!TryGetWordOrStem(source, word, out vad))
					continue;
				
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
		
		public bool TryGetWordOrStem(IDataSource<string, ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution>> source,
		                             string word, out ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution> vad) {
			if (!source.TryGetValue(word, out vad)) {
				// try stemmed word
				string stem = stemmer.stemTerm(word);
				if (stem == word || !source.TryGetValue(stem, out vad))
					return false;
			}
			
			return true;
		}
		
		/*public IDataSource<string, ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution>> ImputeEmotionalContent(List<List<string>> texts, uint repeats) {
			MemorySource<string, ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution>> inputed = new MemorySource<string, ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution>>();
			ComboSource<string, ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution>> combo = new ComboSource<string, ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution>>(source, inputed);
			
			for (uint ii = 0; ii < repeats; ii++) {
				Dictionary<string, double> wordVnumers = new Dictionary<string, double>(), wordVdenoms = new Dictionary<string, double>(),
					wordAnumers = new Dictionary<string, double>(), wordAdenoms = new Dictionary<string, double>(),
					wordDnumers = new Dictionary<string, double>(), wordDdenoms = new Dictionary<string, double>(),
					wordVsumvar = new Dictionary<string, double>(), wordVcounts = new Dictionary<string, double>(),
					wordAsumvar = new Dictionary<string, double>(), wordAcounts = new Dictionary<string, double>(),
					wordDsumvar = new Dictionary<string, double>(), wordDcounts = new Dictionary<string, double>();
				
				uint jj = 0;
				foreach (List<string> words in texts) {
					jj++;
					if (jj % 1000 == 0)
						Console.WriteLine("#" + jj);
					
					double textVnumer = 0, textVdenom = 0, textAnumer = 0, textAdenom = 0, textDnumer = 0, textDdenom = 0;
					double textVsumvar = 0, textVcount = 0, textAsumvar = 0, textAcount = 0, textDsumvar = 0, textDcount = 0;
					foreach (string word in words) {
						if (word.StartsWith(" ") || word.Length <= 2)
							continue;
		
						ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution> vad;
						if (!TryGetWordOrStem(combo, word, out vad))
							continue;
						
						textVnumer += vad.one.Mean / vad.one.Variance;
						textVdenom += 1 / vad.one.Variance;
						textVsumvar += vad.one.Variance;
						textVcount++;
						textAnumer += vad.two.Mean / vad.two.Variance;
						textAdenom += 1 / vad.two.Variance;
						textAsumvar += vad.two.Variance;
						textAcount++;
						textDnumer += vad.three.Mean / vad.three.Variance;
						textDdenom += 1 / vad.three.Variance;
						textDsumvar += vad.three.Variance;
						textDcount++;
					}
					
					double vmean = textVnumer / textVdenom, amean = textAnumer / textAdenom, dmean = textDnumer / textDdenom;
					double vvar = textVsumvar / textVcount, avar = textAsumvar / textAcount, dvar = textDsumvar / textDcount;
					
					if (double.IsNaN(vmean) || double.IsNaN(amean) || double.IsNaN(dmean))
						continue;
					
					foreach (string word in words) {
						if (word.StartsWith(" ") || word.Length <= 2)
							continue;
						
						ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution> vad;
						if (TryGetWordOrStem(source, word, out vad))
							continue;
						
						string stem = stemmer.stemTerm(word);
												
						AddTextNumerDenom(stem, wordVnumers, wordVdenoms, wordVsumvar, wordVcounts, vmean, vvar);
						AddTextNumerDenom(stem, wordAnumers, wordAdenoms, wordAsumvar, wordAcounts, amean, avar);
						AddTextNumerDenom(stem, wordDnumers, wordDdenoms, wordDsumvar, wordDcounts, dmean, dvar);
					}
				}

				foreach (string stem in wordVnumers.Keys) {
					ContinuousDistribution valence = new ClippedGaussianDistribution(wordVnumers[stem] / wordVdenoms[stem], wordVsumvar[stem] / wordVcounts[stem], 0, 1);
					ContinuousDistribution arousal = new ClippedGaussianDistribution(wordAnumers[stem] / wordAdenoms[stem], wordAsumvar[stem] / wordAcounts[stem], 0, 1);
					ContinuousDistribution dominance = new ClippedGaussianDistribution(wordDnumers[stem] / wordDdenoms[stem], wordDsumvar[stem] / wordDcounts[stem], 0, 1);
					
					inputed[stem] = new ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution>(valence, arousal, dominance);
				}
			}
			
			source = combo;
			return inputed;
		}
		
		public void AddTextNumerDenom(string stem, Dictionary<string, double> wordXnumers,
		                              Dictionary<string, double> wordXdenoms, Dictionary<string, double> wordXsumvar, 
		                              Dictionary<string, double> wordXcounts, double xmean, double xvar) {
			double numer, denom, sumvar, count;
			if (!wordXnumers.TryGetValue(stem, out numer)) {
				numer = 0;
				denom = 0;
				sumvar = 0;
				count = 0;
			} else {
				wordXdenoms.TryGetValue(stem, out denom);
				wordXsumvar.TryGetValue(stem, out sumvar);
				wordXcounts.TryGetValue(stem, out count);
				xvar += (xmean - numer / denom) * (xmean - numer / denom);
			}

			numer += xmean / xvar;
			denom += 1 / xvar;
			sumvar += xvar;
			count++;

			wordXnumers[stem] = numer;
			wordXdenoms[stem] = denom;
			wordXsumvar[stem] = sumvar;
			wordXcounts[stem] = count;
		}*/
		
		public IDataSource<string, ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution>> ImputeEmotionalContent(List<List<string>> texts, uint repeats) {
			MemorySource<string, ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution>> inputed = new MemorySource<string, ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution>>();
			ComboSource<string, ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution>> combo = new ComboSource<string, ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution>>(source, inputed);
			
			for (uint ii = 0; ii < repeats; ii++) {
				Dictionary<string, List<KeyValuePair<double, double>>>
					sentencesV = new Dictionary<string, List<KeyValuePair<double, double>>>(),
					sentencesA = new Dictionary<string, List<KeyValuePair<double, double>>>(),
					sentencesD = new Dictionary<string, List<KeyValuePair<double, double>>>();
				
				uint jj = 0;
				foreach (List<string> words in texts) {
					jj++;
					if (jj % 1000 == 0)
						Console.WriteLine("#" + jj);
					
					List<KeyValuePair<double, double>> wordsV = new List<KeyValuePair<double, double>>(),
						wordsA = new List<KeyValuePair<double, double>>(),
						wordsD = new List<KeyValuePair<double, double>>();
					foreach (string word in words) {
						if (word.StartsWith(" ") || word.Length <= 2)
							continue;
		
						ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution> vad;
						if (!TryGetWordOrStem(combo, word, out vad))
							continue;
						
						wordsV.Add(new KeyValuePair<double, double>(vad.one.Mean, 1/vad.one.Variance));
						wordsA.Add(new KeyValuePair<double, double>(vad.two.Mean, 1/vad.two.Variance));
						wordsD.Add(new KeyValuePair<double, double>(vad.three.Mean, 1/vad.three.Variance));
					}
					
					double vmean = WeightedStatistics.Mean(wordsV), amean = WeightedStatistics.Mean(wordsA), dmean = WeightedStatistics.Mean(wordsD);
					double vvar = WeightedStatistics.Variance(wordsV, vmean, true), avar = WeightedStatistics.Variance(wordsA, amean, true), dvar = WeightedStatistics.Variance(wordsD, dmean, true);
					
					if (double.IsNaN(vmean) || double.IsNaN(amean) || double.IsNaN(dmean))
						continue;
					
					foreach (string word in words) {
						if (word.StartsWith(" ") || word.Length <= 2)
							continue;
						
						ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution> vad;
						if (TryGetWordOrStem(source, word, out vad))
							continue;
						
						string stem = stemmer.stemTerm(word);
												
						AddTextNumerDenom(stem, sentencesV, vmean, vvar);
						AddTextNumerDenom(stem, sentencesA, amean, avar);
						AddTextNumerDenom(stem, sentencesD, dmean, dvar);
					}
				}

				foreach (string stem in sentencesV.Keys) {
					double vmean = WeightedStatistics.Mean(sentencesV[stem]), amean = WeightedStatistics.Mean(sentencesA[stem]), dmean = WeightedStatistics.Mean(sentencesD[stem]);
					
					ContinuousDistribution valence = new ClippedGaussianDistribution(vmean, WeightedStatistics.Variance(sentencesV[stem], vmean, true), 0, 1);
					ContinuousDistribution arousal = new ClippedGaussianDistribution(amean, WeightedStatistics.Variance(sentencesA[stem], amean, true), 0, 1);
					ContinuousDistribution dominance = new ClippedGaussianDistribution(dmean, WeightedStatistics.Variance(sentencesD[stem], dmean, true), 0, 1);
					
					inputed[stem] = new ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution>(valence, arousal, dominance);
				}
			}
			
			source = combo;
			return inputed;
		}
		
		public void AddTextNumerDenom(string stem, Dictionary<string, List<KeyValuePair<double, double>>> sentencesX, double xmean, double xvar) {
			List<KeyValuePair<double, double>> pws;
			if (!sentencesX.TryGetValue(stem, out pws))
				pws = new List<KeyValuePair<double, double>>();

			pws.Add(new KeyValuePair<double, double>(xmean, 1/xvar));
			
			sentencesX[stem] = pws;
		}
	}
}

