/******************************************************************\
 *      Class Name:     QueueArena
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Simple implementation of an arena which performs action in
 * a first-in-first-out manner.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.ActionReaction.Evaluations;
using PluggerBase.FastSerializer;

namespace PluggerBase.ActionReaction.Actions
{
    public class QueueArena : IArena
    {
        public static object CallResult(ICallable callable, object value, int maxtime, double exitscore)
        {
            QueueArena arena = new QueueArena();
            BestContinuation getbest = new BestContinuation();
            LastFailure getlast = new LastFailure();

            arena.Call(callable, 100.0, value, getbest.GetContinue(), getlast.GetFail());
						
            while (getbest.Salience <= exitscore && !arena.IsEmpty)
                arena.EvaluateOne();

            if (getbest.Value == null && getlast.Reason != null)
                return new Exception(getlast.Reason);

            return getbest.Value;
        }

        protected Queue<IEvaluable> evaluables;

        public QueueArena()
        {
            evaluables = new Queue<IEvaluable>();
        }

        public bool IsEmpty
        {
            get
            {
                return evaluables.Count == 0;
            }
        }

        public bool EvaluateOne()
        {
            IEvaluable evaluable = evaluables.Dequeue();
            bool done = evaluable.Evaluate();
            if (evaluable is IAgent)
                if (!((IAgent) evaluable).Complete())
                    evaluables.Enqueue(evaluable);
			
            return done;
        }

        #region IFastSerializable Members

        public void Deserialize(SerializationReader reader)
        {
            List<IAgent> agents = reader.ReadList<IAgent>();
            foreach (IAgent agent in agents)
                evaluables.Enqueue((IEvaluable)agent);
        }

        public void Serialize(SerializationWriter writer)
        {
            IEvaluable[] evalscopy = new IEvaluable[evaluables.Count];
            evaluables.CopyTo(evalscopy, 0);

            List<IAgent> agents = new List<IAgent>();
            foreach (IEvaluable eval in evalscopy)
                if (eval is IAgent)
                    agents.Add((IAgent)eval);

            writer.WriteList<IAgent>(agents);
        }

        #endregion

        #region IArena Members

        public bool Evaluate(IEvaluable evaluable, double salience)
        {
            if (evaluable is IAgent)
                ((IAgent)evaluable).Initialize(this, salience);
            evaluables.Enqueue(evaluable);
            return true;
        }

        public bool Call(ICallable callable, double salience, object value, IContinuation succ, IFailure fail)
        {
            return Evaluate(new CallableAsEvaluable(callable, value, succ, fail), salience);
        }

        public bool Continue(IContinuation cont, double salience, object value, IFailure fail)
        {
            return Evaluate(new ContinuationAsEvaluable((IContinuation) cont.Clone(), value, fail), salience);
        }

        public bool Fail(IFailure fail, double salience, string reason, IContinuation skip)
        {
            return Evaluate(new FailureAsEvaluable((IFailure) fail.Clone(), reason, skip), salience);
        }

        #endregion
    }
}
