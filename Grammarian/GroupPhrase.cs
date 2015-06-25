/******************************************************************\
 *      Class Name:     GroupPhrase
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Simple implementaton of an IParsedPhrase that contains other
 * phrase constituents
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.Grammarian
{
    public class GroupPhrase : IParsedPhrase
    {
        protected string part;
        protected IEnumerable<IParsedPhrase> branches;

        public GroupPhrase(string part, IEnumerable<IParsedPhrase> branches)
        {
            this.part = part;
            this.branches = branches;
        }
		
		public GroupPhrase(IParsedPhrase copy)
			: this(copy.Part, copy.Branches) {
		}
		
        #region IParsedPhrase Members

        // Recursively get Text on all branches and concatenate
        public string Text
        {
            get {
                List<string> words = new List<string>();
                foreach (IParsedPhrase branch in branches)
                    words.Add(branch.Text);

                return StringUtilities.JoinWords(words);
            }
        }

        public string Part
        {
            get {
                return part;
            }
        }

        public bool IsLeaf
        {
            get {
                return false;
            }
        }

        public IEnumerable<IParsedPhrase> Branches
        {
            get {
                return branches;
            }
        }

        #endregion
		
		public IEnumerable<IParsedPhrase> GetRange(int start) {
			IEnumerator<IParsedPhrase> e = branches.GetEnumerator();
			int i = 0;
			while (i < start && e.MoveNext())
				i++;
			while (e.MoveNext()) {
				yield return e.Current;
				i++;
		    }
		}
		
		public IParsedPhrase GetBranch(int which) {
			IEnumerator<IParsedPhrase> e = branches.GetEnumerator();
			int i = 0;
			while (i < which && e.MoveNext())
				i++;
			e.MoveNext();
			return e.Current;
		}
		
		public IParsedPhrase FindBranch(string part) {
			IEnumerator<IParsedPhrase> e = branches.GetEnumerator();
			while (e.MoveNext())
				if (e.Current.Part == part)
					return e.Current;
			return null;
		}
		
		public GroupPhrase AddBranch(IParsedPhrase branch) {
			List<IParsedPhrase> allbranches = new List<IParsedPhrase>(branches);
			allbranches.Add(branch);
			return new GroupPhrase(part, allbranches);
		}
				
		public int Count {
			get {
				IEnumerator<IParsedPhrase> e = branches.GetEnumerator();
				int i = 0;
				while (e.MoveNext())
					i++;
				return i;
			}
		}
		
		public static List<string> PhraseToTexts(IParsedPhrase phrase) {
            List<string> words = new List<string>();
			if (phrase.IsLeaf)
				words.Add(phrase.Text);
			else {
				GroupPhrase groupPhrase = new GroupPhrase(phrase);
	            foreach (IParsedPhrase branch in groupPhrase.branches)
	                words.AddRange(PhraseToTexts(branch));
			}

            return words;
		}
		
        #region ICloneable Members

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}
