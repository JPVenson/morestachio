using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Framework.Attributes;

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
			options.Encoding = Encoding.UTF8;
			return options;
		}

		[MorestachioFormatter("ToBase64", "")]
		public static string ToBase64(byte[] data){
			return Convert.ToBase64String(data);
		}

		[MorestachioGlobalFormatter("HttpGet", "Gets an string value from the url")]
		public static async Task<byte[]> GetHttpValue(string source)
		{
			var httpClient = new HttpClient();
			return await httpClient.GetByteArrayAsync(source);
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