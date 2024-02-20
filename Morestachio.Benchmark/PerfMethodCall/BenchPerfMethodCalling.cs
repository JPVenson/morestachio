using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace Morestachio.Benchmark.PerfMethodCall
{
	public class BenchPerfMethodCalling
	{
		public BenchPerfMethodCalling()
		{
			TargetCaller = new TargetCaller();
		}

		public TargetCaller TargetCaller { get; set; }

		public MethodInfo TargetMember { get; set; }
		public MethodInfo TargetMemberTypeInfo { get; set; }

		[GlobalSetup]
		public void Setup()
		{
			TargetMember = typeof(TargetCaller).GetMethod(nameof(TargetCaller.CallTarget));
			TargetMemberTypeInfo = typeof(TargetCaller).GetTypeInfo().GetMethod(nameof(TargetCaller.CallTarget));
			TargetMemberDelegate = TargetMember.CreateDelegate<Func<int, string, string>>(TargetCaller);
			CompiledExpression = BuildExpression(TargetMember);
		}

		private Func<object, object[], object> BuildExpression(MethodInfo method)
		{
			var argsParam = Expression.Parameter(typeof(object[]), "args");
			var instParam = Expression.Parameter(typeof(object), "instance");

			var parameterInfos = method.GetParameters();

			Expression body = Expression.Call(Expression.Convert(instParam, method.DeclaringType), method,
				parameterInfos.Select((parameterInfo, index) =>
					Expression.Convert(Expression.ArrayAccess(argsParam, Expression.Constant(index)),
						parameterInfo.ParameterType)));

			if (body.CanReduce)
			{
				body = body.Reduce();
			}

			return Expression.Lambda<Func<object, object[], object>>(body, true, instParam, argsParam).Compile();
		}

		public Func<int, string, string> TargetMemberDelegate { get; set; }
		public Func<object, object[], object> CompiledExpression { get; set; }


		[Benchmark(Baseline = true, Description = "Cs Direct")]
		public void CallDirectly()
		{
			var a = TargetCaller.CallTarget(24, "Half Truth");
		}

		[Benchmark(Baseline = false, Description = "Reflection")]
		public void CallViaReflection()
		{
			var a = TargetMember.Invoke(TargetCaller, new object[] { 24, "Half Truth" });
		}

		[Benchmark(Baseline = false, Description = "ReflectionGetTypeInfo")]
		public void CallViaReflectionGetTypeInfo()
		{
			var a = TargetMemberTypeInfo.Invoke(TargetCaller, new object[] { 24, "Half Truth" });
		}

		[Benchmark(Baseline = false, Description = "MethodInfo.CreateDelegate")]
		public void CallViaReflectionDelegate()
		{
			var a = TargetMemberDelegate.Invoke(24, "Half Truth");
		}

		[Benchmark(Baseline = false, Description = "CallViaExpression")]
		public void CallViaPointer()
		{
			var a = CompiledExpression(TargetCaller, new object[] { 24, "Half Truth" });
		}
	}

	public class TargetCaller
	{
		public string CallTarget(int a, string b)
		{
			return a + b;
		}
	}
}