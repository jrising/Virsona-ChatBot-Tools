using System;
using System.Collections.Generic;
using System.Text;
using PluggerBase.ActionReaction.Evaluations;
using LanguageNet.Grammarian;
using DataTemple.Codeland;
using DataTemple.AgentEvaluate;

namespace DataTemple.Matching
{
    public class StarEater : Codelet, IFailure
    {
        protected Matcher matcher;
        protected string name;
        protected bool needRemoval;    // true to strip off first element of matcher

        public StarEater(Coderack coderack, double salience, Matcher matcher, string name, bool needRemoval)
            : base(coderack, salience, 0, 10)
        {
            this.matcher = matcher;
            this.name = name;
            this.needRemoval = needRemoval;
        }

        public override int Evaluate()
        {
            // Drill down to a single POSPhrase
            List<IParsedPhrase> unmatched = new List<IParsedPhrase>(matcher.Unmatched);
            IParsedPhrase input = matcher.Input;
            while (!input.IsLeaf)
            {
				GroupPhrase groupPhrase = new GroupPhrase(input);
                unmatched.InsertRange(0, groupPhrase.GetRange(1));
                input = groupPhrase.GetBranch(0);
            }

            List<IContent> elements = matcher.Context.LookupDefaulted<List<IContent>>(name, new List<IContent>());
            elements.Add(new Word(input.Text));
            Context eaten;
            if (needRemoval)
                eaten = new Context(matcher.Context, matcher.Context.Contents.GetRange(1, matcher.Context.Contents.Count - 1));
            else
                eaten = new Context(matcher.Context, matcher.Context.Contents);
            eaten.Map.Add(name, elements);

            // Continue matcher
            if (unmatched.Count == 0)
            {
                matcher.Success.Continue(eaten, matcher.Failure);
            }
            else
            {
                // This matcher is for StarEater to eat another
                Matcher clone = (Matcher)matcher.Clone();
                clone.Context = eaten;
                clone.Input = unmatched[0];
                clone.Unmatched = unmatched.GetRange(1, unmatched.Count - 1);

                StarEater eater = new StarEater(coderack, salience, clone, name, false);

                if (eaten.Contents.Count == 0)
                {
                    // Just add ourselves again
                    coderack.AddCodelet(eater);
                }
                else
                    Matcher.MatchAgainst(salience, eaten, clone.Input, clone.Unmatched, matcher.Success, eater);
            }

            return time;
        }

        #region IFailure Members

        public int Fail(string reason, IContinuation succ)
        {
            coderack.AddCodelet((Codelet) this.Clone());
            return 1;
        }
				
        #endregion
    }
}
