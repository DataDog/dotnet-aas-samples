using System.Threading;
using System.Threading.Tasks;
using Datadog.Integrations.Core;
using Datadog.Integrations.Core.AzureServiceBus;
using Datadog.Metrics.Management;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Junkyard.Web
{
	public class Startup
	{
		private static Task _azureServiceBusTask = null;
		private static readonly CancellationTokenSource ApplicationShutdownToken = new CancellationTokenSource();

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
			_azureServiceBusTask = AzureServiceBus.WatchQueues(ApplicationShutdownToken.Token);
			RuntimeMetricsTracker.Init();
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllersWithViews(options =>
			{
				AspNetCoreConfiguration.ConfigureDynamicOptions(options);
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
			});
		}

		private void OnShutdown()
		{
			ApplicationShutdownToken.Cancel();
			RuntimeMetricsTracker.Shutdown();
		}
	}
}
