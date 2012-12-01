using System;

namespace MathematicTools.Distributions
{
	// Currently just approximates as a clipped Gaussian
	public class DiscretesAsNormal : ContinuousDistribution
	{
		double mean;
		double variance;
		
		public DiscretesAsNormal(double mean, double variance, int lower, int upper)
		{
			this.mean = mean;
			this.variance = variance;
		}
		
		public override double Mean {
			get {
				return mean;
			}
		}

		public override double Variance {
			get {
				return variance;
			}
		}
	}
}

