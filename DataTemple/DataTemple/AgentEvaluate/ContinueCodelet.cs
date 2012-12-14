using System;
using System.Collections.Generic;
using System.Text;
using DataTemple.Codeland;
using PluggerBase.ActionReaction.Evaluations;

namespace DataTemple.AgentEvaluate
{
    // Calls success continue
    public class ContinueCodelet : Codelet, IFailure
    {
        protected Context context;
        protected IContinuation succ;
        protected IFailure fail;

        public ContinueCodelet(double salience, Context context, IContinuation succ, IFailure fail)
            : base(context.Coderack, salience, 4 * 4, 5)
        {
            this.context = context;
            this.succ = succ;
            this.fail = fail;
        }

        public override bool Evaluate()
        {
            succ.Continue(context, fail);

            return true;
        }

        #region IFailure Members

        public bool Fail(string reason, IContinuation succ)
        {
            coderack.AddCodelet((Codelet) this.Clone(), "Fail: " + reason);
            return true;
        }

        #endregion
    }
}
