/******************************************************************\
 *      Class Name:     NounPhrase
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a noun phrase, and how it can enter a larger phrase
 * and be paraphrased.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using LanguageNet.Grammarian;

namespace LanguageNet.AgentParser
{
    public class NounPhrase : Phrase
    {
        public NounPhrase()
            : base("NP")
        {
            precedence = 30;
        }

        public NounPhrase(params Phrase[] phrases)
            : this()
        {
            LoadPhrases(phrases);
        }

        public override bool Transform(Sentence sentence)
        {
 	        if (!sentence.phrases.Contains(this))
                return false;

            bool success = false;

            bool lastFound = true;
            while (lastFound) {
                KeyValuePair<string, string> nks = NeighborKinds(sentence);
                if (nks.Value == "." && constituents[constituents.Count - 1].Text.Length == 1)
                    sentence.AbsorbNext(this);   // eat the period
                else if (nks.Key == "" && nks.Value == "NP")
                    sentence.MergeNext(this);
                else if (nks.Key == "NP" && nks.Value == "NP")
                    sentence.MergeNext(this);
                else if (nks.Key == "NP" && (nks.Value == "" || nks.Value == "." || nks.Value == "!"))
                    sentence.MergePrevious(this);
                else
                    lastFound = false;

                if (lastFound == true)
                    success = true;
            }

			KeyValuePair<string, string> neighbors = NeighborKinds(sentence);
            if (neighbors.Key == "DT" || neighbors.Key == "PRP$") {
                sentence.AbsorbPrevious(this);
				neighbors = NeighborKinds(sentence);
                success = true;
            }

            if (neighbors.Key == "ADJP") {
                List<Phrase> phrases = new List<Phrase>();
                phrases.Add(sentence.PhraseBefore(this));
                phrases.Add(this);
                sentence.Combine(phrases, new NounPhrase());
				neighbors = NeighborKinds(sentence);
                success = true;
            }

            if (neighbors.Value == "PP")
            {
                Phrase after = sentence.PhraseAfter(this);
                if (after.Constituents.Count > 1 && after.Constituents[0].Text.ToLower() == "of")
                {
                    sentence.AbsorbNext(this);
                    return true;
                }
            }
			
            return success;
        }

        public override Phrase Parapharse(Verbs verbs, Nouns nouns, WordNetAccess wordnet, GrammarParser.ParaphraseOptions options, List<Phrase> emphasizes, ref double prob)
        {
            if (IsComposed(typeof(NounPhrase), typeof(Conjunction), typeof(NounPhrase)))
            {
                if (RemoveImprobability(.5, ref prob))
                {
                    Phrase first = constituents[2].Parapharse(verbs, nouns, wordnet, SubMoveToFront(options), emphasizes, ref prob);
                    Phrase and = constituents[1].Parapharse(verbs, nouns, wordnet, SubNotMoved(options), emphasizes, ref prob);
                    Phrase second = constituents[0].Parapharse(verbs, nouns, wordnet, SubMoveOffFront(options), emphasizes, ref prob);

                    return new NounPhrase(first, and, second);
                }
            }

            return base.Parapharse(verbs, nouns, wordnet, options, emphasizes, ref prob);
        }

        public Phrase ParaphraseAsObject(Verbs verbs, Nouns nouns, WordNetAccess wordnet, GrammarParser.ParaphraseOptions options, List<Phrase> emphasizes, ref double prob)
        {
            string asObject = Nouns.AsObject(Text);
            if (asObject == Text)
                return Parapharse(verbs, nouns, wordnet, options, emphasizes, ref prob);
            else
                return new NounPhrase(new Noun(asObject));
        }

        public Phrase ParaphraseAsSubject(Verbs verbs, Nouns nouns, WordNetAccess wordnet, GrammarParser.ParaphraseOptions options, List<Phrase> emphasizes, ref double prob)
        {
            string asSubject = Nouns.AsSubject(Text);
            if (asSubject == Text)
                return Parapharse(verbs, nouns, wordnet, options, emphasizes, ref prob);
            else
                return new NounPhrase(new Noun(asSubject));
        }
    }
}
