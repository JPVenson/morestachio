using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using NUnit.Framework;
using MemberExpression = System.Linq.Expressions.MemberExpression;

namespace Morestachio.Tests.PerfTests
{
	[SetUpFixture]
	public class PerformanceCounter
	{
		public interface IPerformanceCounterEntity
		{
			string PrintAsCsv(string delimiter, 
				IDictionary<string, int> fieldSizes);
		}

		public class ModelPerformanceCounterEntity : IPerformanceCounterEntity
		{
			public ModelPerformanceCounterEntity(string name)
			{
				Name = name;
			}

			[DisplayName("Variation")]
			public string Name { get; private set; }
			[DisplayName("Time/Run")]
			public TimeSpan TimePerRun { get; set; }
			[DisplayName("Runs")]
			public int RunOver { get; set; }
			[DisplayName("Model Depth")]
			public int ModelDepth { get; set; }
			[DisplayName("SubstitutionCount")]
			public int SubstitutionCount { get; set; }
			[DisplayName("Template Size(byte)")]
			public int TemplateSize { get; set; }
			[DisplayName("TokenizingTime")]
			public TimeSpan TokenizingTime { get; set; }
			[DisplayName("ParseTime")]
			public TimeSpan ParseTime { get; set; }
			[DisplayName("RenderTime")]
			public TimeSpan RenderTime { get; set; }
			[DisplayName("CompilerTime")]
			public TimeSpan CompilerTime { get; set; }
			[DisplayName("CompiledRenderTime")]
			public TimeSpan CompiledRenderTime { get; set; }
			[DisplayName("Total Time")]
			public TimeSpan TotalTime { get; set; }

			private static MemberInfo GetMemberName(Expression expression)
			{
				if (expression == null)
				{
					throw new ArgumentException("");
				}
				if (expression is MemberExpression memberExpression)
				{
					// Reference type property or field
					return memberExpression.Member;
				}
				if (expression is MethodCallExpression methodCall)
				{
					return GetMemberName(methodCall.Object);
				}
				if (expression is UnaryExpression)
				{
					// Property, field of method returning value type
					var unaryExpression = (UnaryExpression)expression;
					return GetMemberName(unaryExpression);
				}
				throw new ArgumentException("");
			}

			private static MemberInfo GetMemberName(UnaryExpression unaryExpression)
			{
				if (unaryExpression.Operand is MethodCallExpression)
				{
					var methodExpression = (MethodCallExpression)unaryExpression.Operand;
					throw new ArgumentException("");
					//return methodExpression.Method.Name;
				}
				return ((MemberExpression)unaryExpression.Operand).Member;
			}

			private static string MakeHeaderField(IEnumerable<ModelPerformanceCounterEntity> all,
				Expression<Func<ModelPerformanceCounterEntity, string>> prop,
				IDictionary<string, int> fieldSizes)
			{
				try
				{
					var fieldName = GetMemberName(prop.Body).GetCustomAttribute<DisplayNameAttribute>().DisplayName;
					var maxValue = Math.Max(all.Select(prop.Compile()).Select(f => f.Length).Max() - fieldName.Length, 0);
					var spaces = "";
					if (maxValue > 0)
					{
						spaces = Enumerable.Repeat(" ", maxValue).Aggregate((e, f) => e + f);
					}

					fieldSizes[fieldName] = maxValue + fieldName.Length;
					return $"{fieldName}{spaces}";
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					throw;
				}
			}

			private static string MakeValueField(
				ModelPerformanceCounterEntity entity,
				Expression<Func<ModelPerformanceCounterEntity, string>> prop,
				IDictionary<string, int> fieldSizes)
			{
				try
				{
					var fieldName = GetMemberName(prop.Body).GetCustomAttribute<DisplayNameAttribute>().DisplayName;
					var value = prop.Compile()(entity);
					var maxValue = fieldSizes[fieldName] - value.Length;
					var spaces = "";
					if (maxValue > 0)
					{
						spaces = Enumerable.Repeat(" ", maxValue).Aggregate((e, f) => e + f);
					}
					return $"{value}{spaces}";
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
					throw;
				}
			}

			private static string MakeHeaderFieldSeperator(IEnumerable<ModelPerformanceCounterEntity> all,
				Expression<Func<ModelPerformanceCounterEntity, string>> prop)
			{
				var fieldName = GetMemberName(prop.Body).GetCustomAttribute<DisplayNameAttribute>().DisplayName;
				var maxValue = Math.Max(all.Select(prop.Compile()).Select(f => f.Length).Max(), fieldName.Length) + 2;
				return $"{Enumerable.Repeat("-", maxValue).Aggregate((e, f) => e + f)}";
			}

