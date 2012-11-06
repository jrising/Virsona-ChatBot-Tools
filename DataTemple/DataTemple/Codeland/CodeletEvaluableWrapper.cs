using System;
using PluggerBase.ActionReaction.Evaluations;

namespace DataTemple.Codeland
{
	public class CodeletEvaluableWrapper : Codelet
	{
		protected IEvaluable evaluable;
		
		public CodeletEvaluableWrapper(IEvaluable evaluable, Coderack coderack, double salience, int sp, int tm)
			: base(coderack, salience, sp, tm)
		{
			this.evaluable = evaluable;
		}
		
		public override int Evaluate()
		{
			return evaluable.Evaluate();
		}
	}
}

