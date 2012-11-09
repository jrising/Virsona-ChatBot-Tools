using System;
using System.Collections.Generic;
using System.Text;
using DataTemple.AgentEvaluate;
using LanguageNet.Grammarian;

namespace DataTemple.Matching
{
    public class StarUtilities
    {
        public static string NextStarName(Context context, string prefix)
        {
            string prevStarName = prefix + "#";
            int prevStarCount = context.LookupDefaulted<int>(prevStarName, 0);
            context.Map[prevStarName] = prevStarCount + 1;
            return prefix + (prevStarCount + 1).ToString();
        }

        public static List<IContent> GetStarValue(Context context, string name)
        {
            if (name.Length == 1)
                return GetStarValue(context, name + "1");

            return context.LookupDefaulted<List<IContent>>(name, new List<IContent>());
        }

        public static List<IContent> Produced(Context context, POSTagger tagger, GrammarParser parser)
        {
            List<IContent> result = new List<IContent>();
            foreach (IContent content in context.Contents)
            {
                if (content is Word)
                    result.Add(content);
                else if (content is Special && (content.Name.StartsWith("*") || content.Name.StartsWith("_")))
                {
                    List<IContent> words = GetStarValue(context, content.Name);
					result.AddRange(words);
                }
                else if (content is Variable)
                {
                    IParsedPhrase phrase = ((Variable)content).Produce(context, tagger, parser);
                    if (phrase == null)
                        return null;    // we failed!  don't use it!

                    List<string> words = GroupPhrase.PhraseToTexts(phrase);
                    foreach (string word in words)
                        result.Add(new Word(word));
                }
                else
                    result.Add(content);
            }

            return result;
        }

        public static string ProducedCode(Context context, POSTagger tagger, GrammarParser parser)
        {
            List<IContent> produced = Produced(context, tagger, parser);
            if (produced == null)
                return null;

            return ContentsCode(new Context(context, produced), tagger, parser);
        }

        public static IParsedPhrase ProducedPhrase(Context context, POSTagger tagger, GrammarParser parser)
        {
            List<IParsedPhrase> phrases = new List<IParsedPhrase>();
            foreach (IContent content in context.Contents)
            {
                if (content is Word)
                    phrases.Add(new WordPhrase(content.Name));
                else if (content is Special && (content.Name.StartsWith("*") || content.Name.StartsWith("_")))
                {
                    List<IContent> words = GetStarValue(context, content.Name);
                    foreach (IContent word in words)
                        phrases.Add(new WordPhrase(content.Name));
                }
                else if (content is Variable) {
                    IParsedPhrase phrase = ((Variable)content).Produce(context, tagger, parser);
                    if (phrase == null)
                        return null;    // failed!
                    phrases.Add(phrase);
                }
                else if (content is Concept)
                    phrases.Add(new WordPhrase(((Concept)content).Name));
                else
                    phrases.Add(new WordPhrase(content.Name));
            }

            if (phrases.Count == 0)
                return null;

		    List<KeyValuePair<string, string>> tokens = tagger.ResolveUnknowns(phrases);
			return parser.Parse(tokens);
        }

        public static string ContentsCode(Context context, POSTagger tagger, GrammarParser parser)
        {
            StringBuilder result = new StringBuilder();
            foreach (IContent content in context.Contents)
            {
                string name = content.Name;

                if (content is Variable)
                {
                    IParsedPhrase phrase = ((Variable)content).Produce(context, tagger, parser);
                    if (phrase != null)
                        name = phrase.Text;
                }

                if (name[0] == ' ')
                    result.Append(name.Substring(1));
                else
                {
                    if (result.Length > 0)
                        result.Append(" ");
                    result.Append(name);
                }
            }

            return result.ToString();
        }

    }
}
