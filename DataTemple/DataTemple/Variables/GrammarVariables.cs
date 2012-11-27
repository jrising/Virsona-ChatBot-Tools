using System;
using System.Collections.Generic;
using System.Text;
using GenericTools;
using DataTemple.AgentEvaluate;
using DataTemple.Matching;
using LanguageNet.Grammarian;
using PluggerBase;
using PluggerBase.ActionReaction.Evaluations;

namespace DataTemple.Variables
{
    public class GrammarVariables
    {
        public delegate bool IsWordType(string word);

        public static void LoadVariables(Context context, double salience, Memory memory, PluginEnvironment plugenv)
        {
			Verbs verbs = new Verbs(plugenv);
			POSTagger tagger = new POSTagger(plugenv);
			GrammarParser parser = new GrammarParser(plugenv);
			
            context.Map.Add("%is", new IrregularVerbVariable("%is", memory, Verbs.IsToBe, Verbs.ComposeToBe));
            context.Map.Add("%has", new IrregularVerbVariable("%has", memory, Verbs.IsToHave, Verbs.ComposeToHave));
            context.Map.Add("%do", new IrregularVerbVariable("%do", memory, Verbs.IsToDo, Verbs.ComposeToDo));

            context.Map.Add("%noun", new NounVariable());
			context.Map.Add("%pronoun", new PartListVariable("%pronoun", new string[] {"PRP", "WP"}));
            context.Map.Add("%adj", new PartListVariable("%adj", new string[] {"JJ", "JP"}));
            context.Map.Add("%adv", new PartListVariable("%adv", new string[] {"RB", "ADVP"}));
            context.Map.Add("%verb", new VerbVariable(verbs));
            context.Map.Add("%verbx", new VerbSubVariable("%verbx", verbs, "VBP", Verbs.Convert.ext_V));
            context.Map.Add("%verben", new VerbSubVariable("%verben", verbs, "VBN", Verbs.Convert.ext_Ven));
            context.Map.Add("%verbing", new VerbSubVariable("%verbing", verbs, "VBG", Verbs.Convert.ext_Ving));
            context.Map.Add("%verbed", new VerbSubVariable("%verbed", verbs, "VBD", Verbs.Convert.ext_Ved));
            context.Map.Add("%verbs", new VerbSubVariable("%verbs", verbs, "VBZ", Verbs.Convert.ext_Vs));
            context.Map.Add("%verball", new VerbSubVariable("%verball", verbs, "VP", Verbs.Convert.ext_V));
            context.Map.Add("%will", new ModalVariable());
            context.Map.Add("%event", new EventVariable());
            context.Map.Add("%punct", new PunctuationVariable());
            context.Map.Add("%inall", new PrepositionalPhraseVariable());
            context.Map.Add("%attime", new AtTimeVariable());

            context.Map.Add("%sentence", new SentenceVariable(salience, tagger, parser, new WordPhrase(" .", ".")));   // matches any sentence in the input
            context.Map.Add("%question", new SentenceVariable(salience, tagger, parser, new WordPhrase(" ?", "?")));
            context.Map.Add("%phrase", new PhraseVariable(salience, null, tagger, parser));
            context.Map.Add("%nounphrase", new PhraseVariable(salience, new NounVariable(), tagger, parser));
            context.Map.Add("%paren", new ParentheticalVariable(salience, tagger, parser));
            context.Map.Add("%clause", new ClauseVariable(salience, tagger, parser));  // Like %sentence ... %opt .
            //context.Map.Add("%xconj", new ConjugationVariable(coderack, salience, null));
			context.Map.Add("%sentences", new SentenceSequenceVariable(salience, tagger, parser));

            context.Map.Add("%opt", new Special("%opt"));
        }
    }

    public class IrregularVerbVariable : Variable
    {
        public delegate bool IsVerb(string word);
        public delegate string ConjugateVerb(Verbs.Person person, Verbs.Convert convert);
        //public delegate List<KeyValuePair<Verbs.Person, Verbs.Convert>> MatchConjugation(string word);

        protected Memory memory;
        protected Nouns nouns;

        protected IsVerb isVerb;
        protected ConjugateVerb conjugate;
        //protected MatchConjugation match;

        public IrregularVerbVariable(string name, Memory memory, IsVerb isVerb, ConjugateVerb conjugate)//, MatchConjugation match)
            : base(name)
        {
            this.memory = memory;

            this.isVerb = isVerb;
            this.conjugate = conjugate;
            //this.match = match;
        }

        public override bool IsMatch(IParsedPhrase check)
        {
            if (check.IsLeaf)
                return isVerb(check.Text);

            return false;
        }

