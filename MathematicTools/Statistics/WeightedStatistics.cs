using System;
using System.Collections.Generic;

namespace MathematicTools.Statistics
{
	public class WeightedStatistics
	{
		public WeightedStatistics()
		{
		}
		
		public static double Mean(List<KeyValuePair<double, double>> pws) {
			double numer = 0, denom = 0;
			
			foreach (KeyValuePair<double, double> pw in pws) {
				numer += pw.Key * pw.Value;
				denom += pw.Value;
			}
			
			return numer / denom;
		}
		
		public static double Variance(List<KeyValuePair<double, double>> pws, double mean, bool useInvWeight) {
			double numer = 0, denom = 0;
			
			foreach (KeyValuePair<double, double> pw in pws) {
				double diff = pw.Key - mean;
				if (useInvWeight)
					numer += pw.Value * (diff * diff) + 1;
				else
					numer += pw.Value * (diff * diff);
				denom += pw.Value;
			}
			
			return numer / denom;
		}
	}
}

