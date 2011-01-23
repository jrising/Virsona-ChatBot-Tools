/******************************************************************\
 *      Class Name:     AlphabeticFileDict
 *      Written By:     James Rising
 *      Copyright:      2011, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * A source which produces the data following terms in a file
 * of alphabetically sorted terms.
\******************************************************************/

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GenericTools.DataSources
{
    public class AlphabeticFileDict : AlphabeticFile<string>
    {
        public AlphabeticFileDict(string filename, char[] tokenizer, int keylen)
            : base(filename, tokenizer, keylen)
        {
        }

        public override string ReadStringEntry(string line)
        {
            string[] cols = line.Split(tokenizer, StringSplitOptions.RemoveEmptyEntries);
            if (cols.Length < 1)
                return null;

            return cols[1];
        }
    }
}
