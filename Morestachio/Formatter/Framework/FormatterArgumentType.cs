using System;
using Morestachio.Framework.Expression;

namespace Morestachio.Formatter.Framework;

/// <summary>
///		Contains the meta data for an argument
/// </summary>
public class FormatterArgumentType : IEquatable<FormatterArgumentType>
{
	/// <summary>
	///		Is used only for source object evaluation
	/// </summary>
	public FormatterArgumentType(int index, string name, Type value)
	{
		Index = index;
		Name = name;
		Type = value;
		Value = null;
	}
		
	/// <summary>
	///		Is used to declare a formatter argument
	/// </summary>
	public FormatterArgumentType(int index, string name, object value, IMorestachioExpression expression)
	{
		Index = index;
		Name = name;
		Type = value?.GetType();
		Value = value;
		Expression = expression;
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
	public object Value { get; }

	/// <summary>
	///		Defines the expression that value originates from
	/// </summary>
	public IMorestachioExpression Expression { get; }

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