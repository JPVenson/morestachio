using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Morestachio.Document;
using Morestachio.Framework.IO.SingleStream;
using Morestachio.Rendering;

namespace Morestachio.Benchmark.PerfRendering
{
	public abstract class BenchPerfHarness
	{
		private IRenderer _templateCompiled;
		private object _data;
		
		public virtual async Task Setup()
		{
			_templateCompiled = (await ParserOptionsBuilder.New()
															.WithTemplate(GetTemplate)
															.WithDisableContentEscaping(true)
															.BuildAndParseAsync())
				.CreateCompiledRenderer(new DocumentCompiler());
			_data = GetData();

			var bench = await Bench();
			//Console.WriteLine(bench);
			if (string.IsNullOrWhiteSpace(bench))
			{
				throw new InvalidOperationException("Result does not equal expected value");
			}
		}
		
		public virtual async ValueTask<string> Bench()
		{
			//var output = new ByteCounterTextWriter(new StringWriter(CultureInfo.InvariantCulture), _templateCompiled.ParserOptions);
			var output = new SharedByteCounterStringBuilder(_templateCompiled.ParserOptions);
			await _templateCompiled.RenderAsync(_data, CancellationToken.None, output);
			return output.ToString();
		}

		public const string Lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum";

		public abstract object GetData();

		public virtual string GetTemplate()
		{
			return @"<ul id='products'>
  {{#EACH Products}}
	<li>
	  <h2>{{Name}}</h2>
		   Only {{Price}}
		   {{Description.Truncate(15)}}
	</li>
  {{/EACH}}
</ul>";
		}
	}
}