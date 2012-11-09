/******************************************************************\
 *      Class Name:     SimpleDeclarativePhrase
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * This file is part of AgentParser and is free software: you
 * can redistribute it and/or modify it under the terms of the GNU
 * Lesser General Public License as published by the Free Software
 * Foundation, either version 3 of the License, or (at your option)
 * any later version.
 * 
 * Plugger Base is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with AgentParser.  If not, see
 * <http://www.gnu.org/licenses/>.
 *      -----------------------------------------------------------
 * Encapsulates a declarative phrase and how it can be formed.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using LanguageNet.Grammarian;

namespace LanguageNet.AgentParser
{
    /*
     * A simple declarative sentence consists of a subject and a predicate,
    and either spans the entire utterance, or is separated from the rest
    of the utterance by some sort of appropriate punctuation, typically
    a semicolon, but possibly an em-dash or some other punctuation that
    we'll determine with time.
     */
    public class SimpleDeclarativePhrase : Phrase
    {
        public SimpleDeclarativePhrase()
            : base("S")
        {
        }

        public SimpleDeclarativePhrase(params Phrase[] phrases)
            : this()
        {
            LoadPhrases(phrases);
        }
		
		public override bool IsWhole {
			get {
				return true;
			}
		}

        public override bool Transform(Sentence sentence)
        {
 	        if (!sentence.phrases.Contains(this))
                return false;

            KeyValuePair<string, string> nks;
            bool success = false;

            bool lastFound = true;
            while (lastFound) {
                nks = NeighborKinds(sentence);

                if (nks.Key == "" && nks.Value == ".")
                    sentence.AbsorbNext(this);
                else if (nks.Key == "" && nks.Value == "!")
                    sentence.AbsorbNext(this);
                else
                    lastFound = false;

                if (lastFound)
                    success = true;
            }

            nks = NeighborKinds(sentence);
            if (nks.Key == "" && nks.Value == "")
                return success;

            if (!success) {
                string last = constituents[constituents.Count - 1].Part;
                if (nks.Key == "" && (last == "." || last == "!"))
                {
                    sentence.AddFirstToCompletes();
                    success = true;
                }
                else
                {
                    sentence.Separate(this);
                    success = true;
                }
            }

            return success;
        }

        public override Phrase Parapharse(Verbs verbs, Nouns nouns, WordNetAccess wordnet, GrammarParser.ParaphraseOptions options, List<Phrase> emphasizes, ref double prob)
        {
            // Can we change to passive voice?
            VerbPhrase verbphrase = FindConsituent<VerbPhrase>();
            NounPhrase subjphrase = FindConsituent<NounPhrase>();
            if (verbphrase != null && subjphrase != null)
            {
                Verb verb = verbphrase.FindConsituent<Verb>();
                if (verb.Word == "had" || verb.Word == "have" || verb.Word == "has")
                    verb = null;    // never do passive transformations to this

                if (verb != null && verbs.IsTransitive(verb.Word))
                {
                    bool isToBe = Verbs.IsToBe(verb.Word);
                    if (!isToBe && verbphrase.IsComposed(typeof(Verb), typeof(NounPhrase)))
                    {  // Like "The dog ate the bone."
                        if (RemoveEmphasizedImprobability(.75, emphasizes, subjphrase, ref prob))
                        {
                            NounPhrase objphrase = verbphrase.FindConsituent<NounPhrase>();
                            Phrase newobjphrase = subjphrase.ParaphraseAsObject(verbs, nouns, wordnet, SubMoveOffFront(options), emphasizes, ref prob);
                            Phrase newsubjphrase = objphrase.ParaphraseAsSubject(verbs, nouns, wordnet, SubMoveToFront(options), emphasizes, ref prob);

                            return new SimpleDeclarativePhrase(newsubjphrase, new VerbPhrase(new Verb(Verbs.ComposeToBe(nouns.GetPerson(objphrase.Text), verbs.GetInflection(verb.Word))), new VerbPastParticiple(verbs.InflectVerb(verb.Word, Verbs.Convert.ext_Ven)), new PrepositionalPhrase(new Preposition("by"), newobjphrase)), new Period(" ."));
                        }
                    }
                    else if (!isToBe && verbphrase.IsComposed(typeof(Verb), typeof(NounPhrase), typeof(PrepositionalPhrase)))
                    { // Like "Joe gave a ring to Mary."
                        if (RemoveEmphasizedImprobability(.75, emphasizes, subjphrase, ref prob))
                        {
                            NounPhrase dirobjphrase = verbphrase.FindConsituent<NounPhrase>();
                            Phrase newsubjphrase = dirobjphrase.ParaphraseAsSubject(verbs, nouns, wordnet, SubMoveToFront(options), emphasizes, ref prob);
                            Phrase newdirobjphrase = subjphrase.ParaphraseAsObject(verbs, nouns, wordnet, SubMoveOffFront(options), emphasizes, ref prob);

                            PrepositionalPhrase indobjphrase = verbphrase.FindConsituent<PrepositionalPhrase>();
                            Phrase newindobjphrase = indobjphrase.Parapharse(verbs, nouns, wordnet, options, emphasizes, ref prob);

                            return new SimpleDeclarativePhrase(newsubjphrase, new VerbPhrase(new Verb(Verbs.ComposeToBe(nouns.GetPerson(dirobjphrase.Text), verbs.GetInflection(verb.Word))), new VerbPastParticiple(verbs.InflectVerb(verb.Word, Verbs.Convert.ext_Ven)), newindobjphrase, new PrepositionalPhrase(new Preposition("by"), newdirobjphrase)), new Period(" ."));
                        }
                    }
                    else if (!isToBe && verbphrase.IsComposed(typeof(Verb), typeof(VerbPastParticiple), typeof(PrepositionalPhrase)))
                    { // Like "The bone was eaten by the dog."
                        PrepositionalPhrase byphrase = verbphrase.FindConsituent<PrepositionalPhrase>();
                        if (byphrase.IsComposed(typeof(Preposition), typeof(NounPhrase)) && byphrase.Constituents[0].Text == "by")
                        {
                            if (RemoveEmphasizedImprobability(.4, emphasizes, subjphrase, ref prob))
                            {
                                Phrase newsubjphrase = byphrase.FindConsituent<NounPhrase>().ParaphraseAsSubject(verbs, nouns, wordnet, SubMoveToFront(options), emphasizes, ref prob);
                                Phrase newobjphrase = subjphrase.ParaphraseAsObject(verbs, nouns, wordnet, SubMoveOffFront(options), emphasizes, ref prob);
                                VerbPastParticiple oldverb = verbphrase.FindConsituent<VerbPastParticiple>();
                                Verb newverb = new Verb(verbs.InflectVerb(oldverb.Word, verbs.GetInflection(verb.Word)));

                                return new SimpleDeclarativePhrase(newsubjphrase, new VerbPhrase(newverb, newobjphrase), new Period(" ."));
                            }
                        }
                    }
                    else if (!isToBe && verbphrase.IsComposed(typeof(Verb), typeof(VerbPastParticiple), typeof(PrepositionalPhrase), typeof(PrepositionalPhrase)))
                    { // Like "A ring was given to Mary by Joe."
                        PrepositionalPhrase indobjphrase = verbphrase.FindConsituent<PrepositionalPhrase>(0);
                        PrepositionalPhrase byphrase = verbphrase.FindConsituent<PrepositionalPhrase>(1);
                        if (byphrase.IsComposed(typeof(Preposition), typeof(NounPhrase)) && byphrase.Constituents[0].Text == "by")
                        {
                            if (RemoveEmphasizedImprobability(.4, emphasizes, subjphrase, ref prob))
                            {
                                Phrase newsubjphrase = byphrase.FindConsituent<NounPhrase>().ParaphraseAsSubject(verbs, nouns, wordnet, SubMoveToFront(options), emphasizes, ref prob);
                                Phrase newobjphrase = subjphrase.ParaphraseAsObject(verbs, nouns, wordnet, SubMoveOffFront(options), emphasizes, ref prob);
                                VerbPastParticiple oldverb = verbphrase.FindConsituent<VerbPastParticiple>();
                                Verb newverb = new Verb(verbs.InflectVerb(oldverb.Word, verbs.GetInflection(verb.Word)));

                                return new SimpleDeclarativePhrase(newsubjphrase, new VerbPhrase(newverb, newobjphrase, indobjphrase), new Period(" ."));
                            }
                        }
                    }
                    else if (isToBe && verbphrase.IsComposed(typeof(Verb), typeof(PrepositionalPhrase)))
                    { // Like "The fly is on the wall."
                        if (RemoveEmphasizedImprobability(.6, emphasizes, subjphrase, ref prob))
                        {
                            Phrase newobjphrase = subjphrase.ParaphraseAsObject(verbs, nouns, wordnet, SubMoveOffFront(options), emphasizes, ref prob);
                            Phrase newprepphrase = verbphrase.FindConsituent<PrepositionalPhrase>().Parapharse(verbs, nouns, wordnet, options, emphasizes, ref prob);
                            return new SimpleDeclarativePhrase(new ExistentialThere("There"), new VerbPhrase(verb, newobjphrase, newprepphrase), new Period(" ."));
                        }
                    }
                }
            }

            ExistentialThere there = FindConsituent<ExistentialThere>();
            if (verbphrase != null && there != null)
            {
                Verb verb = verbphrase.FindConsituent<Verb>();
                if (Verbs.IsToBe(verb.Word) && verbphrase.IsComposed(typeof(Verb), typeof(NounPhrase), typeof(PrepositionalPhrase)))
                { // Like "There is a fly on the wall."
                    if (RemoveUnemphasizedImprobability(.4, emphasizes, verbphrase.Constituents[1], ref prob))
                    {
                        Phrase newsubjphrase = verbphrase.FindConsituent<NounPhrase>().ParaphraseAsSubject(verbs, nouns, wordnet, SubMoveToFront(options), emphasizes, ref prob);
                        Phrase newprepphrase = verbphrase.FindConsituent<PrepositionalPhrase>().Parapharse(verbs, nouns, wordnet, options, emphasizes, ref prob);
                        return new SimpleDeclarativePhrase(newsubjphrase, new VerbPhrase(verb, newprepphrase), new Period(" ."));
                    }
                }

            }

            return base.Parapharse(verbs, nouns, wordnet, options, emphasizes, ref prob);
        }
    }
}
