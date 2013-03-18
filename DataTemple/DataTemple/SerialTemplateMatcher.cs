using System;
using System.Collections.Generic;
using PluggerBase;
using PluggerBase.ActionReaction.Evaluations;
using LanguageNet.Grammarian;
using DataTemple.AgentEvaluate;
using DataTemple.Matching;
using DataTemple.Codeland;

namespace DataTemple
{
	public class SerialTemplateMatcher : IContinuation, IFailure
	{
		IMessageReceiver receiver;
		IContinuation succ;
		Coderack coderack;
		TryToRescueMatch tryToRescueMatch;
		
		List<IParsedPhrase> inputs;		
		int inputIndex;
		List<PatternTemplateSource> allDicta;
		Queue<PatternTemplateSource> remainingDicta;
		
		double weight;
		
		public SerialTemplateMatcher(IMessageReceiver receiver, IContinuation succ, Coderack coderack,
		                             TryToRescueMatch tryToRescueMatch, IParsedPhrase input,
		                             List<PatternTemplateSource> dicta, double weight)
		{
			this.receiver = receiver;
			this.succ = succ;
			this.coderack = coderack;
			this.tryToRescueMatch = tryToRescueMatch;

			this.inputs = new List<IParsedPhrase>(input.Branches);
			inputIndex = -1;
			this.allDicta = dicta;
			this.weight = weight;
		}
		
		public void MatchNextSentence() {
			inputIndex++;
			if (inputs.Count == inputIndex)
				return;
			
			remainingDicta = new Queue<PatternTemplateSource>(allDicta);
			
			MatchNextTemplate();
		}
		
		protected bool MatchNextTemplate() {
			if (remainingDicta.Count == 0)
				return false;
			
			PatternTemplateSource dictum = remainingDicta.Dequeue();			
			if (dictum.Pattern.Contents[0].Name == "%sentence") {
				IFailure fail = tryToRescueMatch.MakeFailure(inputs[inputIndex], dictum, this, this, coderack);
				dictum.Generate(coderack, inputs[inputIndex], this, fail, weight);
				
				return true;
			} else if (dictum.Pattern.Contents[0].Name == "%sentences") {
				int count = 1;
				foreach (IContent content in dictum.Pattern.Contents)
					if (content.Name == "/")
						count++;
				
				GroupPhrase groupPhrase = new GroupPhrase("=P", inputs.GetRange(inputIndex, count));
				IFailure fail = tryToRescueMatch.MakeFailure(groupPhrase, dictum, this, this, coderack);
				dictum.Generate(coderack, groupPhrase, this, fail, weight);
				
				return true;
			} else {
				receiver.Receive(String.Format("Template starting with {0} not available in serial mode.", dictum.Pattern.Contents[0].Name), dictum);
				return MatchNextTemplate();
			}
		}
		
		public bool Continue(object value, IFailure fail)
		{
			succ.Continue(value, fail);
			MatchNextSentence();
			return true;
		}
		
		public bool Fail(string reason, IContinuation skip)
		{
			if (!MatchNextTemplate())
				MatchNextSentence();
			return true;
		}
		
		public object Clone()
		{
			return new SerialTemplateMatcher(receiver, succ, coderack, tryToRescueMatch, new GroupPhrase("=P", inputs), allDicta, weight);
		}
	}
}

