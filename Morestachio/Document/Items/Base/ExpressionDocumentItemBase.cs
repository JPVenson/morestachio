using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using Morestachio.Analyzer.DataAccess;
using Morestachio.Document.Contracts;
using Morestachio.Framework;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Document.Items.Base;

/// <summary>
///     A common base class for emitting a single string value
/// </summary>
[Serializable]
public abstract class ExpressionDocumentItemBase : BlockDocumentItemBase, IEquatable<ExpressionDocumentItemBase>
{
	internal ExpressionDocumentItemBase()
	{
			
	}

	/// <param name="location"></param>
	/// <inheritdoc />
	protected ExpressionDocumentItemBase(in CharacterLocation location,
										IMorestachioExpression expression,
										IEnumerable<ITokenOption> tagCreationOptions) : base(location, tagCreationOptions)
	{
		MorestachioExpression = expression;
	}

	/// <inheritdoc />
	protected ExpressionDocumentItemBase(SerializationInfo info, StreamingContext c) : base(info, c)
	{
		MorestachioExpression =
			info.GetValue(nameof(MorestachioExpression), typeof(IMorestachioExpression)) as IMorestachioExpression;
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
	protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
	{
		base.SerializeBinaryCore(info, context);
		info.AddValue(nameof(MorestachioExpression), MorestachioExpression);
	}

	/// <inheritdoc />
	protected override void SerializeXml(XmlWriter writer)
	{
		writer.WriteExpressionToXml(MorestachioExpression);
		base.SerializeXml(writer);
	}

	/// <inheritdoc />
	protected override void DeSerializeXml(XmlReader reader)
	{
		reader.ReadStartElement();
		var subtree = reader.ReadSubtree();
		subtree.Read();
		MorestachioExpression = subtree.ParseExpressionFromKind();
		reader.Skip();
		base.DeSerializeXml(reader);
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

		return Equals((ExpressionDocumentItemBase) obj);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		var hashCode = base.GetHashCode();
		hashCode = (hashCode * 397) ^ MorestachioExpression.GetHashCode();
		return hashCode;
	}

	/// <inheritdoc />
	public override IEnumerable<string> Usage(UsageData data)
	{
		foreach (var usage in MorestachioExpression.InferExpressionUsage(data))
		{
			yield return usage;
		}

		foreach (var usage in base.Usage(data))
		{
			yield return usage;
		}
	}
}