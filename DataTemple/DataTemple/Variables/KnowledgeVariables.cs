using System;
using System.Collections.Generic;
using System.Text;
using DataTemple.AgentEvaluate;
using DataTemple.Matching;
using LanguageNet.Grammarian;
using PluggerBase;
using PluggerBase.ActionReaction.Evaluations;

namespace DataTemple.Variables
{
    public class KnowledgeVariables
    {
        public static void LoadVariables(Context context, double salience, Memory memory, ConceptTranslator produceTranslator, Verbs verbs, Nouns nouns, PluginEnvironment plugenv)
        {
			POSTagger tagger = new POSTagger(plugenv);
			GrammarParser parser = new GrammarParser(plugenv);
			
            context.Map.Add("@know", new KnowRule(salience, memory, tagger, parser));
            context.Map.Add("@event", new EventRule(salience, memory, tagger, parser));

            context.Map.Add("@Subject", new SimpleDatumRule(salience, Relations.Relation.Subject, memory, produceTranslator, tagger, parser));
            context.Map.Add("@Object", new SimpleDatumRule(salience, Relations.Relation.Object, memory, produceTranslator, tagger, parser));
            context.Map.Add("@Indirect", new SimpleDatumRule(salience, Relations.Relation.Indirect, memory, produceTranslator, tagger, parser));
            context.Map.Add("@IsA", new SimpleDatumRule(salience, Relations.Relation.IsA, memory, produceTranslator, tagger, parser));
            context.Map.Add("@HasProperty", new SimpleDatumRule(salience, Relations.Relation.HasProperty, memory, produceTranslator, tagger, parser));
            context.Map.Add("@Means", new SimpleDatumRule(salience, Relations.Relation.Means, memory, produceTranslator, tagger, parser));
            context.Map.Add("@Condition", new SimpleDatumRule(salience, Relations.Relation.Condition, memory, produceTranslator, tagger, parser));
            context.Map.Add("@MotivatedBy", new SimpleDatumRule(salience, Relations.Relation.MotivatedByGoal, memory, produceTranslator, tagger, parser));
            context.Map.Add("@Exists", new SimpleDatumRule(salience, Relations.Relation.Exists, memory, produceTranslator, tagger, parser));
            context.Map.Add("@UsedFor", new SimpleDatumRule(salience, Relations.Relation.UsedFor, memory, produceTranslator, tagger, parser));
            context.Map.Add("@AtTime", new AtTimeRule(salience, memory, tagger, parser));
            context.Map.Add("@InLocation", new InLocationRule(salience, memory, verbs, tagger, parser));

            //context.Map.Add("@ActiveObjects", new AllObjectsRule(salience, true, memory, produceTranslator, tagger, parser));
            //context.Map.Add("@PassiveObjects", new AllObjectsRule(salience, false, memory, produceTranslator, tagger, parser));

            context.Map.Add("@EntityPrep", new EntityPrepRule(salience, memory, verbs, tagger, parser));
            context.Map.Add("@SubjectTense", new TenseRule(salience, Relations.Relation.Subject, memory, verbs, nouns, tagger, parser));

            context.Map.Add("%unknown", new UnknownConceptVariable("%unknown", null));
            context.Map.Add("%unevent", new UnknownConceptVariable("%unevent", Concept.Kind.Event));
            context.Map.Add("%unthing", new UnknownConceptVariable("%unthing", Concept.Kind.Entity));
            context.Map.Add("%unquality", new UnknownConceptVariable("%unquality", Concept.Kind.Attribute));
        }
    }

    public class KnowRule : MatchProduceAgent
    {
        protected Memory memory;

		
        public KnowRule(double salience, Memory memory, POSTagger tagger, GrammarParser parser)
            : base(ArgumentMode.ManyArguments, salience, 2 * 4, 3, tagger, parser)
        {
            this.memory = memory;
            this.tagger = tagger;
        }

        public override bool Produce(Context context, IContinuation succ, IFailure fail)
        {
            Concept concept = CompletePartials(context);
            if (concept == null)
            {
                fail.Fail("Nothing to complete partials", succ);
                return true;
            }

            succ.Continue(new Context(context, new List<IContent>()), fail);
            return true;
        }

