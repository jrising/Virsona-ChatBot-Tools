/******************************************************************\
 *      Class Name:     AlphabeticFileSet
 *      Written By:     James Rising
 *      Copyright:      2011, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * A source which expresses the existence of entries in an
 * alphabetic file of terms
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace GenericTools.DataSources
{
    public class AlphabeticFileSet<T> : AlphabeticFile<T>
    {
        protected T value;

        public AlphabeticFileSet(string filename, char[] tokenizer, int keylen, T value)
            : base(filename, tokenizer, keylen)
        {
            this.value = value;
        }

        public override T ReadStreamEntry(FileStream fs)
        {
            return value;
        }

        public override T ReadStringEntry(string line)
        {
            return value;
        }
    }
}
