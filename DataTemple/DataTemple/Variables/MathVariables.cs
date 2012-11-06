using System;
using System.Collections.Generic;
using System.Text;
using DataTemple.AgentEvaluate;
using PluggerBase.ActionReaction.Evaluations;

namespace DataTemple.Variables
{
    public class MathVariables
    {
        public static void LoadVariables(Context context, double basesal)
        {
            context.Map.Add("%add", new Adder(100.0));
            context.Map.Add("%neg", new Negator(100.0));
        }
    }

    public class Adder : CallAgent
    {
        public Adder(double salience)
            : base(ArgumentMode.ManyArguments, salience, 0, 10)
        {
        }

        public override int Call(object value, IContinuation succ, IFailure fail)
        {
			Context context = (Context) value;

            double result = 0;

            foreach (IContent content in context.Contents)
            {
                double term = 0;
                if (content is Word && double.TryParse(content.Name, out term))
                    result += term;
                else
                {
                    fail.Fail("Argument isn't number", succ);
                    return time;
                }
            }

            List<IContent> cntres = new List<IContent>();
            cntres.Add(new Word(result.ToString()));
            succ.Continue(new Context(context, cntres), fail);

            return time;
        }
    }

    public class Negator : CallAgent
    {
        public Negator(double salience)
            : base(ArgumentMode.SingleArgument, salience, 0, 1)
        {
        }

        public override int Call(object value, IContinuation succ, IFailure fail)
        {
			Context context = (Context) value;
            IContent content = context.Contents[0];
            double term = 0;
            if (content is Word && double.TryParse(content.Name, out term))
            {
                term = -term;
                Context result = new Context(context, context.Contents);
                result.Contents[0] = new Word(term.ToString());
                succ.Continue(result, fail);
            }
            else
                fail.Fail("Argument isn't number", succ);

            return time;
        }
    }
}
