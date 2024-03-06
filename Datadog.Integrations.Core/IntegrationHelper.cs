using System.Threading;

namespace Datadog.Integrations.Core
{
	public class IntegrationHelper
	{
		private static int _integrationIdSeed;

		public static int ClaimIntegrationId()
		{
			return Interlocked.Increment(ref _integrationIdSeed);
		}
	}
}