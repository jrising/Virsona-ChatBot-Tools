/******************************************************************\
 *      Class Name:     Rules
 *      Written By:     James Rising
 *      Copyright:      2001-2007, The University of Sheffield
 *                      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 * 
 *                      Originally written by Mark Hepple,
 *                      modified by Valentin Tablan and Niraj Aswani
 *                      translated to Python by Jerome Scheuring
 *                      translated to C# by James Rising
 *      -----------------------------------------------------------
 * This file is part of HeppleTagger and is free software: you
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
 * License along with HeppleTagger.  If not, see
 * <http://www.gnu.org/licenses/>.
 *      -----------------------------------------------------------
 * Contains all the possible rule classes
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace HeppleTagger
{
    public class Rule_CURWD : Rule
    {
        public Rule_CURWD() { }

        // the current word is as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.wordBuff[3] == context[0]);
        }
    }

    public class Rule_LBIGRAM : Rule
    {
        public Rule_LBIGRAM() { }

        // the current and previous word are as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.wordBuff[2] == context[0] &&
                    tagger.wordBuff[3] == context[1]);
        }
    }

    public class Rule_NEXT1OR2OR3TAG : Rule
    {
        public Rule_NEXT1OR2OR3TAG() { }

        // any of the next speech parts are as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.lexBuff[4][0] == context[0] ||
                    tagger.lexBuff[5][0] == context[0] ||
                    tagger.lexBuff[6][0] == context[0]);
        }
    }

    public class Rule_NEXT1OR2TAG : Rule
    {
        public Rule_NEXT1OR2TAG() { }

        // either of the next speech parts are as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.lexBuff[4][0] == context[0] ||
                    tagger.lexBuff[5][0] == context[0]);
        }
    }

    public class Rule_NEXT1OR2WD : Rule
    {
        public Rule_NEXT1OR2WD() { }

        // either of the next words are as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.wordBuff[4] == context[0] ||
                    tagger.wordBuff[5] == context[0]);

        }
    }

    public class Rule_NEXT2TAG : Rule
    {
        public Rule_NEXT2TAG() { }

        // the after-next speech part is as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.lexBuff[5][0] == context[0]);
        }
    }

    public class Rule_NEXT2WD : Rule
    {
        public Rule_NEXT2WD() { }

        // the after-next word is as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.wordBuff[5] == context[0]);
        }
    }

    public class Rule_NEXTBIGRAM : Rule
    {
        public Rule_NEXTBIGRAM() { }

        // the next two speech parts are as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.lexBuff[4][0] == context[0] &&
                    tagger.lexBuff[5][0] == context[1]);
        }
    }

    public class Rule_NEXTTAG : Rule
    {
        public Rule_NEXTTAG() { }

        // the next speech part is as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.lexBuff[4][0] == context[0]);
        }
    }

    public class Rule_NEXTWD : Rule
    {
        public Rule_NEXTWD() { }

        // the next word is as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.wordBuff[4] == context[0]);
        }
    }

    public class Rule_PREV1OR2OR3TAG : Rule
    {
        public Rule_PREV1OR2OR3TAG() { }

        // any of the previous speech parts are as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.lexBuff[0][0] == context[0] ||
                    tagger.lexBuff[1][0] == context[0] ||
                    tagger.lexBuff[2][0] == context[0]);
        }
    }

    public class Rule_PREV1OR2TAG : Rule
    {
        public Rule_PREV1OR2TAG() { }

        // either of the previous speech parts are as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.lexBuff[1][0] == context[0] ||
                    tagger.lexBuff[2][0] == context[0]);
        }
    }

    public class Rule_PREV1OR2WD : Rule
    {
        public Rule_PREV1OR2WD() { }

        // either of the previous words are as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.wordBuff[1] == context[0] ||
                    tagger.wordBuff[2] == context[0]);
        }
    }

    public class Rule_PREV2TAG : Rule
    {
        public Rule_PREV2TAG() { }

        // the before-previous speech part is as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.lexBuff[1][0] == context[0]);
        }
    }

    public class Rule_PREV2WD : Rule
    {
        public Rule_PREV2WD() { }

        // the before-previous word is as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.wordBuff[1] == context[0]);
        }
    }

    public class Rule_PREVBIGRAM : Rule
    {
        public Rule_PREVBIGRAM() { }

        // both of the previous speech parts are as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.lexBuff[1][0] == context[0] &&
                    tagger.lexBuff[2][0] == context[1]);
        }
    }

    public class Rule_PREVTAG : Rule
    {
        public Rule_PREVTAG() { }

        // the previous speech part is as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.lexBuff[2][0] == context[0]);
        }
    }

    public class Rule_PREVWD : Rule
    {
        public Rule_PREVWD() { }

        // the previous word is as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.wordBuff[2] == context[0]);
        }
    }

    public class Rule_RBIGRAM : Rule
    {
        public Rule_RBIGRAM() { }

        // the current and next word are as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.wordBuff[3] == context[0] &&
                    tagger.wordBuff[4] == context[1]);
        }
    }

    public class Rule_SURROUNDTAG : Rule
    {
        public Rule_SURROUNDTAG() { }

        // the neighboring speech parts are as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.lexBuff[2][0] == context[0] &&
                    tagger.lexBuff[4][0] == context[1]);
        }
    }

    public class Rule_UNKNOWN : Rule
    {
        public Rule_UNKNOWN() { }

        // dummy rule-- never applies
        public override bool checkContext(POSTagger tagger)
        {
            return false;
        }
    }

    public class Rule_UNRESTRICTED : Rule
    {
        public Rule_UNRESTRICTED() { }

        // dummy rule-- always applies
        public override bool checkContext(POSTagger tagger)
        {
            return true;
        }
    }

    public class Rule_WDAND2AFT : Rule
    {
        public Rule_WDAND2AFT() { }

        // the current and after next words are as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.wordBuff[3] == context[0] &&
                    tagger.wordBuff[5] == context[1]);
        }
    }

    public class Rule_WDAND2BFR : Rule
    {
        public Rule_WDAND2BFR() { }

        // the current and before-previous words are as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.wordBuff[1] == context[0] &&
                    tagger.wordBuff[3] == context[1]);
        }
    }

    public class Rule_WDAND2TAGAFT : Rule
    {
        public Rule_WDAND2TAGAFT() { }

        // the current word and after-next speech part are as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.wordBuff[3] == context[0] &&
                    tagger.lexBuff[5][0] == context[1]);
        }
    }

    public class Rule_WDAND2TAGBFR : Rule
    {
        public Rule_WDAND2TAGBFR() { }

        // the current word and before-previous speech part are as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.lexBuff[1][0] == context[0] &&
                    tagger.wordBuff[3] == context[1]);
        }
    }

    public class Rule_WDNEXTTAG : Rule
    {
        public Rule_WDNEXTTAG() { }

        // the current word and next speech part are as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.wordBuff[3] == context[0] &&
                    tagger.lexBuff[4][0] == context[1]);
        }
    }

    public class Rule_WDPREVTAG : Rule
    {
        public Rule_WDPREVTAG() { }

        // the current word and previous speech part are as given
        public override bool checkContext(POSTagger tagger)
        {
            return (tagger.lexBuff[2][0] == context[0] &&
                    tagger.wordBuff[3] == context[1]);
        }
    }  
}