        public Concept CompletePartials(Context context)
        {
            if (context.Contents.Count == 0)
                return null;

            IParsedPhrase phrase = StarUtilities.ProducedPhrase(context, tagger, parser);
            if (phrase == null)
            {
                // cannot do!
                context.Map["$knowPartials"] = new List<Datum>();
                return null;
            }
            Concept concept = memory.NewConcept(phrase);

            List<Datum> completes = context.LookupAndAdd<List<Datum>>("$knowCompletes", new List<Datum>());

            List<Datum> data = context.LookupDefaulted<List<Datum>>("$knowPartials", new List<Datum>());
            foreach (Datum datum in data)
                completes.Add(memory.Know(concept, datum));

            context.Map["$knowPartials"] = new List<Datum>();

            return concept;
        }

        public override bool Match(object check, Context context, IContinuation succ, IFailure fail)
        {
            Concept concept = (Concept) context.Lookup("$knowConcept");
            if (context.Contents.Count == 1)
            {
                if (context.Contents[0] is Variable)
                {
                    Variable left = (Variable)context.Contents[0];
                    if (left.Match(context, concept))
                        succ.Continue(new Context(context, new List<IContent>()), fail);
                    else
                        fail.Fail("Left doesn't match context", succ);
                    return true;
                }
                else if (context.Contents[0].Name == "*" || context.Contents[0].Name == "_")
                {
                    List<IContent> words = new List<IContent>();
                    words.Add(new Word(concept.Name));
                    context.Map.Add(StarUtilities.NextStarName(context, context.Contents[0].Name), words);

                    succ.Continue(new Context(context, new List<IContent>()), fail);
                    return true;
                }
            }

            fail.Fail("Know given multiple values", succ);
            return true;
        }
    }

    public class EventRule : KnowRule
    {
        public EventRule(double salience, Memory memory, POSTagger tagger, GrammarParser parser)
            : base(salience, memory, tagger, parser)
        {
        }

        public override bool Produce(Context context, IContinuation succ, IFailure fail)
        {
            Concept concept = CompletePartials(context);
            if (concept == null)
            {
                fail.Fail("Nothing to complete partials", succ);
                return true;
            }

            IParsedPhrase phrase = ((ClauseVariable)context.Lookup("%clause")).GetPropogated(context, "%clause");
            if (phrase != null)
            {
                EventVariable.AddEventClause(context, phrase);
                EventVariable.AddEventConcept(context, concept);
            }

            succ.Continue(new Context(context, new List<IContent>()), fail);
            return true;
        }
    }

    public class SimpleDatumRule : MatchProduceAgent
    {
        protected Memory memory;
        protected ConceptTranslator produceTranslator;
        protected Relations.Relation kind;


		public SimpleDatumRule(double salience, Relations.Relation kind, Memory memory, ConceptTranslator produceTranslator, POSTagger tagger, GrammarParser parser)
            : base(ArgumentMode.ManyArguments, salience, 3 * 4, 10, tagger, parser)
        {
            this.kind = kind;
            this.memory = memory;
            this.produceTranslator = produceTranslator;
            this.tagger = tagger;
        }

        public override bool Produce(Context context, IContinuation succ, IFailure fail)
        {
            IParsedPhrase other = StarUtilities.ProducedPhrase(context, tagger, parser);
            if (other == null)
            {
                succ.Continue(new Context(context, new List<IContent>()), fail);
                return true; // cannot do!
            }

            Concept concept = produceTranslator.GetConcept(other);

            Datum datum = new Datum(null, kind, concept, context.Weight);

            context.LookupAndAdd<List<Datum>>("$knowPartials", new List<Datum>()).Add(datum);

            succ.Continue(new Context(context, new List<IContent>()), fail);
            return true;
        }

