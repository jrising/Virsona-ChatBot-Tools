/******************************************************************\
 *      Class Name:     WordPhrase
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Simple implementation of IParsedPhrase for objects that
 * represent single words within a parsed structure
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.Grammarian
{
    public class WordPhrase : IParsedPhrase
    {
        // the word contained
        protected string text;
        // the part of speech (usually a Penn Treebank tag)
        protected string part;

        public WordPhrase(string text, string part)
        {
            this.text = text;
            this.part = part;
        }

        #region IParsedPhrase Members

        public string Text
        {
            get {
                return text;
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
                return true;
            }
        }

        public IEnumerable<IParsedPhrase> Branches
        {
            get {
                return new List<IParsedPhrase>();
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
