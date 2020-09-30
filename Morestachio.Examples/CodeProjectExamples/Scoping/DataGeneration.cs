using System;
using System.Collections.Generic;
using System.Text;

namespace Morestachio.Examples.CodeProjectExamples.Scoping
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
			var subData = new Dictionary<string, object>();
			subData["otherother"] = "Test";

			var data = new Dictionary<string, object>();
			data["other"] = subData;
			data["navigateUp"] = "Test 2";
			return data;
		}
	}
}