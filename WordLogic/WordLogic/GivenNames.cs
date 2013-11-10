using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using GenericTools.DataSources;
using BeIT.MemCached;
using GenericTools;

namespace LanguageNet.WordLogic
{
    public class GivenNames : BackedMemcachedSource<string>
    {
        public static string namesPrefix = "name$";
        
        protected static string femaleFilename = "female.txt";
        protected static string maleFilename = "male.txt";
        protected static string petFilename = "pet.txt";

        protected static char[] tokenizer = new char[] { ' ', '\t', '\n', '\r' };
        protected static List<SpeechPart> nounparts = new List<SpeechPart>(new SpeechPart[] { SpeechPart.ProperNoun, SpeechPart.Noun, SpeechPart.NounPhrase });

        public GivenNames(string morphodir, MemcachedClient memcache) :
            base(new ComboSource<string, string>(new AlphabeticFileSet<string>(morphodir + femaleFilename, tokenizer, 20, "female"),
                new ComboSource<string, string>(new AlphabeticFileSet<string>(morphodir + maleFilename, tokenizer, 20, "male"),
                    new AlphabeticFileDict(morphodir + petFilename, tokenizer, 20))),
                namesPrefix, memcache)
        {
        }

        // Try to determine if this is any kind of given name
        public ProbableStrength IsGivenName(IWordLookup informer, string word)
        {
            word = word.ToLower();

            // Try to look it up in me
            string nametype = null;
            if (TryGetValue(word, out nametype))
                return ProbableStrength.Full;

            double weight = informer.GetWeight(word, false);
            return new ProbableStrength(weight * weight, 0.5);
        }

        // Try to determine if this could be a female name (alternatively, we think it's male)
        public ProbableStrength IsFemaleName(string word)
        {
            word = word.ToLower();

            // Try to look it up in me
            string nametype = null;
            if (TryGetValue(word, out nametype))
            {
                if (nametype == "female")
                    return ProbableStrength.Full;
                if (nametype == "male")
                    return ProbableStrength.Zero;
                return ProbableStrength.Half;   // animal name-- could be either
            }

            if (word.EndsWith("a"))
                return new ProbableStrength(0.75, 0.5);

            return ProbableStrength.None;
        }

        // Inform everything that looks like a name
        public void GenderInformAll(InformedPhrase phrase, IWordLookup informer)
        {
            foreach (KeyValuePair<PhraseSense, double> sense in phrase.Senses)
            {
                // Could this be a person?  Have to check all noun phrases, because DummyEnglishParser doesn't properly specify Nouns
                if (nounparts.Contains(sense.Key.SpeechPart()))
                    GenderInform(sense.Key, informer);
                else
                {
                    // Otherwise drill down
                    foreach (InformedPhrase subphr in sense.Key.Phrases)
                        GenderInformAll(subphr, informer);
                }
            }
        }

        // Given a noun phrase sense
        public void GenderInform(PhraseSense sense, IWordLookup informer)
        {
            // Drill down to the first noun word, looking for a current attribute
            PhraseSense first = sense;
            GenderAttribute bypart = (GenderAttribute) first.FindAttribute(typeof(GenderAttribute));

            while (bypart == null)
            {
                // Find a noun sub-phrase
                PhraseSense next = null;
                foreach (InformedPhrase subfirst in first.Phrases)
                {
                    foreach (KeyValuePair<PhraseSense, double> firstsense in subfirst.Senses)
                        if (nounparts.Contains(firstsense.Key.SpeechPart()))
                        {
                            next = firstsense.Key;
                            break;
                        }
                    if (next != null)
                        break;
                }
                if (next == null)
                    break;
                bypart = (GenderAttribute)next.FindAttribute(typeof(GenderAttribute));

                if (next.Phrases.Count == 0)
                    break;
                first = next;
            }

            if (first.SpeechPart() != SpeechPart.ProperNoun)
            {
                if (bypart != null && bypart.Strength.weight > 0.5)
                    return;  // we seem to know!

                List<string> words;
                if (first.Phrases.Count > 0)
                    words = first.Phrases[0].Generate();
                else
                {
                    words = new List<string>();
                    words.Add(first.Name());
                }
                if (informer.GetWeight(words[0], false) < 0.5)
                    return;  // not worth the lookup

                if (bypart == null)
                {
                    GenderAttribute dummy = new GenderAttribute();
                    bypart = (GenderAttribute)dummy.Guess(sense.Phrases[0]);
                }
            }

            // Now guess using the given name
            InformedPhrase tocheck;
            if (first.Phrases.Count > 0)
                tocheck = first.Phrases[0];
            else
            {
                List<KeyValuePair<PhraseSense, double>> senses = new List<KeyValuePair<PhraseSense, double>>();
                senses.Add(new KeyValuePair<PhraseSense, double>(first, 1.0));
                tocheck = new InformedPhrase(sense.Name(), senses);
            }
            GenderAttribute gender = GuessGender(tocheck, informer);
            if (bypart != null)
                gender = MergeGenderAttributes(gender, bypart);

            // Update relevant gender attributes
            sense.AddOrUpdateAttribute(gender);
            if (sense != first)
                first.AddOrUpdateAttribute(gender);
        }

