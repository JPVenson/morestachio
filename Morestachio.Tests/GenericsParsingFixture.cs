using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Morestachio.Document;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Framework.Expression;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture]
	[Parallelizable(ParallelScope.Fixtures)]
	//[Explicit("Currently unable to test. Needs Refactoring!")]
	public class GenericsParsingFixture
	{
		public object Result { get; set; }

		private void CallWithResult(string methodName, IDictionary<string, object> values, object expected)
		{
			Assert.That(Result, Is.Null);
			var morestachioFormatterService = new MorestachioFormatterService();
			morestachioFormatterService.AddFromType<GenericsParsingFixture>();

			var parserOptions = new ParserOptions();
			var types = values.Select((e, i) => new FormatterArgumentType(i, e.Key, e.Value, new MorestachioExpression())).ToArray();

			var cache = morestachioFormatterService.PrepareCallMostMatchingFormatter(GetType(),
				types,
				methodName,
				parserOptions,
				new ScopeData(parserOptions));
			
			morestachioFormatterService.Execute(cache, this, parserOptions, types);
			Assert.That(Result, Is.EqualTo(expected));
			Result = null;
		}

		[MorestachioFormatter("[MethodName]", "")]
		public void SingleGeneric<T>(T value)
		{
			Result = value;
		}

		[Test]
		public void TestSingleGeneric()
		{
			object value = new int[1];

			CallWithResult(nameof(SingleGeneric), new Dictionary<string, object>
			{
				{ "value", value }
			}, value);
			value = "Test";

			CallWithResult(nameof(SingleGeneric), new Dictionary<string, object>
			{
				{ "value", value }
			}, value);
			value = new object();

			CallWithResult(nameof(SingleGeneric), new Dictionary<string, object>
			{
				{ "value", value }
			}, value);
		}

		[MorestachioFormatter("[MethodName]", "")]
		public void SingleNestedGenericIEnumerable<T>(IEnumerable<T> value)
		{
			Result = value;
		}

		[Test]
		public void TestSingleNestedGenericIEnumerable()
		{
			object value = new int[1];

			CallWithResult(nameof(SingleNestedGenericIEnumerable), new Dictionary<string, object>
			{
				{ "value", value }
			}, value);
			value = new List<string>();

			CallWithResult(nameof(SingleNestedGenericIEnumerable), new Dictionary<string, object>
			{
				{ "value", value }
			}, value);
		}

		[MorestachioFormatter("[MethodName]", "")]
		public void SingleNestedGenericTuple<T>(Tuple<T> value)
		{
			Result = value;
		}

		[Test]
		public void TestSingleNestedGenericTuple()
		{
			object value = new Tuple<int>(123);

			CallWithResult(nameof(SingleNestedGenericTuple), new Dictionary<string, object>
			{
				{ "value", value }
			}, value);
			value = new Tuple<string>("TEST");

			CallWithResult(nameof(SingleNestedGenericTuple), new Dictionary<string, object>
			{
				{ "value", value }
			}, value);
			value = new Tuple<IEnumerable<string>>(new[] { "test" });

			CallWithResult(nameof(SingleNestedGenericTuple), new Dictionary<string, object>
			{
				{ "value", value }
			}, value);
		}

		public class Item
		{
			public int Key { get; set; }
			public string Value { get; set; }
		}

		[Test]
		public async Task TestGenericReturnParameter()
		{
			var sourceList = new[]
			{
				new Item { Key = 1, Value = "A" },
				new Item { Key = 2, Value = "B" },
				new Item { Key = 1, Value = "C" }
			};
			MorestachioDocumentResult result = null;

			await ParserFixture.CreateAndParseWithOptions("{{#VAR result = this.GroupBy((e) => e.Key).ToArray()}}",
				sourceList,
				ParserOptionTypes.Precompile,
				null,
				e => e.CaptureVariables = true,
				e => result = e);

			Assert.That(result.CapturedVariables, Contains.Key("result"));
			Assert.That(result.CapturedVariables["result"], Is.AssignableTo(typeof(IEnumerable<IGrouping<object, Item>>)));
			var varResult = result.CapturedVariables["result"] as IEnumerable<IGrouping<object, Item>>;
			Assert.That(varResult.Count(), Is.EqualTo(2));
		}
	}
}