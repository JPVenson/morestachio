using Morestachio.Example.Base;

namespace Morestachio.Examples.CodeProjectExamples.UsingTheCode
{
	public class DataGenerationCustomObject : MorestachioExampleBase
	{
		public class SenderAndReceiver
		{
			public string Name { get; set; }
			public string Sender { get; set; }
		}

		//there must be always a method in the Program class that will be called to obtain the data
		public override object GetData()
		{
			var data = new SenderAndReceiver();
			data.Name = "John";
			data.Sender = "Sally";
			return data;
		}
	}
}