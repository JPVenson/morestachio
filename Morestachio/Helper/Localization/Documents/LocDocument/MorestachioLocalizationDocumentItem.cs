using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using JetBrains.Annotations;
using Morestachio.Document;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;
using Morestachio.Helper.Localization.Documents.CustomCultureDocument;
using Morestachio.Helper.Localization.Documents.LocPDocument;
#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using ObjectPromise = System.Threading.Tasks.ValueTask<object>;
using Promise = System.Threading.Tasks.ValueTask;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using ObjectPromise = System.Threading.Tasks.Task<object>;
using Promise = System.Threading.Tasks.Task;
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
		public MorestachioLocalizationDocumentItem(CharacterLocation location, IMorestachioExpression value, [CanBeNull] IMorestachioExpression explicitCulture = null) : base(location, value)
		{
			ExplicitCulture = explicitCulture;
		}

		/// <inheritdoc />
		protected MorestachioLocalizationDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			ExplicitCulture = info.GetValue(nameof(ExplicitCulture), typeof(IMorestachioExpression)) as IMorestachioExpression;
		}

		/// <summary>
		///		If set gets the explicitly declared culture for this translation
		/// </summary>
		[CanBeNull]
		public IMorestachioExpression ExplicitCulture { get; private set; }

		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream,
			ContextObject context,
			ScopeData scopeData)
		{
			var service =
				context.Options.Formatters.GetService(typeof(IMorestachioLocalizationService)) as
					IMorestachioLocalizationService;
			if (service == null)
			{
				outputStream.Write("IMorestachioLocalizationService not registered");
				return Enumerable.Empty<DocumentItemExecution>();
			}

			var translationOrNull = await GetTranslation(context, scopeData, service);
			outputStream.Write(translationOrNull?.ToString());
			return Enumerable.Empty<DocumentItemExecution>();
		}

		private async ObjectPromise GetTranslation(ContextObject context, ScopeData scopeData, IMorestachioLocalizationService service)
		{
			var valueContext = await MorestachioExpression.GetValue(context, scopeData);
			var culture = context.Options.CultureInfo;

			if (ExplicitCulture != null)
			{
				var cultureValue = (await ExplicitCulture.GetValue(context, scopeData)).Value;
				if (cultureValue is CultureInfo cul)
				{
					culture = cul;
				}
				else if (cultureValue is string strCul)
				{
					culture = new CultureInfo(strCul);
				}
			}
			else
			{
				if (scopeData.CustomData.TryGetValue(MorestachioCustomCultureLocalizationDocumentItem.LocalizationCultureKey,
					out var customCulture) && customCulture is CultureInfo culInfo)
				{
					culture = culInfo;
				}	
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
			visitor.StringBuilder.Append("{{");
			if (Children.Any())
			{
				visitor.StringBuilder.Append(MorestachioLocalizationBlockProvider.OpenTag);
			}
			else
			{
				visitor.StringBuilder.Append(MorestachioLocalizationTagProvider.OpenTag);
			}
			
			visitor.StringBuilder.Append(visitor.ReparseExpression(MorestachioExpression));
			if (ExplicitCulture != null)
			{
				visitor.StringBuilder.Append(" #CULTURE ");
				visitor.StringBuilder.Append(visitor.ReparseExpression(ExplicitCulture));
			}
			visitor.StringBuilder.Append("}}");

			if (!Children.Any())
			{
				return;
			}

			visitor.VisitChildren(this);
			visitor.StringBuilder.Append("{{");
			visitor.StringBuilder.Append(MorestachioLocalizationBlockProvider.CloseTag);
			visitor.StringBuilder.Append("}}");
		}

		protected override void DeSerializeXml(XmlReader reader)
		{
			reader.ReadStartElement();
			base.DeSerializeXml(reader.ReadSubtree());
			reader.Skip();
			if (reader.Name == nameof(ExplicitCulture))
			{
				reader.ReadStartElement();
				var subtree = reader.ReadSubtree();
				subtree.Read();
				ExplicitCulture = subtree.ParseExpressionFromKind();
				reader.Skip();
			}
		}

		protected override void SerializeXml(XmlWriter writer)
		{
			writer.WriteStartElement("Path");
			writer.WriteExpressionToXml(MorestachioExpression);
			writer.WriteEndElement();

			if (ExplicitCulture == null)
			{
				return;
			}
			writer.WriteStartElement("ExplicitCulture");
			writer.WriteExpressionToXml(ExplicitCulture);
			writer.WriteEndElement();
		}

		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(ExplicitCulture), ExplicitCulture);
			base.SerializeBinaryCore(info, context);
		}
	}
}