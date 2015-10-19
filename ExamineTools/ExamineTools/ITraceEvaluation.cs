using System;
using System.Collections.Generic;
using ActionReaction.Evaluations;

namespace ExamineTools
{
	public interface ITraceEvaluation
	{
		string TraceTitle {
			get;
		}

		IContinuation Success {
			get;
		}

		IFailure Failure {
			get;
		}
	}

	public class TraceEvaluationTools {
		public static List<string> GetSuccessTrace(ITraceEvaluation cont) {
			List<string> titles = new List<string>();
			while (cont != null) {
				titles.Add(cont.TraceTitle);
				IContinuation eval = cont.Success;
				if (eval == null)
					break;

				if (eval is ITraceEvaluation)
					cont = (ITraceEvaluation) eval;
				else {
					titles.Add(eval.ToString());
					break;
				}
			}

			return titles;
		}
	}
}