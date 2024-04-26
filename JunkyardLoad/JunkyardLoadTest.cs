using System;
using System.Collections.Generic;

namespace JunkyardLoad;

public static class JunkyardLoadTest
{
    public static List<EndpointTestProfile> EndpointProfiles = new List<EndpointTestProfile>() {
            EndpointTestProfile.Home,
            EndpointTestProfile.AzureServiceBusSend,
            EndpointTestProfile.DapperFailure,
            EndpointTestProfile.DapperRun
        };

    public static List<EndpointTestProfile> EndpointSecurityAspNetCoreProfiles = new List<EndpointTestProfile>() {
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                RequestsPerBatch = 2,
                StatPrefix = "Home.Index",
                Uri = "/",
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                RequestsPerBatch = 1,
                StatPrefix = "Health.PathParams",
                Uri = "/health/params/appscan_fingerprint",
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                RequestsPerBatch = 1,
                StatPrefix = "Health.PathParams",
                Uri = "/health/params/normal-id",
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                RequestsPerBatch = 1,
                StatPrefix = "Health.Query",
                Uri = "/health?[$slice]=value",
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                RequestsPerBatch = 1,
                StatPrefix = "Health.Query",
                Uri = "/health?normal=value",
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                RequestsPerBatch = 1,
                StatPrefix = "Health.PathParamsQuery",
                Uri = "/health/params/appscan_fingerprint?[$slice]=value",
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                RequestsPerBatch = 1,
                StatPrefix = "Health.DiscoveryScans",
                Uri = "/health/login.php",
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                RequestsPerBatch = 1,
                StatPrefix = "Data.BodyForm",
                ContentType = "application/x-www-form-urlencoded",
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Uri = "/data/model",
                Body = "property=test&property2=dummy_rule&property3=18"
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                RequestsPerBatch = 1,
                StatPrefix = "Data.BodyForm",
                ContentType = "application/json",
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Uri = "/dataapi/model",
                Body = "{\"property\":\"dummy_rule\", \"property2\":\"test2\", , \"property3\":18 }"
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                RequestsPerBatch = 1,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                StatPrefix = "Data.RazorPage",
                ContentType = "application/x-www-form-urlencoded",
                Uri = "/datarazorpage",
                Body = "property=test&property2=dummy_rule&property3=18"
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                RequestsPerBatch = 1,
                StatPrefix = "Home.LangHeader",
                Uri = "/home/langheader",
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                RequestsPerBatch = 1,
                StatPrefix = "Home.ExternalWafHeaders",
                Uri = "/?test=external-waf-headers",
                Headers = new Dictionary<string, string> {
                    { "X-SigSci-Tags", "SQLI" },
                    { "X-Amzn-Trace-Id", "Test" },
                    { "X-Cloud-Trace-Context", "Test" }
                }
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                RequestsPerBatch = 1,
                StatPrefix = "Login.Success",
                Uri = "/account/index",
                RequestMethod = System.Net.Http.HttpMethod.Post,
                ContentType = "application/x-www-form-urlencoded",
                Body = "Input.UserName=TestUser&Input.Password=test"
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 1,
                RequestsPerBatch = 1,
                StatPrefix = "Login.Logout",
                Uri = "/account/logout"
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                RequestsPerBatch = 1,
                StatPrefix = "Login.Failure",
                Uri = "/account/index",
                RequestMethod = System.Net.Http.HttpMethod.Post,
                ContentType = "application/x-www-form-urlencoded",
                Body = "Input.UserName=TestUser&Input.Password=bad"
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                RequestsPerBatch = 1,
                StatPrefix = "Login.Sdk",
                Uri = "/user/index"
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                RequestsPerBatch = 2,
                StatPrefix = "Array",
                Uri = "/dataapi/array?model=test&model=test2"
            }, 
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                RequestsPerBatch = 2,
                StatPrefix = "Iast",
                Uri = "/iast/hardcodedSecrets"
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                RequestsPerBatch = 2,
                StatPrefix = "Iast",
                Uri = "/iast/weakhashing"
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                RequestsPerBatch = 2,
                StatPrefix = "Iast",
                Uri = "/Iast/SqlQuery?query=SELECT%20Surname%20from%20Persons%20where%20name%20=%20%27Vicent%27"
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                RequestsPerBatch = 2,
                StatPrefix = "Iast",
                Uri = "/Iast/ExecuteCommand?file=nonexisting.exe&argumentLine=arg1"
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                RequestsPerBatch = 2,
                StatPrefix = "Iast",
                Uri = "/Iast/GetFileContent?file=nonexisting.txt"
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                RequestsPerBatch = 2,
                StatPrefix = "Iast",
                Uri = "/Iast/GetFileContent?file=nonexisting.txt"
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                StatPrefix = "Iast",
                RequestsPerBatch = 2,
                RequestMethod = System.Net.Http.HttpMethod.Post,
                Body = "{ 'query': 'test' }",
                Uri = "/Iast/ExecuteQueryFromBodyText"
            },
            new()
            {
                TimeBetweenBatches = TimeSpan.FromMilliseconds(100),
                BatchesPerRun = 10,
                StatPrefix = "Iast",
                RequestsPerBatch = 2,
                Uri = "/Iast/GetDirectoryContent?directory=bin"
            },
        };
}