        public override bool Match(object check, Context context, IContinuation succ, IFailure fail)
        {
            List<Relations.Relation> kinds = new List<Relations.Relation>();
            kinds.Add(kind);

            Thinker.SearchForMatch(salience, memory, kinds, (Datum) check, context, succ, fail);

            return true;
        }

        public static void Know(Memory memory, Context context, Relations.Relation kind, IParsedPhrase phrase, double weight, ConceptTranslator produceTranslator)
        {
            Concept concept = produceTranslator.GetConcept(phrase);

            Datum datum = new Datum(null, kind, concept, weight);

            context.LookupAndAdd<List<Datum>>("$knowPartials", new List<Datum>()).Add(datum);
        }
    }

    public class AtTimeRule : MatchProduceAgent
    {
        protected Memory memory;
				
        public AtTimeRule(double salience, Memory memory, POSTagger tagger, GrammarParser parser)
            : base(ArgumentMode.ManyArguments, salience, 2 * 4, 10, tagger, parser)
        {
            this.memory = memory;
            this.tagger = tagger;
        }

        public override bool Produce(Context context, IContinuation succ, IFailure fail)
        {
            IParsedPhrase phrase = StarUtilities.ProducedPhrase(context, tagger, parser);
            if (phrase == null)
            {
                succ.Continue(new Context(context, new List<IContent>()), fail);
                return true; // cannot do!
            }

            KnowPhrase(phrase, context, memory);

            succ.Continue(new Context(context, new List<IContent>()), fail);
            return true;
        }

        public static bool KnowPhrase(IParsedPhrase phrase, Context context, Memory memory)
        {
			GroupPhrase groupPhrase = new GroupPhrase(phrase);
            IParsedPhrase datetime = groupPhrase.GetBranch(1);

            Datum datum = new Datum(null, Relations.Relation.AtTime, memory.NewConcept(datetime), context.Weight);

            context.LookupAndAdd<List<Datum>>("$knowPartials", new List<Datum>()).Add(datum);

            return true;
        }

        public override bool Match(object check, Context context, IContinuation succ, IFailure fail)
        {
            List<Relations.Relation> kinds = new List<Relations.Relation>();
            kinds.Add(Relations.Relation.AtTime);

            Thinker.SearchForMatch(salience, memory, kinds, (Datum) check, context, succ, fail);

            return true;
        }
    }

    public class InLocationRule : MatchProduceAgent
    {
        protected Memory memory;
        protected Verbs verbs;

        public InLocationRule(double salience, Memory memory, Verbs verbs, POSTagger tagger, GrammarParser parser)
            : base(ArgumentMode.ManyArguments, salience, 3 * 4, 10, tagger, parser)
        {
            this.memory = memory;
            this.verbs = verbs;
            this.tagger = tagger;
        }

        public override bool Produce(Context context, IContinuation succ, IFailure fail)
        {
            IParsedPhrase phrase = StarUtilities.ProducedPhrase(context, tagger, parser);
            if (phrase == null)
            {
                succ.Continue(new Context(context, new List<IContent>()), fail);
                return true; // cannot do!
            }

            KnowPhrase(phrase, context, memory);

            succ.Continue(new Context(context, new List<IContent>()), fail);
            return true;
        }

        public static bool KnowPhrase(IParsedPhrase phrase, Context context, Memory memory)
        {
            if (phrase.Part == "PP")
            {
                Datum datum = new Datum(null, Relations.Relation.InLocation, memory.NewConcept(phrase), context.Weight);

                context.LookupAndAdd<List<Datum>>("$knowPartials", new List<Datum>()).Add(datum);

                return true;
            }

            return false;
        }

        public override bool Match(object check, Context context, IContinuation succ, IFailure fail)
        {
            List<Relations.Relation> kinds = new List<Relations.Relation>();
            kinds.Add(Relations.Relation.InLocation);

            Thinker.SearchForMatch(salience, memory, kinds, (Datum)check, context, succ, fail);

            return true;
        }
    }

