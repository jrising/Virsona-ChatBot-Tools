using System;
using System.Collections.Generic;
using System.Text;
using GenericTools;

namespace LanguageNet.WordLogic
{
    public class AnaphoraUtilities
    {
        // Does this appear to be a reference for something?
        // Checks the part of our senses, and our sub-parts
        public static ProbableStrength SeemsAnaphora(InformedPhrase phrase)
        {
            ProbableStrength total = new ProbableStrength(0, 0);
            double strengthFactor = total.ImproveStrengthStart();

            foreach (KeyValuePair<PhraseSense, double> sense in phrase.Senses)
            {
                if (sense.Key.Phrases.Count == 1)
                    total.ImproveStrength(SeemsAnaphora(sense.Key.Phrases[0]), ref strengthFactor);
                else
                {
                    ProbableStrength singletotal = sense.Key.SpeechPart().SeemsAnaphora().DownWeight(1.0 / (sense.Key.Phrases.Count + 1));

                    // Combine together results from subphrases
                    ProbableStrength phrasetotal = new ProbableStrength(0, 0);
                    double phraseFactor = phrasetotal.ImproveStrengthStart();
                    foreach (InformedPhrase subphrase in sense.Key.Phrases)
                        phrasetotal.ImproveStrength(SeemsAnaphora(subphrase).DownWeight(1.0 / sense.Key.Phrases.Count), ref phraseFactor);
                    phrasetotal.ImproveStrengthFinish(phraseFactor);

                    // Use both our and the recursive result
                    total.ImproveStrength(singletotal.Combine(phrasetotal), ref strengthFactor);
                }
            }

            total.ImproveStrengthFinish(strengthFactor);

            return total;
        }

        public static ProbableStrength SeemsReferee(InformedPhrase phrase, IWordLookup informer, ProbableStrength anaphora)
        {
            List<string> words = phrase.Generate();

            double weight = 0;
            foreach (string word in words)
                weight += informer.GetWeight(word, false);
            /* words weight result
             * 1     0      0
             * 1     1      1
             * 2     0      0
             * 2     .5     .4
             * 2     1      2/3
             * 2     1.5    .86
             * 2     2      1
             * 3     .5     .3
             * 3     1      .5
             * 3     2      .8
             */
            double myweight = 2.0 * weight / (words.Count + weight);
            if (words.Count == 1)
                myweight /= 2.0;    // down-score 1-word answers
            if (words.Count > 2)
                myweight /= Math.Log(words.Count);

            ProbableStrength notanaphora = anaphora.InverseProbability();

            return new ProbableStrength(Math.Sqrt(myweight * notanaphora.strength), notanaphora.weight + .5 - notanaphora.weight * .5);
        }

        // Could this be an anaphora for the given phrase?  Check the attributes
        public static ProbableStrength AnaphoraOf(InformedPhrase anaphora, PhraseSense prime)
        {
            if (anaphora.IsContained(prime) || prime.IsContained(anaphora))
                return ProbableStrength.Zero;   // can't refer to included structure

            ProbableStrength total = new ProbableStrength(0, 0);
            double strengthFactor = total.ImproveStrengthStart();

            foreach (KeyValuePair<PhraseSense, double> sense in anaphora.Senses)
            {
                ProbableStrength match = AnaphoraOf(sense.Key, prime);
                total.ImproveStrength(match.DownWeight(sense.Value), ref strengthFactor);
            }

            total.ImproveStrengthFinish(strengthFactor);

            return total;
        }

        public static ProbableStrength AnaphoraOf(PhraseSense anaphora, PhraseSense prime)
        {
            ProbableStrength total = new ProbableStrength(0, 0);
            double strengthFactor = total.ImproveStrengthStart();

            // Check if each attribute could describe the phrase
            foreach (PhraseAttribute attribute in anaphora.Attributes)
            {
                if (attribute is PartOSAttribute)
                    continue;   // ignore
                total.ImproveStrength(attribute.Describes(prime), ref strengthFactor);
            }

            // Check if subphrases could be anaphors for the given phrase
            foreach (InformedPhrase subphrase in anaphora.Phrases)
                total.ImproveStrength(AnaphoraOf(subphrase, prime).DownWeight(1.0 / anaphora.Phrases.Count), ref strengthFactor);

            total.ImproveStrengthFinish(strengthFactor);
            return total;
        }

        public static ProbableStrength SeemsReferencePhrase(InformedPhrase phrase, bool not)
        {
            ProbableStrength total = new ProbableStrength(0, 0);
            double strengthFactor = total.ImproveStrengthStart();

            foreach (KeyValuePair<PhraseSense, double> sense in phrase.Senses)
            {
                if (sense.Key.Phrases.Count == 1)
                    total.ImproveStrength(SeemsReferencialVerb(sense.Key.Phrases[0]), ref strengthFactor);
                else
                {
                    // Is this a "helper" verb followed by an anaphora?
                    ProbableStrength foundHelper = ProbableStrength.None, foundBoth = ProbableStrength.None;
                    foreach (InformedPhrase subphr in sense.Key.Phrases)
                    {
                        if (!not)
                        {
                            foundHelper = foundHelper.Better(SeemsReferencialVerb(subphr));
                            foundBoth = foundBoth.Better(foundHelper.Relative(SeemsAnaphora(subphr)));
                        }
                        else
                        {
                            foundHelper = foundHelper.Better(SeemsReferencialVerb(subphr).InverseProbability());
                            foundBoth = foundBoth.Better(foundHelper.Relative(SeemsAnaphora(subphr).InverseProbability()));
                        }
                    }

                    foundBoth.weight = foundBoth.weight + .5 - foundBoth.weight * .5;

                    // Use both our and the recursive result
                    total.ImproveStrength(foundBoth, ref strengthFactor);
                }
            }

            total.ImproveStrengthFinish(strengthFactor);

            return total;
        }

        public static ProbableStrength SeemsReferencialVerb(InformedPhrase phrase)
        {
            if (phrase.IsTerminal)
            {
                string verb = phrase.Name.ToLower();
                if (verb == "do" || verb == "did" || verb == "does" || verb == "done" || verb == "doing")
                    return ProbableStrength.Full;
                if ((verb == "have" || verb == "had" || verb == "has" || verb == "having") ||
                    (verb == "was" || verb == "were" || verb == "am" || verb == "is" || verb == "are" || verb == "being" || verb == "been" || verb == "be"))
                    return ProbableStrength.Half;

                return ProbableStrength.Zero;
            }
            else
            {
                if (phrase.Senses.Count == 1)
                {
                    KeyValuePair<PhraseSense, double> sense = phrase.Senses[0];
                    if (sense.Key.Phrases.Count == 1)
                        return SeemsReferencialVerb(sense.Key.Phrases[0]);
                }

                // Could be, but not so simple!
                return ProbableStrength.None;
            }
        }
    }
}
