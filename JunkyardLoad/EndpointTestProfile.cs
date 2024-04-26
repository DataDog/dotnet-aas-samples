#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net.Http;

namespace JunkyardLoad
{
    public class EndpointTestProfile
    {
        public string Uri { get; set; }
        public string? StatPrefix { get; set; }
        public TimeSpan TimeBetweenBatches { get; set; }
        public int RequestsPerBatch { get; set; }
        public int BatchesPerRun { get; set; }
        public HttpMethod RequestMethod { get; set; } = HttpMethod.Get;
        public string? ContentType { get; internal set; }
        public string? Body { get; internal set; }
        public Dictionary<string, string>? Headers { get; internal set; }

        public static EndpointTestProfile Home = new EndpointTestProfile()
        {
            TimeBetweenBatches = TimeSpan.FromMilliseconds(95),
            BatchesPerRun = 9,
            RequestsPerBatch = 1,
            StatPrefix = "home.index",
            Uri = "/",
        };

        public static EndpointTestProfile AzureServiceBusSend = new EndpointTestProfile()
        {
            TimeBetweenBatches = TimeSpan.FromMilliseconds(450),
            BatchesPerRun = 2,
            RequestsPerBatch = 1,
            StatPrefix = "asb.send",
            Uri = "/AzureServiceBus/HelloWorld",
        };

        public static EndpointTestProfile DapperRun = new EndpointTestProfile()
        {
            TimeBetweenBatches = TimeSpan.FromMilliseconds(120),
            BatchesPerRun = 8,
            RequestsPerBatch = 1,
            StatPrefix = "dapper.run",
            Uri = "/Dapper/Run",
        };

        public static EndpointTestProfile DapperFailure = new EndpointTestProfile()
        {
            TimeBetweenBatches = TimeSpan.FromMilliseconds(120),
            BatchesPerRun = 8,
            RequestsPerBatch = 1,
            StatPrefix = "dapper.failure",
            Uri = "/Dapper/Failure",
        };
    }
}