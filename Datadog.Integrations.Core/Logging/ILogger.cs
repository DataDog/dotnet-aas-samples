using System;

namespace Datadog.Integrations.Core
{
	public interface ILogger
	{
		void Debug(string message, params object[] args);
		void Info(string message, params object[] args);
		void Warn(string message, params object[] args);
		void Error(string message, params object[] args);
		void Error(Exception ex, string message, params object[] args);
		void Critical(string message, params object[] args);
	}
}