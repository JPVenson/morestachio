using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Morestachio.Formatter.Framework;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Morestachio.Tests
{
	[TestFixture(Category = "Playground")]
	public class PlaygroundTests
	{
		public PlaygroundTests()
		{

		}

		public void Generic<T>(IEnumerable<T> value)
		{
			Console.WriteLine(value);
		}

		[Test]
		public void GenericsTest()
		{
			var methodInfo = typeof(PlaygroundTests).GetMethod(nameof(Generic));

			Array value = new int[1];
			var values = new Dictionary<string, object>()
			{
				{ "value", value },
			};
			var endValues = values.Values;
			methodInfo = MorestachioFormatterService.MakeGenericMethodInfoByValues(methodInfo, values);
			
			var parameters = endValues.ToArray();
			methodInfo.Invoke(this, parameters);
		}
	}
}

//void SetGenerics(Type target, Type source)
//{
//	var targetGenerics = target.GetGenericArguments();
//	var sourceGenerics = source.GetGenericArguments();

//	for (var index = 0; index < targetGenerics.Length; index++)
//	{
//		var targetGeneric = targetGenerics[index];
//		var sourceGeneric = sourceGenerics[index];

//		if (targetGeneric.IsGenericParameter)
//		{
//			generics.Add(sourceGeneric);
//		}
//		else
//		{
//			SetGenerics(targetGeneric, sourceGeneric);
//		}
//	}
//}

//foreach (var parameterInfo in methodInfo.GetParameters())
//{
//	var value = values[parameterInfo.Name];
//	endValues.Add(value);

//	if (parameterInfo.ParameterType.ContainsGenericParameters)
//	{
//		if (parameterInfo.ParameterType.IsGenericParameter)
//		{
//			generics.Add(value.GetType());
//		}
//		else
//		{
//			var targetGenerics = parameterInfo.ParameterType.GetGenericArguments();
//			var sourceGenerics = value.GetType().GetGenericArguments();
//			for (var index = 0; index < targetGenerics.Length; index++)
//			{
//				var targetGeneric = targetGenerics[index];
//				var sourceGeneric = sourceGenerics[index];
//				SetGenerics(targetGeneric, sourceGeneric);
//			}
//		}
//	}
//}