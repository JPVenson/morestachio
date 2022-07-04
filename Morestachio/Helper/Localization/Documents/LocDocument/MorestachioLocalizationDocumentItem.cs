using System.Globalization;
using System.Xml;
using Morestachio.Document;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper.Localization.Documents.CustomCultureDocument;
using Morestachio.Helper.Localization.Documents.LocPDocument;
using Morestachio.Helper.Serialization;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Helper.Localization.Documents.LocDocument;

/// <summary>
///		Allows the usage of {{#loc expression}} in combination with an <see cref="IMorestachioLocalizationService"/>
/// </summary>
[System.Serializable]
public class MorestachioLocalizationDocumentItem : BlockDocumentItemBase,
													ToParsableStringDocumentVisitor.IStringVisitor, IEquatable<MorestachioLocalizationDocumentItem>
{

	internal MorestachioLocalizationDocumentItem()
	{

	}

	/// <inheritdoc />
	public MorestachioLocalizationDocumentItem(TextRange location,
												IMorestachioExpression value,
												IMorestachioExpression explicitCulture,
												IEnumerable<ITokenOption> tagCreationOptions) : base(location, tagCreationOptions)
	{
		ExplicitCulture = explicitCulture;
		MorestachioExpression = value;
	}

	/// <inheritdoc />
	protected MorestachioLocalizationDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
	{
		ExplicitCulture = info.GetValueOrDefault<IMorestachioExpression>(c, nameof(ExplicitCulture));
	}

	/// <summary>
	///		If set gets the explicitly declared culture for this translation
	/// </summary>
		
	public IMorestachioExpression ExplicitCulture { get; private set; }

	/// <summary>
	///     The Expression to be evaluated
	/// </summary>
	public IMorestachioExpression MorestachioExpression { get; protected set; }

	/// <inheritdoc />
	public override async ItemExecutionPromise Render(IByteCounterStream outputStream,
													ContextObject context,
													ScopeData scopeData)
	{
		var service =
			scopeData.ParserOptions.Formatters.Services.GetService(typeof(IMorestachioLocalizationService)) as
				IMorestachioLocalizationService;
		if (service == null)
		{
			outputStream.Write("IMorestachioLocalizationService not registered");
			return Enumerable.Empty<DocumentItemExecution>();
		}

		var translationOrNull = await GetTranslation(context, scopeData, service).ConfigureAwait(false);
		outputStream.Write(translationOrNull?.ToString());
		return Enumerable.Empty<DocumentItemExecution>();
	}

	private async ObjectPromise GetTranslation(ContextObject context, ScopeData scopeData, IMorestachioLocalizationService service)
	{
		var valueContext = await MorestachioExpression.GetValue(context, scopeData).ConfigureAwait(false);
		var culture = scopeData.ParserOptions.CultureInfo;

		if (ExplicitCulture != null)
		{
			var cultureValue = (await ExplicitCulture.GetValue(context, scopeData).ConfigureAwait(false)).Value;
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
					(await f.MorestachioExpression.GetValue(context, scopeData).ConfigureAwait(false)).Value))
			.Concat(Children
				.OfType<MorestachioLocalizationDocumentItem>()
				.Select(f =>
					new Func<ObjectPromise>(async () => await f.GetTranslation(context, scopeData, service).ConfigureAwait(false))))
			.ToArray();

		var arguments = new object[args.Length];
		for (var index = 0; index < args.Length; index++)
		{
			var parameters = args[index];
			arguments[index] = (await parameters().ConfigureAwait(false));
		}

		return service.GetTranslationOrNull(valueContext.RenderToString(scopeData).ToString(), culture, arguments);
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
			
		visitor.StringBuilder.Append(MorestachioExpression.AsStringExpression());
		if (ExplicitCulture != null)
		{
			visitor.StringBuilder.Append(" #CULTURE ");
			visitor.StringBuilder.Append(ExplicitCulture.AsStringExpression());
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

	/// <inheritdoc />
	protected override void SerializeXmlBodyCore(XmlWriter writer)
	{
		base.SerializeXmlBodyCore(writer);
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

	/// <inheritdoc />
	protected override void DeSerializeXmlBodyCore(XmlReader reader)
	{
		base.DeSerializeXmlBodyCore(reader);

		if (reader.NodeType == XmlNodeType.Element && reader.Name == GetSerializedMarkerName(GetType()))
		{
			reader.ReadStartElement(); //Path
		}
		var subTree = reader.ReadSubtree();
		subTree.ReadStartElement();
		MorestachioExpression = subTree.ParseExpressionFromKind();
		reader.Skip();
		//reader.ReadEndElement();//Path
		if (reader.Name == nameof(ExplicitCulture))
		{
			reader.ReadStartElement(); //nameof(ExplicitCulture)
			var subtree = reader.ReadSubtree();
			subtree.Read();
			ExplicitCulture = subtree.ParseExpressionFromKind();
			reader.Skip();
			reader.ReadEndElement(); //nameof(ExplicitCulture)
		}
	}
		
	/// <inheritdoc />
	protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
	{
		info.AddValue(nameof(ExplicitCulture), ExplicitCulture);
		base.SerializeBinaryCore(info, context);
	}
		
	/// <inheritdoc />
	public bool Equals(MorestachioLocalizationDocumentItem other)
	{
		if (ReferenceEquals(null, other))
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		return base.Equals(other) && Equals(ExplicitCulture, other.ExplicitCulture);
	}
		
	/// <inheritdoc />
	public override bool Equals(object obj)
	{
		if (ReferenceEquals(null, obj))
		{
			return false;
		}

		if (ReferenceEquals(this, obj))
		{
			return true;
		}

		if (obj.GetType() != this.GetType())
		{
			return false;
		}

		return Equals((MorestachioLocalizationDocumentItem) obj);
	}
		
	/// <inheritdoc />
	public override int GetHashCode()
	{
		unchecked
		{
			return (base.GetHashCode() * 397) ^ (ExplicitCulture != null ? ExplicitCulture.GetHashCode() : 0);
		}
	}
}