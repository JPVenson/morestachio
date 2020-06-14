using System;
using System.Runtime.Serialization;
using System.Xml;
using Morestachio.Framework.Expression;

namespace Morestachio.Document
{
	/// <summary>
	///		A common base class for emitting a single string value
	/// </summary>
	[System.Serializable]
	public abstract class ExpressionDocumentItemBase : DocumentItemBase, IEquatable<ExpressionDocumentItemBase>
	{
		/// <inheritdoc />
		protected ExpressionDocumentItemBase()
		{

		}

		/// <summary>
		///		The Expression to be evaluated
		/// </summary>
		public IMorestachioExpression MorestachioExpression { get; protected set; }

		/// <inheritdoc />
		protected ExpressionDocumentItemBase(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			MorestachioExpression = info.GetValue(nameof(MorestachioExpression), typeof(IMorestachioExpression)) as IMorestachioExpression;
		}

		/// <inheritdoc />
		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			base.SerializeBinaryCore(info, context);
			info.AddValue(nameof(MorestachioExpression), MorestachioExpression);
			base.SerializeBinaryCore(info, context);
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
			if (reader.Name == ExpressionTokenizer.ExpressionNodeName)
			{
				var subtree = reader.ReadSubtree();
				subtree.Read();
				MorestachioExpression = subtree.ParseExpressionFromKind();
				reader.Skip();
			}
			base.DeSerializeXml(reader);
		}
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

			return Equals((ExpressionDocumentItemBase)obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			int hashCode = base.GetHashCode();
			hashCode = (hashCode * 397) ^ (MorestachioExpression.GetHashCode());
			return hashCode;
		}
	}
}