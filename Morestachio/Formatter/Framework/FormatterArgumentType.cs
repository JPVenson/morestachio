using System;

namespace Morestachio.Formatter.Framework
{
	public readonly struct FormatterArgumentType : IEquatable<FormatterArgumentType>
	{
		public FormatterArgumentType(int index, string name, Type value)
		{
			Index = index;
			Name = name;
			Type = value;
			Value = null;
		}
		public FormatterArgumentType(int index, string name, object value)
		{
			Index = index;
			Name = name;
			Type = value?.GetType();
			Value = value;
		}

		public int Index { get; }
		public string Name { get; }
		public Type Type { get; }
		public object Value { get; }

		/// <inheritdoc />
		public bool Equals(FormatterArgumentType other)
		{
			return Index == other.Index && Name == other.Name && Type == other.Type && Equals(Value, other.Value);
		}

		/// <inheritdoc />
		public override bool Equals(object obj)
		{
			return obj is FormatterArgumentType other && Equals(other);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Index;
				hashCode = (hashCode * 397) ^ (Name != null ? Name.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Value != null ? Value.GetHashCode() : 0);
				return hashCode;
			}
		}
	}
}