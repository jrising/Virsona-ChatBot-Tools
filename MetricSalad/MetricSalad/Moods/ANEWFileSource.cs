using System;
using GenericTools;
using GenericTools.DataSources;
using MathematicTools.Distributions;

namespace MetricSalad.Moods
{
	public class ANEWFileSource : AlphabeticFile<ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution>>
	{
		public static char[] delimiter = new char[] {','};
		
        public ANEWFileSource(string filename)
            : base(filename, delimiter, 20)
        {
        }
		
		public override ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution> ReadStringEntry(string line)
		{
            string[] cols = line.Split(delimiter);
			double valenceMean = double.Parse(cols[2]);
			double valenceSD = double.Parse(cols[3]);
			
			ContinuousDistribution valence = new DiscretesAsNormal(valenceMean, valenceSD*valenceSD, 1, 9);
			
			double arousalMean = double.Parse(cols[4]);
			double arousalSD = double.Parse(cols[5]);

			ContinuousDistribution arousal = new DiscretesAsNormal(arousalMean, arousalSD*arousalSD, 1, 9);

			double dominanceMean = double.Parse(cols[6]);
			double dominanceSD = double.Parse(cols[7]);
			
			ContinuousDistribution dominance = new DiscretesAsNormal(dominanceMean, dominanceSD*dominanceSD, 1, 9);

			return new ThreeTuple<ContinuousDistribution, ContinuousDistribution, ContinuousDistribution>(valence.Transform(-1, 1.0/8),
			                                                                arousal.Transform(-1, 1.0/8),
			                                                                dominance.Transform(-1, 1.0/8));
		}
	}
}

