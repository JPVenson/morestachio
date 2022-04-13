using Morestachio.Example.Base;

namespace Morestachio.Examples.HowToUse
{
	public class DataGenerationAnonObject : MorestachioExampleBase
	{
		//there must be always a method in the Program class that will be called to obtain the data
		public override object GetData()
		{
			var model = new
			{
				name = "John", 
				sender = "Sally"
			};
			return model;
		}
	}
}
