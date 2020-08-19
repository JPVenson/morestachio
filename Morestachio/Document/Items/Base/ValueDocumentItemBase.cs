using System;
using System.Runtime.Serialization;
using System.Xml;

namespace Morestachio.Document.Items.Base
{
	/// <summary>
	///		A common base class for emitting a single string value
	/// </summary>
	[System.Serializable]
	public abstract class ValueDocumentItemBase : DocumentItemBase, IEquatable<ValueDocumentItemBase>
	{
		protected ValueDocumentItemBase()
		{

		}

		/// <summary>
		///		A value from the Template
		/// </summary>
		public string Value { get; protected set; }
		
		/// <inheritdoc />
		protected ValueDocumentItemBase(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			Value = info.GetString(nameof(Value));
		}
		
		/// <inheritdoc />
		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			base.SerializeBinaryCore(info, context);
			info.AddValue(nameof(Value), Value);
		}
		
		/// <inheritdoc />
		protected override void SerializeXml(XmlWriter writer)
		{
			if (string.IsNullOrEmpty(Value))
			{
				return;
			}
			writer.WriteStartElement(nameof(Value));
			writer.WriteAttributeString("xml", "space", null, "preserve");
			writer.WriteString(Value);
			writer.WriteEndElement();
		}
		
		/// <inheritdoc />
		protected override void DeSerializeXml(XmlReader reader)
		{
			reader.ReadStartElement();
			if (reader.Name == nameof(Value))
			{
				if (reader.IsEmptyElement)
				{
					return;
				}
				//reader.ReadToFollowing(nameof(Value));
				Value = reader.ReadString();
			}
		}
		
		/// <inheritdoc />
		public bool Equals(ValueDocumentItemBase other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return base.Equals(other) &&
				   ((string.IsNullOrEmpty(Value) && string.IsNullOrEmpty(other.Value))
					   || Value == other.Value);
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

			return Equals((ValueDocumentItemBase)obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			int hashCode = base.GetHashCode();
			hashCode = (hashCode * 397) ^ (!string.IsNullOrWhiteSpace(Value) ? Value.GetHashCode() : 0);
			return hashCode;
		}
	}
}