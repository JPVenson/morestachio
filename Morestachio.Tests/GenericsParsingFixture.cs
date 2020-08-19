using System;
using System.Collections.Generic;
using System.Linq;
using Morestachio.Formatter.Framework;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture()]
	[Parallelizable(ParallelScope.Fixtures)]
	public class GenericsParsingFixture
	{
		public GenericsParsingFixture()
		{
			
		}

		public object Result { get; set; }

		private void CallWithResult(string methodName, IDictionary<string, object> values, object expected)
		{
			Assert.That(Result, Is.Null);
			MorestachioFormatterService.PrepareMakeGenericMethodInfoByValues(GetType()
					.GetMethod(methodName), values.Select(e => e.Value).ToArray())
				.Invoke(this, values.Values.ToArray());	
			Assert.That(Result, Is.EqualTo(expected));
			Result = null;
		}
		

		public void SingleGeneric<T>(T value)
		{
			Result = value;
		}
		[Test]
		public void TestSingleGeneric()
		{
			object value = new int[1];
			CallWithResult(nameof(SingleGeneric), new Dictionary<string, object>()
			{
				{"value", value},
			}, value);
			value = "Test";
			CallWithResult(nameof(SingleGeneric), new Dictionary<string, object>()
			{
				{"value", value},
			}, value);
			value = new object();
			CallWithResult(nameof(SingleGeneric), new Dictionary<string, object>()
			{
				{"value", value},
			}, value);
		}
		
		public void SingleNestedGenericIEnumerable<T>(IEnumerable<T> value)
		{
			Result = value;
		}

		[Test]
		public void TestSingleNestedGenericIEnumerable()
		{
			object value = new int[1];
			CallWithResult(nameof(SingleNestedGenericIEnumerable), new Dictionary<string, object>()
			{
				{"value", value},
			}, value);
			value = new List<string>();
			CallWithResult(nameof(SingleNestedGenericIEnumerable), new Dictionary<string, object>()
			{
				{"value", value},
			}, value);
		}
		
		public void SingleNestedGenericTuple<T>(Tuple<T> value)
		{
			Result = value;
		}
		[Test]
		public void TestSingleNestedGenericTuple()
		{
			object value = new Tuple<int>(123);
			CallWithResult(nameof(SingleNestedGenericTuple), new Dictionary<string, object>()
			{
				{"value", value},
			}, value);
			value  = new Tuple<string>("TEST");
			CallWithResult(nameof(SingleNestedGenericTuple), new Dictionary<string, object>()
			{
				{"value", value},
			}, value);
			value = new Tuple<IEnumerable<string>>(new []{"test"});
			CallWithResult(nameof(SingleNestedGenericTuple), new Dictionary<string, object>()
			{
				{"value", value},
			}, value);
		}
	}
}
