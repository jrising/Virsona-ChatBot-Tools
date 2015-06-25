/******************************************************************\
 *      Class Name:     IParsedPhrase
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Interface for objects that compose a parsed grammar structure
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.Grammarian
{
    public interface IParsedPhrase : ICloneable
    {
        // A string representation (possibly constructed from constituents)
        string Text { get; }
        // The part of speech; typically a Penn Treebank part
        string Part { get; }
        // Is this a single word, or a collection of subphrases?
        bool IsLeaf { get; }
        // Get the constituent subphrases if this is not a single word
        IEnumerable<IParsedPhrase> Branches { get; }
    }
}
