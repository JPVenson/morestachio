using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Morestachio.Tests.PerfTests
{
	[SetUpFixture]
	public class PerformanceCounter
	{
		public interface IPerformanceCounterEntity
		{
			string PrintAsCsv(string delimiter);
		}

		public class ModelPerformanceCounterEntity : IPerformanceCounterEntity
		{
			public ModelPerformanceCounterEntity(string name)
			{
				Name = name;
			}

			public string Name { get; private set; }
			public TimeSpan TimePerRun { get; set; }
			public int RunOver { get; set; }
			public int ModelDepth { get; set; }
			public int SubstitutionCount { get; set; }
			public int TemplateSize { get; set; }
			public TimeSpan TokenizingTime { get; set; }
			public TimeSpan ParseTime { get; set; }
			public TimeSpan RenderTime { get; set; }
			public TimeSpan TotalTime { get; set; }

			public static string Header(string delimiter)
			{
				return
					$"Variation{delimiter} " +
					$"Time/Run{delimiter} " +
					$"Runs{delimiter} " +
					$"Model Depth{delimiter} " +
					$"SubstitutionCount{delimiter} " +
					$"Template Size(byte){delimiter} " +
					$"TokenizingTime{delimiter} " +
					$"ParseTime{delimiter} " +
					$"RenderTime{delimiter} " +
					$"Total Time ";
			}

			public string PrintAsCsv(string delimiter)
			{
				return
					$"{Name}{delimiter}" +
					$"{TimePerRun:c}{delimiter}" +
					$"{RunOver}{delimiter}" +
					$"{ModelDepth}{delimiter}" +
					$"{SubstitutionCount}{delimiter}" +
					$"{TemplateSize}{delimiter}" +
					$"{TokenizingTime:c}{delimiter}" +
					$"{ParseTime:c}{delimiter}" +
					$"{RenderTime:c}{delimiter}" +
					$"{TotalTime:c}";
			}
		}
		public class ExpressionPerformanceCounterEntity : IPerformanceCounterEntity
		{
			public ExpressionPerformanceCounterEntity(string name)
			{
				Name = name;
			}

			public string Name { get; private set; }
			public TimeSpan TimePerRun { get; set; }
			public int RunOver { get; set; }
			public int Width { get; set; }
			public int Depth { get; set; }
			public int NoArguments { get; set; }
			public TimeSpan ParseTime { get; set; }
			public TimeSpan ExecuteTime { get; set; }
			public TimeSpan TotalTime { get; set; }

			public static string Header(string delimiter)
			{
				return
					$"Variation{delimiter} Time/Run{delimiter} Runs{delimiter} Width{delimiter} Depth{delimiter} NoArguments{delimiter} ParseTime{delimiter} ExecuteTime{delimiter} Total Time";
			}

			public string PrintAsCsv(string delimiter)
			{
				return
					$"{Name}{delimiter} {TimePerRun:c}{delimiter} {RunOver}{delimiter} {Width}{delimiter} {Depth}{delimiter} {NoArguments}{delimiter} {ParseTime:c}{delimiter} {ExecuteTime:c}{delimiter} {TotalTime:c}";
			}
		}

		public static ICollection<IPerformanceCounterEntity> PerformanceCounters { get; private set; }

		[OneTimeSetUp]
		public void PerfStart()
		{
			PerformanceCounters = new List<IPerformanceCounterEntity>();
		}

		[OneTimeTearDown]
		public void PrintPerfCounter()
		{
			var output = new StringBuilder();
			//Console.WriteLine(
			//	"Variation: '{8}', Time/Run: {7}ms, Runs: {0}x, Model Depth: {1}, SubstitutionCount: {2}," +
			//	" Template Size: {3}, ParseTime: {4}, RenderTime: {5}, Total Time: {6}",
			//	runs, modelDepth, inserts, sizeOfTemplate, parseTime.Elapsed, renderTime.Elapsed, totalTime.Elapsed,
			//	totalTime.ElapsedMilliseconds / (double) runs, variation);
			var delimiter = " | ";
			output.AppendLine(ModelPerformanceCounterEntity.Header(delimiter));
			foreach (var performanceCounter in PerformanceCounters.OfType<ModelPerformanceCounterEntity>())
			{
				output.AppendLine("| " + performanceCounter.PrintAsCsv(delimiter) + "|");
			}
			output.AppendLine(ExpressionPerformanceCounterEntity.Header(delimiter));
			foreach (var performanceCounter in PerformanceCounters.OfType<ExpressionPerformanceCounterEntity>())
			{
				output.AppendLine("| " + performanceCounter.PrintAsCsv(delimiter) + "|");
			}

			Console.WriteLine(output.ToString());
			//TestContext.Progress.WriteLine(output.ToString());
			File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\MorestachioPerf.md", output.ToString());
		}
	}
}