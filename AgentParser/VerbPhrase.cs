/******************************************************************\
 *      Class Name:     VerbPhrase
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a verb phrase.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using LanguageNet.Grammarian;

namespace LanguageNet.AgentParser
{
    public class VerbPhrase : Phrase
    {
        Dictionary<string, bool> triedBefore;

        public VerbPhrase()
            : base("VP")
        {
            triedBefore = new Dictionary<string, bool>();
            precedence = -5;
        }

        public VerbPhrase(params Phrase[] phrases)
            : this()
        {
            LoadPhrases(phrases);
        }

        public void Reset()
        {
            triedBefore = new Dictionary<string, bool>();
        }

        public override bool Transform(Sentence sentence)
        {
 	         if (!sentence.phrases.Contains(this))
                 return false;

            bool success = false;
            KeyValuePair<string, string> nks = NeighborKinds(sentence);

            Phrase verbphr = FindConsituent<Verb>();
            string verb = (verbphr == null ? "" : verbphr.Text);

            if (nks.Key == "ADVP") {
                sentence.AbsorbPrevious(this);
                success = true;
            }
            else if (nks.Value == "VBN")
            { // is considered
                sentence.AbsorbNext(this);
                success = true;
            }
            else if (nks.Value == "ADVP")
            {
                sentence.AbsorbNext(this);
                success = true;
            }
            else if (nks.Value == "NP")
            {
                sentence.AbsorbNext(this);
                success = true;
            }
            else if (nks.Value == "ADJP" && Verbs.IsToBe(verb))
            {
                sentence.AbsorbNext(this);
                success = true;
            }

            else if (nks.Value == "PP")
            {
                sentence.AbsorbNext(this);
                success = true;
            }

            else if (nks.Key == "TO")
            {
                List<Phrase> us = new List<Phrase>();
                us.Add(sentence.PhraseBefore(this));
                us.Add(this);
                sentence.Combine(us, new PrepositionalPhrase());
                success = true;
            }

            else if (nks.Key == "WHNP" || nks.Key == "WRB")
            {
                string trial = sentence.PhraseBefore(this).ToString() + ";" + ToString() + ";SBARQ";
                if (!triedBefore.ContainsKey(trial))
                {
                    List<Phrase> phrases = new List<Phrase>();
                    phrases.Add(sentence.PhraseBefore(this));
                    phrases.Add(this);
                    sentence.Combine(phrases, new SimpleQuestion());
                    triedBefore.Add(trial, true);
                    success = true;
                }
            }

            else if (nks.Key == "NP" || nks.Key == "EX")
            {
                string trial = sentence.PhraseBefore(this).ToString() + ";" + ToString() + ";S";
                if (!triedBefore.ContainsKey(trial))
                {
                    List<Phrase> phrases = new List<Phrase>();
                    phrases.Add(sentence.PhraseBefore(this));
                    phrases.Add(this);
                    sentence.Combine(phrases, new SimpleDeclarativePhrase());
                    triedBefore.Add(trial, true);
                    success = true;
                }
            }

            return success;
        }

        public override Phrase Parapharse(Verbs verbs, Nouns nouns, WordNetAccess wordnet, GrammarParser.ParaphraseOptions options, List<Phrase> emphasizes, ref double prob)
        {
            if (IsComposed(typeof(VerbPhrase), typeof(Conjunction), typeof(VerbPhrase)))
            {
                if (RemoveImprobability(.5, ref prob))
                {
                    Phrase first = constituents[2].Parapharse(verbs, nouns, wordnet, SubMoveToFront(options), emphasizes, ref prob);
                    Phrase and = constituents[1].Parapharse(verbs, nouns, wordnet, SubNotMoved(options), emphasizes, ref prob);
                    Phrase second = constituents[0].Parapharse(verbs, nouns, wordnet, SubMoveOffFront(options), emphasizes, ref prob);

                    return new VerbPhrase(first, and, second);
                }
            }

            return base.Parapharse(verbs, nouns, wordnet, options, emphasizes, ref prob);
        }
    }
}
