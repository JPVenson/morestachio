using System.Collections.Generic;
using Morestachio.Example.Base;

namespace Morestachio.Examples.EachExample
{
	public class DataGeneration : MorestachioExampleBase
	{
		//there must be always a method in the Program class that will be called to obtain the data
		public override object GetData()
		{
			var collection = new List<object>();

			collection.Add("Hello");
			collection.Add("World");

			var data = new
			{
				Data = collection
			};

			return data;
		}
	}
}