using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Morestachio.Formatter.Framework;

namespace Morestachio.Examples.CustomFormatterExample
{
	public static class DataGeneration
	{
		//this method can be used to obtain a new ParserOptions
		//here you could add custom formatters
		//this method is optional you can safely remove it
		public static ParserOptions Config(string templateText, Encoding encoding, bool shouldEscape)
		{
			var options = new ParserOptions(templateText, null, encoding, shouldEscape);
			options.Formatters.AddFromType(typeof(DataGeneration));
			options.Timeout = TimeSpan.FromMinutes(1);
			return options;
		}

		[MorestachioGlobalFormatter("HttpGet", "Gets an string value from the url")]
		public static async Task<string> GetHttpValue(string source)
		{
			var httpClient = new HttpClient();
			return await httpClient.GetStringAsync(source);
		}


		//there must be always a method in the Program class that will be called to obtain the data
		public static object GetData()
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