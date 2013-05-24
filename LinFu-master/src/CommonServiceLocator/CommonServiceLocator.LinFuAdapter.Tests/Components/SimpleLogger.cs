using System;

namespace CommonServiceLocator.AutofacAdapter.Components
{
	public class SimpleLogger : ILogger
	{
		public void Log(string msg)
		{
			Console.WriteLine(msg);
		}
	}
}