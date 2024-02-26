using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading.Tasks;

namespace Datadog.Integrations.Core
{
	public class IntegrationRunner<THandle, TError>
	{
		private readonly ConcurrentDictionary<int, Func<THandle, Task>> _runners = new ConcurrentDictionary<int, Func<THandle, Task>>();
		private readonly ConcurrentDictionary<int, Func<TError, Task>> _errorHandlers = new ConcurrentDictionary<int, Func<TError, Task>>();

		public void RegisterHandler(
			int integrationId,
			Func<THandle, Task> messageHandler,
			Func<TError, Task> errorHandler)
		{
			_runners.TryAdd(integrationId, messageHandler);
			_errorHandlers.TryAdd(integrationId, errorHandler);
		}

		public void RemoveHandlers(int integrationId)
		{
			_runners.TryRemove(integrationId, out _);
			_errorHandlers.TryRemove(integrationId, out _);
		}

		public async Task Run(THandle arg, int[] integrationIds = null)
		{
			var keys = _runners.Keys;
			foreach (var key in keys)
			{
				if (!integrationIds?.Contains(key) ?? false)
				{
					continue;
				}

				if (_runners.TryGetValue(key, out var handler))
				{
					try
					{
						await handler.Invoke(arg);
					}
					catch (Exception ex)
					{
						MetricsHelper.IntegrationError(handler, ex);
					}
				}
			}
		}

		public async Task HandleError(TError arg, int[] integrationIds = null)
		{
			var keys = _errorHandlers.Keys;
			foreach (var key in keys)
			{
				if (!integrationIds?.Contains(key) ?? false)
				{
					continue;
				}

				if (_errorHandlers.TryGetValue(key, out var handler))
				{
					await handler.Invoke(arg);
				}
			}
		}
	}
}
