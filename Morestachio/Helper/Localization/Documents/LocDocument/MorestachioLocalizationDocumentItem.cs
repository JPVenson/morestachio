using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Morestachio.Document;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;
using Morestachio.Helper.Localization.Documents.LocPDocument;
#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using ObjectPromise = System.Threading.Tasks.ValueTask<object>;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using ObjectPromise = System.Threading.Tasks.Task<object>;
#endif
namespace Morestachio.Helper.Localization.Documents.LocDocument
{
	/// <summary>
	///		Allows the usage of {{#loc expression}} in combination with an <see cref="IMorestachioLocalizationService"/>
	/// </summary>
	[System.Serializable]
	public class MorestachioLocalizationDocumentItem : ExpressionDocumentItemBase,
		ToParsableStringDocumentVisitor.IStringVisitor
	{
		internal MorestachioLocalizationDocumentItem() : base(CharacterLocation.Unknown, null)
		{

		}

		/// <inheritdoc />
		public MorestachioLocalizationDocumentItem(CharacterLocation location, IMorestachioExpression value) : base(location, value)
		{
		}

		/// <inheritdoc />
		protected MorestachioLocalizationDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}

		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream,
			ContextObject context,
			ScopeData scopeData)
		{
			var service = context.Options.Formatters.GetService(typeof(IMorestachioLocalizationService)) as IMorestachioLocalizationService;
			if (service == null)
			{
				outputStream.Write("IMorestachioLocalizationService not registered");
				return Enumerable.Empty<DocumentItemExecution>();
			}

			var translationOrNull = await GetTranslation(context, scopeData, service);
			outputStream.Write(translationOrNull?.ToString());
			return Enumerable.Empty<DocumentItemExecution>();
		}

		private async Task<object> GetTranslation(ContextObject context, ScopeData scopeData, IMorestachioLocalizationService service)
		{
			var valueContext = await MorestachioExpression.GetValue(context, scopeData);

			var culture = context.Options.CultureInfo;
			if (scopeData.CustomData.TryGetValue(MorestachioCustomCultureLocalizationDocumentItem.LocalizationCultureKey,
				out var customCulture) && customCulture is CultureInfo culInfo)
			{
				culture = culInfo;
			}

			var args = Children
				.OfType<MorestachioLocalizationParameterDocumentItem>()
				.Cast<ExpressionDocumentItemBase>()
				.Select(f =>
					new Func<ObjectPromise>(async () =>
						(await f.MorestachioExpression.GetValue(context, scopeData)).Value))
				.Concat(Children
					.OfType<MorestachioLocalizationDocumentItem>()
					.Select(f =>
						new Func<ObjectPromise>(async () => await f.GetTranslation(context, scopeData, service))))
				.ToArray();

			var arguments = new object[args.Length];
			for (var index = 0; index < args.Length; index++)
			{
				var parameters = args[index];
				arguments[index] = (await parameters());
			}

			return service.GetTranslationOrNull(await valueContext.RenderToString(), culture, arguments);
		}

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}

		/// <inheritdoc />
		public void Render(ToParsableStringDocumentVisitor visitor)
		{
			if (!Children.Any())
			{
				visitor.StringBuilder.Append("{{" + MorestachioLocalizationTagProvider.OpenTag + visitor.ReparseExpression(MorestachioExpression) + "}}");
			}
			else
			{
				visitor.StringBuilder.Append("{{" + MorestachioLocalizationBlockProvider.OpenTag + visitor.ReparseExpression(MorestachioExpression) + "}}");
				visitor.VisitChildren(this);
				visitor.StringBuilder.Append("{{" + MorestachioLocalizationBlockProvider.CloseTag + "}}");
			}
		}
	}
}