using System.Net.Http;
using System.Threading.Tasks;

namespace Datadog.Integrations.Core.SqlServer.HttpClient
{
	public class HttpClientIntegration
	{
		public static readonly int Id = IntegrationHelper.ClaimIntegrationId();

		private static readonly System.Net.Http.HttpClient HttpClient = new System.Net.Http.HttpClient();

		public static async Task<HttpResponseMessage> GetAsync(string uri)
		{
			return await HttpClient.GetAsync(uri);
		}

		public static async Task<HttpResponseMessage> PostAsync(string uri)
		{
			return await HttpClient.PostAsync(uri, new StringContent("Hello"));
		}
	}
}
