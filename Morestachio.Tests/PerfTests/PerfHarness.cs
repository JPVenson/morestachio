using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Morestachio.Document;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper;
using Morestachio.Rendering;
using NUnit.Framework;

namespace Morestachio.Tests.PerfTests
{
	[TestFixture]
	public class PerfHarness
	{
		[Test]
		[Explicit]
		public void TestNumberCalls()
		{
			var sw = new Stopwatch();
			var iterrations = 500_000F;
			var a = 0;
			sw.Start();
			for (int i = 0; i < iterrations; i++)
			{
				a += 1;
			}
			sw.Stop();
			var csAdd = sw.Elapsed;
			sw.Reset();

			Number b = 0;
			Number c = 1;
			sw.Start();
			for (int i = 0; i < iterrations; i++)
			{
				b += c;
			}
			sw.Stop();
			var nrAdd = sw.Elapsed;

			Console.WriteLine($"C# calls took '{csAdd}'({csAdd.Ticks / iterrations}) " +
							  $"and Number took '{nrAdd}'({nrAdd.Ticks / iterrations}) that is " +
							  $"'{(nrAdd.Ticks / (float)csAdd.Ticks) * 100}'% of baseline");
		}

		[Test]
		[Explicit]
		//[Ignore("Performance tests")]
		[TestCase("Expression Width", 10, 1, 0, 30000)]
		[TestCase("Expression Width", 20, 1, 0, 30000)]
		[TestCase("Expression Width", 10, 1, 10, 30000)]
		[TestCase("Expression Width", 20, 1, 10, 30000)]

		[TestCase("Expression Depth", 1, 10, 0, 30000)]
		[TestCase("Expression Depth", 1, 20, 0, 30000)]
		[TestCase("Expression Depth", 1, 10, 10, 30000)]
		[TestCase("Expression Depth", 1, 20, 10, 30000)]

		[TestCase("Expression Depth and Width", 10, 10, 0, 30000)]
		[TestCase("Expression Depth and Width", 10, 20, 0, 30000)]
		[TestCase("Expression Depth and Width", 10, 20, 0, 30000)]
		[TestCase("Expression Depth and Width", 20, 10, 0, 30000)]
		[TestCase("Expression Depth and Width", 20, 20, 0, 30000)]
		public async Task TestExpressionRuns(string variation, int width, int depth, int noArguments, int runs)
		{
			var expression = ConstructExpression("", width, depth, noArguments);
			ExpressionParser.ParseExpression("data", TokenzierContext.FromText("data"));

			var data = new Dictionary<string, object>();
			for (int i = 0; i < width; i++)
			{
				data["data"] = new Dictionary<string, object>();
			}


			var totalTime = Stopwatch.StartNew();
			var parseTime = Stopwatch.StartNew();
			IMorestachioExpression morestachioExpression = null;
			for (var i = 0; i < runs; i++)
			{
				morestachioExpression = ExpressionParser.ParseExpression(expression.Item1,
					TokenzierContext.FromText(expression.Item1));
			}

			parseTime.Stop();
			var executeTime = Stopwatch.StartNew();
			for (int i = 0; i < runs; i++)
			{
				var parserOptions = new ParserOptions("");
				await morestachioExpression.GetValue(new ContextObject(".", null, data)
				{
				}, new ScopeData(parserOptions));
			}
			executeTime.Stop();
			totalTime.Stop();
			PerformanceCounter.PerformanceCounters.Add(new PerformanceCounter.ExpressionPerformanceCounterEntity(variation)
			{
				TimePerRun = new TimeSpan(parseTime.ElapsedTicks / runs),
				RunOver = runs,
				Width = width,
				Depth = depth,
				NoArguments = noArguments,
				ParseTime = parseTime.Elapsed,
				TotalTime = totalTime.Elapsed,
				ExecuteTime = executeTime.Elapsed
			});
		}

		private Tuple<string, Dictionary<string, object>> ConstructExpression(string exp, int width, int depth, int noArguments)
		{
			var widthExp = "";
			var data = new Dictionary<string, object>();
			for (int j = 0; j < width; j++)
			{
				widthExp += ".data";
			}

			exp += widthExp.Trim('.');

			exp += "(";

			var argExp = "";
			for (int j = 0; j < noArguments; j++)
			{
				argExp += ",arg";
			}

			exp += argExp.Trim(',');

			if (depth != 0)
			{
				var constructExpression = ConstructExpression(exp, width, --depth, noArguments);
				exp = constructExpression.Item1;
				data["data"] = constructExpression.Item2;
			}

			exp += ")";
			return new Tuple<string, Dictionary<string, object>>(exp, data);
		}

		[Test()]
		[TestCase("Model Depth", 100, 30000, 10, 5000)]
		public async Task TestTokenizerTime(string variation, int modelDepth, int sizeOfTemplate, int inserts, int runs)
		{
			var model = ConstructModelAndPath(modelDepth);
			var baseTemplate = Enumerable.Range(1, 5)
				.Aggregate("", (seed, current) => seed += " {{" + model.Item2 + "}}\r\n");
			while (baseTemplate.Length <= sizeOfTemplate)
			{
				baseTemplate += model.Item2;
			}

			var tokenizingOptions = new ParserOptions(baseTemplate, () => Stream.Null);
			var tokenizingTime = new Stopwatch();
			var stringMatchingTime = new Stopwatch();

			for (var i = 0; i < runs; i++)
			{
				var tokenzierContext = new TokenzierContext(new List<int>(), tokenizingOptions.CultureInfo);
				stringMatchingTime.Start();
				var tokens = tokenizingOptions.Template.Matches(tokenzierContext).ToArray();
				stringMatchingTime.Stop();
				tokenizingTime.Start();
				var tokenizerResult = await Tokenizer.Tokenize(tokenizingOptions, tokenzierContext, tokens);
				tokenizingTime.Stop();
			}

			Console.WriteLine($"Tokenizing time: {tokenizingTime.Elapsed:c}; Matching time: {stringMatchingTime.Elapsed:c}");
		}

