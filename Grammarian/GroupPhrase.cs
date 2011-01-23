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

        #region ICloneable Members

        public object Clone()
        {
            return MemberwiseClone();
        }

        #endregion
    }
}
