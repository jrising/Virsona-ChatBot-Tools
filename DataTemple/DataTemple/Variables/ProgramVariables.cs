using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.ActionReaction.Evaluations;
using DataTemple.AgentEvaluate;

namespace DataTemple.Variables
{
    public class ProgramVariables
    {
        public static void LoadVariables(Context env, double basesal)
        {
            // @defin0 takes no arguments, but uses the context map of the caller
            env.Map.Add("@definz", new CallAgentWrapper(DefineInNoArgRule, ArgumentMode.RemainderUnevaluated, basesal, 4, 10, basesal));
        }

        public static int DefineInNoArgRule(Context context, IContinuation succ, IFailure fail, params object[] args)
        {
            double salience = (double) args[0];

            List<IContent> contents = context.Contents;
            string name = contents[0].Name;
            
            Context definition = new Context(context, contents.GetRange(1, contents.Count - 1));

            context.Map.Add(name, new CallAgentWrapper(EvaluateDefinition, ArgumentMode.NoArugments, salience, definition.Size, 10, salience, definition));

            Context empty = new Context(context, new List<IContent>());
            succ.Continue(empty, fail);

            return 10;
        }

        public static int EvaluateDefinition(Context context, IContinuation succ, IFailure fail, params object[] args)
        {
            double salience = (double) args[0];
            Context definition = (Context)args[1];

            Evaluator eval = new Evaluator(salience, ArgumentMode.ManyArguments, succ, new NopCallable(), true);
            eval.Continue(definition, fail);

            return 10;
        }
    }
}