        public GenderAttribute MergeGenderAttributes(GenderAttribute byname, GenderAttribute bypart)
        {
            if (byname.Gender == GenderAttribute.GenderOptions.Neuter && bypart.Gender == GenderAttribute.GenderOptions.Neuter)
            {
                GenderAttribute attr = new GenderAttribute(GenderAttribute.GenderOptions.Neuter);
                attr.Strength = byname.Strength.Combine(bypart.Strength);
                return attr;
            }

            // Human likelihood
            ProbableStrength human = byname.Match(GenderAttribute.IsHuman).Combine(bypart.Match(GenderAttribute.IsHuman));
            if (human.strength > 0.5)
            {
                if (byname.Gender == GenderAttribute.GenderOptions.Neuter)
                {
                    // we don't have male/female info
                    bypart.Strength = human;
                    return bypart;
                }
                else
                {
                    // use male/female info or just human
                    byname.Strength = human;
                    return byname;
                }
            }
            else
            {
                GenderAttribute attr = new GenderAttribute(GenderAttribute.GenderOptions.Neuter);
                attr.Strength = human.InverseProbability();
                return attr;
            }
        }

        public GenderAttribute GuessGender(InformedPhrase word, IWordLookup informer)
        {
            List<string> parts = word.Generate();
            string part = parts[0];

            ProbableStrength given = IsGivenName(informer, part);
            if (given.strength < .25 && given.weight > 0.75)
            {
                GenderAttribute attr = new GenderAttribute(GenderAttribute.GenderOptions.Neuter);
                attr.Strength = given.InverseProbability();
                return attr;
            }

            ProbableStrength female = IsFemaleName(part);
            if (female.strength > 0.5)
            {
                GenderAttribute attr = new GenderAttribute(GenderAttribute.GenderOptions.Female);
                attr.Strength = (new ProbableStrength(2.0 * (female.strength - 0.5), female.weight)).Relative(given);
                return attr;
            }
            else if (female.strength < 0.5 && female.weight > 0.25)
            {
                GenderAttribute attr = new GenderAttribute(GenderAttribute.GenderOptions.Male);
                attr.Strength = (new ProbableStrength(2.0 * (0.5 - female.strength), female.weight)).Relative(given);
                return attr;
            }
            else
            {
                GenderAttribute attr = new GenderAttribute(GenderAttribute.GenderOptions.Human);
                attr.Strength = given;
                return attr;
            }
        }

        public string RandomName(string kind, Random randgen)
        {
            FileStream fs;
            if (kind == "any")
                kind = randgen.Next(3) == 0 ? "pet" : "human";
            if (kind == "human")
                kind = randgen.Next(2) == 0 ? "female" : "male";
            if (kind == "female")
                fs = new FileStream(femaleFilename, FileMode.Open);
            else if (kind == "male")
                fs = new FileStream(maleFilename, FileMode.Open);
            else if (kind == "pet")
                fs = new FileStream(petFilename, FileMode.Open);
            else
                throw new Exception("unknown kind " + kind);

            // Find the encoding
            StreamReader reader = new StreamReader(fs);
            Encoding enc = reader.CurrentEncoding;

            // Find the end
            fs.Seek(0, SeekOrigin.End);
			long bottom = fs.Position;

            // Jump to a random point
            fs.Seek(randgen.Next((int) bottom), SeekOrigin.Begin);
            // Read to the end of the line
            while (fs.ReadByte() != '\n' && fs.Position < fs.Length) ;
            if (fs.Position == fs.Length)
                fs.Seek(0, SeekOrigin.Begin);

            // Read the next element
            byte[] data = new byte[20];
            string line = "";
            while (line.Trim(tokenizer) == "" || line[0] == '#')
                line = enc.GetString(data);

            string[] parts = line.Split(tokenizer);

            return parts[0];
        }
    }
}
