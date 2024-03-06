using System;
using Datadog.Metrics.Management;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Hosting;

namespace Datadog.Integrations.Core
{
	public static class AspNetCoreConfiguration
	{
		public static void ConfigureDynamicOptions(MvcOptions options)
		{
			RuntimeMetricsTracker.AddGlobalTag("app_service", Configuration.Datadog.Service);
			RuntimeMetricsTracker.AddGlobalTag("app_env", Configuration.Datadog.Env);
			RuntimeMetricsTracker.AddGlobalTag("app_version", Configuration.Datadog.Version);
			options.Filters.Add(typeof(ErrorHandlingFilter));
		}

		public static IHostBuilder ConfigureDynamicSetup(this IHostBuilder builder)
		{
			ConfigureInversionOfControl(builder);
			ConfigureLogging(builder);
			return builder;
		}

		private static void ConfigureInversionOfControl(IHostBuilder builder)
		{
			var inversionOfControl = Environment.GetEnvironmentVariable("JUNKYARD_IOC") ?? "LAMAR";

			switch (inversionOfControl.ToUpperInvariant())
			{
				case "LAMAR":
					builder.UseLamar();
					break;
				default:
					builder.UseLamar();
					break;
			}
		}

		private static void ConfigureLogging(IHostBuilder builder)
		{
		}

		public class ErrorHandlingFilter : ExceptionFilterAttribute
		{
			public override void OnException(ExceptionContext context)
			{
				var exception = context.Exception;
				RuntimeMetricsTracker.RequestUnhandledException(exception);
			}
		}
	}
}