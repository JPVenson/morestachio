﻿using System.Globalization;
using Morestachio.Document;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper.Serialization;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Helper.Localization.Documents.CustomCultureDocument;

/// <summary>
///		Will try to get the culture declared by the value and sets the <see cref="LocalizationCultureKey"/> in the <see cref="ScopeData.CustomData"/>
/// </summary>
[System.Serializable]
public class MorestachioCustomCultureLocalizationDocumentItem : BlockExpressionDocumentItemBase,
																ToParsableStringDocumentVisitor.IStringVisitor,
																ISupportCustomAsyncCompilation
{
	internal MorestachioCustomCultureLocalizationDocumentItem()
	{
	}

	/// <inheritdoc />
	protected MorestachioCustomCultureLocalizationDocumentItem(SerializationInfo info, StreamingContext c) : base(info,
		c)
	{
	}

	/// <inheritdoc />
	public MorestachioCustomCultureLocalizationDocumentItem(TextRange location,
															IMorestachioExpression expression,
															IEnumerable<ITokenOption> tagCreationOptions)
		: base(location, expression, tagCreationOptions)
	{
	}

	/// <summary>
	///		Get name of the changed culture in the <see cref="ScopeData.CustomData"/>
	/// </summary>
	public const string LocalizationCultureKey = "LocalizationService.CustomCulture";


	/// <param name="compiler"></param>
	/// <param name="parserOptions"></param>
	/// <inheritdoc />
	public CompilationAsync Compile(IDocumentCompiler compiler, ParserOptions parserOptions)
	{
		var children = compiler.Compile(Children, parserOptions);
		var expression = MorestachioExpression.Compile(parserOptions);

		return async (outputStream, context, scopeData) =>
		{
			var oldCulture = scopeData.ParserOptions.CultureInfo;

			if (scopeData.CustomData.TryGetValue(LocalizationCultureKey, out var customCulture) &&
				customCulture is CultureInfo culInfo)
			{
				oldCulture = culInfo;
			}

			var expValue = (await expression(context, scopeData).ConfigureAwait(false));

			CultureInfo requestedCulture;

			if (expValue.Value is CultureInfo culture)
			{
				requestedCulture = culture;
			}
			else
			{
				requestedCulture = CultureInfo.GetCultureInfo(expValue.RenderToString(scopeData).ToString());
			}

			scopeData.CustomData[LocalizationCultureKey] = requestedCulture;
			await children(outputStream, context, scopeData).ConfigureAwait(false);
			scopeData.CustomData[LocalizationCultureKey] = oldCulture;
		};
	}

	/// <inheritdoc />
	public override async ItemExecutionPromise Render(IByteCounterStream outputStream,
													ContextObject context,
													ScopeData scopeData)
	{
		var oldCulture = scopeData.ParserOptions.CultureInfo;

		if (scopeData.CustomData.TryGetValue(LocalizationCultureKey, out var customCulture) &&
			customCulture is CultureInfo culInfo)
		{
			oldCulture = culInfo;
		}

		var expValue = (await MorestachioExpression.GetValue(context, scopeData).ConfigureAwait(false));

		CultureInfo requestedCulture;

		if (expValue.Value is CultureInfo culture)
		{
			requestedCulture = culture;
		}
		else
		{
			requestedCulture = CultureInfo.GetCultureInfo(expValue.RenderToString(scopeData).ToString());
		}

		scopeData.CustomData[LocalizationCultureKey] = requestedCulture;

		var childs = Children.ToList();
		childs.Add(new ResetCultureDocumentItem(base.Location, oldCulture, TagCreationOptions));
		return childs.WithScope(context);
	}

	/// <summary>
	///		Internal DocumentItem that should reset the culture
	/// </summary>
	public class ResetCultureDocumentItem : DocumentItemBase
	{
		private readonly CultureInfo _culture;


		/// <inheritdoc />
		public ResetCultureDocumentItem()
		{
		}

		/// <inheritdoc />
		public ResetCultureDocumentItem(TextRange location,
										CultureInfo culture,
										IEnumerable<ITokenOption> tagCreationOptions) : base(location,
			(IEnumerable<ITokenOption>)tagCreationOptions)
		{
			_culture = culture;
		}

		/// <inheritdoc />
		public override ItemExecutionPromise Render(IByteCounterStream outputStream,
													ContextObject context,
													ScopeData scopeData)
		{
			scopeData.CustomData[LocalizationCultureKey] = _culture;
			return Enumerable.Empty<DocumentItemExecution>().ToPromise();
		}

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			throw new NotImplementedException();
		}
	}

	/// <inheritdoc />
	public override void Accept(IDocumentItemVisitor visitor)
	{
		visitor.Visit(this);
	}

	/// <inheritdoc />
	public void Render(ToParsableStringDocumentVisitor visitor)
	{
		visitor.StringBuilder.Append("{{" + MorestachioCustomCultureLocalizationBlockProvider.OpenTag +
			MorestachioExpression.AsStringExpression() + "}}");
		visitor.VisitChildren(this);
		visitor.StringBuilder.Append("{{" + MorestachioCustomCultureLocalizationBlockProvider.CloseTag + "}}");
	}
}