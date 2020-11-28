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
			public TimeSpan CompilerTime { get; set; }
			public TimeSpan CompiledRenderTime { get; set; }
			public TimeSpan TotalTime { get; set; }

			private static string MakeHeaderField(
				IList<ModelPerformanceCounterEntity> all, 
				Func<ModelPerformanceCounterEntity, string> prop,
				string fieldName)
			{
				try
				{
					var maxValue = Math.Max(all.Select(prop).Select(f => f.Length).Max() - fieldName.Length, 0);
					var spaces = "";
					if (maxValue > 0)
					{
						spaces = Enumerable.Repeat(" ", maxValue).Aggregate((e, f) => e + f);
					}
					return $"{fieldName}{spaces}";
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					throw;
				}
			}
			private static string MakeHeaderFieldSeperator(
				IList<ModelPerformanceCounterEntity> all, 
				Func<ModelPerformanceCounterEntity, string> prop,
				string fieldName)
			{
				var maxValue = Math.Max(all.Select(prop).Select(f => f.Length).Max(), fieldName.Length) + 2;
				return $"{Enumerable.Repeat("-", maxValue).Aggregate((e, f) => e + f)}";
			}

			public static string Header(string delimiter, IList<ModelPerformanceCounterEntity> all)
			{
				return
					$"{delimiter} {MakeHeaderField(all, f => f.Name, "Variation")} {delimiter} " +
					$"{MakeHeaderField(all, f => f.Name, "Time/Run")} {delimiter} " +
					$"{MakeHeaderField(all, f => f.TimePerRun.ToString("c"), "Runs")} {delimiter} " +
					$"{MakeHeaderField(all, f => f.RunOver.ToString(), "Model Depth")} {delimiter} " +
					$"{MakeHeaderField(all, f => f.ModelDepth.ToString(), "SubstitutionCount")} {delimiter} " +
					$"{MakeHeaderField(all, f => f.SubstitutionCount.ToString(), "Template Size(byte)")} {delimiter} " +
					$"{MakeHeaderField(all, f => f.TemplateSize.ToString(), "TokenizingTime")} {delimiter} " +
					$"{MakeHeaderField(all, f => f.TokenizingTime.ToString("c"), "ParseTime")} {delimiter} " +
					$"{MakeHeaderField(all, f => f.RenderTime.ToString("c"), "RenderTime")} {delimiter} " +
					$"{MakeHeaderField(all, f => f.CompilerTime.ToString("c"), "CompilerTime")} {delimiter} " +
					$"{MakeHeaderField(all, f => f.CompiledRenderTime.ToString("c"), "CompiledRenderTime")} {delimiter} " +
					$"{MakeHeaderField(all, f => f.TotalTime.ToString("c"), "Total Time")} {delimiter}" +
					$"\r\n" +
					$"{delimiter}{MakeHeaderFieldSeperator(all, f => f.Name, "Variation")}{delimiter}" +
					$"{MakeHeaderFieldSeperator(all, f => f.Name, "Time/Run")}{delimiter}" +
					$"{MakeHeaderFieldSeperator(all, f => f.TimePerRun.ToString("c"), "Runs")}{delimiter}" +
					$"{MakeHeaderFieldSeperator(all, f => f.RunOver.ToString(), "Model Depth")}{delimiter}" +
					$"{MakeHeaderFieldSeperator(all, f => f.ModelDepth.ToString(), "SubstitutionCount")}{delimiter}" +
					$"{MakeHeaderFieldSeperator(all, f => f.SubstitutionCount.ToString(), "Template Size(byte)")}{delimiter}" +
					$"{MakeHeaderFieldSeperator(all, f => f.TemplateSize.ToString(), "TokenizingTime")}{delimiter}" +
					$"{MakeHeaderFieldSeperator(all, f => f.TokenizingTime.ToString("c"), "ParseTime")}{delimiter}" +
					$"{MakeHeaderFieldSeperator(all, f => f.RenderTime.ToString("c"), "RenderTime")}{delimiter}" +
					$"{MakeHeaderFieldSeperator(all, f => f.CompilerTime.ToString("c"), "CompilerTime")}{delimiter}" +
					$"{MakeHeaderFieldSeperator(all, f => f.CompiledRenderTime.ToString("c"), "CompiledRenderTime")}{delimiter}" +
					$"{MakeHeaderFieldSeperator(all, f => f.TotalTime.ToString("c"), "Total Time")}{delimiter}" 
					;
			}

			public string PrintAsCsv(string delimiter)
			{
				return
					$"{delimiter} {Name} {delimiter} " +
					$"{TimePerRun:c} {delimiter} " +
					$"{RunOver} {delimiter} " +
					$"{ModelDepth} {delimiter} " +
					$"{SubstitutionCount} {delimiter} " +
					$"{TemplateSize} {delimiter} " +
					$"{TokenizingTime:c} {delimiter} " +
					$"{ParseTime:c} {delimiter} " +
					$"{RenderTime:c} {delimiter} " +
					$"{CompilerTime:c} {delimiter} " +
					$"{CompiledRenderTime:c} {delimiter} " +
					$"{TotalTime:c} {delimiter}";
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
			var delimiter = "|";
			var perfCounter = PerformanceCounters.OfType<ModelPerformanceCounterEntity>().ToArray();
			if (perfCounter.Any())
			{
				output.AppendLine(ModelPerformanceCounterEntity.Header(delimiter, perfCounter));
				foreach (var performanceCounter in perfCounter)
				{
					output.AppendLine(performanceCounter.PrintAsCsv(delimiter));
				}
			}
			var expPerCounter = PerformanceCounters.OfType<ExpressionPerformanceCounterEntity>();

			if (expPerCounter.Any())
			{
				
				output.AppendLine(ExpressionPerformanceCounterEntity.Header(delimiter));
				foreach (var performanceCounter in expPerCounter)
				{
					output.AppendLine("| " + performanceCounter.PrintAsCsv(delimiter) + "|");
				}
			}

			Console.WriteLine(output.ToString());
			//TestContext.Progress.WriteLine(output.ToString());
			File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\MorestachioPerf.md", output.ToString());
		}
	}
}