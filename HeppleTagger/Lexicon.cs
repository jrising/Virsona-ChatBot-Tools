/******************************************************************\
 *      Class Name:     Lexicon
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *      -----------------------------------------------------------
 * Reads and stores the parts of speech for every word
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace HeppleTagger
{
    // Acts as a dictionary of parts of speech
    public class Lexicon : Dictionary<string, string[]>
    {
        // read a file with each line of the format "word pos pos pos ..."
        public Lexicon(string lexiconFile)
        {
            StreamReader file = new StreamReader(lexiconFile);

            while (!file.EndOfStream)
            {
                string line = file.ReadLine();
                string[] tokens = line.Split(' ');
                string[] parts = new string[tokens.Length - 1];
                for (int ii = 1; ii < tokens.Length; ii++)
                    parts[ii - 1] = tokens[ii];
                this[tokens[0]] = parts;
            }
        }
    }
}
