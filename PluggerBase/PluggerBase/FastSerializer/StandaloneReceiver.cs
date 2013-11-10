using System;
namespace PluggerBase
{
	public class StandaloneReceiver : IMessageReceiver
	{
		protected bool silent;
		
		public StandaloneReceiver(bool silent)
		{
			this.silent = silent;
		}
		
        public bool Receive(string message, object reference) {
			if (!silent)
				Console.WriteLine(message);
			
			return true;
		}
	}
}

