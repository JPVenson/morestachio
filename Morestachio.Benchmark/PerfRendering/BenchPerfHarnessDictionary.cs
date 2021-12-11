using System;
using System.Collections.Generic;

namespace Morestachio.Benchmark.PerfRendering
{
	public class BenchPerfHarnessDictionary : BenchPerfHarness
	{

		public override object GetData()
		{
			var productsDict = new Dictionary<string, object>();
			const int ProductCount = 500;

			List<Dictionary<string, object>> prodList = new List<Dictionary<string, object>>();
			productsDict.Add("Products", prodList);

			var lorem = Lorem.AsMemory();
			for (int i = 0; i < ProductCount; i++)
			{
				prodList.Add(new Dictionary<string, object>()
				{
					{ "Name", "Name" + i },
					{ "Price", i },
					{ "Description", lorem },
				});
			}

			return productsDict;
		}
	}
}