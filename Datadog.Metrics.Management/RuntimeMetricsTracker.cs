using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace Datadog.Metrics.Management
{
	public static class RuntimeMetricsTracker
	{
		public static readonly long AppDomainIdentifier = DateTime.UtcNow.Ticks;

		private static TimeSpan _delay;
		private static Task _metricMonitor;
		private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
		private static double _maximumCpu;

		private static readonly object UnhandledLock = new object();
		private static readonly object RequestUnhandledLock = new object();
		private static readonly object FirstChanceLock = new object();
		private static ConcurrentDictionary<string, long> _requestUnhandledExceptionCounts = new ConcurrentDictionary<string, long>();
		private static ConcurrentDictionary<string, long> _unhandledExceptionCounts = new ConcurrentDictionary<string, long>();
		private static ConcurrentDictionary<string, long> _runningFirstChanceExceptionCounts = new ConcurrentDictionary<string, long>();

		private static readonly object MetricsLock = new object();
		private static Dictionary<string, double> _metricsCollection = new Dictionary<string, double>();
		private static List<FakeMetric> _ultimateStats = new List<FakeMetric>();

		private static readonly object CustomMetricsLock = new object();
		private static List<FakeMetric> _customMetrics = new List<FakeMetric>();

		private static readonly HashSet<string> _globalTags = new HashSet<string>();

		public static void AddGlobalTag(string key, string value)
		{
			_globalTags.Add($"{key}:{value}");
		}

		public static void AddCustomMetric(string key, double value, HashSet<string> tags, bool shouldAggregate = false)
		{
			var metric = new FakeMetric(tags)
			{
				Name = key,
				ShouldAggregate = shouldAggregate,
				Value = value,
			};

			lock (CustomMetricsLock)
			{
				_customMetrics.Add(metric);
			}
		}

		public static List<FakeMetric> GetMetricsCollection()
		{
			lock (MetricsLock)
			{
				return _ultimateStats;
			}
		}

		private static void SetMetricsCollection()
		{
			ConcurrentDictionary<string, long> requestUnhandledExceptionCounts;
			ConcurrentDictionary<string, long> unhandledExceptionCounts;
			ConcurrentDictionary<string, long> firstChanceExceptionCounts;

			lock (UnhandledLock)
			{
				unhandledExceptionCounts = _unhandledExceptionCounts;
				_unhandledExceptionCounts = new ConcurrentDictionary<string, long>();
			}

			lock (RequestUnhandledLock)
			{
				requestUnhandledExceptionCounts = _requestUnhandledExceptionCounts;
				_requestUnhandledExceptionCounts = new ConcurrentDictionary<string, long>();
			}

			lock (FirstChanceLock)
			{
				firstChanceExceptionCounts = _runningFirstChanceExceptionCounts;
				_runningFirstChanceExceptionCounts = new ConcurrentDictionary<string, long>();
			}

			var metrics = new Dictionary<string, double>
			{
				{"maximum.cpu.time", _maximumCpu},
			};

			void CountExceptions(string prefix, ConcurrentDictionary<string, long> exceptionCounts)
			{
				var globalKey = $"{prefix}.exception.rate";
				long totalExceptionCount = 0;
				foreach (var exceptionCount in exceptionCounts)
				{
					var key = $"{globalKey}.{exceptionCount.Key}";
					totalExceptionCount += exceptionCount.Value;
					if (metrics.ContainsKey(key))
					{
						metrics[key] = exceptionCount.Value;
					}
					else
					{
						metrics.Add(key, exceptionCount.Value);
					}
				}

				if (metrics.ContainsKey(globalKey))
				{
					metrics[globalKey] = totalExceptionCount;
				}
				else
				{
					metrics.Add(globalKey, totalExceptionCount);
				}
			}

			CountExceptions("firstchance", firstChanceExceptionCounts);
			CountExceptions("unhandled", unhandledExceptionCounts);
			CountExceptions("request.unhandled", requestUnhandledExceptionCounts);

			var totalCpu = TimeSpan.Zero;
			var userCpu = TimeSpan.Zero;
			var systemCpu = TimeSpan.Zero;
			long totalMemory = 0;
			long totalThreadCount = 0;

			while (ProcessHelpers.StatSetsToSend.TryDequeue(out var processMetrics))
			{
				try
				{
					var tags = new HashSet<string>
						{$"pid:{processMetrics.ProcessId}", $"process_name:{processMetrics.ProcessName}"};

					AddCustomMetric($"process.monitor.active", 1, tags);

					if (processMetrics.ExitCode != null)
					{
						tags.Add($"exit.code:{processMetrics.ExitCode}");
						AddCustomMetric($"process.monitor.exit", processMetrics.ExitCode.Value, tags);
					}

					totalCpu += processMetrics.IntervalTotalProcessorTime;
					userCpu += processMetrics.IntervalUserProcessorTime;
					systemCpu += processMetrics.IntervalSystemProcessorTime;
					totalMemory += processMetrics.PrivateMemorySize;
					totalThreadCount += processMetrics.ThreadCount;

					AddCustomMetric($"system.cpu.time.{processMetrics.ProcessName}", processMetrics.IntervalSystemProcessorTime.TotalMilliseconds, tags);
					AddCustomMetric($"user.cpu.time.{processMetrics.ProcessName}", processMetrics.IntervalUserProcessorTime.TotalMilliseconds, tags);
					AddCustomMetric($"total.cpu.time.{processMetrics.ProcessName}", processMetrics.IntervalTotalProcessorTime.TotalMilliseconds, tags);
					AddCustomMetric($"private.memory.size.{processMetrics.ProcessName}", processMetrics.PrivateMemorySize, tags);
					AddCustomMetric($"thread.total.count.{processMetrics.ProcessName}", processMetrics.ThreadCount, tags);
				}
				catch (Exception ex)
				{
					AddCustomMetric($"process.monitor.aggregate.failure", 1, new HashSet<string> { $"message:{ex.Message}", $"ex:{ex.GetType().Name}", $"pid:{processMetrics.ProcessId}", $"process_name:{processMetrics.ProcessName}" });
				}
			}

			metrics.Add("system.cpu.time", systemCpu.TotalMilliseconds);
			metrics.Add("user.cpu.time", userCpu.TotalMilliseconds);
			metrics.Add("total.cpu.time", totalCpu.TotalMilliseconds);
			metrics.Add("private.memory.size", totalMemory);
			metrics.Add("thread.total.count", totalThreadCount);

			var accessDeniedCount = ProcessHelpers.AccessDeniedProcessIds.Count;

			if (accessDeniedCount > 0)
			{
				var accessDeniedIdTag = string.Join("|", ProcessHelpers.AccessDeniedProcessIds);
				var accessDeniedNameTag = string.Join("|", ProcessHelpers.AccessDeniedProcessNames);

				AddCustomMetric($"process.monitoring.denied", accessDeniedCount,
					new HashSet<string> { $"denied.pids:{accessDeniedIdTag}", $"denied.names:{accessDeniedNameTag}" });
			}

			List<FakeMetric> customMetrics;

			lock (CustomMetricsLock)
			{
				customMetrics = _customMetrics;
				_customMetrics = new List<FakeMetric>();
			}

			var ultimateStats = new List<FakeMetric>();

			var groupTagsListBasis = _globalTags.ToList();

			foreach (var customMetricGroup in customMetrics.GroupBy(m => $"{m.Name}_{m.ShouldAggregate}"))
			{
				var mainMetric = customMetricGroup.First();
				var shouldAggregate = mainMetric.ShouldAggregate;
				if (shouldAggregate)
				{
					mainMetric.Tags.UnionWith(groupTagsListBasis);
					mainMetric.Value = customMetricGroup.Sum(m => m.Value);
					ultimateStats.Add(mainMetric);
				}
				else
				{
					foreach (var customMetric in customMetricGroup)
					{
						customMetric.Tags.UnionWith(_globalTags);
						ultimateStats.Add(customMetric);
					}
				}
			}

			foreach (var metric in metrics)
			{
				var stat = new FakeMetric(_globalTags)
				{
					Name = metric.Key,
					ShouldAggregate = false,
					Value = metric.Value
				};

				ultimateStats.Add(stat);
			}

			string groupingTag = $"metrics.group:{DateTime.UtcNow.Ticks / 10_000}";
			foreach (var ultimateStat in ultimateStats)
			{
				ultimateStat.Tags.Add(groupingTag);
			}

			lock (MetricsLock)
			{
				_ultimateStats = ultimateStats;
			}
		}

		public static void Init()
		{
			_delay = TimeSpan.FromSeconds(10);
			AddGlobalTag("domain.start", AppDomainIdentifier.ToString());
			_metricMonitor = MonitorProcessMetrics();

			try
			{
				AppDomain.CurrentDomain.FirstChanceException += FirstChanceException;
				AppDomain.CurrentDomain.UnhandledException += UnhandledException;
			}
			catch
			{
				// Log.Error(ex, "Exceptions won't be monitored");
				Console.WriteLine("Exceptions won't be monitored");
			}
		}

		public static void Shutdown()
		{
			AppDomain.CurrentDomain.FirstChanceException -= FirstChanceException;
			AppDomain.CurrentDomain.UnhandledException -= UnhandledException;
			CancellationTokenSource.Cancel();
		}

		private static async Task MonitorProcessMetrics()
		{
			while (true)
			{
				if (CancellationTokenSource.IsCancellationRequested)
				{
					return;
				}

				try
				{
					ProcessHelpers.CheckAppDomain();
					ProcessHelpers.RefreshProcessRuntimeMetrics();
					TryRefreshDriveData();

					// Note: the behavior of Environment.ProcessorCount has changed a lot across version: https://github.com/dotnet/runtime/issues/622
					// What we want is the number of cores attributed to the container, which is the behavior in 3.1.2+ (and, I believe, in 2.x)
					// We report the other metrics in ticks ultimately
					_maximumCpu = Environment.ProcessorCount * _delay.TotalMilliseconds * 10_000;

					SetMetricsCollection();
				}
				catch (Exception ex)
				{
					// Log.Warning(ex, "Error while updating runtime metrics");
					Console.WriteLine("Error while updating runtime metrics");
				}

				await Task.Delay(_delay, CancellationTokenSource.Token).ConfigureAwait(false);
			}
		}

		public static void RequestUnhandledException(Exception e)
		{
			var name = e.GetType().Name;
			lock (RequestUnhandledLock)
			{
				_requestUnhandledExceptionCounts.AddOrUpdate(name, 1, (_, count) => count + 1);
			}
		}

		public static void UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			var name = e.GetType().Name;
			lock (UnhandledLock)
			{
				_unhandledExceptionCounts.AddOrUpdate(name, 1, (_, count) => count + 1);
			}
		}

		private static void FirstChanceException(object sender, FirstChanceExceptionEventArgs e)
		{
			var name = e.Exception.GetType().Name;
			lock (FirstChanceLock)
			{
				_runningFirstChanceExceptionCounts.AddOrUpdate(name, 1, (_, count) => count + 1);
			}
		}

		private static void TryRefreshDriveData()
		{
			try
			{
				DriveInfo[] allDrives = DriveInfo.GetDrives();

				foreach (DriveInfo d in allDrives)
				{
					try
					{
						if (d.IsReady)
						{
							var tags = new HashSet<string>() { $"name:{d.Name}", $"type:{d.DriveType}", $"label:{d.VolumeLabel}", $"format:{d.DriveFormat}" };
							AddCustomMetric($"drive.space.available", d.AvailableFreeSpace, tags);
							AddCustomMetric($"drive.space.total", d.TotalFreeSpace, tags);
							AddCustomMetric($"drive.size.total", d.TotalSize, tags);
						}
					}
					catch (Exception ex)
					{
						AddCustomMetric($"drive.checker.failure", 1, new HashSet<string> { $"message:{ex.Message}", $"ex:{ex.GetType().Name}", $"drive:{d.Name}" });
					}
				}
			}
			catch (Exception ex)
			{
				AddCustomMetric($"drive.refresh.unavailable", 1, new HashSet<string> { $"message:{ex.Message}", $"ex:{ex.GetType().Name}" });
			}
		}
	}
}