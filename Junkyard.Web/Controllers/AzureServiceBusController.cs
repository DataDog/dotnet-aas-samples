using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Datadog.Integrations.Core.AzureServiceBus;

namespace Junkyard.Web.Controllers
{
	public class AzureServiceBusController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public AzureServiceBusController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		public async Task<string> HelloWorld()
		{
			await AzureServiceBus.Send(HelloWorldMesage());
			var message = "Added a Hello World message";
			_logger.LogInformation(message);
			return message;
		}

		public async Task<string> HelloWorldBatch(int? batchSize = null)
		{
			batchSize ??= 3;
			var batch = new List<ServiceBusMessage>();
			while (batchSize-- > 0)
			{
				batch.Add(HelloWorldMesage());
			}
			await AzureServiceBus.SendBatch(batch);
			var message = $"Added {batchSize} Hello World messages";
			_logger.LogInformation(message);
			return message;
		}

		private ServiceBusMessage HelloWorldMesage()
		{
			return new ServiceBusMessage($"[{DateTime.Now.ToUniversalTime().Ticks}] [{Guid.NewGuid()}] Hello World");
		}
	}
}
