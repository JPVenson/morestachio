using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Morestachio.Analyzer.DataAccess;
using NUnit.Framework;

namespace Morestachio.Tests
{
	[TestFixture(ParserOptionTypes.UseOnDemandCompile)]
	[TestFixture(ParserOptionTypes.Precompile)]
	[Parallelizable(ParallelScope.All)]
	public class InferedUsageTests
	{
		private readonly ParserOptionTypes _options;

		public InferedUsageTests(ParserOptionTypes options)
		{
			_options = options;
		}

		[Test]
		public async Task CanInferExpressionUsage()
		{
			var template = @"{{data.propertyA}}";
			var morestachioDocumentInfo = await ParserFixture.CreateWithOptionsStream(template, _options);
			var dataAccessAnalyzer = new DataAccessAnalyzer(morestachioDocumentInfo.Document);
			var usage = dataAccessAnalyzer.GetUsageFromDeclared()?.AsText();

			Assert.That(usage, Is.Not.Null);
			Assert.That(usage, Has.One.Items.EqualTo("data.propertyA"));
		}

		[Test]
		public async Task CanInferMultipleDifferentExpressionUsage()
		{
			var template = @"{{data.propertyA}}{{source.propertyB}}";
			var morestachioDocumentInfo = await ParserFixture.CreateWithOptionsStream(template, _options);
			var dataAccessAnalyzer = new DataAccessAnalyzer(morestachioDocumentInfo.Document);
			var usage = dataAccessAnalyzer.GetUsageFromDeclared()?.AsText();

			Assert.That(usage, Is.Not.Null);
			Assert.That(usage, Has.One.Items.EqualTo("data.propertyA"));
			Assert.That(usage, Has.One.Items.EqualTo("source.propertyB"));
		}

		[Test]
		public async Task CanInferMultipleExpressionSameRootUsage()
		{
			var template = @"{{data.propertyA}}{{data.propertyB}}";
			var morestachioDocumentInfo = await ParserFixture.CreateWithOptionsStream(template, _options);
			var dataAccessAnalyzer = new DataAccessAnalyzer(morestachioDocumentInfo.Document);
			var usage = dataAccessAnalyzer.GetUsageFromDeclared()?.AsText();

			Assert.That(usage, Is.Not.Null);
			Assert.That(usage, Has.One.Items.EqualTo("data.propertyA"));
			Assert.That(usage, Has.One.Items.EqualTo("data.propertyB"));
		}

		[Test]
		public async Task CanInferMultipleExpressionScopedUsage()
		{
			var template = @"{{#SCOPE data}}{{propertyA}}{{propertyB}}{{/SCOPE}}";
			var morestachioDocumentInfo = await ParserFixture.CreateWithOptionsStream(template, _options);
			var dataAccessAnalyzer = new DataAccessAnalyzer(morestachioDocumentInfo.Document);
			var usage = dataAccessAnalyzer.GetUsageFromDeclared()?.AsText();

			Assert.That(usage, Is.Not.Null);
			Assert.That(usage, Has.One.Items.EqualTo("data.propertyA"));
			Assert.That(usage, Has.One.Items.EqualTo("data.propertyB"));
		}

		[Test]
		public async Task CanInferMultipleExpressionEachUsage()
		{
			var template = @"{{#EACH data}}{{propertyA}}{{propertyB}}{{/EACH}}";
			var morestachioDocumentInfo = await ParserFixture.CreateWithOptionsStream(template, _options);
			var dataAccessAnalyzer = new DataAccessAnalyzer(morestachioDocumentInfo.Document);
			var usage = dataAccessAnalyzer.GetUsageFromDeclared()?.AsText();

			Assert.That(usage, Is.Not.Null);
			Assert.That(usage, Has.One.Items.EqualTo("data.[].propertyA"));
			Assert.That(usage, Has.One.Items.EqualTo("data.[].propertyB"));
		}

		[Test]
		public async Task CanInferVariableUsageUsage()
		{
			var template = @"{{#VAR va = data}}{{va.propertyA}}";
			var morestachioDocumentInfo = await ParserFixture.CreateWithOptionsStream(template, _options);
			var dataAccessAnalyzer = new DataAccessAnalyzer(morestachioDocumentInfo.Document);
			var usage = dataAccessAnalyzer.GetUsageFromDeclared()?.AsText();

			Assert.That(usage, Is.Not.Null);
			Assert.That(usage, Has.One.Items.EqualTo("data.propertyA"));
		}

