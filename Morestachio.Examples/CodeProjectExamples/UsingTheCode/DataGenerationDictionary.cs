using System.Collections.Generic;
using Morestachio.Example.Base;

namespace Morestachio.Examples.CodeProjectExamples.UsingTheCode
{
	public class DataGenerationDictionary : MorestachioExampleBase
	{
		//there must be always a method in the Program class that will be called to obtain the data
		public override object GetData()
		{
			var data = new Dictionary<string, object>();
			data["name"] = "John";
			data["sender"] = "Sally";
			return data;
		}
	}
}