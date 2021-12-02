﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Morestachio.Analyzer.DataAccess;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Defines the start of a Scope
	/// </summary>
	[Serializable]
	public class ExpressionScopeDocumentItem : ExpressionDocumentItemBase, ISupportCustomAsyncCompilation
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal ExpressionScopeDocumentItem()
		{

		}

		/// <inheritdoc />
		public ExpressionScopeDocumentItem(CharacterLocation location,
			IMorestachioExpression value,
			IEnumerable<ITokenOption> tagCreationOptions) : base(location, value, tagCreationOptions)
		{
		}

		/// <inheritdoc />
		
		protected ExpressionScopeDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}

		/// <param name="compiler"></param>
		/// <param name="parserOptions"></param>
		/// <inheritdoc />
		public CompilationAsync Compile(IDocumentCompiler compiler, ParserOptions parserOptions)
		{
			var expression = MorestachioExpression.Compile(parserOptions);
			var children = compiler.Compile(Children, parserOptions);
			return async (stream, context, scopeData) =>
			{
				var c = await expression(context, scopeData);
				if (c.Exists())
				{
					await children(stream, c, scopeData);
				}
			};
		}
		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			//var c = await context.GetContextForPath(Value, scopeData);
			var c = await MorestachioExpression.GetValue(context, scopeData);
			if (c.Exists())
			{
				return Children.WithScope(c);
			}

			return Enumerable.Empty<DocumentItemExecution>();
		}

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}

		/// <inheritdoc />
		public override IEnumerable<string> Usage(UsageData data)
		{
			var path = MorestachioExpression.InferExpressionUsage(data).ToArray();
			var mainPath = path.FirstOrDefault();
			if (mainPath != null)
			{
				mainPath = mainPath.TrimEnd('.') + ".";
				data.ScopeTo(mainPath);
			}

			foreach (var usage in path)
			{
				yield return usage;
			}

			foreach (var usage in Children.OfType<IReportUsage>().SelectMany(f => f.Usage(data)))
			{
				yield return usage;
			}
			data.PopScope(mainPath);
		}
	}
}