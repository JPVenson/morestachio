using System.Collections.Generic;
using Morestachio.Example.Base;

namespace Morestachio.Examples.Partials
{
	public class DataGeneration : MorestachioExampleBase
	{
		//there must be always a method in the Program class that will be called to obtain the data
		public override object GetData()
		{
			var subData = new Dictionary<string, object>();
			subData["otherother"] = "Test";

			var data = new Dictionary<string, object>();
			data["other"] = subData;
			data["navigateUp"] = "Test 2";
			return data;
		}
	}
}