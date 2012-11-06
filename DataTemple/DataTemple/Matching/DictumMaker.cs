using System;
using System.Collections.Generic;
using System.Text;
using DataTemple.AgentEvaluate;

namespace DataTemple.Matching
{
    public class DictumMaker
    {
        protected Context context;
        protected string source;

        public DictumMaker(Context context, string source)
        {
            this.context = context;
            this.source = source;
        }

        public PatternTemplateSource MakeDictum(string pattern, string template)
        {
            return MakeDictum(pattern, template, 1.0);
        }

        public PatternTemplateSource MakeDictum(string pattern, string template, double score)
        {
            Context patternEnv = Interpreter.ParseCommands(context, pattern);
            Context templateEnv = Interpreter.ParseCommands(context, template);
            return new PatternTemplateSource(patternEnv, templateEnv, score, source);
        }

        public List<PatternTemplateSource> Reverse(List<PatternTemplateSource> oneways)
        {
            List<PatternTemplateSource> revs = new List<PatternTemplateSource>();

            foreach (PatternTemplateSource oneway in oneways)
                revs.Add(new PatternTemplateSource(oneway.Template, oneway.Pattern, oneway.Score, oneway.Source));

            return revs;
        }
    }
}
