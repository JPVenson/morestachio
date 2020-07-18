using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JetBrains.dotMemoryUnit;
using Morestachio.Formatter.Framework;
using Morestachio.Helper;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Morestachio.Tests
{
	[TestFixture(Category = "Playground")]
	[Explicit]
	public class PlaygroundTests
	{
		public PlaygroundTests()
		{

		}

		public void Generic<T>(IEnumerable<T> value)
		{
			Console.WriteLine(value);
		}

		public const string TextTemplateMorestachio = @"
<ul id='products'>
  {{#each Products}}
	<li>
	  <h2>{{Name}}</h2>
		   Only {{Price}}
		   {{Description.Truncate(15)}}
	</li>
  {{/each}}
</ul>
";
		private const string Lorem = "Lorem ipsum dolor sit amet, consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum";

		public class Product
		{
			public Product(string name, float price, string description)
			{
				Name = name;
				Price = price;
				Description = description;
			}

			public string Name { get; set; }

			public float Price { get; set; }

			public string Description { get; set; }
		}

		[Test]
		[Explicit]
		public async Task PerformanceDebuggerTest()
		{
			var _products = new List<IDictionary<string, object>>(500);
			for (int i = 0; i < 500; i++)
			{
				_products.Add(new Dictionary<string, object>()
				{
					{"Name", "Name" + i},
					{"Price", i},
					{"Description", Lorem},
				});
			}

			var parsingOptions = new Morestachio.ParserOptions(TextTemplateMorestachio);
			var parsed = Morestachio.Parser.ParseWithOptions(parsingOptions);
			var andStringifyAsync = await parsed.CreateAndStringifyAsync(new
			{
				Products = _products
			});
			var sw = new Stopwatch();
			sw.Start();
			for (int i = 0; i < 200; i++)
			{
				andStringifyAsync = await parsed.CreateAndStringifyAsync(new
				{
					Products = _products
				});
			}

			var swElapsed = sw.Elapsed;
			Console.WriteLine("Done in: " + swElapsed + " thats " + new TimeSpan(sw.Elapsed.Ticks / 200) + " per run");
		}

		[Test]
		[Explicit]
		public void GenericsTest()
		{
			var x = 0;
			var y = new Number(0);

			var sw = Stopwatch.StartNew();
			var runs = 1_000_000;
			for (int i = 0; i < runs; i++)
			{
				x += 1;
			}

			sw.Stop();
			Console.WriteLine("Native Operation: " + sw.Elapsed);

			sw = Stopwatch.StartNew();

			for (int i = 0; i < runs; i++)
			{
				y += 1;
			}

			sw.Stop();
			Console.WriteLine("Number Operation: " + sw.Elapsed);
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