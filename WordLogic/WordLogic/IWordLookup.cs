/******************************************************************\
 *      Class Name:     IWordLookup
 *      Written By:     James.R
 *      Copyright:      Virsona Inc
 *      -----------------------------------------------------------
 * 
 *
\******************************************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace LanguageNet.WordLogic
{
    // Interface to all of the basic information we have about a word
    public interface IWordLookup
    {
        // List of synonyms, with a 0-1 score of how close the synonym is
        Dictionary<string, double> GetSynonyms(string word, bool stemmed);
        // The weight of the word, 0-1, based on its frequency (less frequent = higher)
        double GetWeight(string word, bool stemmed);
        // The speech parts of a word, typically from WordNet; not provided for stop words
        List<KeyValuePair<SpeechPart, double>> GetSpeechParts(string word, bool stemmed);
        // InformedPhrase object, currently only used for stop words
        InformedPhrase GetInformed(string word, bool stemmed);
    }
}
