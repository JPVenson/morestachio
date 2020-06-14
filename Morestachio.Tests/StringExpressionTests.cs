using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Morestachio.Framework;
using Morestachio.Framework.Expression;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture]
	public class StringExpressionTests
	{
		public StringExpressionTests()
		{

		}

		public static ContextObject StringTestContext()
		{
			return new ContextObject(new ParserOptions(""), ".", null);
		}

		[Test]
		public async Task CanParseString()
		{
			var text = "'test'";
			var result = ExpressionTokenizer.ParseExpressionOrString(text, out var context);
			Assert.That(context.Errors, Is.Empty, () => context.Errors.Select(f => f.GetException().ToString())
				.Aggregate((e,f) => e + "\r\n-------------" + f));
			Assert.That(result, Is.TypeOf<MorestachioExpressionString>());
			var expressionString = (result as MorestachioExpressionString);
			Assert.That(expressionString.Location.ToFormatString(), Is.EqualTo("1:1"));
			Assert.That(expressionString.StringParts.Count, Is.EqualTo(1));
			Assert.That((await expressionString.GetValue(StringTestContext(), new ScopeData())).Value, Is.EqualTo("test"));
		}

		[Test]
		public async Task CanParseStringWithEscaptedChars()
		{
			var text = "\"a string, with a comma, and other {[]}{§$%& stuff. also a escaped \\\" and \\\\\" and so on\"";
			var result = ExpressionTokenizer.ParseExpressionOrString(text, out var context);
			Assert.That(context.Errors, Is.Empty, () => context.Errors.Select(f => f.GetException().ToString())
				.Aggregate((e,f) => e + "\r\n-------------" + f));

			Assert.That(result, Is.TypeOf<MorestachioExpressionString>());
			var expressionString = (result as MorestachioExpressionString);
			Assert.That(expressionString.Location.ToFormatString(), Is.EqualTo("1:1"));
			Assert.That(expressionString.StringParts.Count, Is.EqualTo(1));
			Assert.That((await expressionString.GetValue(StringTestContext(), new ScopeData())).Value, Is.EqualTo("a string, with a comma, and other {[]}{§$%& stuff. also a escaped \" and \\\" and so on"));
		}
	}
}
