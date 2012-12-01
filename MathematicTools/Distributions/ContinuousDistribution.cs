using System;

namespace MathematicTools.Distributions
{
	public abstract class ContinuousDistribution
	{
		public abstract double Mean {
			get;
		}
		
		public abstract double Variance {
			get;
		}
		
		public virtual ContinuousDistribution Transform(double shift, double scale) {
			return new TransformedDistribution(this, shift, scale);
		}
		
		public virtual ContinuousDistribution Clip(double lower, double upper) {
			throw new NotImplementedException();
		}
		
		public virtual double PDF(double x) {
			throw new NotImplementedException();
		}

		public virtual double CDF(double x) {
			throw new NotImplementedException();
		}

		public virtual ContinuousDistribution Plus(ContinuousDistribution two) {
			throw new NotImplementedException();
		}

		public virtual ContinuousDistribution Minus(ContinuousDistribution two) {
			return this.Plus(two.Negate());
		}

		public virtual ContinuousDistribution Times(ContinuousDistribution two) {
			throw new NotImplementedException();
		}
		
		public virtual ContinuousDistribution DividedBy(ContinuousDistribution two) {
			throw new NotImplementedException();			
		}
		
		public virtual ContinuousDistribution Negate() {
			throw new NotImplementedException();
		}
	}
}

