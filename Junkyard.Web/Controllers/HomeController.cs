using Junkyard.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Datadog.Integrations.Core.SqlServer.Dapper;
using Datadog.Metrics.Management;

namespace Junkyard.Web.Controllers
{
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;

		public HomeController(ILogger<HomeController> logger)
		{
			_logger = logger;
		}

		public IActionResult Index()
		{
			_logger.LogInformation("Home page.");
			return View();
		}

		public IActionResult Privacy()
		{
			_logger.LogInformation("Privacy page.");
			return View();
		}

		public List<FakeMetric> Metrics()
		{
			_logger.LogInformation("Retrieving metrics.");
			return RuntimeMetricsTracker.GetMetricsCollection();
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
