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
		public IExpression Expression { get; protected set; }

		/// <inheritdoc />
		protected ExpressionDocumentItemBase(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			Expression = info.GetValue(nameof(Expression), typeof(IExpression)) as IExpression;
		}

		/// <inheritdoc />
		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			base.SerializeBinaryCore(info, context);
			info.AddValue(nameof(Expression), Expression);
			base.SerializeBinaryCore(info, context);
		}

		/// <inheritdoc />
		protected override void SerializeXml(XmlWriter writer)
		{
			writer.WriteExpressionToXml(Expression);
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
				Expression = subtree.ParseExpressionFromKind();
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

			return base.Equals(other) && Expression.Equals(other.Expression);
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
			hashCode = (hashCode * 397) ^ (Expression.GetHashCode());
			return hashCode;
		}
	}
}