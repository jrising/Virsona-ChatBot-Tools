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
 * Expose the various sources and handlers in Morophologer
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using PluggerBase;
using PluggerBase.FastSerializer;
using LanguageNet.Grammarian;
using GenericTools.DataSources;

namespace LanguageNet.Morphologer
{
    [PluginDisplayName("Morphologer")]
	public class MorphologerInterface : IPlugin
	{
		public MorphologerInterface()
		{
		}

        #region IPlugin Members

		public uint Version {
			get {
				return 1;
			}
		}

        public InitializeResult Initialize(PluginEnvironment env, Assembly assembly, IMessageReceiver receiver)
        {
            // Data files contained in [datadrectory]/wordnet
            string basedir = env.GetConfigDirectory("datadirectory") + Path.DirectorySeparatorChar + "morpho" + Path.DirectorySeparatorChar;
			
			InitializeNouns(env, basedir);
			InitializeVerbs(env, basedir);
			
			return InitializeResult.Success();
        }

		public void InitializeNouns(PluginEnvironment env, string basedir) {
			int key_len = 256;
			char[] tokenizer = new char[] { ' ' };

			// Noun type source
			IDataSource<string, Nouns.NounType> ambigSource = new AlphabeticFileSet<Nouns.NounType>(basedir + "person_ambig.txt", tokenizer, key_len, Nouns.NounType.ProperEither);
			IDataSource<string, Nouns.NounType> femaleSource = new AlphabeticFileSet<Nouns.NounType>(basedir + "person_female.txt", tokenizer, key_len, Nouns.NounType.ProperFemale);
			IDataSource<string, Nouns.NounType> maleSource = new AlphabeticFileSet<Nouns.NounType>(basedir + "person_male.txt", tokenizer, key_len, Nouns.NounType.ProperMale);
			IDataSource<string, Nouns.NounType> citySource = new AlphabeticFileSet<Nouns.NounType>(basedir + "city.txt", tokenizer, key_len, Nouns.NounType.ProperCity);
			IDataSource<string, Nouns.NounType> countrySource = new AlphabeticFileSet<Nouns.NounType>(basedir + "country.txt", tokenizer, key_len, Nouns.NounType.ProperCountry);
			IDataSource<string, Nouns.NounType> regionSource = new AlphabeticFileSet<Nouns.NounType>(basedir + "region.txt", tokenizer, key_len, Nouns.NounType.ProperProvince);			
			IDataSource<string, Nouns.NounType> countSource = new AlphabeticFileSet<Nouns.NounType>(basedir + "count_nouns.txt", tokenizer, key_len, Nouns.NounType.Count);
			IDataSource<string, Nouns.NounType> massSource = new AlphabeticFileSet<Nouns.NounType>(basedir + "mass_nouns.txt", tokenizer, key_len, Nouns.NounType.Mass);
			IDataSource<string, Nouns.NounType> theSource = new AlphabeticFileSet<Nouns.NounType>(basedir + "the_nouns.txt", tokenizer, key_len, Nouns.NounType.The);			
			
            env.SetDataSource<string, Nouns.NounType>(Nouns.NounTypeSourceName, new ComboSource<string, Nouns.NounType>(new ComboSource<string, Nouns.NounType>(
			                                          new ComboSource<string, Nouns.NounType>(new ComboSource<string, Nouns.NounType>(ambigSource, femaleSource), maleSource),
			                                          new ComboSource<string, Nouns.NounType>(new ComboSource<string, Nouns.NounType>(citySource, countrySource), regionSource)),
			                                          new ComboSource<string, Nouns.NounType>(new ComboSource<string, Nouns.NounType>(countSource, massSource), theSource)));
			
			IDataSource<string, Nouns.Gender> feminineSource = new AlphabeticFileSet<Nouns.Gender>(basedir + "nouns_female.txt", tokenizer, key_len, Nouns.Gender.Female);
			IDataSource<string, Nouns.Gender> masculineSource = new AlphabeticFileSet<Nouns.Gender>(basedir + "nouns_male.txt", tokenizer, key_len, Nouns.Gender.Male);
			IDataSource<string, Nouns.Gender> eitherSource = new AlphabeticFileSet<Nouns.Gender>(basedir + "nouns_malefem.txt", tokenizer, key_len, Nouns.Gender.Either);
			
			env.SetDataSource<string, Nouns.Gender>(Nouns.GenderSourceName,
			                                        new ComboSource<string, Nouns.Gender>(new ComboSource<string, Nouns.Gender>(feminineSource, masculineSource), eitherSource));
                                                         
			MemorySource<string, string> toSingular = new MemorySource<string, string>();
			MemorySource<string, string> toPlural = new MemorySource<string, string>();
			ReadNounNumber(basedir + "nouns_number.txt", toSingular, toPlural);
			
			env.SetDataSource<string, Nouns.Number>(Nouns.NumberSourceName,
			                                        new ComboSource<string, Nouns.Number>(new MapDataSource<string, string, Nouns.Number>(toSingular, ToShared, Nouns.Number.Plural), 
			                                                                              new MapDataSource<string, string, Nouns.Number>(toPlural, ToShared, Nouns.Number.Singular)));

			env.AddAction(new ChangeNounHandler(Nouns.Number.Singular, toSingular));
			env.AddAction(new ChangeNounHandler(Nouns.Number.Plural, toPlural));
		}
		