		[Test()]
		[Explicit]
		//[Ignore("Performance tests")]
		//[Category("Explicit")]
		[TestCase("Model Depth", 5, 30000, 10, 5000)]
		[TestCase("Model Depth", 10, 30000, 10, 5000)]
		[TestCase("Model Depth", 100, 30000, 10, 5000)]
		[TestCase("Substitutions", 5, 30000, 10, 5000)]
		[TestCase("Substitutions", 5, 30000, 50, 5000)]
		[TestCase("Substitutions", 5, 30000, 100, 5000)]
		[TestCase("Template Size", 5, 15000, 10, 5000)]
		[TestCase("Template Size", 5, 25000, 10, 5000)]
		[TestCase("Template Size", 5, 30000, 10, 5000)]
		[TestCase("Template Size", 5, 50000, 10, 5000)]
		[TestCase("Template Size", 5, 100000, 10, 5000)]
		public async Task TestRuns(string variation, int modelDepth, int sizeOfTemplate, int inserts, int runs)
		{
			var model = ConstructModelAndPath(modelDepth);
			var baseTemplate = Enumerable.Range(1, 5)
				.Aggregate("", (seed, current) => seed += " {{" + model.Item2 + "}}");
			while (baseTemplate.Length <= sizeOfTemplate)
			{
				baseTemplate += model.Item2 + "\r\n";
			}

			MorestachioDocumentInfo template = null;
			TokenizerResult tokenizerResult = null;

			//make sure this class is JIT'd before we start timing.
			var morestachioDocumentInfo = await Parser.ParseWithOptionsAsync(new ParserOptions("asdf"));
			var docRenderer = morestachioDocumentInfo.CreateRenderer();
			var compRenderer = morestachioDocumentInfo.CreateCompiledRenderer();

			(await docRenderer.RenderAsync(new object())).Stream.Dispose();
			(await compRenderer.RenderAsync(new object())).Stream.Dispose();

			var totalTime = Stopwatch.StartNew();

			var tokenizingOptions = new ParserOptions(baseTemplate, () => Stream.Null);

			var tokenizingTime = new Stopwatch();
			var stringMatchingTime = new Stopwatch();

			for (var i = 0; i < runs; i++)
			{
				var tokenzierContext = new TokenzierContext(new List<int>(), tokenizingOptions.CultureInfo);
				stringMatchingTime.Start();
				var tokens = tokenizingOptions.Template.Matches(tokenzierContext).ToArray();
				stringMatchingTime.Stop();
				tokenizingTime.Start();
				tokenizerResult = await Tokenizer.Tokenize(tokenizingOptions, tokenzierContext, tokens);
				tokenizingTime.Stop();
			}

			var parseTime = Stopwatch.StartNew();
			for (var i = 0; i < runs; i++)
			{
				var options = new ParserOptions(baseTemplate, () => Stream.Null);
				template = new MorestachioDocumentInfo(options, Parser.Parse(tokenizerResult, options));
			}

			parseTime.Stop();

			var renderTime = Stopwatch.StartNew();

			for (var i = 0; i < runs; i++)
			{
				var morestachioDocumentResult = await docRenderer.RenderAsync(model.Item1);
				morestachioDocumentResult.Stream.Dispose();
			}

			renderTime.Stop();
			totalTime.Stop();

			var compileTime = Stopwatch.StartNew();
			for (var i = 0; i < runs; i++)
			{
				template.CreateCompiledRenderer();
			}

			compileTime.Stop();

			var compiledRenderTime = Stopwatch.StartNew();
			for (var i = 0; i < runs; i++)
			{
				var morestachioDocumentResult = await compRenderer.RenderAsync(model.Item1, CancellationToken.None);
				morestachioDocumentResult.Stream.Dispose();
			}

			compiledRenderTime.Stop();

			var modelPerformanceCounterEntity = new PerformanceCounter.ModelPerformanceCounterEntity(variation)
			{
				TimePerRun = new TimeSpan((tokenizingTime.ElapsedTicks / runs) +
										  (parseTime.ElapsedTicks / runs) +
										  (renderTime.ElapsedTicks / runs)),
				RunOver = runs,
				ModelDepth = modelDepth,
				SubstitutionCount = inserts,
				TemplateSize = sizeOfTemplate,
				TokenMatchTime = stringMatchingTime.Elapsed,
				TokenizingTime = tokenizingTime.Elapsed,
				ParseTime = parseTime.Elapsed,
				RenderTime = renderTime.Elapsed,
				TotalTime = totalTime.Elapsed,
				CompilerTime = compileTime.Elapsed,
				CompiledRenderTime = compiledRenderTime.Elapsed
			};
			PerformanceCounter.PerformanceCounters.Add(modelPerformanceCounterEntity);
			//Console.WriteLine(PerformanceCounter.ModelPerformanceCounterEntity.Header(" | "));
			//Console.WriteLine(modelPerformanceCounterEntity.PrintAsCsv(" | "));
		}

		public static Tuple<Dictionary<string, object>, string> ConstructModelAndPath(int modelDepth, string path = null)
		{
			path = "D380C66729254CA2BAECA9ABFF90EA1C";
			var model = new Dictionary<string, object>();

			if (modelDepth > 1)
			{
				var child = ConstructModelAndPath(modelDepth - 1, path);
				model[path] = child.Item1;
				path = path + "." + child.Item2;
			}

			return Tuple.Create(model, path);
		}
	}
}