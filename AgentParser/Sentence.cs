/******************************************************************\
 *      Class Name:     Sentence
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
 * Encapsulates the logic for parsing phrases, and the part of 
 * speech name source.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using PluggerBase;

namespace LanguageNet.AgentParser
{
    public class Sentence : IDataSource<string, Type>
    {
        protected static Dictionary<string, Type> posClassLookup;
		protected static List<string> abbreviations;

        static Sentence()
        {
            if (posClassLookup != null)
            {
                // Another thread already did this job
                return;
            }

            posClassLookup = new Dictionary<string, Type>();
            posClassLookup.Add("CC", typeof(Conjunction));
            posClassLookup.Add("CD", typeof(CardinalNumber));
            posClassLookup.Add("DT", typeof(Determiner));
            posClassLookup.Add("EX", typeof(ExistentialThere));
            posClassLookup.Add("FW", typeof(ForeignWord));
            posClassLookup.Add("IN", typeof(Preposition));
            posClassLookup.Add("JJ", typeof(Adjective));
            posClassLookup.Add("JJR", typeof(AdjectiveComparative));
            posClassLookup.Add("JJS", typeof(AdjectiveSuperlative));
            posClassLookup.Add("LS", typeof(ListItem));
            posClassLookup.Add("MD", typeof(ModalVerb));
            posClassLookup.Add("NN", typeof(Noun));
            posClassLookup.Add("NNS", typeof(NounPlural));
            posClassLookup.Add("NNP", typeof(NounProper));
            posClassLookup.Add("NNPS", typeof(NounProperPlural));
            posClassLookup.Add("PDT", typeof(Predeterminer));
            posClassLookup.Add("POS", typeof(PossessiveEnding));
            posClassLookup.Add("PRP", typeof(PronounPersonal));
            posClassLookup.Add("PRP$", typeof(PronounPossessive));
            posClassLookup.Add("RB", typeof(Adverb));
            posClassLookup.Add("RBR", typeof(AdverbComparative));
            posClassLookup.Add("RBS", typeof(AdverbSuperlative));
            posClassLookup.Add("RP", typeof(Particle));
            posClassLookup.Add("SYM", typeof(Symbol));
            posClassLookup.Add("TO", typeof(To));
            posClassLookup.Add("UH", typeof(Interjection));
            posClassLookup.Add("VB", typeof(Verb));
            posClassLookup.Add("VBD", typeof(VerbSimplePast));
            posClassLookup.Add("VBG", typeof(VerbGerund));
            posClassLookup.Add("VBN", typeof(VerbPastParticiple));
            posClassLookup.Add("VBP", typeof(VerbNon3rdSingularPresent));
            posClassLookup.Add("VBZ", typeof(Verb3rdSingularPresent));
            posClassLookup.Add("WDT", typeof(WhDeterminer));
            posClassLookup.Add("WP", typeof(WhPronoun));
            posClassLookup.Add("WP$", typeof(WhPronounPossessive));
            posClassLookup.Add("WRB", typeof(WhAdverb));
            posClassLookup.Add("#", typeof(PoundSign));
            posClassLookup.Add("$", typeof(DollarSign));
            posClassLookup.Add(".", typeof(Period));
            posClassLookup.Add("?", typeof(QuestionMark));
            posClassLookup.Add("!", typeof(Exclamation));
            posClassLookup.Add(",", typeof(Comma));
            posClassLookup.Add(":", typeof(Colon));
            posClassLookup.Add(";", typeof(Semicolon));
            posClassLookup.Add("(", typeof(LeftParens));
            posClassLookup.Add(")", typeof(RightParens));
            posClassLookup.Add("\"", typeof(StraightDoubleQuote));
            posClassLookup.Add("'", typeof(StraightSingleQuote));
            posClassLookup.Add("??", typeof(UnknownPart));
			
			abbreviations = new List<string>();
			abbreviations.AddRange(new string[] {"Dr", "Mr", "Ms", "Mrs", "Esq", "Prof", "Gen", "Rep", "Sen", "St", "Sr", "Jr"});
        }

        // token is word, pos
        public List<Phrase> phrases;

        protected List<Phrase> completes;

        public Sentence(IEnumerable<KeyValuePair<string, string>> tokens)
        {
            // Initially, the tokens of the sentence are 'free', and belong to no phrase.
            // That will quickly change, as phrases vie for their proper place in the sentence. 
            this.phrases = new List<Phrase>();
            this.completes = new List<Phrase>();

            Populate(tokens);
        }

        public Sentence(XmlNode root)
        {
            List<KeyValuePair<string, string>> tokens = new List<KeyValuePair<string, string>>();
            phrases = new List<Phrase>();
            completes = new List<Phrase>();

            foreach (XmlNode child in root)
            {
                string type = child.Attributes["type"].Value;
                string word = child.InnerText;

                tokens.Add(new KeyValuePair<string,string>(word, type));
            }

            Populate(tokens);
        }

        public Sentence(List<Phrase> phrases)
        {
            this.phrases = phrases;
            this.completes = new List<Phrase>();
        }

        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            foreach (object phrase in phrases) {
                if (result.Length > 0)
                    result.Append(" ");
                result.Append(phrase.ToString());
            }

            return result.ToString();
        }

        public void AddFirstToCompletes()
        {
            completes.Add(phrases[0]);
            // remove this from our list
            phrases.RemoveAt(0);
            // Reset all VerbPhrases
            foreach (Phrase phrase in phrases)
                if (phrase is VerbPhrase)
                    ((VerbPhrase)phrase).Reset();
        }

        public static POSPhrase GetPOSPhrase(string word, string type)
        {
            Type posClass = typeof(POSPhrase);
            posClassLookup.TryGetValue(type, out posClass);
            if (posClass == null)
                return null;

            return (POSPhrase)Activator.CreateInstance(posClass, word);
        }

        // The initial operation on the sentence is to transform tagged tokens to phrases.
        public void Populate(IEnumerable<KeyValuePair<string, string>> tokens)
        {
            foreach (KeyValuePair<string, string> token in tokens)
            {
                POSPhrase phrase = GetPOSPhrase(token.Key, token.Value);
                if (phrase == null)
                {
                    // unknown!
                    //Unilog.Notice(this, "Could not find part for {0}/{1}", token.Key, token.Value);
                    phrases.Add(new UnknownPart(token.Key));
                    continue;
                }
                phrases.Add(phrase);
            }
        }

        public Phrase Parse() {
            while (!Complete()) {
				bool active = false;
                // Sift the phrases into precedence classes
                Dictionary<int, List<KeyValuePair<int, Phrase>>> pclasses = new Dictionary<int, List<KeyValuePair<int, Phrase>>>();
				int phraseindex = 0;
                foreach (Phrase phrase in phrases)
                {
                    List<KeyValuePair<int, Phrase>> pclass;
                    if (!pclasses.TryGetValue(phrase.Precedence, out pclass))
                        pclasses.Add(phrase.Precedence, pclass = new List<KeyValuePair<int, Phrase>>());

                    pclass.Insert(0, new KeyValuePair<int, Phrase>(phraseindex, phrase));   // put in reverse order
					phraseindex++;
                }

                List<int> precedenceOrder = new List<int>(pclasses.Keys);
                precedenceOrder.Sort();
                precedenceOrder.Reverse();
				
				foreach (int precedence in precedenceOrder) {
                    List<KeyValuePair<int, Phrase>> pclass = pclasses[precedence];
					int skipToSentenceBefore = -1;
                    foreach (KeyValuePair<int, Phrase> kvp in pclass)
                    {
						int index = kvp.Key;
						if (skipToSentenceBefore >= 0) {
							if (ProbablyDifferentSentence(index, skipToSentenceBefore))
								skipToSentenceBefore = -1;
							else {
								skipToSentenceBefore = index;
								continue;
							}
						}
						
						int iters = 20;
						Phrase phrase = kvp.Value;
                        if (RecursiveTransform(phrase, ref iters))
                        {
                            active = true;
							skipToSentenceBefore = index;
                        }
                    }

                    if (active)
                        break;
                }
			
                if (!active && !Complete()) {
                    // None of the phrases can decide what to do next.
                    bool found = false;
                    // Look for the next punctuation
                    for (int ii = 0; ii < phrases.Count; ii++)
                    {
						if (ProbablyEndOfSentence(ii)) {
                            // Cut this off!
                            Combine(phrases.GetRange(0, ii + 1), new Fragment());
                            AddFirstToCompletes();
                            found = true;
                            break;							
						}
                    }
                    if (found)
                        continue;   // more processing to do!

                    if (phrases.Count > 0)
                    {
                        // Wrap the sentence in a fragment
                        Fragment fragment = new Fragment();
                        List<Phrase> copy = new List<Phrase>(phrases);
                        Combine(copy, fragment);
                    }

                    break;
                }
            }

            // Combine the various completes
            if (completes.Count > 0)
            {
                if (phrases.Count > 0 || completes.Count > 1)
                {
                    Paragraph paragraph = new Paragraph();
                    foreach (Phrase phrase in completes)
                        paragraph.Constituents.Add(phrase);
                    if (phrases.Count > 0)
                    {
                        paragraph.Constituents.Add(phrases[0]);
                        phrases[0] = paragraph;
                    }
                    else
                        phrases.Add(paragraph);
                } else
                    phrases.Add(completes[0]);  // no need for a paragraph
            }
			
            return phrases[0];
        }

        public bool RecursiveTransform(Phrase phrase, ref int iters)
        {
            iters--;
            if (iters <= 0)
                return false;

            int index = phrases.IndexOf(phrase);

            bool active = phrase.Transform(this);
            if (active)
            {
                int newindex = phrases.IndexOf(phrase);
                if (newindex != -1)
                    index = newindex;
				Phrase newphrase = phrases[index];

                // Do after first
                if (index < phrases.Count - 1 && phrases[index + 1].Precedence >= newphrase.Precedence)
                    RecursiveTransform(phrases[index + 1], ref iters);

                newindex = phrases.IndexOf(phrase);
                if (newindex != -1)
                    index = newindex;
				newphrase = phrases[index];

                if (index > 0 && phrases[index - 1].Precedence > newphrase.Precedence)
                    RecursiveTransform(phrases[index - 1], ref iters);

                // Do it again, if it's still here
                if (phrases.Contains(phrase)) // is the original phrase still here?
                    RecursiveTransform(phrase, ref iters);
            }

            return active;
        }
        
        public bool Complete() {
            return phrases.Count <= 1;
        }

        public Phrase PhraseBefore(Phrase sourcePhrase) {
            int location = phrases.IndexOf(sourcePhrase);
            if (location < 1)
                return null;

            return phrases[location - 1];
        }

        public Phrase PhraseAfter(Phrase sourcePhrase) {
            int location = phrases.IndexOf(sourcePhrase);
            if (location == -1 || location == phrases.Count - 1)
                return null;

            return phrases[location + 1];
        }

        // Mergers happen--for example--when there are two or more noun phrases
        // in a row; they merge into one noun phrase with the combined constituents
        // of the merge.
        public bool MergeNext(Phrase sourcePhrase) {
            Phrase targetPhrase = PhraseAfter(sourcePhrase);
            sourcePhrase.Constituents.AddRange(targetPhrase.Constituents);
            phrases.Remove(targetPhrase);
            return true;
        }

        public bool MergePrevious(Phrase sourcePhrase) {
            Phrase targetPhrase = PhraseBefore(sourcePhrase);
            sourcePhrase.Constituents.InsertRange(0, targetPhrase.Constituents);
            phrases.Remove(targetPhrase);
            return true;
        }

        public bool AbsorbNext(Phrase sourcePhrase) {
            Phrase targetPhrase = PhraseAfter(sourcePhrase);
            sourcePhrase.Constituents.Add(targetPhrase);
            phrases.Remove(targetPhrase);
            return true;
        }
        
        public bool AbsorbPrevious(Phrase sourcePhrase) {
            Phrase targetPhrase = PhraseBefore(sourcePhrase);
            sourcePhrase.Constituents.Insert(0, targetPhrase);
            phrases.Remove(targetPhrase);
            return true;
        }

        // phrases is a list of phrases that propose forming a larger
        // entity with themselves as constituents.
        public void Combine(List<Phrase> phrases, Phrase proposedPhrase) {
            int start = this.phrases.IndexOf(phrases[0]);
            int end = this.phrases.IndexOf(phrases[phrases.Count - 1]);

            this.phrases.RemoveRange(start, end - start + 1);
            this.phrases.Insert(start, proposedPhrase);
            proposedPhrase.Constituents = phrases;
        }

        // Removes a higher-level phrase, replacing it with its constituents
        public void Separate(Phrase phrase) {
            int location = this.phrases.IndexOf(phrase);
            List<Phrase> phrases = new List<Phrase>(phrase.Constituents);
            this.phrases.RemoveAt(location);
            this.phrases.InsertRange(location, phrases);
        }

		// Without parsing, see if these are probably in different sentences
		public bool ProbablyDifferentSentence(int index1, int index2) {
			// Look for end of sentence
			for (int ii = index1+1; ii < index2; ii++) {
				if (phrases[ii] is SimpleDeclarativePhrase || phrases[ii] is SimpleQuestion || phrases[ii] is Fragment)
					return true;
				if (ProbablyEndOfSentence(ii))
					return true;
			}
			
			return false;
		}
		
		public bool ProbablyEndOfSentence(int ii) {
			Phrase phrase = phrases[ii];
            if (phrase is Period || phrase is QuestionMark || phrase is Exclamation)
            {
                if (ii == 0 || !PossiblyAbbreviation(phrases[ii - 1].Text))   // don't cut at A.
					return true;
            }
			
			return false;
		}

		// Only call when need to-- for example, if the next character is a period
		public bool PossiblyAbbreviation(string text) {
			if (text.Length > 5)
				return false;
			
			if (text.Length == 1 || text.ToUpper() == text)
				return true;
			
			if (abbreviations.Contains(text))
				return true;
			
			return false;
		}
	
	    // The data source of part of speech tags to object types
			
	    #region IDataSource<string,Type> Members
	
	    public bool TryGetValue(string key, out Type value)
	    {
			return posClassLookup.TryGetValue(key, out value);
	    }
	
	    #endregion
	
	    #region IEnumerable<KeyValuePair<string,Type>> Members
	
	    public IEnumerator<KeyValuePair<string, Type>> GetEnumerator()
	    {
			return posClassLookup.GetEnumerator();
	    }
	
	    #endregion
	
	    #region IEnumerable Members
	
	    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
	    {
			return posClassLookup.GetEnumerator();
	    }
	
	    #endregion
	
	}
}
