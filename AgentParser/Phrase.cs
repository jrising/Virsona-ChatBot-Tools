/******************************************************************\
 *      Class Name:     Phrase
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
 * The base class for all phrases.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using LanguageNet.Grammarian;

/*
 *  A phrase is the basic morphological unit of a sentence; indeed, a sentence
    can be thought of--and is, in our representation--a specialized kind of phrase.
    
    A phrase consists of a type--usually specified by a subclass--and a sequence of
    constituents, which may be either tokens or phrases.

    We have to have a method that allows us to "grow" a phrase, but I don't know
    if this is properly a part of the Phrase class.
    
    Ah.  There are steps that involve locating and possibly MERGING phrases.
    
    Initially, we scan the sentence and construct phrases where obvious, like
    nouns belonging in noun phrases.
 */
namespace LanguageNet.AgentParser
{
    public class Phrase : IParsedPhrase, IComparable<Phrase>
    {
        protected string part = "PHRASE";
        protected int precedence = 0;

        protected List<Phrase> constituents;

        public Phrase(string part)
            : this(part, null)
        {
        }

        public Phrase(string part, List<Phrase> constituents)
        {
            this.part = part;
            this.constituents = (constituents == null ? new List<Phrase>() : constituents);
        }

        public string Part
        {
            get
            {
                return part;
            }
        }

        public int Precedence
        {
            get
            {
                return precedence;
            }
        }

        public List<string> Words
        {
            get
            {
                List<string> result = new List<string>();

                if (this is POSPhrase)
                {
                    result.Add(((POSPhrase)this).Word);
                    return result;
                }

                foreach (Phrase constituent in constituents)
                {
                    if (constituent is POSPhrase)
                        result.Add(((POSPhrase)constituent).Word);
                    else
                        result.AddRange(constituent.Words);
                }

                return result;
            }
        }

        public string Text
        {
            get
            {
                return StringUtilities.JoinWords(Words);
            }
        }
		
		public virtual bool IsLeaf
		{
			get
			{
				return false;
			}
		}
		
		public IEnumerable<IParsedPhrase> Branches
		{
			get
			{
				List<IParsedPhrase> branches = new List<IParsedPhrase>();
				foreach (IParsedPhrase branch in constituents)
					branches.Add(branch);
				return branches;
			}
		}

        public List<Phrase> Constituents
        {
            get
            {
                return constituents;
            }
            set
            {
                constituents = value;
            }
        }

        public override string ToString()
        {
            StringBuilder output = new StringBuilder();
            output.Append("(");
            output.Append(part);
            foreach (Phrase constituent in constituents) {
                output.Append(" ");
                output.Append(constituent.ToString());
            }
            output.Append(")");

            return output.ToString();
        }

        public T FindConsituent<T>()
            where T : Phrase
        {
            return FindConsituent<T>(0);
        }

        public T FindConsituent<T>(int ii)
            where T : Phrase {
            foreach (Phrase constituent in constituents)
                if (constituent is T)
                {
                    if (ii == 0)
                        return (T)constituent;
                    else
                        ii--;
                }

            return default(T);
        }

        public T FindContained<T>()
            where T : Phrase
        {
            int ii = 0;
            return FindContained<T>(ref ii);
        }

        public T FindContained<T>(ref int ii)
            where T : Phrase
        {
            foreach (Phrase constituent in constituents)
            {
                if (constituent is T)
                {
                    if (ii == 0)
                        return (T)constituent;
                    else
                        ii--;
                }
                else if (!(constituent is POSPhrase))
                {
                    T result = FindContained<T>(ref ii);
                    if (result != null)
                        return result;
                }
            }

            return default(T);
        }

        public bool IsComposed(params Type[] types)
        {
            if (types.Length != constituents.Count)
                return false;

            for (int ii = 0; ii < types.Length; ii++)
                if (!types[ii].IsInstanceOfType(constituents[ii]))
                    return false;

            return true;
        }

        public Phrase GetSubphrase(int start)
        {
            if (constituents.Count <= start)
                return null;
            if (constituents.Count - 1 == start)
                return constituents[start];

            Phrase clone = (Phrase) Clone();
            clone.constituents = constituents.GetRange(start, constituents.Count - start);
            return clone;
        }

        public KeyValuePair<string, string> NeighborKinds(Sentence sentence) {
            Phrase before = sentence.PhraseBefore(this);
            string beforeKind = (before == null ? "" : before.part);
            Phrase after = sentence.PhraseAfter(this);
            string afterKind = (after == null ? "" : after.part);
            return new KeyValuePair<string,string>(beforeKind, afterKind);
        }

        public List<Phrase> Neighborhood(Sentence sentence) {
            List<Phrase> neighbors = new List<Phrase>();
            Phrase before = sentence.PhraseBefore(this);
            Phrase after = sentence.PhraseAfter(this);
            if (before != null)
                neighbors.Add(before);
            neighbors.Add(this);
            if (after != null)
                neighbors.Add(after);

            return neighbors;
        }

