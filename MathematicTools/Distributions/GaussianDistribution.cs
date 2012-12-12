using System;
namespace MathematicTools.Distributions
{
	public class GaussianDistribution : ContinuousDistribution
	{	
		double mean;
		double variance;
		
		public GaussianDistribution(double mean, double variance)
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
		
		public override double PDF(double x) {
			return Math.Exp(-.5 * Math.Pow(x - mean, 2) / variance) / Math.Sqrt(2 * Math.PI * variance);
		}

		public override double CDF(double x) {
			return .5 + .5*Erf((x - mean) / Math.Sqrt(2 * variance));
		}

		public static double Erf(double x)
	    {
	        // constants
	        double a1 = 0.254829592;
	        double a2 = -0.284496736;
	        double a3 = 1.421413741;
	        double a4 = -1.453152027;
	        double a5 = 1.061405429;
	        double p = 0.3275911;
	
	        // Save the sign of x
	        int sign = 1;
	        if (x < 0)
	            sign = -1;
	        x = Math.Abs(x);
	
	        // A&S formula 7.1.26
	        double t = 1.0 / (1.0 + p*x);
	        double y = 1.0 - (((((a5*t + a4)*t) + a3)*t + a2)*t + a1)*t*Math.Exp(-x*x);
	
	        return sign*y;
	    }
		
		public override ContinuousDistribution Transform(double shift, double scale)
		{
			return new GaussianDistribution(mean + shift, variance * scale * scale);
		}
		
		public override ContinuousDistribution Clip(double lower, double upper)
		{
			return new ClippedGaussianDistribution(mean, variance, lower, upper);
		}
		
		// Approximate as normal (true if other is Gaussian)
		public override ContinuousDistribution Plus(ContinuousDistribution two) {
			return new GaussianDistribution(mean + two.Mean, variance + two.Variance);
			/*return new GaussianDistribution((mean * two.Variance + two.Mean * variance) / (variance + two.Variance),
			                                variance * two.Variance / (variance + two.Variance));*/
		}
		
		// Approximate as normal (poor approximation) - from (mu1 + s1) * (mu2 + s2)
		public override ContinuousDistribution Times(ContinuousDistribution two) {
			return new GaussianDistribution(mean * two.Mean, Math.Abs(mean) * two.Variance + Math.Abs(two.Mean) * variance);
		}
		
		// Approximate as normal-- very poor approximation -- assume far from 0
		public override ContinuousDistribution DividedBy(ContinuousDistribution two) {
			return Times(new GaussianDistribution(1/two.Mean, two.Variance / (two.Mean*two.Mean))) ;
		}

		public override ContinuousDistribution Negate() {
			return new GaussianDistribution(-mean, variance);
		}
	}
}

