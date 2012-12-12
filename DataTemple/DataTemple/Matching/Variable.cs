using System;
using System.Collections.Generic;
using System.Text;
using LanguageNet.Grammarian;
using DataTemple.AgentEvaluate;
using GenericTools;

namespace DataTemple.Matching
{
    public class Variable : Special
    {
        public delegate object GetValue(object defval, params object[] args);

        public Variable(string name)
            : base(name)
        {
        }

        public virtual bool Match(Context env, IParsedPhrase check)
        {
            if (IsMatch(check))
            {
                Propogate(env, check, 1.0);
				env.Map[StarUtilities.NextStarName(env, name)] = check.Text;

                return true;
            }

            return false;
        }

        public virtual bool Match(Context env, Concept check)
        {
            if (IsMatch(check))
            {
                Propogate(env, check, 1.0);
                return true;
            }

            return false;
        }

        public virtual bool Match(Context env, Concept check, GetValue propfunc, params object[] args)
        {
            // Apply propfunc now, and save for later
            object value = propfunc(check, args);
            bool matched = false;
            if (value is Concept)
                matched = IsMatch((Concept) value);
            else if (value is IParsedPhrase)
                matched = IsMatch((IParsedPhrase) value);

            if (matched)
            {
                object propogate = new ThreeTuple<GetValue, object, object[]>(propfunc, check, args);

                Propogate(env, propogate, 1.0);
                return true;
            }

            return false;
        }

        public virtual void Propogate(Context env, object matched, double strength)
        {
            //object myMatched = env.LookupDefaulted<object>("$p$" + name, null);
            double myStrength = env.LookupDefaulted<double>("$s$" + name, 0.0);

            if (myStrength < strength)
            {
                env.Map["$p$" + name] = matched;
                env.Map["$s$" + name] = strength;
            }
        }

        public virtual bool IsMatch(IParsedPhrase check)
        {
            return false;
        }

        public virtual bool IsMatch(Concept concept)
        {
            return false;
        }

        public virtual IParsedPhrase Produce(Context env, POSTagger tagger, GrammarParser parser)
        {
            object var = env.LookupDefaulted<object>("$p$" + name, null);
            if (var is ThreeTuple<GetValue, object, object[]>)
            {
                ThreeTuple<GetValue, object, object[]> tuple = (ThreeTuple<GetValue, object, object[]>)var;
                var = tuple.one(tuple.two, tuple.three);
            }

            if (var is IParsedPhrase)
                return (IParsedPhrase)var;
            else if (var is Concept)
                return ConceptToPhrase(env, (Concept)var, tagger, parser);

            return null;    // don't produce anything!
        }

        public virtual IParsedPhrase ConceptToPhrase(Context context, Concept concept, POSTagger tagger, GrammarParser parser)
        {
            if (concept.IsUnknown)
                return null;

            return concept.ToPhrase(tagger, parser);
        }
    }
}
