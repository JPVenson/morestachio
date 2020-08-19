using System;
using System.Text;

namespace Morestachio.Examples.HelloWorld
{
	public static class DataGeneration
	{
		//this method can be used to obtain a new ParserOptions
		//here you could add custom formatters
		//this method is optional you can safely remove it
		public static ParserOptions Config(string templateText, Encoding encoding, bool shouldEscape)
		{
			var options = new ParserOptions(templateText, null, encoding, shouldEscape);
			options.Timeout = TimeSpan.FromMinutes(1);
			return options;
		}


		//there must be always a method in the Program class that will be called to obtain the data
		public static object GetData()
		{
			var data = new {
				Data = new {
					Value = "Hello World"
				}
			};

			return data;
		}
	}
}
