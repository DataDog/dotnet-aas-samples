#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Datadog.Metrics.Management;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using StatsdClient;

namespace JunkyardLoad
{
    public static class JunkyardLoad
    {
        private const string StatsPrefix = "perf.junkyard";
        private const string LoadTestInterval = "*/1 * * * * *";
        private const string MetricsInterval = "*/10 * * * * *";
        private const string SecurityAppUrlPrefix = "dd-dotnet-security-aspnetcore";
        private const string SecurityService = "security-aspnetcore7";

        private static DogStatsdService _statsService;

        [FunctionName("metrics-aggregator")]
        public static async Task JunkyardNetcore31Metrics([TimerTrigger(MetricsInterval)] TimerInfo myTimer, ILogger log)
        {
            await GetMetrics("dd-netcore31-junkyard-baseline", service: "netcore31-baseline", log);
            await GetMetrics("dd-netcore31-junkyard", service: "netcore31-full", log);
            await GetMetrics("dd-dotnet-latest-build", service: "netcore31-latest-build", log);
            await GetMetrics("dd-dotnet-latest-build-stats", service: "netcore31-latest-build-stats", log);
            await GetMetrics("dd-netcore31-calltarget-full", service: "netcore31-calltarget-full", log);
            await GetMetrics("dd-dotnet-latest-build-profiler-only", service: "netcore31-latest-build-profiler-only", log);
            await GetMetrics("dd-dotnet-linux-baseline", service: "net8-linux-baseline", log);
            await GetMetrics("dd-dotnet-linux-latest-build", service: "net8-linux-latest-build", log);
            await GetMetrics("dd-dotnet-linux-latest-build-stats", service: "net8-linux-latest-build-stats", log);
            await GetMetrics("dd-dotnet-linux-latest-build-profiler-default", service: "net8-linux-latest-build-profiler-default", log);
            await GetMetrics("dd-dotnet-linux-latest-build-profiler-all", service: "net8-linux-latest-build-profiler-all", log);
            await GetMetrics("dd-dotnet-linux-latest-build-profiler-all", service: "net8-linux-latest-build-profiler-all", log);
            await GetMetrics(SecurityAppUrlPrefix, SecurityService, log);
        }

        [FunctionName("dd-netcore31-calltarget-full")]
        public static async Task JunkyardNetcore31CallTargetFull([TimerTrigger(LoadTestInterval)] TimerInfo myTimer, ILogger log)
        {
            await JunkyardDump("dd-netcore31-calltarget-full", log: log, service: "netcore31-calltarget-full");
        }

        [FunctionName("dd-netcore31-junkyard")]
        public static async Task JunkyardNetcore31([TimerTrigger(LoadTestInterval)] TimerInfo myTimer, ILogger log)
        {
            await JunkyardDump("dd-netcore31-junkyard", log: log, service: "netcore31-full");
        }

        [FunctionName("dd-netcore31-junkyard-baseline")]
        public static async Task JunkyardNetcore31Baseline([TimerTrigger(LoadTestInterval)] TimerInfo myTimer, ILogger log)
        {
            await JunkyardDump("dd-netcore31-junkyard-baseline", log: log, service: "netcore31-baseline");
        }

        [FunctionName("dd-netcore31-junkyard-dev")]
        public static async Task JunkyardNetcore31Dev([TimerTrigger(LoadTestInterval)] TimerInfo myTimer, ILogger log)
        {
            await JunkyardDump("dd-dotnet-latest-build", log: log, service: "netcore31-latest-build");
        }

        [FunctionName("dd-netcore31-junkyard-latest-build-stats")]
        public static async Task JunkyardNetcore31DevStats([TimerTrigger(LoadTestInterval)] TimerInfo myTimer, ILogger log)
        {
            await JunkyardDump("dd-dotnet-latest-build-stats", log: log, service: "netcore31-latest-build-stats");
        }

        [FunctionName("dd-netcore31-junkyard-latest-build-profiler-only")]
        public static async Task JunkyardNetcore31DevProfilerOnly([TimerTrigger(LoadTestInterval)] TimerInfo myTimer, ILogger log)
        {
            await JunkyardDump("dd-dotnet-latest-build-profiler-only", log: log, service: "netcore31-latest-build-profiler-only");
        }

        [FunctionName("dd-dotnet-profiler-backend-test-latest-build")]
        public static async Task JunkyardNetcore31DevProfilerBackendTest([TimerTrigger(LoadTestInterval)] TimerInfo myTimer, ILogger log)
        {
            await JunkyardDump("dd-dotnet-profiler-backend-test-latest-build", log: log, service: "dd-dotnet-profiler-backend-test-latest-build");
        }

        [FunctionName("dd-net8-linux-junkyard-baseline")]
        public static async Task JunkyardNet8LinuxDevBaseline([TimerTrigger(LoadTestInterval)] TimerInfo myTimer, ILogger log)
        {
            await JunkyardDump("dd-dotnet-linux-baseline", log: log, service: "net8-linux-baseline");
        }

        [FunctionName("dd-net8-linux-junkyard-dev")]
        public static async Task JunkyardNet8LinuxDev([TimerTrigger(LoadTestInterval)] TimerInfo myTimer, ILogger log)
        {
            await JunkyardDump("dd-dotnet-linux-latest-build", log: log, service: "net8-linux-latest-build");
        }

        [FunctionName("dd-net8-linux-junkyard-latest-build-stats")]
        public static async Task JunkyardNet8LinuxDevStats([TimerTrigger(LoadTestInterval)] TimerInfo myTimer, ILogger log)
        {
            await JunkyardDump("dd-dotnet-linux-latest-build-stats", log: log, service: "net8-linux-latest-build-stats");
        }

