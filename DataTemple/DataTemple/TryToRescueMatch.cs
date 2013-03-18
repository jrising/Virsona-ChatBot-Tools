using System;
using PluggerBase.ActionReaction.Evaluations;
using GenericTools;
using LanguageNet.Grammarian;
using DataTemple.Matching;
using DataTemple.Codeland;

namespace DataTemple
{
	public class TryToRescueMatch
	{
		public TryToRescueMatch()
		{
		}
				
		// default is failure to rescue
		public virtual bool CallRescue(Coderack coderack, IParsedPhrase input, PatternTemplateSource patternTemplateSource, string reason, IContinuation skip, IContinuation succ, IFailure fail) {
			return fail.Fail(reason, skip);
		}
		
		public IFailure MakeFailure(IParsedPhrase input, PatternTemplateSource patternTemplateSource, IContinuation succ, IFailure fail, Coderack coderack) {
			return new FailletWrapper(FailToTryToRescue, this, input, patternTemplateSource, succ, fail, coderack);
		}
		
		bool FailToTryToRescue(IArena arena, double salience, string reason, IContinuation skip, params object[] args) {
			TryToRescueMatch tryToRescueMatch = (TryToRescueMatch) args[0];
			IParsedPhrase input = (IParsedPhrase) args[1];
			PatternTemplateSource patternTemplateSource = (PatternTemplateSource) args[2];
			IContinuation succ = (IContinuation) args[3];
			IFailure fail = (IFailure) args[4];
			Coderack coderack = (Coderack) args[5];
			return tryToRescueMatch.CallRescue(coderack, input, patternTemplateSource, reason, skip, succ, fail);
		}
	}
}

