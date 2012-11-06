using System;
using System.Collections.Generic;
using System.Text;
using DataTemple.Variables;
using DataTemple.AgentEvaluate;
using LanguageNet.Grammarian;
using PluggerBase.ActionReaction.Evaluations;

namespace DataTemple.Matching
{
    // Calls succ with the unconsumed elements
    // The first element must be evaluated!  Call EvaluateFirst to add
    public class Matcher : ContinueAgentCodelet
    {
        protected IParsedPhrase input;
        protected List<IParsedPhrase> unmatched;

        // Don't call this.  Call MatchAgainst
        protected Matcher(double salience, IParsedPhrase input, List<IParsedPhrase> unmatched, IContinuation succ)
            : base(salience, 2 * 4, 10, succ)
        {
			if (input == null)
				throw new NullReferenceException("Input cannot be null.");
            this.input = input;
            this.unmatched = new List<IParsedPhrase>(unmatched);   // make copy, or %opt-fail has effects
        }

        public IParsedPhrase Input
        {
            get
            {
                return input;
            }
            set
            {
                input = value;
            }
        }

        public List<IParsedPhrase> Unmatched
        {
            get
            {
                return unmatched;
            }
            set
            {
                unmatched = value;
            }
        }
		
        public override int Evaluate()
        {
            List<IContent> contents = context.Contents;

            // Does our first element match the whole thing?
            IContent first = contents[0];

            // always consider dropping interjections
            IFailure myfail = fail;
            if (input.Part == "UH")
            {
                IFailure skipfail;
                if (unmatched.Count == 0)
                {
                    // then fail becomes success!
                    skipfail = new ContinueCodelet(salience, context, succ, fail);
                }
                else
                {
                    Matcher matchskip = new Matcher(salience, unmatched[0], unmatched.GetRange(1, unmatched.Count - 1), succ);
                    skipfail = new ContinueCodelet(salience, context, matchskip, fail);
                }

                myfail = skipfail;
            }

            if (first.Name == "*")
            {
                // Failure state has the first POS eaten by the star
                StarEater eater = new StarEater(coderack, salience, this, StarUtilities.NextStarName(context, "*"), true);

                if (context.Contents.Count == 1)
                {
                    // We ran out elements, but we still have some unmatched
                    coderack.AddCodelet(eater);
                    return time;
                }
                else
                {
                    Context newctx = new Context(context, contents.GetRange(1, contents.Count - 1));
                    MatchAgainst(salience, newctx, input, unmatched, succ, eater);
                    return time;
                }
            }
            else if (first.Name == "_")
            {
                StarEater eater = new StarEater(coderack, salience, this, StarUtilities.NextStarName(context, "_"), true);

                coderack.AddCodelet(eater);
                return time;
            }
            else if (first.Name == "%opt")
            {
                // Try with, and if that fails, do without
                int end = context.Contents.IndexOf(Special.EndDelimSpecial);
                if (end == -1) {
                    // It's all optional-- the failure is success!
                    Context without = new Context(context, new List<IContent>());
                    IFailure withoutfail = new ContinueCodelet(salience, without, succ, myfail);

                    Context with = new Context(context, context.Contents.GetRange(1, context.Contents.Count - 1));
                    Matcher.MatchAgainst(salience, with, input, unmatched, succ, withoutfail);
                } else {
                    Context without = new Context(context, context.Contents.GetRange(end + 1, context.Contents.Count - (end + 1)));
                    Evaluator evalfail = MakeMatcherContinue(salience, without, input, unmatched, succ);
                    IFailure withoutfail = new ContinueCodelet(salience, without, evalfail, myfail);

                    Context with = new Context(context, context.Contents.GetRange(1, end - 1));
                    with.Contents.AddRange(without.Contents);
                    Matcher.MatchAgainst(salience, with, input, unmatched, succ, withoutfail);
                }
                return time;
            }
            else if (first is Variable)
            {
                if (((Variable)first).Match(context, input))
                {
                    ContinueNextUnmatched(new Context(context, contents.GetRange(1, contents.Count - 1)));
                    return time;
                }
                else if (input.IsLeaf)
                {
                    // we didn't match-- fail!
                    fail.Fail("Initial variable didn't match", succ);
                    return time;
                }
                else
                {
					GroupPhrase groupPhrase = new GroupPhrase(input);
                    unmatched.InsertRange(0, groupPhrase.GetRange(1));
                    // Call again with the same evaluated first argument
                    Matcher matchrest = new Matcher(salience, groupPhrase.GetBranch(0), unmatched, succ);
                    matchrest.Continue(context, myfail);
                    return time;
                }
            }
            else if (first is Value && ((Value) first).Data is MatchProduceAgent)
            {
                IContinuation mysucc = succ;
                if (unmatched.Count != 0)
                    mysucc = new Matcher(salience, unmatched[0], unmatched.GetRange(1, unmatched.Count - 1), succ);
                ContextAppender appender = new ContextAppender(salience, context, -1, mysucc);

                MatchProduceAgent agent = (MatchProduceAgent)((Value) first).Data;
                context.Map["$check"] = input;
                ContinueToCallAgent codelet = new ContinueToCallAgent(agent, appender);

                codelet.Continue(context, fail);
                return time;
            }

            if (first is Word && input.IsLeaf)
            {
                if (input.Text == first.Name)
                {
                    ContinueNextUnmatched(new Context(context, contents.GetRange(1, contents.Count - 1)));
                    return time;
                }
                else
                {
                    // failure!
                    fail.Fail(string.Format("Pattern [{0}] does not match [{1}]", first.Name, input.Text), succ);
                    return time;
                }
            } else if (first is Word) {
				GroupPhrase groupPhrase = new GroupPhrase(input);
                unmatched.InsertRange(0, groupPhrase.GetRange(1));
                Matcher matchcont = new Matcher(salience, groupPhrase.GetBranch(0), unmatched, succ);
                matchcont.Continue(context, myfail);
                return time;
            }

            // We can't handle this!  fail
            fail.Fail("Unknown first element", succ);

            return time;
        }

        public static void MatchAgainst(double salience, Context context, IParsedPhrase input, List<IParsedPhrase> unmatched, IContinuation succ, IFailure fail)
        {
            Evaluator eval = MakeMatcherContinue(salience, context, input, unmatched, succ);

            eval.Continue(context, fail);
        }

        public static Evaluator MakeMatcherContinue(double salience, Context context, IParsedPhrase input, List<IParsedPhrase> unmatched, IContinuation succ)
        {
            context.Map["$check"] = null;

            // Match this to first constituent, the continue for others
            Matcher matcheval = new Matcher(salience, input, unmatched, succ);

            ContinuationAppender appender = new ContinuationAppender(context, matcheval);

            Evaluator eval = new Evaluator(salience, ArgumentMode.SingleArgument, appender, appender);
            eval.Lineage = NewLineage();
            appender.RegisterCaller(eval.Lineage);
            appender.RegisterCaller(eval.Lineage);

            return eval;
        }

        public void ContinueNextUnmatched(Context context)
        {
            if (unmatched.Count == 0)
            {
                succ.Continue(context, fail);
                return;
            }

            if (context.Contents.Count == 0)
            {
                // We ran out elements, but we still have some unmatched
                fail.Fail("Ran out of elements before matched all", succ);
                return;
            }

            MatchAgainst(salience, context, unmatched[0], unmatched.GetRange(1, unmatched.Count - 1), succ, fail);
        }
    }
}
