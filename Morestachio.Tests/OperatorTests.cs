using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Framework.Expression.Visitors;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture(ParserOptionTypes.UseOnDemandCompile)]
	[TestFixture(ParserOptionTypes.Precompile)]
	[Parallelizable(ParallelScope.All)]
	public class OperatorTests
	{
		private readonly ParserOptionTypes _opts;

		public OperatorTests(ParserOptionTypes opts)
		{
			_opts = opts;
		}

		[Test]
		public async Task TestNullCoalescingOperatorSingleValues()
		{
			var template = "{{A ?? B}}";
			var data = new Dictionary<string, object>()
			{
				{ "A", "VALA" },
				{ "B", "VALB" },
			};
			Assert.That(await ParserFixture.CreateAndParseWithOptions(template, data, _opts), Is.EqualTo(data["A"]));
			data = new Dictionary<string, object>()
			{
				{ "A", null },
				{ "B", "VALB" },
			};
			Assert.That(await ParserFixture.CreateAndParseWithOptions(template, data, _opts), Is.EqualTo(data["B"]));
		}

		[Test]
		public async Task TestNullCoalescingOperatorFromParaValues()
		{
			var template = "{{((A) ?? (B))}}";
			var data = new Dictionary<string, object>()
			{
				{ "A", "VALA" },
				{ "B", "VALB" },
			};
			Assert.That(await ParserFixture.CreateAndParseWithOptions(template, data, _opts), Is.EqualTo(data["A"]));
			data = new Dictionary<string, object>()
			{
				{ "A", null },
				{ "B", "VALB" },
			};
			Assert.That(await ParserFixture.CreateAndParseWithOptions(template, data, _opts), Is.EqualTo(data["B"]));
		}

		[Test]
		public async Task TestInvertOperator()
		{
			var template = "{{(!A && B).ToString().ToLower()}}";
			var data = new Dictionary<string, object>()
			{
				{ "A", false },
				{ "B", true },
			};
			Assert.That(await ParserFixture.CreateAndParseWithOptions(template, data, _opts), Is.EqualTo("true"));
		}

		[Test]
		[TestCase("(e, f) => e + f")]
		[TestCase("(e, f) => e.Call(f)")]
		[TestCase("(e) => e.Call()")]
		[TestCase("() => this.Call()")]
		public void TestLambdaCanBeParsed(string expressionText, bool expectError = false)
		{
			var exp = ExpressionParser.ParseExpression(expressionText, out var ctx);

			if (expectError)
			{
				Assert.That(ctx.Errors.Count, Is.GreaterThan(0));
			}

			Assert.That(ctx.Errors.Count, Is.EqualTo(0));
			Assert.That(exp, Is.Not.Null);

			var visitor = new ToParsableStringExpressionVisitor();
			exp.Accept(visitor);
			Assert.That(visitor.StringBuilder.ToString(), Is.EqualTo(expressionText));
		}
	}
}