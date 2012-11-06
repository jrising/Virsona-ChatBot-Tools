using System;
using System.Collections.Generic;
using System.Text;
using DataTemple.AgentEvaluate;
using DataTemple.Codeland;
using PluggerBase.ActionReaction.Evaluations;

namespace DataTemple.Matching
{
    public class Thinker : ContinueAgentCodelet, IFailure
    {
        protected const int timeEach = 10;

        protected Memory memory;
        protected List<Relations.Relation> kinds;

        protected Variable.GetValue propfunc = null;
        protected object[] propargs = null;

        protected List<Datum> directchecks;
        protected Queue<Concept> parentchecks;

        public Thinker(double salience, Memory memory, List<Relations.Relation> kinds, Datum check, IContinuation succ)
            : base(salience, 4 * 7, timeEach, succ)
        {
            this.memory = memory;
            this.kinds = kinds;
            directchecks = new List<Datum>();
            directchecks.Add(check);
            parentchecks = new Queue<Concept>();
            parentchecks.Enqueue(check.Left);
            parentchecks.Enqueue(check.Right);
        }

        public Thinker(double salience, Memory memory, List<Relations.Relation> kinds, Datum check, IContinuation succ, Variable.GetValue propfunc, params object[] propargs)
            : this(salience, memory, kinds, check, succ)
        {
            this.propfunc = propfunc;
            this.propargs = propargs;
        }

        public override int Evaluate()
        {
            // Look through my concepts for a possible match
            List<IContent> contents = context.Contents;

            if (contents.Count == 1) {
                if (contents[0] is Variable)
                {
                    Variable check = (Variable)contents[0];

                    // If we already have a concept in mind, just check its data
                    Concept left = context.LookupDefaulted<Concept>("$knowConcept", null);
                    if (left != null)
                    {
                        List<Datum> data = memory.GetData(left);
                        foreach (Datum datum in data)
                        {
                            if (datum.Left == left && kinds.Contains(datum.Relation) && check.Match(context, datum.Right))
                            {
                                List<Datum> completes = context.LookupAndAdd<List<Datum>>("$knowCompletes", new List<Datum>());
                                completes.Add(datum);

                                succ.Continue(new Context(context, new List<IContent>()), fail);
                                return time;
                            }
                        }

                        // Can't match anything!
                        fail.Fail("Initial variable didn't match anything", succ);
                        return time;
                    }

                    // Does anything here match?
                    for (int ii = 0; ii < directchecks.Count; ii++)
                    {
                        bool matched = kinds.Contains(directchecks[ii].Relation);
                        if (matched)
                        {
                            matched = check.Match(context, directchecks[ii].Right);
                            if (propfunc != null) // always do propfuncs, but only affect matched if failed
                                matched = check.Match(context, directchecks[ii].Right, propfunc, propargs) || matched;
                        }

                        if (matched)
                        {
                            Context child = new Context(context, new List<IContent>());
                            child.Map["$knowConcept"] = directchecks[ii].Left;

                            List<Datum> completes = child.LookupAndAdd<List<Datum>>("$knowCompletes", new List<Datum>());
                            completes.Add(directchecks[ii]);

                            // Make a clone of ourselves, if this fails
                            succ.Continue(child, MakeContinuingFailure(ii));
                            return time;
                        }
                    }
                }
                else if (contents[0].Name == "*" || contents[0].Name == "_")
                {
                    // If we already have a concept in mind, just check its data
                    Concept left = context.LookupDefaulted<Concept>("$knowConcept", null);
                    if (left != null)
                    {
                        List<Datum> data = memory.GetData(left);
                        foreach (Datum datum in data)
                        {
                            if (datum.Left == left && kinds.Contains(datum.Relation))
                            {
                                List<IContent> words = new List<IContent>();
                                if (!datum.Right.IsUnknown)
                                    words.Add(new Word(datum.Right.Name));
                                context.Map.Add(StarUtilities.NextStarName(context, contents[0].Name), words);

                                List<Datum> completes = context.LookupAndAdd<List<Datum>>("$knowCompletes", new List<Datum>());
                                completes.Add(datum);

                                succ.Continue(new Context(context, new List<IContent>()), fail);
                                return time;
                            }
                        }

                        // Can't match anything!
                        fail.Fail("Could not find a matching datum", succ);
                        return time;
                    }

                    // Does anything here match?
                    for (int ii = 0; ii < directchecks.Count; ii++)
                        if (kinds.Contains(directchecks[ii].Relation))
                        {
                            Context child = new Context(context, new List<IContent>());
                            
                            List<IContent> words = new List<IContent>();
                            if (!directchecks[ii].Right.IsUnknown)
                                words.Add(new Word(directchecks[ii].Right.Name));
                            child.Map.Add(StarUtilities.NextStarName(child, contents[0].Name), words);
                            child.Map["$knowConcept"] = directchecks[ii].Left;

                            List<Datum> completes = child.LookupAndAdd<List<Datum>>("$knowCompletes", new List<Datum>());
                            completes.Add(directchecks[ii]);

                            succ.Continue(child, MakeContinuingFailure(ii));
                            return time;
                        }
                }

                // Nothing matched!  Expand search
                if (parentchecks.Count > 0)
                {
                    Concept parent = parentchecks.Dequeue();
                    directchecks = memory.GetData(parent);
                    Continue(new Context(context, context.Contents, context.Weight * 0.95), fail);
                    return time;
                }
            }

            // Don't fail if it's just a *
            if (contents.Count == 1 && contents[0].Name == "*")
            {
                Context child = new Context(context, new List<IContent>());

                List<IContent> empty = new List<IContent>();
                child.Map.Add(StarUtilities.NextStarName(child, contents[0].Name), empty);

                succ.Continue(child, fail);
                return time;
            }

            fail.Fail("No data matches", succ);
            return time;
        }

        public static void SearchForMatch(double salience, Memory memory, List<Relations.Relation> kinds, Datum check, Context context, IContinuation succ, IFailure fail)
        {
            Thinker thinker = new Thinker(salience, memory, kinds, check, succ);
            thinker.Continue(context, fail);
        }

        public static void SearchForMatch(double salience, Memory memory, List<Relations.Relation> kinds, Datum check, Context context, IContinuation succ, IFailure fail, Variable.GetValue propfunc, params object[] propargs)
        {
            Thinker thinker = new Thinker(salience, memory, kinds, check, succ, propfunc, propargs);
            thinker.Continue(context, fail);
        }

        // we've processed directchecks through ii
        protected IFailure MakeContinuingFailure(int ii)
        {
            if (directchecks.Count > ii + 1 || parentchecks.Count > 0)
            {
                Thinker clone = (Thinker)Clone();
                clone.directchecks = directchecks.GetRange(ii + 1, directchecks.Count - ii - 1);
                clone.time = timeEach;
                return clone;
            }

            return fail;
        }

        #region IFailure Members

        public int Fail(string reason, IContinuation succ)
        {
            // continue our work
            coderack.AddCodelet((Codelet) this.Clone());
            return 1;
        }

        #endregion
    }
}
