
using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Custom;
using Morestachio.Framework;


namespace Morestachio.LessCompiler
{
	/// <inheritdoc />
	[PublicAPI]
	public class LessCompilerDocumentItemProvider : BlockDocumentItemProviderBase
	{
		public LessCompilerDocumentItemProvider() : base("#Less", "/Less")
		{
		}

		/// <inheritdoc />
		public override IDocumentItem CreateDocumentItem(string tag, string value, TokenPair token, ParserOptions options)
		{
			return new CompileLessDocumentItem();
		}
	}
}
