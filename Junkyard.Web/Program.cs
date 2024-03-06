using System;
using Microsoft.Extensions.Hosting;
using Datadog.Integrations.Core;
using Microsoft.AspNetCore.Hosting;
using Serilog;

namespace Junkyard.Web
{
	public class Program
	{
		public static void Main(string[] args)
		{
			CreateHostBuilder(args).Build().Run();
		}

		public static IHostBuilder CreateHostBuilder(string[] args)
		{
			var builder = Host.CreateDefaultBuilder(args);
			builder.ConfigureDynamicSetup();

			var logsInjectionVar = Environment.GetEnvironmentVariable("DD_LOGS_INJECTION") ?? "false";
			var forceSerilog = Environment.GetEnvironmentVariable("DD_SERILOG_SINK_FORCE") ?? "false";

			bool logsInjectionEnabled = IsTrue(logsInjectionVar) || IsTrue(forceSerilog);

			if (logsInjectionEnabled)
			{
				builder.UseSerilog((context, config) =>
				{
					// Figure out why env wasn't added in AAS, this should happen via the tracer
					var tags = new[] {$"env:{Environment.GetEnvironmentVariable("DD_ENV") ?? "not_set"}"};
					config.WriteTo.DatadogLogs(Environment.GetEnvironmentVariable("DD_API_KEY"), tags: tags);
					config.Enrich.FromLogContext();

					var enableFileSink = Environment.GetEnvironmentVariable("BB_DEBUG_SERILOG") ?? "false";
					if (enableFileSink.Equals("true", StringComparison.OrdinalIgnoreCase))
					{
						var logPath = Environment.GetEnvironmentVariable("SERILOG_FILE_PATH") ??
						              @"C:\home\LogFiles\Serilog\applicationlog.txt";
						config.WriteTo.File(
							logPath,
							outputTemplate:
							"{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}{Properties}{NewLine}",
							rollingInterval: RollingInterval.Day,
							rollOnFileSizeLimit: true,
							fileSizeLimitBytes: 10 * 1024 * 1024);
					}

				});
			}

			builder.ConfigureWebHostDefaults(webBuilder =>
			{
				webBuilder.UseStartup<Startup>();
			});

			return builder;
		}

		private static bool IsTrue(string logsInjectionVar)
		{
			return logsInjectionVar.Equals("true", StringComparison.OrdinalIgnoreCase) || logsInjectionVar.Equals("1", StringComparison.OrdinalIgnoreCase);
		}
	}
}
