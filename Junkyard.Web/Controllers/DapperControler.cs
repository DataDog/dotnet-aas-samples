using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Datadog.Integrations.Core.SqlServer.Dapper;

namespace Junkyard.Web.Controllers
{
	public class DapperController : Controller
	{
		private readonly ILogger<DapperController> _logger;

		public DapperController(ILogger<DapperController> logger)
		{
			_logger = logger;
		}

		public async Task<string> Run()
		{
			await DapperIntegration.RunAsync();
			var message = "Dapper integration executed.";
			_logger.LogInformation(message);
			return message;
		}

		public async Task<string> Failure()
		{
			var message = "Dapper integration intentional failure.";
			_logger.LogError(message);
			await DapperIntegration.IntentionalFailure();
			return message;

		}
	}
}
