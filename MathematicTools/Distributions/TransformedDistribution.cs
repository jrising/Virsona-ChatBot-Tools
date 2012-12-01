using System;
namespace MathematicTools.Distributions
{
	// First add shift, then scale result
	public class TransformedDistribution : ContinuousDistribution
	{
		protected ContinuousDistribution distribution;
		protected double shift;
		protected double scale;
		
		public TransformedDistribution(ContinuousDistribution distribution, double shift, double scale)
		{
			this.distribution = distribution;
			this.shift = shift;
			this.scale = scale;
		}
		
		public override double Mean {
			get {
				return (distribution.Mean + shift) * scale;
			}
		}

		public override double Variance {
			get {
				return distribution.Variance * scale * scale;
			}
		}

	}
}

