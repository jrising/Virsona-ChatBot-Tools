using System;
using MathematicTools.Distributions;

namespace MathematicTools.Statistics
{
	public class RandomMatrix
	{
		public static ContinuousDistribution[,] MakeGaussians(double[,] means, double[,] sds) {
			ContinuousDistribution[,] result = new ContinuousDistribution[means.GetLength(0), means.GetLength(1)];
			for (int rr = 0; rr < means.GetLength(0); rr++)
				for (int cc = 0; cc < means.GetLength(1); cc++)
					result[rr, cc] = new GaussianDistribution(means[rr, cc], sds[rr, cc]*sds[rr, cc]);
			
			return result;
		}
		
		public static ContinuousDistribution[,] Transpose(ContinuousDistribution[,] xx) {
			ContinuousDistribution[,] yy = new ContinuousDistribution[xx.GetLength(1), xx.GetLength(0)];
			for (int rr = 0; rr < xx.GetLength(0); rr++)
				for (int cc = 0; cc < xx.GetLength(1); cc++)
					yy[cc, rr] = xx[rr, cc];
			return yy;
		}
		
		public static ContinuousDistribution[,] Multiply(ContinuousDistribution[,] xx, ContinuousDistribution[,] yy) {
			if (xx.GetLength(1) != yy.GetLength(0))
				throw new ArgumentException("Inside dimensions must match");
			
			ContinuousDistribution[,] zz = new ContinuousDistribution[xx.GetLength(0), yy.GetLength(1)];
			for (int rr = 0; rr < xx.GetLength(0); rr++)
				for (int cc = 0; cc < yy.GetLength(1); cc++) {
					ContinuousDistribution sum = new FlatDistribution();
					for (int ii = 0; ii < xx.GetLength(1); ii++)
						sum = sum.Plus(xx[rr, ii].Times(yy[ii, cc]));
					zz[rr, cc] = sum;
				}
			
			return zz;
		}
		
		public static ContinuousDistribution[,] Inverse(ContinuousDistribution[,] xx) {
			if (xx.GetLength(0) == 2 && xx.GetLength(1) == 2) {
				ContinuousDistribution det = RandomMatrix.Determinant(xx);
				ContinuousDistribution[,] yy = new ContinuousDistribution[2, 2];
				yy[0, 0] = xx[1, 1].DividedBy(det);
				yy[0, 1] = xx[0, 1].Negate().DividedBy(det);
				yy[1, 0] = xx[1, 0].Negate().DividedBy(det);
				yy[1, 1] = xx[0, 0].DividedBy(det);
				return yy;
			}
			
			if (xx.GetLength(0) == 3 && xx.GetLength(1) == 3) {
				// from http://www.dr-lex.be/random/matrix_inv.html
				ContinuousDistribution det = RandomMatrix.Determinant(xx);
				ContinuousDistribution[,] yy = new ContinuousDistribution[3, 3];
				yy[0, 0] = xx[2,2].Times(xx[1,1]).Minus(xx[2,1].Times(xx[1,2])).DividedBy(det);
				yy[0, 1] = xx[2,2].Times(xx[0,1]).Minus(xx[2,1].Times(xx[0,2])).Negate().DividedBy(det);
				yy[0, 2] = xx[1,2].Times(xx[0,1]).Minus(xx[1,1].Times(xx[0,2])).DividedBy(det);
				yy[1, 0] = xx[2,2].Times(xx[1,0]).Minus(xx[2,0].Times(xx[1,2])).Negate().DividedBy(det);
				yy[1, 1] = xx[2,2].Times(xx[0,0]).Minus(xx[2,0].Times(xx[0,2])).DividedBy(det);
				yy[1, 2] = xx[1,2].Times(xx[0,0]).Minus(xx[1,0].Times(xx[0,2])).Negate().DividedBy(det);
				yy[2, 0] = xx[2,1].Times(xx[1,0]).Minus(xx[2,0].Times(xx[1,1])).DividedBy(det);
				yy[2, 1] = xx[2,1].Times(xx[0,0]).Minus(xx[2,0].Times(xx[0,1])).Negate().DividedBy(det);
				yy[2, 2] = xx[1,1].Times(xx[0,0]).Minus(xx[1,0].Times(xx[0,1])).DividedBy(det);
				return yy;
			}
			
			throw new ArgumentException("Unknown matrix dimensions");
		}
		
		public static ContinuousDistribution Determinant(ContinuousDistribution[,] xx) {
			if (xx.GetLength(0) == 1 && xx.GetLength(1) == 1)
				return xx[0, 0];
			
			if (xx.GetLength(0) == 2 && xx.GetLength(1) == 2)
				return xx[0, 0].Times(xx[1, 1]).Minus(xx[0, 1].Times(xx[1, 0]));
			
			if (xx.GetLength(0) == 3 && xx.GetLength(1) == 3) {
				ContinuousDistribution one = xx[0,0].Times(xx[2,2].Times(xx[1,1]).Minus(xx[2,1].Times(xx[1,2])));
				ContinuousDistribution two = xx[1,0].Times(xx[2,2].Times(xx[0,1]).Minus(xx[2,1].Times(xx[0,2])));
				ContinuousDistribution three = xx[2,0].Times(xx[1,2].Times(xx[0,1]).Minus(xx[1,1].Times(xx[0,2])));
				return one.Minus(two).Plus(three);
			}
			
			throw new ArgumentException("Unknown matrix dimensions");
		}
	}
}

