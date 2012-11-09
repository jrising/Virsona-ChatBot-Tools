using System;
using System.Collections.Generic;
using System.Text;
using GenericTools;

namespace DataTemple
{
	public class CodeletTrace
	{
		Stack<TwoTuple<object, string>> stack;
		
		public CodeletTrace()
		{
			stack = new Stack<TwoTuple<object, string>>();
		}
		
		public CodeletTrace(Stack<TwoTuple<object, string>> stack) {
			this.stack = stack;
		}
		
		public Stack<TwoTuple<object, string>> Stack {
			get {
				return stack;
			}
		}
		
		public CodeletTrace AppendFrame(object source, string location) {
			CodeletTrace trace = new CodeletTrace(stack);
			trace.stack.Push(new TwoTuple<object, string>(source, location));
			return trace;
		}
		
		public override string ToString ()
		{
			StringBuilder result = new StringBuilder();
			foreach (TwoTuple<object, string> frame in stack) {
				if (frame.one == null)
					result.AppendLine("Unknown: " + frame.two);
				else
					result.AppendLine(frame.one.ToString() + ": " + frame.two);
			}
				
			return result.ToString();
		}
	}
}

