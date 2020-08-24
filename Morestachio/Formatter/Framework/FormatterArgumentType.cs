using System;
using JetBrains.Annotations;

namespace Morestachio.Formatter.Framework
{
	/// <summary>
	///		Contains the meta data for an argument
	/// </summary>
	public readonly struct FormatterArgumentType : IEquatable<FormatterArgumentType>
	{
		/// <summary>
		///		
		/// </summary>
		public FormatterArgumentType(int index, string name, Type value)
		{
			Index = index;
			Name = name;
			Type = value;
			Value = null;
		}
		
		/// <summary>
		///		
		/// </summary>
		public FormatterArgumentType(int index, string name, object value)
		{
			Index = index;
			Name = name;
			Type = value?.GetType();
			Value = value;
		}

		/// <summary>
		///		The index of the argument
		/// </summary>
		public int Index { get; }

		/// <summary>
		///		The name of the argument
		/// </summary>
		public string Name { get; }

		/// <summary>
		///		The declared type of the object
		/// </summary>
		public Type Type { get; }

		/// <summary>
		///		if present, the known value
		/// </summary>
		[CanBeNull]
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