    /*public class AllObjectsRule : MatchProduceAgent
    {
        protected bool isActive;

        protected Memory memory;
        protected ConceptTranslator produceTranslator;
		protected GrammarParser parser;

        public AllObjectsRule(double salience, bool isActive, Memory memory, ConceptTranslator produceTranslator, POSTagger tagger, GrammarParser parser)
            : base(ArgumentMode.ManyArguments, salience, 2 * 4, 10, tagger)
        {
            this.isActive = isActive;

            this.memory = memory;
            this.produceTranslator = produceTranslator;
			this.parser = parser;
        }

        public override int Produce(Context context, IContinuation succ, IFailure fail)
        {
            IParsedPhrase phrase = StarUtilities.ProducedPhrase(context, tagger, parser);
            if (phrase == null)
            {
                // no objects!
                succ.Continue(new Context(context, new List<IContent>()), fail);
                return time;
            }

            if (phrase.Part == "NN" || phrase.Part == "NP" || phrase.Part == "PP")
                HandleElement(phrase, context);
            else
                foreach (IParsedPhrase constituent in phrase.Branches)
                    HandleElement(constituent, context);
            
            succ.Continue(new Context(context, new List<IContent>()), fail);
            return time;
        }

        public override int Match(object check, Context context, IContinuation succ, IFailure fail)
        {
            List<Relations.Relation> kinds = new List<Relations.Relation>();
            kinds.Add(Relations.Relation.Object);
            kinds.Add(Relations.Relation.Indirect);

            Thinker.SearchForMatch(salience, memory, kinds, (Datum)check, context, succ, fail);

            return time;
        }

        public delegate bool KnowPhraseDelegate(IParsedPhrase phrase, Context context, Memory memory);
        
        public void HandleElement(IParsedPhrase phrase, Context context)
        {
            if (phrase.Part == "NN" || phrase.Part == "NP")
            {
                if (isActive)
                    SimpleDatumRule.Know(memory, context, Relations.Relation.Object, phrase, context.Weight, produceTranslator);
                else
                    SimpleDatumRule.Know(memory, context, Relations.Relation.Indirect, phrase, context.Weight, produceTranslator);
            }
            else if (phrase.Part == "PP")
            {
				GroupPhrase groupPhrase = new GroupPhrase(phrase;
                IParsedPhrase preposition = groupPhrase.FindBranch("IN");
                if (!isActive && preposition.Text == "by")
                    SimpleDatumRule.Know(memory, context, Relations.Relation.Subject, groupPhrase.GetBranch(1), context.Weight, produceTranslator);
                else
                {
                    BestScore<KnowPhraseDelegate> producer = new BestScore<KnowPhraseDelegate>();

                    // Try out different interpretations:
                    IParsedPhrase obj = groupPhrase.GetBranch(1);
                    SourceWordLookup informer = new SourceWordLookup();
                    InformedPhrase informed = AgentParser.BuildInformedPhrase(obj, informer);

                    // Temporalization?  Use AtTimeRule
                    producer.Improve(ValidateUtilities.SeemsTime(obj.Simple).ToDouble(0.5), AtTimeRule.KnowPhrase);

                    // Localization? Use InLocationRule
                    LocationProperty location = new LocationProperty(true);
                    ProbableStrength isLocation = location.Describes(informed.Senses[0].Key);
                    producer.Improve(isLocation.ToDouble(0.5), InLocationRule.KnowPhrase);

                    if (producer.Payload == null)
                        SimpleDatumRule.Know(memory, context, Relations.Relation.Indirect, phrase, context.Weight, produceTranslator);
                    else
                        producer.Payload(phrase, context, memory);
                }
            }
            else if (phrase is Punctuation)
            {
                // ignore it
            }
            else
            {
                // Unknown!
                SimpleDatumRule.Know(memory, context, Relations.Relation.HasProperty, phrase, context.Weight, produceTranslator);
            }
        }
    }*/

    public class EntityPrepRule : MatchProduceAgent
    {
        protected Memory memory;
        protected Verbs verbs;

        public EntityPrepRule(double salience, Memory memory, Verbs verbs, POSTagger tagger, GrammarParser parser)
            : base(ArgumentMode.ManyArguments, salience, 3 * 4, 10, tagger, parser)
        {
            this.memory = memory;
            this.verbs = verbs;
            this.tagger = tagger;
        }

