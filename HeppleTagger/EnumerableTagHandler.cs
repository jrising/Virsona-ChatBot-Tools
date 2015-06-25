/******************************************************************\
 *      Class Name:     EnumerableTagHandler
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Exposes a plugin action to tag every word in an enumeration
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using PluggerBase.ActionReaction.Actions;
using PluggerBase.ActionReaction.Interfaces;

namespace HeppleTagger
{
    public class EnumerableTagHandler : UnitaryHandler
    {
        protected POSTagger tagger;
        
        public EnumerableTagHandler(POSTagger tagger)
            // Describe the action to UnitaryHandler
            : base("Part of Speech List Tag",
                "Tag every element of a grammatical list with a part of speech.",
                new EnumerableArgumentType(int.MaxValue, new StringArgumentType(int.MaxValue, ".+", "buffalo")),
                LanguageNet.Grammarian.POSTagger.TagEnumerationResultType, 100)
        {
            this.tagger = tagger;
        }

        // Tag every token in a list of tokens
        public IEnumerable<string> Handle(IEnumerable<string> words)
        {
            return tagger.tagSentence(words);
        }

        // Convert an enumerable of objects to an enumerable of strings,
        //   then call Handle(IEnumerable<string> words)
        public override object Handle(object arg)
        {
            List<string> words = new List<string>();

            foreach (object elt in (IEnumerable)arg)
                words.Add((string)elt);

            return Handle(words);
        }
    }
}
