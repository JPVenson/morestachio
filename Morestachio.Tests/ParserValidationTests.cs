using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Morestachio.TemplateContainers;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture]
	public class ParserValidationTests
	{
		public ParserValidationTests()
		{
			
		}

		[Test]
		public async Task ParserDoesValidateInvalidTemplate()
		{
			var errors = await Parser.Validate(new StringTemplateContainer("{{#invalid template}}"));
			Assert.That(errors, Is.Not.Empty);
		}

		[Test]
		public async Task ParserDoesValidateValidTemplate()
		{
			var errors = await Parser.Validate(new StringTemplateContainer("{{Validtemplate}}"));
			Assert.That(errors, Is.Empty);
		}
	}
}