        public override bool Produce(Context context, IContinuation succ, IFailure fail)
        {
            IParsedPhrase phrase = StarUtilities.ProducedPhrase(context, tagger, parser);
            if (phrase == null)
            {
                succ.Continue(new Context(context, new List<IContent>()), fail);
                return true; // cannot do!
            }

            if (phrase.Part == "PP")
            {
                // the ring on my finger
                // the ring of gold
            }

            succ.Continue(new Context(context, new List<IContent>()), fail);
            return true;
        }

        public override bool Match(object check, Context context, IContinuation succ, IFailure fail)
        {
            List<Relations.Relation> kinds = new List<Relations.Relation>();
            kinds.Add(Relations.Relation.InLocation);
            kinds.Add(Relations.Relation.HasProperty);

            Thinker.SearchForMatch(salience, memory, kinds, (Datum)check, context, succ, fail);

            return true;
        }
    }

    public class TenseRule : MatchProduceAgent
    {
        protected Relations.Relation nounrel;

        protected Memory memory;
        protected Verbs verbs;
        protected Nouns nouns;

        public TenseRule(double salience, Relations.Relation nounrel, Memory memory, Verbs verbs, Nouns nouns, POSTagger tagger, GrammarParser parser)
            : base(ArgumentMode.ManyArguments, salience, 2 * 4, 10, tagger, parser)
        {
            this.nounrel = nounrel;

            this.memory = memory;
            this.verbs = verbs;
            this.nouns = nouns;
        }

        public override bool Produce(Context context, IContinuation succ, IFailure fail)
        {
            string word = StarUtilities.ProducedCode(context, tagger, parser);
            Datum datum;

            if (word == "past")
                datum = new Datum(null, Relations.Relation.Tense, memory.past, context.Weight);
            else if (word.Contains(" "))
                datum = new Datum(null, Relations.Relation.Tense, memory.NewConcept(word, Concept.Kind.Entity), context.Weight);
            else
            {
                bool isPast = verbs.GetInflection(word) == Verbs.Convert.ext_Ved;
                if (isPast)
                    datum = new Datum(null, Relations.Relation.Tense, memory.past, context.Weight);
                else
                    datum = new Datum(null, Relations.Relation.Tense, memory.now, context.Weight);
            }

            context.LookupAndAdd<List<Datum>>("$knowPartials", new List<Datum>()).Add(datum);

            succ.Continue(new Context(context, new List<IContent>()), fail);
            return true;
        }

        public override bool Match(object check, Context context, IContinuation succ, IFailure fail)
        {
            List<Relations.Relation> kinds = new List<Relations.Relation>();
            kinds.Add(Relations.Relation.Tense);

            Thinker.SearchForMatch(salience, memory, kinds, (Datum)check, context, succ, fail, DecideValue, check);

            return true;
        }

        protected object DecideValue(object defval, object[] args)
        {
            Datum start = (Datum)args[0];
            Datum noundat = KnowledgeUtilities.GetClosestDatum(memory, start, nounrel);
            Datum tensedat = KnowledgeUtilities.GetClosestDatum(memory, start, Relations.Relation.Tense);

            if (noundat.Left.IsEvent)
            {
                Verbs.Person person;
                if (noundat != null)
                    person = nouns.GetPerson(noundat.Right.Name);
                else
                    person = Verbs.Person.ThirdSingle;
                
                // Conjugate this verb accordingly
                string conjugated = Relations.ConjugateToTense(memory, tensedat.Left.Name, person, tensedat.Right, verbs);
                return new WordPhrase(conjugated, "VB");
            }

            return defval;
        }
    }

    public class UnknownConceptVariable : Variable
    {
        protected Concept.Kind? kind;

        public UnknownConceptVariable(string name, Concept.Kind? kind)
            : base("name")
        {
            this.kind = kind;
        }

        public override bool IsMatch(Concept concept)
        {
            return (!kind.HasValue || concept.RawKind == kind.Value);
        }
    }

}
