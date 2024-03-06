using System;

namespace Datadog.Integrations.Core.Logging.Implementations
{
	public class ConsoleLogger : ILogger
	{
		public void Debug(string message, params object[] args)
		{
			Console.WriteLine(message, args);
		}

		public void Info(string message, params object[] args)
		{
			Console.WriteLine(message, args);
		}

		public void Warn(string message, params object[] args)
		{
			Console.WriteLine(message, args);
		}

		public void Error(string message, params object[] args)
		{
			Console.Error.WriteLine(message, args);
		}

		public void Error(Exception ex, string message, params object[] args)
		{
			Console.Error.WriteLine($"{message}{Environment.NewLine}{ex}", args);
		}

		public void Critical(string message, params object[] args)
		{
			Console.Error.WriteLine(message, args);
		}
	}
}