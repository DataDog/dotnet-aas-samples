using System;
using StatsdClient;

namespace Datadog.Integrations.Core
{
	public class MetricsHelper
	{
		private static DogStatsdService _statsService;

		static MetricsHelper()
		{
			GetStatsService();
		}

		public static void Increment(string name, string[] tags)
		{
			if (Configuration.Datadog.StatsEnabled)
			{
				_statsService.Increment(name, tags: tags);
			}
		}

		public static void IntegrationError(object integrationMethod, Exception ex)
		{
			if (Configuration.Datadog.StatsEnabled)
			{
				var exDetail = $"{ex.GetType()}_{ex.Message.Replace(" ", string.Empty)}";
				string loggerDetail;

				try
				{
					loggerDetail = $"{integrationMethod.GetType().DeclaringType}";
				}
				catch
				{
					loggerDetail = $"{integrationMethod.GetType().Name}";
				}

				Increment("integration_exception", new[] { $"source_type:{loggerDetail}", $"exception:{exDetail}" });
			}
		}

		private static DogStatsdService GetStatsService()
		{
			if (_statsService == null)
			{
				var env = Environment.GetEnvironmentVariable("DD_ENV") ?? "apm-aas";

				_statsService = new DogStatsdService();
				_statsService.Configure(new StatsdConfig() { ConstantTags = new[] { $"env:{env}" } });
			}

			return _statsService;
		}
	}
}