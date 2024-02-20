using System;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Formatter.Framework.Converter;

namespace Morestachio.Tests
{
	public static class CustomConverterFormatter
	{
		public class TestObject
		{
			public int No { get; set; }
		}

		public class ExpectedObject
		{
			public int No { get; set; }
		}

		public class TestToExpectedObjectConverter : IFormatterValueConverter
		{
			public bool CanConvert(Type sourceType, Type requestedType)
			{
				return requestedType == typeof(ExpectedObject) && sourceType == typeof(TestObject);
			}

			public object Convert(object value, Type requestedType)
			{
				return new ExpectedObject()
				{
					No = (value as TestObject).No
				};
			}
		}

		[MorestachioFormatter("ReturnValue", "XXX")]
		public static int ReturnValue(ExpectedObject value)
		{
			return value.No;
		}

		[MorestachioFormatter("ReturnValueExplicitConverter", "XXX")]
		public static int ReturnValueA(
			[FormatterValueConverter(typeof(TestToExpectedObjectConverter))] ExpectedObject value)
		{
			return value.No;
		}
	}
}