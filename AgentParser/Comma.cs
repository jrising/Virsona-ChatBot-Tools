/******************************************************************\
 *      Class Name:     Comma
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 *                      Originally written by Jerome Scheuring
 *                      translated to C# and extended
 *      -----------------------------------------------------------
 * Encapsulates a comma (like ,), and how it affects
 * the form of a sentence.
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.AgentParser
{
    public class Comma : Punctuation
    {
        public Comma(string word)
            : base(",", word)
        {
            precedence = 10;
        }

        public override bool Transform(Sentence sentence)
        {
            Phrase next = sentence.PhraseAfter(this);

            if (next != null && next.Part == "WHNP")
            {
                Phrase afternext = sentence.PhraseAfter(next);
                if (afternext == null || afternext is Period || afternext is QuestionMark || afternext is Exclamation || afternext is Semicolon || afternext is Colon)
                {
                    Parenthetical parenthetical = new Parenthetical();
                    List<Phrase> consts = new List<Phrase>();
                    consts.Add(this);
                    consts.Add(next);
                    sentence.Combine(consts, parenthetical);
                    Phrase before = sentence.PhraseBefore(parenthetical);
                    if (before != null)
                    {
                        // would the last subphrase better take this?
                        if (!AbsorbInLast(sentence, before, parenthetical))
                            sentence.AbsorbNext(before);
                    }
                    return true;
                }

                if (afternext is Comma)
                {
                    Parenthetical parenthetical = new Parenthetical();
                    List<Phrase> consts = new List<Phrase>();
                    consts.Add(this);
                    consts.Add(next);
                    consts.Add(afternext);
                    sentence.Combine(consts, parenthetical);
                    Phrase before = sentence.PhraseBefore(parenthetical);
                    if (before != null)
                        // would the last subphrase better take this?
                        if (!AbsorbInLast(sentence, before, parenthetical))
                            sentence.AbsorbNext(before);
                    return true;
                }
            }

            return false;
        }

        public bool AbsorbInLast(Sentence sentence, Phrase before, Phrase parenthetical)
        {
            if (before is POSPhrase)
                return false;

            if (before is NounPhrase)
            {
                before.Constituents.Add(parenthetical);
                sentence.phrases.Remove(parenthetical);
                return true;
            }

            return AbsorbInLast(sentence, before.Constituents[before.Constituents.Count - 1], parenthetical);
        }
    }
}
