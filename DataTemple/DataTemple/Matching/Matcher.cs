using System;
using System.Collections.Generic;
using System.Text;
using DataTemple.Variables;
using DataTemple.AgentEvaluate;
using LanguageNet.Grammarian;
using PluggerBase.ActionReaction.Evaluations;
using InOutTools;

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
		
        public override bool Evaluate()
        {			
            List<IContent> contents = context.Contents;
			if (contents.Count == 0) {
				Unilog.Notice(this, "Ran out of template before input");
				fail.Fail("Ran out of tokens before matched all input", succ);
				return true;
			}
			
            // Does our first element match the whole thing?
            IContent first = contents[0];
			
			//Console.WriteLine("Match " + first.Name + " against " + input.Text + " + " + unmatched.Count);

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
                    coderack.AddCodelet(eater, "Evaluate *");
                    return true;
                }
                else
                {
                    MatchAgainst(salience, context.ChildRange(1), input, unmatched, succ, eater);
                    return true;
                }
            }
            else if (first.Name == "_")
            {
                StarEater eater = new StarEater(coderack, salience, this, StarUtilities.NextStarName(context, "_"), true);

                coderack.AddCodelet(eater, "Evaluate _");
                return true;
            }
            else if (first.Name == "%opt")
            {
                // Try with, and if that fails, do without
                int end = context.Contents.IndexOf(Special.EndDelimSpecial);
                if (end == -1) {
                    // It's all optional-- but on fail return to match to ensure blank
                    Context without = new Context(context, new List<IContent>());
					Evaluator evalfail = MakeMatcherContinue(salience, without, input, unmatched, succ);
                    IFailure withoutfail = new ContinueCodelet(salience, without, evalfail, myfail);

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
                return true;
            }
            else if (first is Variable)
            {
                if (((Variable)first).Match(context, input))
                {
                    ContinueNextUnmatched(context.ChildRange(1));
                    return true;
                }
                else if (input.IsLeaf)
                {
                    // we didn't match-- fail!
					Unilog.Notice(this, first.Name + " does not match " + input.Text);
                    fail.Fail("Initial variable didn't match", succ);
                    return true;
                }
                else
                {
					GroupPhrase groupPhrase = new GroupPhrase(input);
                    unmatched.InsertRange(0, groupPhrase.GetRange(1));
                    // Call again with the same evaluated first argument
                    Matcher matchrest = new Matcher(salience, groupPhrase.GetBranch(0), unmatched, succ);
                    matchrest.Continue(context, myfail);
                    return true;
                }
            }
            else if (first is Value && ((Value) first).Data is MatchProduceAgent)
            {
                IContinuation mysucc = succ;
				// Check if we have values to match later
                if (unmatched.Count != 0)
                    mysucc = MakeNextUnmatchedContinue(mysucc);
                ContextAppender appender = new ContextAppender(salience, context, -1, mysucc);

                MatchProduceAgent agent = (MatchProduceAgent)((Value) first).Data;
                context.Map["$check"] = input;
                ContinueToCallAgent codelet = new ContinueToCallAgent(agent, appender);
				
				IFailure deepenfail = myfail;
				if (!input.IsLeaf) {
					// Continue to deeper
					GroupPhrase groupPhrase = new GroupPhrase(input);
		            unmatched.InsertRange(0, groupPhrase.GetRange(1));
		            // Call again with the same evaluated first argument
		            Matcher matchrest = new Matcher(salience, groupPhrase.GetBranch(0), unmatched, succ);
					deepenfail = new FailToContinue(context, matchrest, myfail);
				}
				
                codelet.Continue(context, deepenfail);
                return true;
            }

            if (first is Word && input.IsLeaf)
            {
				WordComparer comparer = (WordComparer) context.LookupSimple("$Compare");
                if (comparer.Match(input.Text, first.Name))
                {
                    ContinueNextUnmatched(context.ChildRange(1));
                    return true;
                }
                else
                {
                    // failure!
                    fail.Fail(string.Format("Pattern [{0}] does not match [{1}]", first.Name, input.Text), succ);
                    return true;
                }
            } else if (first is Word) {
				GroupPhrase groupPhrase = new GroupPhrase(input);
                unmatched.InsertRange(0, groupPhrase.GetRange(1));
                Matcher matchcont = new Matcher(salience, groupPhrase.GetBranch(0), unmatched, succ);
                matchcont.Continue(context, myfail);
                return true;
            }

            // We can't handle this!  fail
            fail.Fail("Unknown first element", succ);

            return true;
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

            Evaluator eval = new Evaluator(salience, ArgumentMode.SingleArgument, appender.AsIndex(0), appender.AsIndex(1), true);

            return eval;
        }
		
		public Evaluator MakeNextUnmatchedContinue(IContinuation succ) {
			return MakeMatcherContinue(salience, context.ChildRange(1), unmatched[0], unmatched.GetRange(1, unmatched.Count - 1), succ);
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
				Unilog.Notice(this, "Ran out of tokens before input");
                fail.Fail("Ran out of tokens before matched all input", succ);
                return;
            }

            MatchAgainst(salience, context, unmatched[0], unmatched.GetRange(1, unmatched.Count - 1), succ, fail);
        }
				
		public static bool IsRemainderOptional(List<IContent> contents) {
			for (int ii = 0; ii < contents.Count; ii++) {
				IContent content = contents[ii];
				if (content.Name == "%opt") {
					int end = contents.IndexOf(Special.EndDelimSpecial, ii);
                	if (end == -1)
						return true;
					ii = end;
					continue;
				}
				if (content is Special && (content.Name.StartsWith("*") || content.Name == "%end"))
					continue;
				return false;
			}
			
			return true;
		}
		
		public override string ToString()
		{
			GroupPhrase groupPhrase = new GroupPhrase("FRAG", unmatched);
			StringBuilder code = new StringBuilder();
			foreach (IContent content in context.Contents)
				code.Append(content.Name + " ");
			return string.Format("[Matcher: Contents={0}, Input={1}, Unmatched={2}]", code.ToString(), input.Text, groupPhrase.Text);
		}
		
		public override string TraceTitle {
			get {
				return "Matcher";
			}
		}
		
		protected class FailToContinue : IFailure {
			protected Context context;
			protected IContinuation succ;
			protected IFailure fail;
			
			public FailToContinue(Context context, IContinuation succ, IFailure fail) {
				this.context = context;
				this.succ = succ;
				this.fail = fail;
			}
			
			public bool Fail(string reason, IContinuation skip)
			{
				return succ.Continue(context, fail);
			}
			
			public object Clone()
			{
				return new FailToContinue(context, succ, fail);
			}
		}
    }
}
