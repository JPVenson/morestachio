using System.Dynamic;
using Morestachio.Example.Base;

namespace Morestachio.Examples.CodeProjectExamples.UsingTheCode
{
	public class DataGenerationDynamic : MorestachioExampleBase
	{
		//there must be always a method in the Program class that will be called to obtain the data
		public override object GetData()
		{
			dynamic data = new ExpandoObject();
			data.name = "John";
			data.sender = "Sally";
			return data;
		}
	}
}