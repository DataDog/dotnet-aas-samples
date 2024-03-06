using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;

namespace Datadog.Integrations.Core.AzureServiceBus
{
	/// <summary>
	/// https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-get-started-with-queues
	/// </summary>
	public class AzureServiceBus
	{
		public static IntegrationRunner<ProcessMessageEventArgs, ProcessErrorEventArgs> IntegrationRunner = new IntegrationRunner<ProcessMessageEventArgs, ProcessErrorEventArgs>();

		private static readonly string ConnectionString = Configuration.AzureServiceBus.ConnectionString;
		private static readonly string QueueName = Configuration.AzureServiceBus.QueueName;

		static AzureServiceBus()
		{
			// All integrations to run go here
			DapperHandler.Init();
		}

		public static async Task Send(ServiceBusMessage message)
		{
			await using ServiceBusClient client = new ServiceBusClient(ConnectionString);
			await using ServiceBusSender sender = client.CreateSender(QueueName);
			await sender.SendMessageAsync(message);
		}

		public static async Task SendBatch(ICollection<ServiceBusMessage> messages)
		{
			await using ServiceBusClient client = new ServiceBusClient(ConnectionString);
			await using ServiceBusSender sender = client.CreateSender(QueueName);

			// start a new batch 
			using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

			int messagesToSend = messages.Count;

			while (messagesToSend > 0)
			{
				foreach (var message in messages)
				{
					if (!messageBatch.TryAddMessage(message))
					{
						break;
					}

					messagesToSend--;
				}

				await sender.SendMessagesAsync(messageBatch);
			}
		}

		public static async Task WatchQueues(CancellationToken cancellationToken)
		{
			await using (ServiceBusClient client = new ServiceBusClient(ConnectionString))
			{
				// create a processor that we can use to process the messages
				ServiceBusProcessor processor = client.CreateProcessor(QueueName, new ServiceBusProcessorOptions());

				// add handler to process messages
				processor.ProcessMessageAsync += HandleMessage;

				// add handler to process any errors
				processor.ProcessErrorAsync += HandleError;

				// start processing 
				await processor.StartProcessingAsync(cancellationToken);

				while (!cancellationToken.IsCancellationRequested)
				{
					await Task.Delay(100, cancellationToken);
				}

				await processor.StopProcessingAsync(cancellationToken);
			}
		}

		private static async Task HandleMessage(ProcessMessageEventArgs args)
		{
			await IntegrationRunner.Run(args);
		}

		private static async Task HandleError(ProcessErrorEventArgs args)
		{
			await IntegrationRunner.HandleError(args);
		}
	}
}
