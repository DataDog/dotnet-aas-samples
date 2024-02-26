using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;

namespace Datadog.Metrics.Management
{
	public static class ProcessHelpers
	{
		public static HashSet<int> AccessDeniedProcessIds = new HashSet<int>();
		public static HashSet<string> AccessDeniedProcessNames = new HashSet<string>();

		public static ProcessMetrics CurrentProcessMetrics = null;

		public static readonly ConcurrentDictionary<int, ProcessMetrics> MonitoredProcesses =
			new ConcurrentDictionary<int, ProcessMetrics>();

		public static readonly ConcurrentQueue<ProcessMetrics> StatSetsToSend = new ConcurrentQueue<ProcessMetrics>();

		public static void CheckAppDomain()
		{
			try
			{
				var domain = AppDomain.CurrentDomain;
				AppDomainEventHelper.MonitorAppDomain(domain);
			}
			catch
			{
				RuntimeMetricsTracker.AddCustomMetric($"process.appdomain.check", 1,
					new HashSet<string> { $"status:failure", $"id:0", $"name:error" });
			}
		}

		public static void RefreshProcessRuntimeMetrics()
		{
			var thisProcess = Process.GetCurrentProcess();

			if (CurrentProcessMetrics == null)
			{
				CurrentProcessMetrics = new ProcessMetrics(thisProcess);
			}
			else
			{
				CurrentProcessMetrics.RefreshIntervalData(thisProcess);
			}

			StatSetsToSend.Enqueue(CurrentProcessMetrics);

			var currentMonitors = MonitoredProcesses.Values;

			foreach (var processMetrics in currentMonitors)
			{
				processMetrics.RefreshIntervalData(thisProcess);
				StatSetsToSend.Enqueue(processMetrics);

				if (processMetrics.ExitCode != null)
				{
					MonitoredProcesses.TryRemove(processMetrics.ProcessId, out _);
				}
			}

			// Monitor as much as we can, why not?
			// var processes = Process.GetProcesses();

			var processes = new List<Process>();

			processes.AddRange(Process.GetProcessesByName("dotnet"));
			processes.AddRange(Process.GetProcessesByName("dogstatsd"));
			processes.AddRange(Process.GetProcessesByName("datadog-trace-agent"));

			foreach (var process in processes)
			{
				if (process.Id == CurrentProcessMetrics.ProcessId)
				{
					continue;
				}

				if (AccessDeniedProcessIds.Contains(process.Id))
				{
					continue;
				}

				if (AccessDeniedProcessNames.Contains(process.ProcessName))
				{
					continue;
				}

				try
				{

					if (MonitoredProcesses.ContainsKey(process.Id))
					{
						// already handled, skip it
						continue;
					}

					var processMetrics = new ProcessMetrics(process);
					StatSetsToSend.Enqueue(processMetrics);
				}
				catch
				{
					try
					{
						AccessDeniedProcessIds.Add(process.Id);
						AccessDeniedProcessNames.Add(process.ProcessName);
					}
					catch (Exception ex)
					{
						RuntimeMetricsTracker.AddCustomMetric($"process.monitor.failure", 1, new HashSet<string> { $"message:{ex.Message}", $"ex:{ex.GetType().Name}" });
					}
				}
			}
		}
	}

	public class ProcessMetrics
	{
		public ProcessMetrics(Process process)
		{
			ProcessId = process.Id;
			ProcessName = process.ProcessName;
			RefreshIntervalData(process);
		}

		public int ProcessId { get; }
		public string ProcessName { get; }
		public int? ExitCode { get; set; }
		public long Ticks { get; set; }

		public TimeSpan IntervalTotalProcessorTime { get; set; }
		public TimeSpan IntervalUserProcessorTime { get; set; }
		public TimeSpan IntervalSystemProcessorTime { get; set; }

		public TimeSpan UserProcessorTime { get; set; }
		public TimeSpan SystemCpuTime { get; set; }

		public long ThreadCount { get; set; }
		public long PrivateMemorySize { get; set; }

		public void RefreshIntervalData(Process process)
		{
			try
			{
				if (process.HasExited)
				{
					try
					{
						ExitCode = process.ExitCode;
					}
					catch
					{
						ExitCode = 42;
					}
				}

				// For some reason refresh doesn't work all the time for external processes
				// If our CPU stats are zero, try to re-grab the process by Id.
				// Also, Get by PID doesn't work, we can't see CPU, so use name because for some reason that works
				if (process.UserProcessorTime <= TimeSpan.Zero && process.PrivilegedProcessorTime <= TimeSpan.Zero)
				{
					var processes = Process.GetProcessesByName(ProcessName);

					if (processes.Length == 0)
					{
						ExitCode = 42;
					}
					else
					{
						process = processes[0];
					}
				}

				ThreadCount = process.Threads.Count;
				PrivateMemorySize = process.PrivateMemorySize64;

				var newUserProcessorTime = process.UserProcessorTime;
				var newSystemCpuTime = process.PrivilegedProcessorTime;

				IntervalUserProcessorTime = newUserProcessorTime - UserProcessorTime;
				IntervalSystemProcessorTime = newSystemCpuTime - SystemCpuTime;
				IntervalTotalProcessorTime = IntervalSystemProcessorTime + IntervalUserProcessorTime;

				// Set for the next interval calculation
				UserProcessorTime = newUserProcessorTime;
				SystemCpuTime = newSystemCpuTime;

			}
			catch (Exception ex)
			{
				RuntimeMetricsTracker.AddCustomMetric($"process.monitor.refresh.failure", 1, new HashSet<string> { $"message:{ex.Message}", $"pid:{ProcessId}", $"process_name:{ProcessName}", $"name:{ex.GetType().Name}" });
				ProcessHelpers.AccessDeniedProcessIds.Add(ProcessId);
				ProcessHelpers.AccessDeniedProcessNames.Add(ProcessName);
				ProcessHelpers.MonitoredProcesses.TryRemove(ProcessId, out _);
			}
		}
	}
}