        public override bool IsMatch(Concept concept)
        {
            // Is this a time?  If so, we can work with it!
            if (concept.IsSpecial && (concept == memory.past || concept == memory.now || concept == memory.future))
                return true;

            List<string> parts = StringUtilities.SplitWords(concept.Name, true);
            foreach (string part in parts)
                if (!(part == "en" || part == "ing" || Verbs.IsModal(part) || Verbs.IsToBe(part) || Verbs.IsToDo(part) || Verbs.IsToHave(part)))
                    return false;

            return true;
        }

        public override IParsedPhrase ConceptToPhrase(Context context, Concept concept, POSTagger tagger, GrammarParser parser)
        {
            Verbs.Person person = Verbs.Person.ThirdSingle;

            object start = context.LookupDefaulted<object>("$check", null);
            if (start != null && start is Datum) {
                Datum noundat = KnowledgeUtilities.GetClosestDatum(memory, (Datum) start, Relations.Relation.Subject);
                person = nouns.GetPerson(noundat.Right.Name);
            }

            return Relations.ConjugateToPhrase(memory, conjugate, person, concept);
        }
    }

    public class NounVariable : Variable
    {
        public NounVariable()
            : base("%noun")
        {
        }

        public override bool IsMatch(IParsedPhrase check)
        {
            return (check.Part == "NN" || check.Part == "NP");
        }

        public override bool IsMatch(Concept concept)
        {
            return concept.IsEntity;
        }
    }
	
	public class PartListVariable: Variable
	{
		protected string[] parts;
		
		public PartListVariable(string name, string[] parts)
			: base(name) {
			this.parts = parts;
		}
		
        public override bool IsMatch(IParsedPhrase check)
        {
			foreach (string part in parts)
				if (check.Part == part)
					return true;
			
			return false;
        }

	}
			
    public class VerbVariable : Variable
    {
        protected Verbs verbs;

        public VerbVariable(Verbs verbs)
            : base("%verb")
        {
            this.verbs = verbs;
        }

        public override bool IsMatch(IParsedPhrase check)
        {
            return (check.Part.StartsWith("VB") && !Verbs.IsToBe(check.Text));
        }

        public override bool IsMatch(Concept concept)
        {
            return concept.IsEvent;
        }

        public override void Propogate(Context env, object matched, double strength)
        {
            base.Propogate(env, matched, strength);

            if (matched is IParsedPhrase)
            {
                IParsedPhrase matchphr = (IParsedPhrase)matched;
                ((Variable)env.Lookup("%verbx")).Propogate(env, new WordPhrase(verbs.InputToBase(matchphr.Text), "VB"), .5 * strength);
                ((Variable)env.Lookup("%verben")).Propogate(env, new WordPhrase(verbs.ComposePastpart(matchphr.Text), "VBN"), .5 * strength);
                ((Variable)env.Lookup("%verbing")).Propogate(env, new WordPhrase(verbs.ComposePrespart(matchphr.Text), "VBG"), .5 * strength);
                ((Variable)env.Lookup("%verbed")).Propogate(env, new WordPhrase(verbs.ComposePast(matchphr.Text), "VBD"), .5 * strength);
                ((Variable)env.Lookup("%verbs")).Propogate(env, new WordPhrase(verbs.ComposePresent(matchphr.Text), "VBZ"), .5 * strength);
            }
        }
    }

    public class VerbSubVariable : Variable
    {
        protected Verbs verbs;
        protected string pos;
        Verbs.Convert inflection;

        public VerbSubVariable(string name, Verbs verbs, string pos, Verbs.Convert inflection)
            : base(name)
        {
            this.verbs = verbs;
            this.pos = pos;
            this.inflection = inflection;
        }

        public override bool IsMatch(IParsedPhrase check)
        {
            if (check.Part == pos)
                return true;

            if (check.Part.StartsWith("VB"))
            {
                return !Verbs.IsToBe(check.Text) && verbs.GetInflection(check.Text) == inflection;
            }
            else
                return false;
        }

        public override bool IsMatch(Concept concept)
        {
            return concept.IsEvent;
        }

        public override void Propogate(Context env, object matched, double strength)
        {
            base.Propogate(env, matched, strength);

            if (strength > .5 && matched is IParsedPhrase)
                ((Variable)env.Lookup("%verb")).Propogate(env, new WordPhrase(verbs.InputToBase(((IParsedPhrase) matched).Text), "VB"), .5 * strength);
        }
    }

    public class ModalVariable : Variable
    {
        public ModalVariable()
            : base("%will")
        {
        }

