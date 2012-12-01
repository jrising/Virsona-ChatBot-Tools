using System;

namespace MathematicTools.Statistics
{
	public class OLS
	{
		public OLS()
		{
		}
		
		public static double[,] PointsRegression(double[,] points) {
			double[,] xxs = AddConstant(Matrix.GetColumn(points, 0));
			double[,] yy = Matrix.GetColumn(points, 1);
			
			return CalcCoefficients(yy, xxs);
		}
		
		public static double PointsRSqr(double[,] points) {
			double[,] betas = PointsRegression(points);
			double[,] xxs = AddConstant(Matrix.GetColumn(points, 0));
			return CalcRSqr(Matrix.GetColumn(points, 1), CalcEstimates(betas, xxs));
		}
		
		public static double[,] AddConstant(double[,] xxs) {
			return Matrix.HorizontalConcat(Matrix.Ones(xxs.GetLength(0), 1), xxs);
		}
		
		public static double[,] CalcCoefficients(double[,] yy, double[,] xxs) {
			return Matrix.Multiply(Matrix.Inverse(Matrix.Multiply(Matrix.Transpose(xxs), xxs)), Matrix.Multiply(Matrix.Transpose(xxs), yy));
		}
		
		public static double[,] CalcEstimates(double[,] betas, double[,] xxs) {
			return Matrix.Multiply(xxs, betas);
		}
		
		public static double CalcRSqr(double[,] yy, double[,] yyhat) {
			double numer = 0, denom = 0;
			for (int ii = 0; ii < yy.GetLength(0); ii++) {
				double diff = yy[ii, 0] - yyhat[ii, 0];
				numer += diff * diff;
				denom += yy[ii, 0] * yy[ii, 0];
			}
			
			return 1.0 - numer / denom;
		}
	}
}

