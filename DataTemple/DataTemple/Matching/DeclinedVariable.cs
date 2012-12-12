using System;
using LanguageNet.Grammarian;
using DataTemple.AgentEvaluate;

namespace DataTemple.Matching
{
	public class DeclinedVariable : Variable
	{
		protected Variable variable;
		protected IDeclination declination;
		
		public DeclinedVariable(string name, Variable variable, IDeclination declination)
			: base(name)
		{
			this.variable = variable;
			this.declination = declination;
		}
		
		public override bool IsMatch(IParsedPhrase check)
		{
			if (variable.IsMatch(check))
				return declination.IsInDeclination(check);
			
			return false;
		}
		
		public override IParsedPhrase Produce(Context env, POSTagger tagger, GrammarParser parser)
		{
			IParsedPhrase phrase = variable.Produce(env, tagger, parser);
			return (IParsedPhrase) declination.Decline(phrase);
		}
	}
}