        public override bool IsMatch(IParsedPhrase check)
        {
            string words = check.Text;
            return Verbs.IsModal(words);

            // TODO: better use of "used to" etc, if parsed [ought [to verbx]]
            // TODO: can I encorporate "%is [not] going to"?
        }
    }

    public class EventVariable : Variable
    {
        public EventVariable()
            : base("%event")
        {
        }

        public override bool Match(Context env, IParsedPhrase check)
        {
            List<IParsedPhrase> saved = GetEventClauses(env);
            if (saved.Contains(check))
            {
                Propogate(env, check, 1.0);
                return true;
            }

            return false;

            /* Better to use all and only identified events
            NounPhrase subject = check.FindConsituent<NounPhrase>();
            if (subject == null)
                return false;

            VerbPhrase verbphr = check.FindConsituent<VerbPhrase>();
            if (verbphr == null)
                return false;

            return true;*/
        }

        public static void AddEventClause(Context env, IParsedPhrase phrase)
        {
            List<IParsedPhrase> copy = new List<IParsedPhrase>(GetEventClauses(env));
            copy.Add(phrase);
            env.Map["$clause$%event"] = copy;
        }

        public static List<IParsedPhrase> GetEventClauses(Context env)
        {
            return env.LookupDefaulted<List<IParsedPhrase>>("$clause$%event", new List<IParsedPhrase>());
        }

        public static void AddEventConcept(Context env, Concept verb)
        {
            List<Concept> copy = new List<Concept>(GetEventConcepts(env));
            copy.Add(verb);
            env.Map["$concept$%event"] = copy;
        }

        public static List<Concept> GetEventConcepts(Context env)
        {
            return env.LookupDefaulted<List<Concept>>("$concept$%event", new List<Concept>());
        }
    }

    public class PunctuationVariable : Variable
    {
        public PunctuationVariable()
            : base("%punct")
        {
        }

        public override bool IsMatch(IParsedPhrase check)
        {
            return (check.Part == "." || check.Part == "!" || check.Part == "?");
        }
    }

    public class PrepositionalPhraseVariable : Variable
    {
        public PrepositionalPhraseVariable()
            : base("%inall")
        {
        }

        public override bool IsMatch(IParsedPhrase check)
        {
            return (check.Part == "PP");
        }
    }

    public class AtTimeVariable : Variable
    {
        public AtTimeVariable()
            : base("%attime")
        {
        }

        public override bool IsMatch(IParsedPhrase check)
        {
            if (check.Part == "PP") {
				GroupPhrase groupPhrase = new GroupPhrase(check);
                // Take everything except the preposition
                string datetime = groupPhrase.GetBranch(1).Text;

                ProbableStrength time = ValidateUtilities.SeemsTime(datetime);
                return time.IsLikely(0.5);
            } else
                return ValidateUtilities.SeemsSingleDay(check.Text).IsLikely(0.5);
        }
    }

    public class SentenceVariable : MatchProduceAgent
    {
        protected IParsedPhrase final;

        public SentenceVariable(double salience, POSTagger tagger, GrammarParser parser, IParsedPhrase final)
            : base(ArgumentMode.RemainderUnevaluated, salience, 8, 10, tagger, parser)
        {
            this.tagger = tagger;
            this.final = final;
        }

        public override int Match(object check, Context context, IContinuation succ, IFailure fail)
        {
            // Add optional punctuation to end
            Context childctx = new Context(context, context.Contents);
            childctx.Contents.Add(new Special("%opt"));
            childctx.Contents.Add(new Special("%punct"));
			
			if (check is IParsedPhrase) {
				IParsedPhrase phrase = (IParsedPhrase) check;
	            if (phrase.Part == "=P")
	            {
	                // Match against each element
	                foreach (IParsedPhrase constituent in phrase.Branches)
	                    Matcher.MatchAgainst(salience, context, constituent, new List<IParsedPhrase>(), succ, fail);
	            }
	            else if (phrase.Part == "FRAG" || phrase.Part == "S" || phrase.Part == "SBARQ")
	            {
	                // Do a match using my contents
	                Matcher.MatchAgainst(salience, context, (IParsedPhrase) check, new List<IParsedPhrase>(), succ, fail);
	            }
			}

            return time;
        }

        public override int Produce(Context context, IContinuation succ, IFailure fail)
        {
            // Evaluate all children
            ContinueToCallAgent cont = CallAgentWrapper.MakeContinuation(ConstructSentence, succ, 100.0, 0, 10);

            ContinuationAppender appender = new ContinuationAppender(context, cont);

            Evaluator eval = new Evaluator(salience, ArgumentMode.ManyArguments, appender, appender);
            eval.Lineage = ContinueAgentCodelet.NewLineage();
            appender.RegisterCaller(eval.Lineage);
            appender.RegisterCaller(eval.Lineage);

            eval.Continue(context, fail);

            return time;
        }

