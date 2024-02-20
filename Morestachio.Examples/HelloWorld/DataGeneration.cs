using Morestachio.Example.Base;

namespace Morestachio.Examples.HelloWorld
{
	public class DataGeneration : MorestachioExampleBase
	{
		//there must be always a method in the Program class that will be called to obtain the data
		public override object GetData()
		{
			var data = new
			{
				Data = new
				{
					Value = "Hello World"
				}
			};

			return data;
		}
	}
}