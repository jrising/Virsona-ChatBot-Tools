using System;
namespace MathematicTools.Distributions
{
	public class ClippedGaussianDistribution : ContinuousDistribution
	{
		double mean;
		double variance;
		double lower;
		double upper;
		
		// mean and variance are of the underlying gaussian, and selectors will return different values
		public ClippedGaussianDistribution(double mean, double variance, double lower, double upper)
		{
			this.mean = mean;
			this.variance = variance;
			this.lower = lower;
			this.upper = upper;
		}
		
		// from http://en.wikipedia.org/wiki/Truncated_normal_distribution
		public override double Mean {
			get {
				GaussianDistribution stdnorm = new GaussianDistribution(0, 1);
				double sigma = Math.Sqrt(variance);
				double alpha = (lower - mean) / sigma;
				double beta = (upper - mean) / sigma;
				double Z = stdnorm.CDF(beta) - stdnorm.CDF(alpha);
				if (Z == 0 && mean > upper)
					return upper;
				if (Z == 0 && mean < lower)
					return lower;
				
				return mean + (stdnorm.PDF(alpha) - stdnorm.PDF(beta)) * sigma / Z;
			}
		}
		
		// from http://en.wikipedia.org/wiki/Truncated_normal_distribution
		public override double Variance {
			get {
				GaussianDistribution stdnorm = new GaussianDistribution(0, 1);
				double sigma = Math.Sqrt(variance);
				double alpha = (lower - mean) / sigma;
				double beta = (upper - mean) / sigma;
				double Z = stdnorm.CDF(beta) - stdnorm.CDF(alpha);
				if (Z == 0)
					return variance;

				return variance * (1 + (alpha*stdnorm.PDF(alpha) - beta*stdnorm.PDF(beta)) / Z -
				                   Math.Pow((stdnorm.PDF(alpha) - stdnorm.PDF(beta)) / Z, 2));
			}
		}
		
		public override ContinuousDistribution Clip(double lower, double upper)
		{
			return new ClippedGaussianDistribution(mean, variance, Math.Max(this.lower, lower), Math.Min(this.upper, upper));
		}
		
		public override ContinuousDistribution Plus(ContinuousDistribution two) {
			if (two is ClippedGaussianDistribution) {
				ClippedGaussianDistribution other = (ClippedGaussianDistribution) two;
				return new ClippedGaussianDistribution(mean + two.Mean, variance + two.Variance, lower + other.lower, upper + other.upper);
			}
				
			return new GaussianDistribution(mean + two.Mean, variance + two.Variance);
		}
		
		public override ContinuousDistribution Times(ContinuousDistribution two) {
			return new GaussianDistribution((mean * two.Variance + two.Mean * variance) / (variance + two.Variance),
			                                variance * two.Variance / (variance + two.Variance));
		}
		
		public override ContinuousDistribution Negate() {
			return new ClippedGaussianDistribution(-mean, variance, -upper, -lower);
		}
	}
}

