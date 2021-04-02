using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Morestachio.Document;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Expression.Parser;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture]
	[Parallelizable(ParallelScope.All)]
	public class StringExpressionTests
	{
		public StringExpressionTests()
		{

		}

		public static ContextObject StringTestContext()
		{
			return new ContextObject(".", null, null);
		}

		[Test]
		public async Task CanParseString()
		{
			var text = "'test'";
			var result = ExpressionParser.ParseExpression(text, out var context, CultureInfo.CurrentCulture);
			Assert.That(context.Errors, Is.Empty, () => context.Errors.Select(f => f.GetException().ToString())
				.Aggregate((e, f) => e + "\r\n-------------" + f));
			Assert.That(result, Is.TypeOf<MorestachioExpressionString>());
			var expressionString = (result as MorestachioExpressionString);
			Assert.That(expressionString.Location.ToFormatString(), Is.EqualTo("1:1,1"));
			Assert.That(expressionString.StringParts.Count, Is.EqualTo(1));
			Assert.That((await expressionString.GetValue(StringTestContext(), new ScopeData(new ParserOptions("")))).Value, Is.EqualTo("test"));
		}

		[Test]
		public async Task CanNotParseUnclosedString()
		{
			var text = "\"";
			var result = await ExpressionParser.EvaluateExpression(text, new ParserOptions(), null, TokenzierContext.FromText(text));
			Assert.That(result, Is.Null);
		}

		[Test]
		public async Task CanParseStringWithEscaptedChars()
		{
			var text = "\"a string, with a comma, and other {[]}{§$%& stuff. also a escaped \\\" and \\\\\" and so on\"";
			var result = ExpressionParser.ParseExpression(text, out var context, CultureInfo.CurrentCulture);
			Assert.That(context.Errors, Is.Empty, () => context.Errors.Select(f => f.GetException().ToString())
				.Aggregate((e, f) => e + "\r\n-------------" + f));

			Assert.That(result, Is.TypeOf<MorestachioExpressionString>());
			var expressionString = (result as MorestachioExpressionString);
			Assert.That(expressionString.Location.ToFormatString(), Is.EqualTo("1:1,1"));
			Assert.That(expressionString.StringParts.Count, Is.EqualTo(1));
			Assert.That((await expressionString.GetValue(StringTestContext(), new ScopeData(new ParserOptions("")))).Value, Is.EqualTo("a string, with a comma, and other {[]}{§$%& stuff. also a escaped \" and \\\" and so on"));
		}

		[Test]
		public void TestSubstringFormatter()
		{
			Assert.That(Morestachio.Formatter.Predefined.StringFormatter.Substring("ABCDEFGHIJ", 3, 20), Is.EqualTo("DEFGHIJ"));
		}
	}
}
