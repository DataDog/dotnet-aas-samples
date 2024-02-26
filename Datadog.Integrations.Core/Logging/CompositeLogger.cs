using System;
using System.Collections.Generic;

namespace Datadog.Integrations.Core.Logging
{
	public class CompositeLogger : ILogger
	{
		private readonly List<ILogger> _loggers = new List<ILogger>();

		public void Register(ILogger logger)
		{
			_loggers.Add(logger);
		}

		public void Remove(ILogger logger)
		{
			_loggers.Remove(logger);
		}

		public void Debug(string message, params object[] args)
		{
			foreach (var logger in _loggers)
			{
				try
				{
					logger.Debug(message, args);
				}
				catch (Exception ex)
				{
					IncrementError(logger, nameof(Debug), ex);
				}
			}
		}

		public void Info(string message, params object[] args)
		{
			foreach (var logger in _loggers)
			{
				try
				{
					logger.Info(message, args);
				}
				catch (Exception ex)
				{
					IncrementError(logger, nameof(Debug), ex);
				}
			}
		}

		public void Warn(string message, params object[] args)
		{
			foreach (var logger in _loggers)
			{
				try
				{
					logger.Warn(message, args);
				}
				catch (Exception ex)
				{
					IncrementError(logger, nameof(Debug), ex);
				}
			}
		}

		public void Error(string message, params object[] args)
		{
			foreach (var logger in _loggers)
			{
				try
				{
					logger.Error(message, args);
				}
				catch (Exception ex)
				{
					IncrementError(logger, nameof(Debug), ex);
				}
			}
		}

		public void Error(Exception error, string message, params object[] args)
		{
			foreach (var logger in _loggers)
			{
				try
				{
					logger.Error(error, message, args);
				}
				catch (Exception ex)
				{
					IncrementError(logger, nameof(Debug), ex);
				}
			}
		}

		public void Critical(string message, params object[] args)
		{
			foreach (var logger in _loggers)
			{
				try
				{
					logger.Critical(message, args);
				}
				catch (Exception ex)
				{
					IncrementError(logger, nameof(Debug), ex);
				}
			}
		}

		private void IncrementError(ILogger logger, string postfix, Exception ex)
		{
			var exDetail = $"{ex.GetType()}_{ex.Message.Replace(" ", string.Empty)}";
			var loggerDetail = $"{logger.GetType().Name}_{postfix}";
			MetricsHelper.Increment("logger_exception", new[] { $"logger:{loggerDetail}", $"exception:{exDetail}" });
		}
	}
}