        public int ConstructSentence(Context context, IContinuation succ, IFailure fail, params object[] args) {
            // Need to produce all my contents!
            IParsedPhrase phrase = StarUtilities.ProducedPhrase(context, tagger, parser);
            if (phrase == null)
            {
                // oops, we failed to produce
                fail.Fail("Context could not be produced", succ);
                return time;
            }

            if (!(phrase.Part == "=P"))
            {
                if (phrase.Part == "FRAG" || phrase.Part == "S" || phrase.Part == "SBARQ")
                {
                    if (final != null)
                    {
						GroupPhrase groupPhrase = new GroupPhrase(phrase);
                        IParsedPhrase last = groupPhrase.GetBranch(groupPhrase.Count - 1);
                        if (!(last.Part == "." || last.Part == "!" || last.Part == "?")) {
							List<IParsedPhrase> branches = new List<IParsedPhrase>();
							branches.AddRange(phrase.Branches);
							branches.Add((IParsedPhrase)final.Clone());
							phrase = new GroupPhrase(phrase.Part, branches);
						}
                    }
                }
                else
                {
					List<IParsedPhrase> branches = new List<IParsedPhrase>();
					branches.Add(phrase);
                    if (final != null)
                        branches.Add((IParsedPhrase)final.Clone());
					phrase = new GroupPhrase("FRAG", branches);
                }
            }

            List<IContent> contents = new List<IContent>();
            contents.Add(new Word(phrase.Text));
            Context child = new Context(context, contents);
            succ.Continue(child, fail);

            return time;
        }
    }

    public class PhraseVariable : MatchProduceAgent
    {
        protected Variable variable;

        public PhraseVariable(double salience, Variable variable, POSTagger tagger, GrammarParser parser)
            : base(ArgumentMode.RemainderUnevaluated, salience, 4, 10, tagger, parser)
        {
            this.variable = variable;
        }

        public override int Match(object check, Context context, IContinuation succ, IFailure fail)
        {
            if (check is IParsedPhrase && ((IParsedPhrase) check).IsLeaf)
                return 1;

            // Match against each element
            foreach (IParsedPhrase constituent in ((IParsedPhrase) check).Branches)
            {
                // Set up propogate-on-clear
                ContinueToCallAgent cont = CallAgentWrapper.MakeContinuation(PropogateOnClear, succ, 100.0, 4, 10, "%phrase", check);
                
                if (variable == null || variable.IsMatch(constituent))
                    Matcher.MatchAgainst(salience, context, constituent, new List<IParsedPhrase>(), cont, fail);
                if (!(constituent.IsLeaf))
                {
                    Context child = new Context(context, context.Contents);
                    child.Map["$check"] = constituent;
                    ContinueToCallAgent.Instantiate(new PhraseVariable(salience, variable, tagger, parser), child, cont, fail);
                }
            }

            return time;
        }

        public override int Produce(Context context, IContinuation succ, IFailure fail)
        {
            return ProducePropogated(context, "%phrase");
        }
    }

    public class ParentheticalVariable : MatchProduceAgent
    {
        public ParentheticalVariable(double salience, POSTagger tagger, GrammarParser parser)
            : base(ArgumentMode.RemainderUnevaluated, salience, 0, 10, tagger, parser)
        {
        }

        public override int Match(object check, Context context, IContinuation succ, IFailure fail)
        {
            if (check is IParsedPhrase && ((IParsedPhrase) check).Part == "PRN")
            {
                // Set up propogate-on-clear
                ContinueToCallAgent cont = CallAgentWrapper.MakeContinuation(PropogateOnClear, succ, 100.0, 4, 10, "%paren", check);

                // Do a match using my contents
                Matcher.MatchAgainst(salience, context, (IParsedPhrase) check, new List<IParsedPhrase>(), cont, fail);
                return time;
            }
            else
            {
                fail.Fail("check is not parenthetical", succ);
                return 1;
            }
        }

        public override int Produce(Context context, IContinuation succ, IFailure fail)
        {
            return ProducePropogated(context, "%paren");
        }
    }

    public class ClauseVariable : MatchProduceAgent
    {
        public ClauseVariable(double salience, POSTagger tagger, GrammarParser parser)
            : base(ArgumentMode.RemainderUnevaluated, salience, 0, 10, tagger, parser)
        {
        }

