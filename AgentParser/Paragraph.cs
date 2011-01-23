/******************************************************************\
 *      Class Name:     Paragraph
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Encapsulates a paragraph, a collection of sentences.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using LanguageNet.Grammarian;

namespace LanguageNet.AgentParser
{
    public class Paragraph : Phrase
    {
        public Paragraph()
            : base("=P")
        {
        }

        public override Phrase Parapharse(Verbs verbs, Nouns nouns, WordNetAccess wordnet, GrammarParser.ParaphraseOptions options, List<Phrase> emphasizes, ref double prob)
        {
            Paragraph paragraph = new Paragraph();

            foreach (Phrase constituent in constituents)
                paragraph.constituents.Add(constituent.Parapharse(verbs, nouns, wordnet, options | GrammarParser.ParaphraseOptions.IsStayingStart, emphasizes, ref prob));

            return paragraph;
        }
    }
}