			public static string Header(string delimiter, IList<ModelPerformanceCounterEntity> all, out IDictionary<string, int> fieldSizes)
			{
				fieldSizes = new Dictionary<string, int>();
				return
					$"{delimiter} {MakeHeaderField(all, f => f.Name, fieldSizes)} {delimiter} " +
					$"{MakeHeaderField(all, f => f.TimePerRun.ToString("c"), fieldSizes)} {delimiter} " +
					$"{MakeHeaderField(all, f => f.RunOver.ToString(), fieldSizes)} {delimiter} " +
					$"{MakeHeaderField(all, f => f.ModelDepth.ToString(), fieldSizes)} {delimiter} " +
					$"{MakeHeaderField(all, f => f.SubstitutionCount.ToString(), fieldSizes)} {delimiter} " +
					$"{MakeHeaderField(all, f => f.TemplateSize.ToString(), fieldSizes)} {delimiter} " +
					$"{MakeHeaderField(all, f => f.TokenizingTime.ToString("c"), fieldSizes)} {delimiter} " +
					$"{MakeHeaderField(all, f => f.ParseTime.ToString("c"), fieldSizes)} {delimiter} " +
					$"{MakeHeaderField(all, f => f.RenderTime.ToString("c"), fieldSizes)} {delimiter} " +
					$"{MakeHeaderField(all, f => f.CompilerTime.ToString("c"), fieldSizes)} {delimiter} " +
					$"{MakeHeaderField(all, f => f.CompiledRenderTime.ToString("c"), fieldSizes)} {delimiter} " +
					$"{MakeHeaderField(all, f => f.TotalTime.ToString("c"), fieldSizes)} {delimiter}" +
					$"\r\n" +
					$"{delimiter}{MakeHeaderFieldSeperator(all, f => f.Name)}{delimiter}" +
					$"{MakeHeaderFieldSeperator(all, f => f.TimePerRun.ToString("c"))}{delimiter}" +
					$"{MakeHeaderFieldSeperator(all, f => f.RunOver.ToString())}{delimiter}" +
					$"{MakeHeaderFieldSeperator(all, f => f.ModelDepth.ToString())}{delimiter}" +
					$"{MakeHeaderFieldSeperator(all, f => f.SubstitutionCount.ToString())}{delimiter}" +
					$"{MakeHeaderFieldSeperator(all, f => f.TemplateSize.ToString())}{delimiter}" +
					$"{MakeHeaderFieldSeperator(all, f => f.TokenizingTime.ToString("c"))}{delimiter}" +
					$"{MakeHeaderFieldSeperator(all, f => f.ParseTime.ToString("c"))}{delimiter}" +
					$"{MakeHeaderFieldSeperator(all, f => f.RenderTime.ToString("c"))}{delimiter}" +
					$"{MakeHeaderFieldSeperator(all, f => f.CompilerTime.ToString("c"))}{delimiter}" +
					$"{MakeHeaderFieldSeperator(all, f => f.CompiledRenderTime.ToString("c"))}{delimiter}" +
					$"{MakeHeaderFieldSeperator(all, f => f.TotalTime.ToString("c"))}{delimiter}"
					;
			}

			public string PrintAsCsv(string delimiter,
				IDictionary<string, int> fieldSizes)
			{
				return
					$"{delimiter} " +
					$"{MakeValueField(this, e => e.Name, fieldSizes)} {delimiter} " +
					$"{MakeValueField(this, e => e.TimePerRun.ToString("c"), fieldSizes)} {delimiter} " +
					$"{MakeValueField(this, e => e.RunOver.ToString(), fieldSizes)} {delimiter} " +
					$"{MakeValueField(this, e => e.ModelDepth.ToString(), fieldSizes)} {delimiter} " +
					$"{MakeValueField(this, e => e.SubstitutionCount.ToString(), fieldSizes)} {delimiter} " +
					$"{MakeValueField(this, e => e.TemplateSize.ToString(), fieldSizes)} {delimiter} " +
					$"{MakeValueField(this, e => e.TokenizingTime.ToString("c"), fieldSizes)} {delimiter} " +
					$"{MakeValueField(this, e => e.ParseTime.ToString("c"), fieldSizes)} {delimiter} " +
					$"{MakeValueField(this, e => e.RenderTime.ToString("c"), fieldSizes)} {delimiter} " +
					$"{MakeValueField(this, e => e.CompilerTime.ToString("c"), fieldSizes)} {delimiter} " +
					$"{MakeValueField(this, e => e.CompiledRenderTime.ToString("c"), fieldSizes)} {delimiter} " +
					$"{MakeValueField(this, e => e.TotalTime.ToString("c"), fieldSizes)} {delimiter} ";
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

			public string PrintAsCsv(string delimiter,
				IDictionary<string, int> fieldSizes)
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
			output.AppendLine("## Test Net" + Environment.Version + "   ");
			output.AppendLine("> Run at " + DateTime.Today.ToString("g") + "   ");
#if DEBUG
			output.AppendLine("WARNING - RUN IN DEBUG - WARNING   ");
#endif
			//Console.WriteLine(
			//	"Variation: '{8}', Time/Run: {7}ms, Runs: {0}x, Model Depth: {1}, SubstitutionCount: {2}," +
			//	" Template Size: {3}, ParseTime: {4}, RenderTime: {5}, Total Time: {6}",
			//	runs, modelDepth, inserts, sizeOfTemplate, parseTime.Elapsed, renderTime.Elapsed, totalTime.Elapsed,
			//	totalTime.ElapsedMilliseconds / (double) runs, variation);
			var delimiter = "|";
			var perfCounter = PerformanceCounters.OfType<ModelPerformanceCounterEntity>().ToArray();
			if (perfCounter.Any())
			{
				output.AppendLine(ModelPerformanceCounterEntity.Header(delimiter, perfCounter, out var fieldSizes));
				foreach (var performanceCounter in perfCounter)
				{
					output.AppendLine(performanceCounter.PrintAsCsv(delimiter, fieldSizes));
				}
			}
			var expPerCounter = PerformanceCounters.OfType<ExpressionPerformanceCounterEntity>();

			if (expPerCounter.Any())
			{

				output.AppendLine(ExpressionPerformanceCounterEntity.Header(delimiter));
				foreach (var performanceCounter in expPerCounter)
				{
					output.AppendLine("| " + performanceCounter.PrintAsCsv(delimiter, null) + "|");
				}
			}

			Console.WriteLine(output.ToString());
			//TestContext.Progress.WriteLine(output.ToString());
			File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory) + @"\MorestachioPerf.md", output.ToString());
		}
	}
}