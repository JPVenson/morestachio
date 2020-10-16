using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using Morestachio.Document;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;
using Morestachio.Helper.Localization.Documents.CustomCultureDocument;
#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#endif

namespace Morestachio.Helper.Localization
{
	/// <summary>
	///		Will try to get the culture declared by the value and sets the <see cref="LocalizationCultureKey"/> in the <see cref="ScopeData.CustomData"/>
	/// </summary>
	[System.Serializable]
	public class MorestachioCustomCultureLocalizationDocumentItem : ExpressionDocumentItemBase,
		ToParsableStringDocumentVisitor.IStringVisitor
	{
		internal MorestachioCustomCultureLocalizationDocumentItem()
		{

		}

		/// <inheritdoc />
		protected MorestachioCustomCultureLocalizationDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}

		/// <inheritdoc />
		public MorestachioCustomCultureLocalizationDocumentItem(IMorestachioExpression expression)
		{
			MorestachioExpression = expression;
		}

		/// <summary>
		///		Get name of the changed culture in the <see cref="ScopeData.CustomData"/>
		/// </summary>
		public const string LocalizationCultureKey = "LocalizationService.CustomCulture";

		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			var oldCulture = context.Options.CultureInfo;
			if (scopeData.CustomData.TryGetValue(LocalizationCultureKey, out var customCulture) && customCulture is CultureInfo culInfo)
			{
				oldCulture = culInfo;
			}

			var expValue = (await MorestachioExpression.GetValue(context, scopeData)).Value;

			CultureInfo requestedCulture;
			if (expValue is CultureInfo culture)
			{
				requestedCulture = culture;
			}
			else
			{
				requestedCulture = CultureInfo.GetCultureInfo(expValue.ToString());
			}

			scopeData.CustomData[LocalizationCultureKey] = requestedCulture;

			var childs = Children.ToList();
			childs.Add(new ResetCultureDocumentItem(oldCulture));
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
			public ResetCultureDocumentItem(CultureInfo culture)
			{
				_culture = culture;
			}

			/// <inheritdoc />
			public override ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
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
			visitor.StringBuilder.Append("{{" + MorestachioCustomCultureLocalizationBlockProvider.OpenTag + visitor.ReparseExpression(MorestachioExpression) + "}}");
			visitor.VisitChildren(this);
			visitor.StringBuilder.Append("{{" + MorestachioCustomCultureLocalizationBlockProvider.CloseTag + "}}");
		}
	}
}