using System.Collections.Generic;

namespace JunkyardLoad
{
	public static class JunkyardLoadTest
	{
		public static List<EndpointTestProfile> EndpointProfiles = new List<EndpointTestProfile>() {
			EndpointTestProfile.Home,
			EndpointTestProfile.AzureServiceBusSend,
			EndpointTestProfile.DapperFailure,
			EndpointTestProfile.DapperRun
		};
	}
}