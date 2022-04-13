using System.Collections.Generic;
using Morestachio.Example.Base;

namespace Morestachio.Examples.CodeProjectExamples.NestedObjects
{
	public class DataGeneration : MorestachioExampleBase
	{
		//there must be always a method in the Program class that will be called to obtain the data
		public override object GetData()
		{
			var subData = new Dictionary<string, object>();
			subData["name"] = "John"; 
			subData["sender"] = "Sally";

			var data = new Dictionary<string, object>();
			data["other"] = subData;
			return data;
		}
	}
}