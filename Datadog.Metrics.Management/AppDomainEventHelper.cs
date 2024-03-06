using System;
using System.Collections.Generic;
using System.Reflection;

namespace Datadog.Metrics.Management
{
	public static class AppDomainEventHelper
	{
		private static AppDomain _domain = null;
		private static readonly HashSet<string> _emptyList = new HashSet<string>();

		public static void MonitorAppDomain(AppDomain domain)
		{
			RuntimeMetricsTracker.AddCustomMetric($"process.appdomain.check", 1, new HashSet<string> { $"status:failure", $"appdomain_id:{domain.Id}", $"appdomain_name:{domain.FriendlyName}" });

			if (_domain != null)
			{
				// We already did this part
				return;
			}

			_domain = domain;

			try
			{
				domain.AssemblyResolve += new ResolveEventHandler(ResolveEventHandler);
				domain.AssemblyLoad += new AssemblyLoadEventHandler(LoadEventHandler);
			}
			catch
			{
				RuntimeMetricsTracker.AddCustomMetric($"process.appdomain.event.assemblyresolve.subscribe.failure", 1, _emptyList);
			}

			try
			{
				var assemblies = _domain.GetAssemblies();

				foreach (var assembly in assemblies)
				{
					var assemblyName = assembly.GetName();
					RuntimeMetricsTracker.AddCustomMetric($"process.appdomain.assembly_load", 1,
						new HashSet<string>
						{
							$"assembly_name:{assemblyName.Name}",
							$"assembly_version:{assemblyName.Version}",
							$"assembly_full_name:{assemblyName.FullName}",
							$"appdomain_id:{_domain.Id}",
							$"appdomain_name:{_domain.FriendlyName}"
						});
				}
			}
			catch
			{
				RuntimeMetricsTracker.AddCustomMetric($"process.appdomain.event.assemblyresolve.subscribe.failure", 1, _emptyList);
			}
		}

		private static void LoadEventHandler(object sender, AssemblyLoadEventArgs args)
		{
			try
			{
				var assemblyName = args.LoadedAssembly.GetName();
				RuntimeMetricsTracker.AddCustomMetric($"process.appdomain.assembly_load", 1,
					new HashSet<string>
					{
						$"assembly_name:{assemblyName.Name}",
						$"assembly_version:{assemblyName.Version}",
						$"assembly_full_name:{assemblyName.FullName}",
						$"appdomain_id:{_domain.Id}",
						$"appdomain_name:{_domain.FriendlyName}"
					});
			}
			catch
			{
				RuntimeMetricsTracker.AddCustomMetric($"process.appdomain.event.assemblyresolve.failure", 1, _emptyList);
			}
		}

		private static Assembly ResolveEventHandler(object sender, ResolveEventArgs args)
		{
			try
			{
				RuntimeMetricsTracker.AddCustomMetric($"process.appdomain.assemblyresolve", 1,
					new HashSet<string>
					{
						$"assembly_name:{args.Name}",
						$"requesting_assembly:{args.RequestingAssembly}",
						$"appdomain_id:{_domain.Id}",
						$"appdomain_name:{_domain.FriendlyName}"
					});
			}
			catch
			{
				RuntimeMetricsTracker.AddCustomMetric($"process.appdomain.event.assemblyresolve.failure", 1, _emptyList);
			}

			return null;
		}
	}
}