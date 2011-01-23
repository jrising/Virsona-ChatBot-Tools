/******************************************************************\
 *      Class Name:     PhraseResolveHandler
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Resolves every element of an IParsedPhrase with an unknown part
 *   of speech (i.e. "??"), returning the lowest level elements
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using PluggerBase.ActionReaction.Actions;
using PluggerBase.ActionReaction.Interfaces;
using LanguageNet.Grammarian;

namespace HeppleTagger
{
    public class PhraseResolveHandler : UnitaryHandler
    {
        protected POSTagger tagger;

        // Describe interface to UnitaryHandler
        public PhraseResolveHandler(POSTagger tagger)
            : base("Phrase List Tag",
                "Resolve the unknown elements contained in a list of Phrases.",
                new EnumerableArgumentType(int.MaxValue, new TypedArgumentType(typeof(IParsedPhrase), new WordPhrase("bufallo", "NN"))),
                LanguageNet.Grammarian.POSTagger.TagEnumerationResultType, 80)
        {
            this.tagger = tagger;
        }

        // Apply tagger's ResolveUnknowns method
        public IEnumerable<string> Handle(IEnumerable<IParsedPhrase> phrases)
        {
            return tagger.ResolveUnknowns(phrases);
        }

        // Translate an enumeration of objects to one of IParsedPhrases
        public override object Handle(object arg)
        {
            List<IParsedPhrase> phrases = new List<IParsedPhrase>();

            foreach (object elt in (IEnumerable)arg)
                phrases.Add((IParsedPhrase)elt);

            return Handle(phrases);
        }
    }
}