		public void ReadNounNumber(string filename, Dictionary<string, string> toSingular, Dictionary<string, string> toPlural) {
            string[] lines = File.ReadAllLines(filename);
            if (lines == null)
                throw new FileLoadException("Failed to open noun number data");

            for (int ii = 0; ii < lines.Length; ii += 2) {
                if (lines[ii].Length == 0)
                    break;
                AuxString auxstr1 = new AuxString(lines[ii]);
                AuxString auxstr2 = new AuxString(lines[ii + 1]);
                
                string t1, line1;
                if (auxstr1.WordCount() > 1) {
                    t1 = auxstr1.Word(1);
                    line1 = auxstr1.Word(0).ToLower();
                } else {
                    t1 = "";
                    line1 = lines[ii].ToLower();
                }

                string t2, line2;
                if (auxstr2.WordCount() > 1) {
                    t2 = auxstr2.Word(1);
                    line2 = auxstr2.Word(0).ToLower();
                } else {
                    t2 = "";
                    line2 = lines[ii + 1].ToLower();
                }

                if (t1 == "P" || t1 == "TP")
                    toSingular.Add(line2, "#");
                else if (t2 == "P")
                    toSingular.Add(line2, "");
                else if (t1 != "S") {
                    if (t1.Length > 0 && t1 != "T")
                        Console.WriteLine(string.Format("T1={0}", t1));
                    toSingular.Add(line2, line1);
                }

                if (t1 == "S")
                    toPlural.Add(line1, "#");
                else
                    toPlural.Add(line1, line2);

                if (t2.Length > 0 && t2 != "P") {
                    if (t2.Length == 1 && t2 != "T")
                        Console.WriteLine(string.Format("T2={0}", t2));
                    else if (t2 != "T")
                        toSingular.Add(t2, line2);
                }
            }
		}
		
		public Nouns.Number ToShared(string value, object shared) {
			return (Nouns.Number) shared;
		}
		
		public void InitializeVerbs(PluginEnvironment env, string basedir) {
			VerbsData verbsData = new VerbsData(basedir);
			ConjugateHandler conjugator = new ConjugateHandler(verbsData);
			
			env.AddAction(conjugator);
			env.AddAction(new TransitiveTestHandler(verbsData, conjugator));
			env.AddAction(new InflectionTestHandler(verbsData, conjugator));

			char[] tokenizer = new char[] { '\n' };
			int key_len = 256;

			IDataSource<string, Verbs.VerbType> transitiveSource = new AlphabeticFileSet<Verbs.VerbType>(basedir + "verb_transitive.txt", tokenizer, key_len, Verbs.VerbType.VT);
			IDataSource<string, Verbs.VerbType> intransitiveSource = new AlphabeticFileSet<Verbs.VerbType>(basedir + "verb_intransitive.txt", tokenizer, key_len, Verbs.VerbType.VI);
			IDataSource<string, Verbs.VerbType> eitherSource = new AlphabeticFileSet<Verbs.VerbType>(basedir + "verb_either.txt", tokenizer, key_len, Verbs.VerbType.VE);
			IDataSource<string, Verbs.VerbType> advTransitiveSource = new AlphabeticFileSet<Verbs.VerbType>(basedir + "verb_adv_transitive.txt", tokenizer, key_len, Verbs.VerbType.VT);
			IDataSource<string, Verbs.VerbType> advIntransitiveSource = new AlphabeticFileSet<Verbs.VerbType>(basedir + "verb_adv_intransitive.txt", tokenizer, key_len, Verbs.VerbType.VI);
			IDataSource<string, Verbs.VerbType> advEitherSource = new AlphabeticFileSet<Verbs.VerbType>(basedir + "verb_adv_either.txt", tokenizer, key_len, Verbs.VerbType.VE);
			IDataSource<string, Verbs.VerbType> advPrepSource = new AlphabeticFileSet<Verbs.VerbType>(basedir + "verb_adv_prep.txt", tokenizer, key_len, Verbs.VerbType.VT);

            env.SetDataSource<string, Verbs.VerbType>(Verbs.VerbTypeSourceName, new ComboSource<string, Verbs.VerbType>(new ComboSource<string, Verbs.VerbType>(
			                                          new ComboSource<string, Verbs.VerbType>(transitiveSource, intransitiveSource),
			                                          new ComboSource<string, Verbs.VerbType>(eitherSource, advTransitiveSource)),
			                                                                                                            new ComboSource<string, Verbs.VerbType>(
			                                          new ComboSource<string, Verbs.VerbType>(advIntransitiveSource, advEitherSource), advPrepSource)));
		}
		
        #endregion
		
        #region IFastSerializable Members

        public void Deserialize(SerializationReader reader)
        {
            // do nothing
        }

        public void Serialize(SerializationWriter writer)
        {
            // do nothing
        }

        #endregion

        #region IMessageReceiver Members

        public bool Receive(string message, object reference)
        {
            return false;
        }

        #endregion

	}
}

