/******************************************************************\
 *      Class Name:     StringTagHandler
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Splits a string into tokens (using StringUtilities.SplitWords)
 *   and tags each with a part of speech
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using ActionReaction.Actions;
using ActionReaction.Interfaces;
using LanguageNet.Grammarian;

namespace HeppleTagger
{
    public class StringTagHandler : UnitaryHandler
    {
        protected POSTagger tagger;

        public StringTagHandler(POSTagger tagger)
            : base("Part of Speech String Tag",
                "Tag each element of a string with a part of speech",
                new StringArgumentType(int.MaxValue, ".+", "buffalo buffalo"),
                LanguageNet.Grammarian.POSTagger.TagEnumerationResultType, 120)
        {
            this.tagger = tagger;
        }

        public IEnumerable<string> Handle(string input)
        {
            List<string> words = StringUtilities.SplitWords(input, true);
            return tagger.tagSentence(words);
        }

        #region IUnitaryHandler Members

        public override object Handle(object arg)
        {
            return Handle((string)arg);
        }

        #endregion
    }
}
