using System.Xml;
using Morestachio.Analyzer.DataAccess;
using Morestachio.Document.Contracts;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper.Serialization;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Document.Items.Base;

/// <summary>
///     A common base class for emitting a single string value for a Tag.
/// </summary>
[Serializable]
public abstract class ExpressionDocumentItemBase
	: DocumentItemBase,
	IEquatable<ExpressionDocumentItemBase>,
	IReportUsage
{
	internal ExpressionDocumentItemBase()
	{
	}

	/// <param name="location"></param>
	/// <inheritdoc />
	protected ExpressionDocumentItemBase(
		in TextRange location,
		IMorestachioExpression expression,
		IEnumerable<ITokenOption> tagCreationOptions
	) : base(location, tagCreationOptions)
	{
		MorestachioExpression = expression;
	}

	/// <inheritdoc />
	protected ExpressionDocumentItemBase(SerializationInfo info, StreamingContext c) : base(info, c)
	{
		MorestachioExpression = info.GetValueOrDefault<IMorestachioExpression>(c, nameof(MorestachioExpression));
	}

	/// <summary>
	///     The Expression to be evaluated
	/// </summary>
	public IMorestachioExpression MorestachioExpression { get; protected set; }

	/// <inheritdoc />
	public bool Equals(ExpressionDocumentItemBase other)
	{
		if (ReferenceEquals(null, other))
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		return base.Equals(other) && MorestachioExpression.Equals(other.MorestachioExpression);
	}

	/// <inheritdoc />
	public virtual void ReportUsage(UsageData data)
	{
		var inferedExpressionUsage = MorestachioExpression.GetInferedExpressionUsage(data);
		inferedExpressionUsage?.Parent?.AddDependent(inferedExpressionUsage);
	}

	/// <inheritdoc />
	protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
	{
		base.SerializeBinaryCore(info, context);
		info.AddValue(nameof(MorestachioExpression), MorestachioExpression, typeof(IMorestachioExpression));
	}

	/// <inheritdoc />
	protected override void SerializeXmlBodyCore(XmlWriter writer)
	{
		base.SerializeXmlBodyCore(writer);
		writer.WriteExpressionToXml(MorestachioExpression);
	}

	/// <inheritdoc />
	protected override void DeSerializeXmlBodyCore(XmlReader reader)
	{
		base.DeSerializeXmlBodyCore(reader);

		if (reader.NodeType == XmlNodeType.Element)
		{
			var subtree = reader.ReadSubtree();
			subtree.Read();
			MorestachioExpression = subtree.ParseExpressionFromKind();
			reader.Skip();
		}
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

		if (obj.GetType() != GetType())
		{
			return false;
		}

		return Equals((ExpressionDocumentItemBase)obj);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		var hashCode = base.GetHashCode();
		hashCode = (hashCode * 397) ^ MorestachioExpression.GetHashCode();

		return hashCode;
	}
}