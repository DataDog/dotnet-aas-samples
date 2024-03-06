using System.Collections.Generic;

namespace Datadog.Metrics.Management
{
	public class FakeMetric
	{
		public FakeMetric()
		{
			Tags = new HashSet<string>();
		}

		public FakeMetric(HashSet<string> tags)
		{
			Tags = new HashSet<string>();
			Tags.UnionWith(tags);
		}

		public string Name { get; set; }

		public double Value { get; set; }

		public HashSet<string> Tags { get; }

		public bool ShouldAggregate { get; set; }
	}
}