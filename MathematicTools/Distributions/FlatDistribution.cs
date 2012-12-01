using System;
namespace MathematicTools.Distributions
{
	public class FlatDistribution : ContinuousDistribution
	{
		public FlatDistribution()
		{
		}
		
		public override double Mean {
			get {
				return double.NaN;
			}
		}
		
		public override double Variance {
			get {
				return double.NaN;
			}
		}
		
		public override ContinuousDistribution Plus(ContinuousDistribution two) {
			return two;
		}

		public override ContinuousDistribution Times(ContinuousDistribution two) {
			return two;
		}
	}
}

