using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Example.Base;

namespace Morestachio.Examples.CustomFormatterExample
{
	public class DataGeneration : MorestachioExampleBase
	{
		//this method can be used to obtain a new ParserOptions
		//here you could add custom formatters
		//this method is optional you can safely remove it
		public override ParserOptions Configure(
			string templateText,
			Encoding encoding,
			bool shouldEscape,
			IServiceProvider serviceProvider
		)
		{
			var options = ParserOptionsBuilder.New()
											.WithTemplate(templateText)
											.WithEncoding(encoding)
											.WithDisableContentEscaping(shouldEscape)
											.WithTimeout(TimeSpan.FromSeconds(5))
											.WithServiceProvider(serviceProvider)
											.WithFormatters<DataGeneration>();

			return options.Build();
		}

		[MorestachioFormatter("ToBase64", "")]
		public static string ToBase64(byte[] data)
		{
			return Convert.ToBase64String(data);
		}

		[MorestachioGlobalFormatter("HttpGet", "Gets an string value from the url")]
		public static async Task<byte[]> GetHttpValue(string source, [ExternalData] HttpClient httpClient)
		{
			return await httpClient.GetByteArrayAsync(source);
		}

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