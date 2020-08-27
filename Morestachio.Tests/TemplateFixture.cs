using System.Collections.Generic;
using System.Linq;
using Morestachio.Framework;
using Morestachio.Helper;
using Morestachio.Parsing.ParserErrors;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[Parallelizable(ParallelScope.All)]
	public class TemplateFixture
	{
		[Test]
		[TestCase(200)]
		[TestCase(80000)]
		[TestCase(700000)]
		public void TemplateMaxSizeLimit(int maxSize)
		{
			var tempdata = new List<string>();
			var sizeOfOneChar = ParserFixture.DefaultEncoding.GetByteCount(" ");
			for (var i = 0; i < maxSize / sizeOfOneChar; i++)
			{
				tempdata.Add(" ");
			}

			var template = "{{#each Data}}{{.}}{{/each}}";
			var templateFunc =
				Parser.ParseWithOptions(new ParserOptions(template, null, ParserFixture.DefaultEncoding, maxSize));
			var templateStream = templateFunc.Create(new Dictionary<string, object>
			{
				{"Data", tempdata}
			});
			Assert.AreEqual(templateStream.Stream.Length, maxSize);
		}

		[Test]
		[TestCase(6)]
		[TestCase(7)]
		[TestCase(8)]
		[TestCase(200)]
		[TestCase(80000)]
		[TestCase(700000)]
		public void TemplateMaxSizeOverLimit(int maxSize)
		{
			var tempdata = new List<string>();
			var sizeOfOneChar = ParserFixture.DefaultEncoding.GetByteCount(" ");
			for (var i = 0; i < (maxSize / sizeOfOneChar) + sizeOfOneChar; i++)
			{
				tempdata.Add(" ");
			}

			var template = "{{#each Data}}{{.}}{{/each}}";
			var templateFunc =
				Parser.ParseWithOptions(new ParserOptions(template, null, ParserFixture.DefaultEncoding, maxSize));
			var templateStream = templateFunc.Create(new Dictionary<string, object>
			{
				{"Data", tempdata}
			});
			Assert.That(templateStream.Stream.Length, Is.EqualTo(maxSize).Or.EqualTo(maxSize - 1));
		}

		[TestCase(new int[] { })]
		[TestCase(false)]
		[TestCase("")]
		[TestCase(0.0)]
		[TestCase(0)]
		[Test]
		public void TemplatesShoudlNotRenderFalseyComplexStructures(object falseyModelValue)
		{
			var model = new Dictionary<string, object>
			{
				{"outer_level", falseyModelValue}
			};

			var template = "{{#outer_level}}Shouldn't be rendered!{{inner_level}}{{/outer_level}}";

			var result = Parser.ParseWithOptions(new ParserOptions(template, null, ParserFixture.DefaultEncoding))
				.CreateAndStringify(model);

			Assert.AreEqual(string.Empty, result);
		}

		[TestCase(new int[] { })]
		[TestCase(false)]
		[TestCase("")]
		[TestCase(0.0)]
		[TestCase(0)]
		[Test]
		public void TemplateShouldTreatFalseyValuesAsEmptyArray(object falseyModelValue)
		{
			var model = new Dictionary<string, object>
			{
				{"locations", falseyModelValue}
			};

			var template = "{{#each locations}}Shouldn't be rendered!{{/each}}";

			var result = Parser.ParseWithOptions(new ParserOptions(template, null, ParserFixture.DefaultEncoding))
				.CreateAndStringify(model);

			Assert.AreEqual(string.Empty, result);
		}

		[TestCase(0)]
		[TestCase(0.0)]
		[Test]
		public void TemplateShouldRenderZeroValue(object value)
		{
			var model = new Dictionary<string, object>
			{
				{"times_won", value}
			};

			var template = "You've won {{times_won}} times!";

			var result = Parser.ParseWithOptions(new ParserOptions(template, null, ParserFixture.DefaultEncoding))
				.CreateAndStringify(model);

			Assert.AreEqual("You've won 0 times!", result);
		}

		[Test]
		public void CommentsAreExcludedFromOutput()
		{
			var model = new Dictionary<string, object>();

			var plainText = @"as{{!stu
			ff}}df";
			var rendered = Parser.ParseWithOptions(new ParserOptions(plainText, null, ParserFixture.DefaultEncoding))
				.CreateAndStringify(model);

			Assert.AreEqual("asdf", rendered);
		}

		[Test]
		public void TestMultiLineCommentsAreExcludedFromOutput()
		{
			var model = new Dictionary<string, object>();

			var plainText = @"A{{!}}ZZZ{{/!}}B{{!}} {{123}} {{'{{'}} {{'}} }} {{/!}}C";
			var rendered = Parser.ParseWithOptions(new ParserOptions(plainText, null, ParserFixture.DefaultEncoding))
				.CreateAndStringify(model);

			Assert.AreEqual("ABC", rendered);
		}

		[Test]
		public void HtmlIsEscapedByDefault()
		{
			var model = new Dictionary<string, object>();

			model["stuff"] = "<b>inner</b>";

			var plainText = @"{{stuff}}";
			var rendered = Parser.ParseWithOptions(new ParserOptions(plainText, null, ParserFixture.DefaultEncoding))
				.CreateAndStringify(model);

			Assert.AreEqual("&lt;b&gt;inner&lt;/b&gt;", rendered);
		}

		[Test]
		public void HtmlIsNotEscapedWhenUsingUnsafeSyntaxes()
		{
			var model = new Dictionary<string, object>();

			model["stuff"] = "<b>inner</b>";

			var plainText = @"{{&stuff}}";
			var rendered = Parser.ParseWithOptions(new ParserOptions(plainText, null, ParserFixture.DefaultEncoding))
				.CreateAndStringify(model);
			Assert.AreEqual("<b>inner</b>", rendered);
		}

		[Test]
		public void NegationGroupRendersContentWhenValueNotSet()
		{
			var model = new Dictionary<string, object>();

			var plainText = @"{{^stuff}}No Stuff Here.{{/stuff}}";
			var rendered = Parser.ParseWithOptions(new ParserOptions(plainText, null, ParserFixture.DefaultEncoding))
				.CreateAndStringify(model);

			Assert.AreEqual("No Stuff Here.", rendered);
		}

		[Test]
		public void TemplateRendersContentWithNoVariables()
		{
			var plainText = "ASDF";
			var template = Parser.ParseWithOptions(new ParserOptions("ASDF", null, ParserFixture.DefaultEncoding));
			Assert.AreEqual(plainText, template.CreateAndStringify(null));
		}


		[Test]
		public void TemplateRendersWithComplexEachPath()
		{
			var template =
				@"{{#each Company.ceo.products}}<li>{{name}} and {{version}} and has a CEO: {{../../last_name}}</li>{{/each}}";

			var parsingOptions = new ParserOptions(template, null, ParserFixture.DefaultEncoding);
			var parsedTemplate =
				Parser.ParseWithOptions(parsingOptions);

			var model = new Dictionary<string, object>();

			var company = new Dictionary<string, object>();
			model["Company"] = company;

			var ceo = new Dictionary<string, object>();
			company["ceo"] = ceo;
			ceo["last_name"] = "Smith";

			var products = Enumerable.Range(0, 3).Select(k =>
			{
				var r = new Dictionary<string, object>();
				r["name"] = "name " + k;
				r["version"] = "version " + k;
				return r;
			}).ToArray();

			ceo["products"] = products;

			var result = parsedTemplate.Create(model).Stream.Stringify(true, ParserFixture.DefaultEncoding);

			Assert.AreEqual("<li>name 0 and version 0 and has a CEO: Smith</li>" +
						 "<li>name 1 and version 1 and has a CEO: Smith</li>" +
						 "<li>name 2 and version 2 and has a CEO: Smith</li>", result);
			SerilalizerTests.SerializerTest.AssertDocumentItemIsSameAsTemplate(parsingOptions.Template, parsedTemplate.Document);
		}

		[Test]
		public void TemplateRendersWithComplexScopePath()
		{
			var template =
				@"{{#Company.ceo}}{{#each products}}<li>{{name}} and {{version}} and has a CEO: {{../../last_name}}</li>{{/each}}{{/Company.ceo}}";

			var parsingOptions = new ParserOptions(template, null, ParserFixture.DefaultEncoding);
			var parsedTemplate =
				Parser.ParseWithOptions(parsingOptions);

			var model = new Dictionary<string, object>();

			var company = new Dictionary<string, object>();
			model["Company"] = company;

			var ceo = new Dictionary<string, object>();
			company["ceo"] = ceo;
			ceo["last_name"] = "Smith";

			var products = Enumerable.Range(0, 3).Select(k =>
			{
				var r = new Dictionary<string, object>();
				r["name"] = "name " + k;
				r["version"] = "version " + k;
				return r;
			}).ToArray();

			ceo["products"] = products;

			var result = parsedTemplate.CreateAndStringify(model);

			Assert.AreEqual("<li>name 0 and version 0 and has a CEO: Smith</li>" +
						 "<li>name 1 and version 1 and has a CEO: Smith</li>" +
						 "<li>name 2 and version 2 and has a CEO: Smith</li>", result);
			SerilalizerTests.SerializerTest.AssertDocumentItemIsSameAsTemplate(parsingOptions.Template, parsedTemplate.Document);
		}

		[Test]
		public void TemplateRendersWithComplexRootScopePath()
		{
			var template =
				@"{{#data}}{{~root}}{{/data}}";

			var parsedTemplate =
				Parser.ParseWithOptions(new ParserOptions(template, null, ParserFixture.DefaultEncoding));

			var model = new Dictionary<string, object>()
			{
				{"data", "test" },
				{"root", "tset" }
			};

			var result = parsedTemplate.CreateAndStringify(model);

			Assert.AreEqual(model["root"], result);
		}

		[Test]
		public void TemplateDoesNotScope()
		{
			var template =
				@"{{#IF data}}{{.}}{{/IF}}";

			var parsedTemplate =
				Parser.ParseWithOptions(new ParserOptions(template, null, ParserFixture.DefaultEncoding));

			var model = new Dictionary<string, object>()
			{
				{"data", "test" },
				{"root", "tset" }
			};

			var result = parsedTemplate.CreateAndStringify(model);

			Assert.AreEqual(model.ToString(), result);
		}

		[Test]
		public void TemplateIfRendersRootScopePath()
		{
			var template =
				@"{{#IF ~data}}{{data}}{{/IF}}";

			var parsedTemplate =
				Parser.ParseWithOptions(new ParserOptions(template, null, ParserFixture.DefaultEncoding));

			var model = new Dictionary<string, object>()
			{
				{"data", "test" },
				{"root", "tset" }
			};

			var result = parsedTemplate.CreateAndStringify(model);

			Assert.AreEqual(model["data"], result);
		}

		[Test]
		public void TemplateIfElse()
		{
			var template =
				@"{{#IF data}}{{data}}{{/IF}}{{#else}}{{root}}{{/else}}";

			var parsingOptions = new ParserOptions(template, null, ParserFixture.DefaultEncoding);
			var parsedTemplate =
				Parser.ParseWithOptions(parsingOptions);

			var model = new Dictionary<string, object>()
			{
				{"data", "false" },
				{"root", "true" }
			};

			var result = parsedTemplate.CreateAndStringify(model);

			Assert.AreEqual(model["data"], result);
			SerilalizerTests.SerializerTest.AssertDocumentItemIsSameAsTemplate(parsingOptions.Template, parsedTemplate.Document);
		}

		[Test]
		public void TemplateInvertedIfElse()
		{
			var template =
				@"{{^IF data}}{{data}}{{/IF}}{{#else}}{{root}}{{/else}}";

			var parsedTemplate =
				Parser.ParseWithOptions(new ParserOptions(template, null, ParserFixture.DefaultEncoding));

			var model = new Dictionary<string, object>()
			{
				{"data", "false" },
				{"root", "true" }
			};

			var result = parsedTemplate.CreateAndStringify(model);

			Assert.AreEqual(model["root"], result);
		}



		[Test]
		public void TemplateIfElseCombined()
		{
			var template =
				@"{{#IF data}}{{data}}{{#ifelse}}{{root}}{{/else}}";

			var parsedTemplate =
				Parser.ParseWithOptions(new ParserOptions(template, null, ParserFixture.DefaultEncoding));

			var model = new Dictionary<string, object>()
			{
				{"data", "false" },
				{"root", "true" }
			};

			var result = parsedTemplate.CreateAndStringify(model);

			Assert.AreEqual(model["data"], result);
		}

		[Test]
		public void TemplateInvertedIfElseCombined()
		{
			var template =
				@"{{^IF data}}{{data}}{{#ifelse}}{{root}}{{/else}}";

			var parsedTemplate =
				Parser.ParseWithOptions(new ParserOptions(template, null, ParserFixture.DefaultEncoding));

			var model = new Dictionary<string, object>()
			{
				{"data", "false" },
				{"root", "true" }
			};

			var result = parsedTemplate.CreateAndStringify(model);

			Assert.AreEqual(model["root"], result);
		}

		[Test]
		public void TemplateInvalidContentBetweenIfAndElse()
		{
			var template =
				@"{{^IF data}}{{data}}{{/IF}}{{data}}{{#else}}{{root}}{{/else}}";

			var parsedTemplate =
				Parser.ParseWithOptions(new ParserOptions(template, null, ParserFixture.DefaultEncoding));

			var model = new Dictionary<string, object>()
			{
				{"data", "false" },
				{"root", "true" }
			};

			Assert.That(parsedTemplate.Errors
				.OfType<MorestachioSyntaxError>()
				.FirstOrDefault(e => e.Location.Equals(CharacterLocation.FromFormatString("1:38"))), Is.Not.Null );
		}

		[Test]
		public void TemplateRendersWithComplexRootScopePathInIf()
		{
			var template =
				@"{{#IF data}}{{root}}{{/if}}";

			var parsedTemplate =
				Parser.ParseWithOptions(new ParserOptions(template, null, ParserFixture.DefaultEncoding));

			var model = new Dictionary<string, object>()
			{
				{"data", "test" },
				{"root", "tset" }
			};

			var result = parsedTemplate.CreateAndStringify(model);

			Assert.AreEqual(model["root"], result);
		}

		[Test]
		public void TemplateRendersWithScopeWithAliasPath()
		{
			var template =
				@"{{#data AS test}}{{#~root AS rootTest}}{{test}},{{rootTest}}{{/rootTest}}{{rootTest}}{{/test}}{{test}}";

			var parsedTemplate =
				Parser.ParseWithOptions(new ParserOptions(template, null, ParserFixture.DefaultEncoding));

			var model = new Dictionary<string, object>()
			{
				{"data", "test" },
				{"root", "tset" }
			};

			var result = parsedTemplate.CreateAndStringify(model);

			Assert.AreEqual(model["data"] + "," + model["root"], result);
		}
		
		[Test]
		public void TemplateRendersWithEachWithAliasPath()
		{
			var template =
				@"{{#each data AS dd}}{{dd}}{{/each}}";

			var parsedTemplate =
				Parser.ParseWithOptions(new ParserOptions(template, null, ParserFixture.DefaultEncoding));

			var value = new List<int>()
			{
				1,2,3,4,5
			};
			var model = new Dictionary<string, object>()
			{
				{
					"data", value
				},
			};

			var result = parsedTemplate.CreateAndStringify(model);

			Assert.AreEqual(value.Select(e => e.ToString()).Aggregate((e,f) => e + "" + f), result);
		}

		[Test]
		public void TemplateRendersWithComplexUpScopePath()
		{
			var template =
				@"{{#Data1.Data2.NullableInit}}{{../../../root}}{{/Data1.Data2.NullableInit}}";

			var parsedTemplate =
				Parser.ParseWithOptions(new ParserOptions(template, null, ParserFixture.DefaultEncoding));

			var model = new Dictionary<string, object>()
			{
				{
					"Data1", new Dictionary<string, object>()
					{
						{
							"Data2", new Dictionary<string, object>()
							{
								{"NullableInit", (int?) 1}
							}
						}
					}
				},
				{"root", "tset"}
			};

			var result = parsedTemplate.CreateAndStringify(model);

			Assert.AreEqual(model["root"], result);
		}

		[Test]
		public void TemplateRendersWithComplexRootScopePathWithFormatting()
		{
			var template =
				@"{{#Data1.Data2.NullableInit}}{{~root}}{{/Data1.Data2.NullableInit}}";

			var parsedTemplate =
				Parser.ParseWithOptions(new ParserOptions(template, null, ParserFixture.DefaultEncoding));

			var model = new Dictionary<string, object>()
			{
				{
					"Data1", new Dictionary<string, object>()
					{
						{
							"Data2", new Dictionary<string, object>()
							{
								{"NullableInit", (int?) 1}
							}
						}
					}
				},
				{"root", "tset"}
			};

			//1.ToString("E")

			var result = parsedTemplate.CreateAndStringify(model);

			Assert.AreEqual(model["root"], result);
		}

		[Test]
		public void TemplateRendersWithComplexUpScopePathWithFormatting()
		{
			var template =
				@"{{#d.d.n}}{{../../../r.('c')}}{{/d.d.n}}";

			var parsedTemplate =
				Parser.ParseWithOptions(new ParserOptions(template, null, ParserFixture.DefaultEncoding));

			var model = new Dictionary<string, object>()
			{
				{
					"d", new Dictionary<string, object>()
					{
						{
							"d", new Dictionary<string, object>()
							{
								{"n", (int?) 1}
							}
						}
					}
				},
				{"r", "tset"}
			};

			var result = parsedTemplate.CreateAndStringify(model);

			Assert.AreEqual(model["r"], result);
		}

		[Test]
		public void TemplateShouldNotRenderNullValue()
		{
			var model = new Dictionary<string, object>
			{
				{"times_won", null}
			};

			var template = "You've won {{times_won}} times!";

			var result = Parser.ParseWithOptions(new ParserOptions(template, null, ParserFixture.DefaultEncoding))
				.CreateAndStringify(model);

			Assert.AreEqual("You've won  times!", result);
		}

		[Test]
		public void TemplateShouldProcessVariablesInInvertedGroup()
		{
			var model = new Dictionary<string, object>
			{
				{"not_here", false},
				{"placeholder", "a placeholder value"}
			};

			var template = "{{^not_here}}{{../placeholder}}{{/not_here}}";

			var result = Parser.ParseWithOptions(new ParserOptions(template, null, ParserFixture.DefaultEncoding))
				.CreateAndStringify(model);

			Assert.AreEqual("a placeholder value", result);
		}

		[Test]
		public void TemplateShouldRenderFalseValue()
		{
			var model = new Dictionary<string, object>
			{
				{"times_won", false}
			};

			var template = "You've won {{times_won}} times!";

			var result = Parser.ParseWithOptions(new ParserOptions(template, null, ParserFixture.DefaultEncoding))
				.CreateAndStringify(model);

			Assert.AreEqual("You've won False times!", result);
		}
	}
}