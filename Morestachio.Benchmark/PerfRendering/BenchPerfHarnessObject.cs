using System;
using System.Collections.Generic;

namespace Morestachio.Benchmark.PerfRendering
{
	public class BenchPerfHarnessObject : BenchPerfHarness
	{
		public override object GetData()
		{
			const int ProductCount = 500;

			var items = new List<object>();
			var lorem = Lorem.AsMemory();
			for (int i = 0; i < ProductCount; i++)
			{
				items.Add(new
				{
					Name = "Name" + i,
					Price = i,
					Description = lorem
				});
			}

			return new
			{
				Products = items
			};
		}
	}
}