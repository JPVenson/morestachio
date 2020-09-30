using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Morestachio.Formatter.Framework;

namespace Morestachio.Examples.CodeProjectExamples.UsingTheCode
{
	public static class DataGenerationDynamic
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
			dynamic data = new ExpandoObject();
			data.name = "John";
			data.sender = "Sally";
			return data;
		}
	}
}