        public List<Phrase> FindPhrases(string simple)
        {
            List<Phrase> phrases = new List<Phrase>();

            if (Text.ToLower() == simple.ToLower())
            {
                phrases.Add(this);
                return phrases;
            }

            foreach (Phrase constituent in constituents)
                phrases.AddRange(constituent.FindPhrases(simple));

            return phrases;
        }
        
        public void LoadPhrases(Phrase[] phrases)
        {
            foreach (Phrase phrase in phrases)
                constituents.Add(phrase);
        }

        public virtual bool Transform(Sentence sentence)
        {
            return false;
        }

        public virtual Phrase Parapharse(Verbs verbs, Nouns nouns, WordNetAccess wordnet, GrammarParser.ParaphraseOptions options, List<Phrase> emphasizes, ref double prob)
        {
            List<Phrase> newconsts = new List<Phrase>();
            foreach (Phrase constituent in constituents)
            {
                newconsts.Add(constituent.Parapharse(verbs, nouns, wordnet, options, emphasizes, ref prob));
                options &= ~(GrammarParser.ParaphraseOptions.MoveToStart | GrammarParser.ParaphraseOptions.MoveOffStart | GrammarParser.ParaphraseOptions.IsStayingStart);
            }

            return new Phrase(part, newconsts);
        }

        public bool IsStart(GrammarParser.ParaphraseOptions options)
        {
            return ((options & GrammarParser.ParaphraseOptions.IsStayingStart) != GrammarParser.ParaphraseOptions.NoOptions ||
                    (options & GrammarParser.ParaphraseOptions.MoveToStart) != GrammarParser.ParaphraseOptions.NoOptions);
        }
        
        public GrammarParser.ParaphraseOptions SubNotMoved(GrammarParser.ParaphraseOptions options)
        {
            return options & ~(GrammarParser.ParaphraseOptions.MoveToStart | GrammarParser.ParaphraseOptions.MoveOffStart);
        }

        public GrammarParser.ParaphraseOptions SubMoveToFront(GrammarParser.ParaphraseOptions options)
        {
            if ((options & GrammarParser.ParaphraseOptions.IsStayingStart) != GrammarParser.ParaphraseOptions.NoOptions)
                return options | GrammarParser.ParaphraseOptions.MoveToStart;

            return options;
        }

        public GrammarParser.ParaphraseOptions SubMoveOffFront(GrammarParser.ParaphraseOptions options)
        {
            if ((options & GrammarParser.ParaphraseOptions.IsStayingStart) != GrammarParser.ParaphraseOptions.NoOptions)
                options = options | GrammarParser.ParaphraseOptions.MoveOffStart;

            if ((options & GrammarParser.ParaphraseOptions.MoveToStart) != GrammarParser.ParaphraseOptions.NoOptions)
                options = options & ~GrammarParser.ParaphraseOptions.MoveToStart;

            return options;
        }

        protected bool RemoveImprobability(double level, ref double prob)
        {
            if (prob >= level)
            {
                prob = (prob - level) / (1.0 - level);
                return true;
            }
            else
            {
                prob = prob / level;
                return false;
            }
        }

        protected bool DeepContains(List<Phrase> list, Phrase elt)
        {
            foreach (Phrase phrase in list)
                if (phrase == elt)
                    return true;
            foreach (Phrase constituent in elt.constituents)
                if (DeepContains(list, constituent))
                    return true;

            return false;
        }

        protected bool RemoveEmphasizedImprobability(double level, List<Phrase> emphasizes, Phrase phrase, ref double prob)
        {
            if (emphasizes.Count > 0)
            {
                if (DeepContains(emphasizes, phrase))
                    level = level * (1.0 - .5 / emphasizes.Count);  // decrease level
                else
                    level = level + .5 / emphasizes.Count - .5 * level / emphasizes.Count;
            }

            return RemoveImprobability(level, ref prob);
        }

        protected bool RemoveUnemphasizedImprobability(double level, List<Phrase> emphasizes, Phrase phrase, ref double prob)
        {
            if (emphasizes.Count > 0)
            {
                if (DeepContains(emphasizes, phrase))
                    level = level + .5 / emphasizes.Count - .5 * level / emphasizes.Count;
                else
                    level = level * (1.0 - .5 / emphasizes.Count);  // decrease level
            }

            return RemoveImprobability(level, ref prob);
        }

        protected int ImprobabilityToInt(int count, ref double prob)
        {
            if (prob == 1.0)
                return count - 1;

            double dwhich = prob * count;
            int iwhich = (int)dwhich;
            prob -= iwhich;

            return iwhich;
        }

        #region IComparable<Phrase> Members

        public int CompareTo(Phrase other)
        {
            if (other.precedence == precedence)
                return 0;

            return (other.precedence > precedence ? -1 : 1);
        }

        #endregion

        #region ICloneable Members

        public object Clone()
        {
            Phrase clone = (Phrase) MemberwiseClone();
            clone.constituents = new List<Phrase>();
            foreach (Phrase constituent in constituents)
                clone.constituents.Add((Phrase) constituent.Clone());

            return clone;
        }

        #endregion
    }
}