        public override int Match(object check, Context context, IContinuation succ, IFailure fail)
        {
			if (check is IParsedPhrase) {
				IParsedPhrase phrase = (IParsedPhrase) check;
	            if (phrase.Part == "=P")
	            {
	                // Match against each element
	                foreach (IParsedPhrase constituent in phrase.Branches)
	                {
	                    Context child = new Context(context, context.Contents);
	                    child.Map["$check"] = constituent;
	                    ContinueToCallAgent.Instantiate(new ClauseVariable(salience, tagger, parser), child, succ, fail);
	                }
	            }
	            else if (phrase.Part == "FRAG" || phrase.Part == "S" || phrase.Part == "SBARQ")
	            {
					GroupPhrase groupPhrase = new GroupPhrase(phrase);
	                IParsedPhrase last = groupPhrase.GetBranch(groupPhrase.Count - 1);
	                if (last.Part == "." || last.Part == "?" || last.Part == "!" || last.Part == ";")
	                    phrase = new GroupPhrase("?", groupPhrase.GetRange(0));
	
	                // Set up propogate-on-clear
	                ContinueToCallAgent cont = CallAgentWrapper.MakeContinuation(PropogateOnClear, succ, 100.0, 4, 10, "%clause", phrase);
	                
	                // Do a match using my contents
	                Matcher.MatchAgainst(salience, context, phrase, new List<IParsedPhrase>(), cont, fail);
	            }
			}

            return time;
        }

        public override int Produce(Context context, IContinuation succ, IFailure fail)
        {
            return ProducePropogated(context, "%clause");
        }
    }
	
    public class SentenceSequenceVariable : MatchProduceAgent
    {
        public SentenceSequenceVariable(double salience, POSTagger tagger, GrammarParser parser)
            : base(ArgumentMode.RemainderUnevaluated, salience, 8, 10, tagger, parser)
        {
			//this.breakpointCall = true;
        }

        public override int Match(object check, Context context, IContinuation succ, IFailure fail)
        {
			if (!(check is IParsedPhrase)) {
				fail.Fail("Cannot match a " + check.GetType(), succ);
				return time;
			}
			
			IParsedPhrase phrase = (IParsedPhrase) check;
			if (phrase.Part != "=P") {
				fail.Fail("Can only match a paragraph", succ);
				return time;
			}
			
			context.Map["$sentences.check"] = check;
			TwoTuple<List<IContent>, IContinuation> parts = SplitArguments(context, succ);
			List<IContent> chunk = parts.one;
			
            // Match against each element
			int sentenceStart = 0;
			foreach (IParsedPhrase constituent in phrase.Branches) {
	            Context first = new Context(context, chunk);
				first.Map["$sentences.index"] = sentenceStart + 1;
				
                Matcher.MatchAgainst(salience, first, constituent, new List<IParsedPhrase>(), parts.two, fail);
			}

            return time;
        }
		
		public void NextSentence(Context context, IContinuation succ, IFailure fail, params object[] args) {
			int? sentenceStart = context.LookupDefaulted<int?>("$sentences.index", null);
			
			TwoTuple<List<IContent>, IContinuation> parts = SplitArguments(context, succ);
			List<IContent> chunk = parts.one;

            Context first = new Context(context, chunk);
			first.Map["$sentences.index"] = sentenceStart + 1;
				
			GroupPhrase groupPhrase = new GroupPhrase((IParsedPhrase) context.Lookup("$sentences.check"));
            Matcher.MatchAgainst(salience, first, groupPhrase.GetBranch(sentenceStart.Value), new List<IParsedPhrase>(), parts.two, fail);
		}

		// Take first sentence and return remainder as second element of tuple
		public TwoTuple<List<IContent>, IContinuation> SplitArguments(Context context, IContinuation succ) {
			// Construct a %sentence for the first segment
			List<IContent> sentence = new List<IContent>();
            sentence.Add(new Special("%sentence"));
			
			int index = 0;
			while (index < context.Contents.Count && context.Contents[index].Name != "/") {
				sentence.Add(context.Contents[index]);
				index++;
			}
			List<IContent> remains = new List<IContent>();
			if (index < context.Contents.Count) {
				index++;
				while (index < context.Contents.Count) {
					remains.Add(context.Contents[index]);
					index++;
				}
			}
			
			IContinuation mysucc = succ;
			if (remains.Count > 0) {
				mysucc = new ContinueAgentWrapper(NextSentence, succ);
				mysucc = new ContextAppender(salience, new Context(context, remains), null, mysucc);
			}

			return new TwoTuple<List<IContent>, IContinuation>(sentence, mysucc);
		}
    }
}