using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Datadog.Integrations.Core.SqlServer.Dapper;

namespace Datadog.Integrations.Core.AzureServiceBus
{
	public class DapperHandler
	{
		public static void Init()
		{
			AzureServiceBus.IntegrationRunner.RegisterHandler(DapperIntegration.Id, HandleMessage, HandleError);
		}

		public static void Remove()
		{
			AzureServiceBus.IntegrationRunner.RemoveHandlers(DapperIntegration.Id);
		}

		private static async Task HandleMessage(ProcessMessageEventArgs args)
		{
			await DapperIntegration.RunAsync();
		}

		private static async Task HandleError(ProcessErrorEventArgs args)
		{
			// no-op
			await Task.Delay(1);
		}
	}
}
