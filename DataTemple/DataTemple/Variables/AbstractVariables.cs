using System;
using System.Collections.Generic;
using System.Text;
using DataTemple.AgentEvaluate;
using DataTemple.Matching;
using PluggerBase;
using PluggerBase.ActionReaction.Evaluations;
using LanguageNet.Grammarian;
using LanguageNet.ConceptNet;

namespace DataTemple.Variables
{
    public class AbstractVariables
    {
        public static void LoadVariables(Context context, PluginEnvironment plugenv, double basesal, POSTagger tagger)
        {
			GrammarParser parser = new GrammarParser(plugenv);
            context.Map.Add("%AtLocation", new ConceptRelation(basesal, plugenv, "AtLocation", tagger, parser));
            context.Map.Add("%DefinedAs", new ConceptRelation(basesal, plugenv, "DefinedAs", tagger, parser));
        }
    }

    public class ConceptRelation : CallAgent
    {
        protected string relation;
        protected POSTagger tagger;
		protected GrammarParser parser;

        protected IDataSource<string, Notion> principleSource;
        protected IDataSource<KeyValuePair<Notion, string>, List<Assertion>> assertionSource;

        public ConceptRelation(double salience, PluginEnvironment plugenv, string relation, POSTagger tagger, GrammarParser parser)
            : base(ArgumentMode.ManyArguments, salience, 4, 100)
        {
            this.relation = relation;
            this.tagger = tagger;
			this.parser = parser;

            // Look up the datasources for ConceptNet
            principleSource = plugenv.GetDataSource<string, Notion>(ConceptNetUtilities.PrincipleSourceName);
            assertionSource = plugenv.GetDataSource<KeyValuePair<Notion, string>, List<Assertion>>(ConceptNetUtilities.AssertionSourceName);
        }

        public override bool Call(object value, IContinuation succ, IFailure fail)
        {
			Context context = (Context) value;
            if (principleSource == null || assertionSource == null) {
                fail.Fail("ConceptNet sources missing", succ);
                return true;
            }

            Notion concept;
            if (!principleSource.TryGetValue(StarUtilities.ContentsCode(context, tagger, parser), out concept))
            {
                fail.Fail("Could not find produced in ConceptNet", succ);
                return true;
            }

            List<Assertion> assertions;
            if (!assertionSource.TryGetValue(new KeyValuePair<Notion, string>(concept, relation), out assertions))
                assertions = new List<Assertion>();

            List<IContent> contents = new List<IContent>();

            foreach (Assertion assertion in assertions)
            {
                contents.Add(new Word(assertion.Sentence));
                contents.Add(new Word(" ."));
            }

            succ.Continue(new Context(context, contents), fail);
            return true;
        }
    }
}