        [FunctionName("dd-net8-linux-junkyard-latest-build-profiler-default")]
        public static async Task JunkyardNet8LinuxDevProfilerDefault([TimerTrigger(LoadTestInterval)] TimerInfo myTimer, ILogger log)
        {
            await JunkyardDump("dd-dotnet-linux-latest-build-profiler-default", log: log, service: "net8-linux-latest-build-profiler-default");
        }

        [FunctionName("dd-net8-linux-junkyard-latest-build-profiler-all")]
        public static async Task JunkyardNet8LinuxDevProfilerAll([TimerTrigger(LoadTestInterval)] TimerInfo myTimer, ILogger log)
        {
            await JunkyardDump("dd-dotnet-linux-latest-build-profiler-all", log: log, service: "net8-linux-latest-build-profiler-all");
        }

        [FunctionName("dd-dotnet-security-aspnetcore")]
        public static async Task JunkyardSecuritySampleAspNetCore([TimerTrigger(LoadTestInterval)] TimerInfo myTimer, ILogger log)
        {
            await JunkyardDump(SecurityAppUrlPrefix, log: log, service: SecurityService);
        }

        private static DogStatsdService GetStatsService()
        {
            if (_statsService == null)
            {
                var env = Environment.GetEnvironmentVariable("DD_ENV") ?? "aas-junkyard";

                _statsService = new DogStatsdService();
                _statsService.Configure(new StatsdConfig() { ConstantTags = new[] { $"env:{env}" } });
            }

            return _statsService;
        }

        private static async Task JunkyardDump(string appUrlPrefix, ILogger log, string service, IEnumerable<EndpointTestProfile>? endpointTestProfiles = null)
        {
            var statsService = GetStatsService();
            var tags = new[] { $"app:{service ?? appUrlPrefix}", $"appUrlPrefix:{appUrlPrefix}" };
            try
            {
                statsService.Increment($"{StatsPrefix}.function.call", tags: tags);

                var allTasks = new List<Task>();
                using var httpClient = GetClient(appUrlPrefix);
                foreach (var endpointProfile in endpointTestProfiles ?? JunkyardLoadTest.EndpointProfiles)
                {
                    allTasks.Add(RunEndpointProfile(httpClient, statsService, appUrlPrefix, endpointProfile, service));
                }

                await Task.WhenAll(allTasks).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // ignore
                var tagsList = tags.ToList();
                tagsList.Add(ex.ToString());
                statsService.Increment($"{StatsPrefix}.function.failure", tags: tagsList.ToArray());
                log?.LogError(ex, "Function failed to run.");
            }
            finally
            {
                statsService.Flush();
            }
        }

        private static async Task RunEndpointProfile(HttpClient httpClient, DogStatsdService statsService, string appUrlPrefix, EndpointTestProfile profile, string service)
        {
            var tags = new[] { $"app:{service ?? appUrlPrefix}", $"appUrlPrefix:{appUrlPrefix}" };
            var batches = profile.BatchesPerRun;
            var allTasks = new List<Task>();

            while (batches-- > 0)
            {
                var delay = Task.Delay(profile.TimeBetweenBatches);

                using (statsService.StartTimer($"{StatsPrefix}.batch", tags: tags))
                {
                    var batchSize = profile.RequestsPerBatch;

                    while (batchSize-- > 0)
                    {
                        allTasks.Add(Task.Run(async () =>
                        {
                            statsService.Increment($"{StatsPrefix}.{profile.StatPrefix}.count", tags: tags);
                            using (statsService.StartTimer($"{StatsPrefix}.{profile.StatPrefix}", tags: tags))
                            {
                                if (profile.Headers is not null)
                                {
                                    foreach (var header in profile.Headers)
                                    {
                                        httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                                    }
                                }
                                if (profile.RequestMethod == HttpMethod.Get)
                                {
                                    await httpClient.GetStringAsync(profile.Uri).ConfigureAwait(false);
                                }
                                else
                                {
                                }
                            }
                        }));
                    }

                    await delay.ConfigureAwait(false);
                }
            }

            await Task.WhenAll(allTasks);
        }

        private static async Task GetMetrics(string appUrlPrefix, string service = null, ILogger log = null)
        {
            var statsService = GetStatsService();
            try
            {
                var baseTags = new HashSet<string> { $"app:{service ?? appUrlPrefix}", $"appUrlPrefix:{appUrlPrefix}" };
                using var httpClient = GetClient(appUrlPrefix);
                var metricsResponse = await httpClient.GetAsync("/home/metrics").ConfigureAwait(false);
                var metrics = await metricsResponse.Content.ReadAsAsync<List<FakeMetric>>();
                foreach (var metric in metrics)
                {
                    metric.Tags.UnionWith(baseTags);
                    statsService.Gauge($"{StatsPrefix}.{metric.Name}", value: metric.Value, tags: metric.Tags.ToArray());
                }
            }
            catch (Exception ex)
            {
                log?.LogError(ex, "Exception throw while getting metrics");
            }
            finally
            {
                statsService.Flush();
            }
        }

        private static HttpClient GetClient(string app)
        {
            var uriText = $"https://{app}.azurewebsites.net/";
            var uri = new Uri(uriText);

            var httpClient = new HttpClient()
            {
                BaseAddress = uri,
            };

            httpClient.DefaultRequestHeaders.CacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };

            return httpClient;
        }
    }
}
