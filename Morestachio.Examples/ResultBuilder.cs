using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Morestachio.Parsing.ParserErrors;
using Morestachio.Rendering;

namespace Morestachio.Example.Base
{
	public static class ResultBuilder
	{
		public const string RESULT_TIMES = "RESULT_TIMES";
		public const string RESULT_NAME = "COMPILED_RESULT";
		public const string ERRORS_NAME = "ERRORS";
		public const string JSONTEXT_NAME = "JSON";
		public const string XMLTEXT_NAME = "XML";
	}

	public partial class MorestachioExampleBase
	{
		private IDictionary<string, object> FailResultBuilder(params IMorestachioError[] errors)
		{
			var result = new Dictionary<string, object>
			{
				//[ResultBuilder.ERRORS_NAME] = JsonConvert.SerializeObject(errors)
			};
			return result;
		}

		private IDictionary<string, object> OkResult(string result, string jsonResult, string xmlResult, KeyValuePair<string, TimeSpan>[] times)
		{
			var resultBuilder = new Dictionary<string, object>();
			resultBuilder[ResultBuilder.RESULT_NAME] = result;
			resultBuilder[ResultBuilder.JSONTEXT_NAME] = jsonResult;
			resultBuilder[ResultBuilder.XMLTEXT_NAME] = xmlResult;
			resultBuilder[ResultBuilder.RESULT_TIMES] = times;
			return resultBuilder;
		}

		public IDictionary<string, object> Run(string templateText, Encoding encoding, bool shouldEscape)
		{
			return (RunCore(templateText, encoding, shouldEscape)).GetAwaiter().GetResult();
		}

		public async Task<IDictionary<string, object>> RunCore(string templateText, Encoding encoding, bool shouldEscape)
		{
			var times = new Dictionary<string, TimeSpan>();

			T Evaluate<T>(Func<T> fnc, string name)
			{
				var sw = Stopwatch.StartNew();

				try
				{
					return fnc();
				}
				finally
				{
					sw.Stop();
					times[name] = sw.Elapsed;
				}
			}

			async Task<T> EvaluateAsync<T>(Func<Task<T>> fnc, string name)
			{
				var sw = Stopwatch.StartNew();

				try
				{
					return await fnc();
				}
				finally
				{
					sw.Stop();	
					times[name] = sw.Elapsed;
				}
			}

			var options = Evaluate(() => Configurate(templateText, encoding, shouldEscape), "Configurate");
			var documentInfo = await EvaluateAsync(async () => await Parse(options), "Parse");

			if (documentInfo.Errors.Any())
			{
				return FailResultBuilder(documentInfo.Errors.ToArray());
			}

			var data = Evaluate(GetData, "GetData");
			var result = await EvaluateAsync(async () =>
			{
				return (await documentInfo.CreateRenderer().RenderAndStringifyAsync(data, CancellationToken.None));
			}, "Render");

			var jsonResult = Evaluate(() => SerializeToJsonText(documentInfo.Document), "Json Serialization");
			var xmlResult = Evaluate(() => SerializeToXmlText(documentInfo.Document), "Xml Serialization");

			return OkResult(result, jsonResult, xmlResult, times.ToArray());
		}

		
	}
}