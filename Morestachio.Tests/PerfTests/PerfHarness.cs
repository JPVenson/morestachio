using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Morestachio.Framework;
using Morestachio.Framework.Expression;
using NUnit.Framework;

namespace Morestachio.Tests.PerfTests
{
	[TestFixture]
	public class PerfHarness
	{
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
			MorestachioExpression.ParseFrom("data", TokenzierContext.FromText("data"), out _);

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
				morestachioExpression = MorestachioExpression.ParseFrom(expression.Item1, TokenzierContext.FromText(expression.Item1), out _);
			}

			parseTime.Stop();
			var executeTime = Stopwatch.StartNew();
			for (int i = 0; i < runs; i++)
			{
				await morestachioExpression.GetValue(new ContextObject(new ParserOptions(""), ".", null, data)
				{
				}, new ScopeData());
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

			//make sure this class is JIT'd before we start timing.
			Parser.ParseWithOptions(new ParserOptions("asdf"));

			var totalTime = Stopwatch.StartNew();
			var parseTime = Stopwatch.StartNew();
			Stopwatch renderTime;
			for (var i = 0; i < runs; i++)
			{
				template = Parser.ParseWithOptions(new ParserOptions(baseTemplate, () => Stream.Null));
			}

			parseTime.Stop();

			var tmp = template.CreateAndStringifyAsync(model.Item1);

			renderTime = Stopwatch.StartNew();
			for (var i = 0; i < runs; i++)
			{
				var morestachioDocumentResult = await template.CreateAsync(model.Item1);
				using (var f = morestachioDocumentResult.Stream)
				{
				}
			}

			renderTime.Stop();
			totalTime.Stop();

			var modelPerformanceCounterEntity = new PerformanceCounter.ModelPerformanceCounterEntity(variation)
			{
				TimePerRun = new TimeSpan(totalTime.ElapsedTicks / runs),
				RunOver = runs,
				ModelDepth = modelDepth,
				SubstitutionCount = inserts,
				TemplateSize = sizeOfTemplate,
				ParseTime = parseTime.Elapsed,
				RenderTime = renderTime.Elapsed,
				TotalTime = totalTime.Elapsed
			};
			PerformanceCounter.PerformanceCounters.Add(modelPerformanceCounterEntity);
			Console.WriteLine(PerformanceCounter.ModelPerformanceCounterEntity.Header(" | "));
			Console.WriteLine(modelPerformanceCounterEntity.PrintAsCsv(" | "));
		}

		private Tuple<Dictionary<string, object>, string> ConstructModelAndPath(int modelDepth, string path = null)
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