		[Test]
		public async Task CanInferMultipleExpressionEachAliasUsage()
		{
			var template = @"{{#FOREACH item IN data}}{{item.propertyA}}{{item.propertyB}}{{/FOREACH}}";
			var morestachioDocumentInfo = await ParserFixture.CreateWithOptionsStream(template, _options);
			var dataAccessAnalyzer = new DataAccessAnalyzer(morestachioDocumentInfo.Document);
			var usage = dataAccessAnalyzer.GetUsageFromDeclared()?.AsText();

			Assert.That(usage, Is.Not.Null);
			Assert.That(usage, Has.One.Items.EqualTo("data.[].propertyA"));
			Assert.That(usage, Has.One.Items.EqualTo("data.[].propertyB"));
		}

		[Test]
		public async Task CanInferMultipleExpressionEachInScopeAliasUsage()
		{
			var template
				= @"{{#FOREACH item IN data}}{{#SCOPE item.VAL}}{{item.propertyA}}{{item.propertyB}}{{/SCOPE}}{{/FOREACH}}";
			var morestachioDocumentInfo = await ParserFixture.CreateWithOptionsStream(template, _options);
			var dataAccessAnalyzer = new DataAccessAnalyzer(morestachioDocumentInfo.Document);
			var usage = dataAccessAnalyzer.GetUsageFromDeclared()?.AsText();

			Assert.That(usage, Is.Not.Null);
			Assert.That(usage, Has.One.Items.EqualTo("data.[].VAL"));
			Assert.That(usage, Has.One.Items.EqualTo("data.[].propertyA"));
			Assert.That(usage, Has.One.Items.EqualTo("data.[].propertyB"));
		}

		[Test]
		public async Task CanInferPlainForEachLoopUsage()
		{
			var template = @"{{#FOREACH item IN data}}{{/FOREACH}}";
			var morestachioDocumentInfo = await ParserFixture.CreateWithOptionsStream(template, _options);
			var dataAccessAnalyzer = new DataAccessAnalyzer(morestachioDocumentInfo.Document);
			var usage = dataAccessAnalyzer.GetUsageFromDeclared()?.AsText();

			Assert.That(usage, Is.Not.Null);
			Assert.That(usage, Has.One.Items.EqualTo("data.[]"));
		}

		[Test]
		public async Task CanInferExpressionIgnoreConstValuesUsage()
		{
			var template = @"{{data.propertyA}}{{data.propertyB}}{{#VAR val = 'test'}}{{val.Length}}";
			var morestachioDocumentInfo = await ParserFixture.CreateWithOptionsStream(template, _options);
			var dataAccessAnalyzer = new DataAccessAnalyzer(morestachioDocumentInfo.Document);
			var usage = dataAccessAnalyzer.GetUsageFromDeclared()?.AsText();

			Assert.That(usage, Is.Not.Null);
			Assert.That(usage, Has.Exactly(2).Items);
			Assert.That(usage, Has.One.Items.EqualTo("data.propertyA"));
			Assert.That(usage, Has.One.Items.EqualTo("data.propertyB"));
		}

		[Test]
		public async Task CanInferExpressionParameterUsage()
		{
			var template = @"{{data.propertyA.Function(data.propertyB, 'str', 123.ToString(data.strValue))}}";
			var morestachioDocumentInfo = await ParserFixture.CreateWithOptionsStream(template, _options);
			var dataAccessAnalyzer = new DataAccessAnalyzer(morestachioDocumentInfo.Document);
			var usage = dataAccessAnalyzer.GetUsageFromDeclared()?.AsText();

			Assert.That(usage, Is.Not.Null);
			Assert.That(usage, Has.Exactly(3).Items);
			Assert.That(usage, Has.One.Items.EqualTo("data.propertyA"));
			Assert.That(usage, Has.One.Items.EqualTo("data.propertyB"));
			Assert.That(usage, Has.One.Items.EqualTo("data.strValue"));
		}
	}
}