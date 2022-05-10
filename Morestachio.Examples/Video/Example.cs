using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Morestachio.Formatter.Framework;
using Morestachio.TemplateContainers;

namespace Morestachio.Examples.Video
{
	public static class Example
	{
		public static async Task Main()
		{
			var template = "Hello my name is {{data.name.TrimEx()}}";
			var data = new
			{
				data = new
				{
					name = " Frank "
				}
			};
			//var options = ParserOptionsBuilder.New()
			//								.WithTemplate(template)
			//								.WithEncoding(encoding)
			//								.WithDisableContentEscaping(shouldEscape)
			//								.WithTimeout(TimeSpan.FromSeconds(5));

			//return options.Build();
			//var options = new ParserOptions(new StringTemplateContainer(template));
			//options.Formatters.AddSingle(new Func<string, string>(inputSource => " Sir " + inputSource.Trim()), "TrimEx");
			
			//var documentResult = await Parser.ParseWithOptionsAsync(options);

			//var text = await documentResult.CreateAndStringifyAsync(data);
			//Console.WriteLine(text);
			//Console.ReadKey();
		}
	}
}
