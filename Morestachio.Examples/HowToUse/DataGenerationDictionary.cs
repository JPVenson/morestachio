using System.Collections.Generic;
using Morestachio.Example.Base;

namespace Morestachio.Examples.HowToUse
{
	public class DataGenerationDictionary : MorestachioExampleBase
	{
		//there must be always a method in the Program class that will be called to obtain the data
		public override object GetData()
		{
			var model = new Dictionary<string, object>();
			model["name"] = "John";
			model["sender"] = "Sally";
			return model;
		}
	}
}