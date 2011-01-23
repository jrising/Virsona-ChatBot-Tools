/******************************************************************\
 *      Class Name:     MorphologerInterface
 *      Written By:     James Rising
 *      Copyright:      2009, Virsona, Inc.
 *                      GNU Lesser General Public License, Ver. 3
 *                      (see license.txt and license.lesser.txt)
 *
 * 						Modified from DoMuchMore by David Levy
 *      -----------------------------------------------------------
 * This file is part of Morphologer and is free software: you
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
 * License along with Morphologer.  If not, see
 * <http://www.gnu.org/licenses/>.
 *      -----------------------------------------------------------
 * Collect all of the data from the verb table files
\******************************************************************/
using System;
using System.Collections.Generic;
using System.IO;
using GenericTools;

namespace LanguageNet.Morphologer
{
	public class VerbsData
	{
		internal Dictionary<string, TwoTuple<string, string>> base_past;
    	internal Dictionary<string, TwoTuple<string, string>> past_base;
    	internal Dictionary<string, string> base_pastpart;
    	internal Dictionary<string, string> pastpart_base;
    	internal Dictionary<string, string> verb_tobase;
        internal Dictionary<string, bool> todouble;

        internal Dictionary<string, bool> transitive;
        internal Dictionary<string, bool> intransitive;
        internal Dictionary<string, bool> either;

        internal Dictionary<string, string> phrVerbT;	// transitive phrasal verbs
        internal Dictionary<string, string> phrVerbI;	// intransitive phrasal verbs
        internal Dictionary<string, string> phrVerbE;	// either phrasal verbs
        internal Dictionary<string, string> phrVAP;		// Verb Adverb Preposition phrasal verbs

        internal Dictionary<string, TwoTuple<string, string>> verb_ING;
        internal Dictionary<string, TwoTuple<string, string>> verb_ED;
        internal Dictionary<string, TwoTuple<string, string>> verb_S;
		
		public VerbsData(string basedir)
		{
		    base_past = new Dictionary<string, TwoTuple<string,string>>();
        	past_base = new Dictionary<string, TwoTuple<string,string>>();
        	base_pastpart = new Dictionary<string,string>();
        	pastpart_base = new Dictionary<string,string>();
        	verb_tobase = new Dictionary<string,string>();
			
            string[] lines = File.ReadAllLines(basedir + "verb_table.txt");
            if (lines == null)
                throw new FileLoadException(string.Format("Failed to open verb table"));

            foreach (string line in lines) {
                if (line.Length == 0)
                    break;
                AuxString auxstr = new AuxString(line);
                base_past[auxstr.Word(0)] = new TwoTuple<string, string>(auxstr.Word(1), auxstr.Word(3));
                past_base[auxstr.Word(1)] = new TwoTuple<string, string>(auxstr.Word(0), auxstr.Word(3));
                base_pastpart[auxstr.Word(0)] = auxstr.Word(2);
                pastpart_base[auxstr.Word(2)] = auxstr.Word(0);
            }

            lines = File.ReadAllLines(basedir + "verb_exceptions.txt");
            if (lines == null)
                throw new FileLoadException("Failed to open verb exceptions");

            for (int ii = 0; ii < lines.Length; ii += 2) {
                if (lines[ii].Length == 0)
                    break;
                verb_tobase.Add(lines[ii], lines[ii + 1]);
            }

            todouble = FillBag(basedir + "verb_todouble.txt", "Failed to open verb double table");

            either = FillBag(basedir + "verb_either.txt", "Failed to open verb either table");
            transitive = FillBag(basedir + "verb_transitive.txt", "Failed to open verb transitive table");
            intransitive = FillBag(basedir + "verb_intransitive.txt", "Failed to open verb intransitive table");

            phrVerbT = GlomDictionary(basedir + "verb_adv_transitive.txt", "Failed to open transitive verbal phrase table");
            phrVerbI = GlomDictionary(basedir + "verb_adv_intransitive.txt", "Failed to open intransitive verbal phrase table");
            phrVerbE = GlomDictionary(basedir + "verb_adv_either.txt", "Failed to open either verbal phrase table");

            lines = File.ReadAllLines(basedir + "verb_adv_prep.txt");
            if (lines == null)
                throw new FileLoadException("Failed to open either verbal phrase table");

            phrVAP = new Dictionary<string,string>();

            foreach (string line in lines) {
                if (line.Length == 0)
                    break;
                AuxString auxstr = new AuxString(line);
                AppendString(phrVAP, auxstr.Word(0), auxstr.Word(1) + " " + auxstr.Word(2), "#");
            }

			verb_ING = new Dictionary<string, TwoTuple<string,string>>();
            verb_ED = new Dictionary<string, TwoTuple<string,string>>();
            verb_S = new Dictionary<string, TwoTuple<string,string>>();

			lines = File.ReadAllLines(basedir + "verb_special.txt");
            if (lines == null)
                throw new FileLoadException(string.Format("Failed to open verb special table"));
			
            foreach (string line in lines) {
                if (line.Length == 0)
                    break;
                AuxString auxstr = new AuxString(line);
                if (auxstr.Letter(0) != '#') // ignore comments
                {
                    if (auxstr.WordCount() == 0)
                        continue;
                    if (auxstr.WordCount() != 5)
                        throw new FieldAccessException(string.Format("Bad format in special table at \"{0}\"", line));
                    else
                    {
                        string base_word = auxstr.Word(0);
                        string base_ing = auxstr.Word(1);
                        string base_ed = auxstr.Word(2);
                        string base_s = auxstr.Word(3);
                        string type = auxstr.Word(4);

                        if (base_ing != "-")
                            verb_ING.Add(base_ing, new TwoTuple<string, string>(base_word, type));

                        if (base_ed != "-")
                            verb_ED.Add(base_ed, new TwoTuple<string, string>(base_word, type));

                        if (base_s != "-")
                            verb_S.Add(base_s, new TwoTuple<string, string>(base_word, type));
                    }
                }
            }		
		}

        protected Dictionary<string, string> GlomDictionary(string filename, string error) {
            string[] lines = File.ReadAllLines(filename);
            if (lines == null)
                throw new FileLoadException("Failed to open transitive verbal phrase table");

            Dictionary<string, string> result = new Dictionary<string,string>();

            foreach (string line in lines) {
                if (line.Length == 0)
                    break;
                AuxString auxstr = new AuxString(line);
                AppendString(result, auxstr.Word(0), auxstr.Word(1), "#");
            }

            return result;
        }

        public Dictionary<string, bool> FillBag(string filename, string error)
        {
            string[] lines = File.ReadAllLines(filename);
            if (lines == null)
                throw new FileLoadException(error);

            Dictionary<string, bool> result = new Dictionary<string, bool>();

            foreach (string line in lines)
            {
                if (line.Length == 0)
                    break;
                result[line] = true;
            }

            return result;
        }

        public void AppendString(Dictionary<string, string> dict, string key, string value, string separator)
        {
            string newval = value;
            if (dict.TryGetValue(key, out newval))
                newval += value;

            dict[key] = newval;
        }	
	}
}

