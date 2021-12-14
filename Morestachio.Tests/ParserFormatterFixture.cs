using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Morestachio.Formatter.Framework;
using Morestachio.Linq;
using Morestachio.Rendering;
using Morestachio.Helper;
using Morestachio.Util;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture]
	[Parallelizable(ParallelScope.All)]
	public class ParserFormatterFixture
	{
		[Test]
		public void TestCanExecuteAsyncFormatter()
		{
			var options = new ParserOptions("{{#each data.OrderBy('it')}}{{this}},{{/each}}", null,
				ParserFixture.DefaultEncoding);
			var collection = new[] { 0, 1, 2, 3, 5, 4, 6, 7 };
			options.Formatters.AddFromType(typeof(DynamicLinq));
			var report = Parser.ParseWithOptions(options).CreateRenderer().Render(new Dictionary<string, object>
			{
				{
					"data", collection
				}
			}).Stream.Stringify(true, ParserFixture.DefaultEncoding);
			Assert.That(report,
				Is.EqualTo(collection.OrderBy(e => e).Select(e => e.ToString()).Aggregate((e, f) => e + "," + f) + ","));
			Console.WriteLine(report);
		}

		public object ToValStr(object source, ReadOnlyMemory<char> var)
		{
			return source.ToString();
		}

#if NET5_0_OR_GREATER
		public static ReadOnlyMemory<char> Truncate(ReadOnlyMemory<char> source, int length, string ellipsis = "...")
		{
			if (source.IsEmpty)
			{
				return ReadOnlyMemory<char>.Empty;
			}
			ellipsis = ellipsis ?? "...";
			int lMinusTruncate = length - ellipsis.Length;
			if (source.Length > length)
			{
				var builder = new ValueStringBuilder(length + ellipsis.Length);
				builder.Append(source[..(lMinusTruncate < 0 ? 0 : lMinusTruncate)].Span);
				builder.Append(ellipsis);
				return builder.ToString().AsMemory();
			}
			return source;
		}

		[Test]
		[Explicit]
		public void BuildCallGate()
		{
			var parameters = new object[] { "HELLO WORLD I AM READ TO ROCK".AsMemory(), 15 };
			var invoke = PrepareFormatterComposingResult.BuildCaller(GetType().GetMethod(nameof(Truncate)), parameters)
				.Invoke(null, parameters);
			Assert.That(invoke, Is.EqualTo("HELLO WORLD ..."));
		}

		private Func<object[], object> BuildGate(object source, MethodInfo info, object[] arguments)
		{
			return null;
			//var vsb = new ValueStringBuilder();

			//vsb.Append($"private object Call{info.Name}(");
			//if (source != null)
			//{
			//	vsb.Append(source.GetType().FullName);
			//	vsb.Append(" ");
			//	vsb.Append("source, ");
			//}
			//vsb.Append("object[] values");
			//vsb.Append("){");
			//vsb.Append(Environment.NewLine);
			//vsb.Append("return ");
			//vsb.Append("source.");
			//vsb.Append(info.Name);
			//vsb.Append("(");

			//for (int i = 0; i < arguments.Length; i++)
			//{
			//	var arg = arguments[i];
			//	if (arg == null)
			//	{
			//		vsb.Append("null");
			//	}
			//	else
			//	{
			//		vsb.Append($"({arg.GetType().FullName})object[{i}]");
			//	}
			//	if (i < arguments.Length - 1)
			//	{
			//		vsb.Append(", ");
			//	}
			//}
			//vsb.Append(");");
			//vsb.Append(Environment.NewLine);
			//vsb.Append("}");

			//var code = vsb.ToString();

			//return null;



			//var parameterTypes = info.GetParameters().Select(f => f.ParameterType).ToArray();
			//var callFormatter = new DynamicMethod(
			//	$"KafkaDynamicMethodHeaders",
			//	MethodAttributes.Public | MethodAttributes.Static | MethodAttributes.HideBySig,
			//	info.CallingConvention,
			//	typeof(object), 
			//	new Type[]
			//	{
			//		typeof(object)
			//	},
			//	info.DeclaringType,
			//	true);
			//var ilGenerator = callFormatter.GetILGenerator();
			//var getValueOfArray = typeof(Array)
			//	.GetMethods()
			//	.Where(e =>
			//	{
			//		var parameterInfos = e.GetParameters();
			//		return parameterInfos.Length == 1 && parameterInfos[0].ParameterType == typeof(int);
			//	})
			//	.First(e => e.Name == nameof(Array.GetValue));

			//for (int i = 0; i < arguments.Length; i++)
			//{
			//	//var arrVariable = ilGenerator.DeclareLocal(typeof(object));
			//	ilGenerator.Emit(OpCodes.Ldc_I4, i);

			//	ilGenerator.EmitCall(OpCodes.Call, getValueOfArray, null);
			//	ilGenerator.Emit(OpCodes.Stloc_S, i);
			//	ilGenerator.Emit(OpCodes.Pop);
			//}

			//for (int i = 0; i < arguments.Length; i++)
			//{
			//	ilGenerator.Emit(OpCodes.Ldloc_S, i);
			//}

			//ilGenerator.EmitCall(OpCodes.Callvirt, info, parameterTypes);
			//ilGenerator.Emit(OpCodes.Ret);
			//return callFormatter.CreateDelegate<Func<object[], object>>();
		}
#endif
	}
}