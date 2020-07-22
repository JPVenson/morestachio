using System;

namespace Morestachio.Framework.Expression.StringParts
{
	/// <summary>
	///		A constant part of an string
	/// </summary>
	public class ExpressionStringConstPart : IEquatable<ExpressionStringConstPart>
	{
		/// <summary>
		///		The content of the Text Part
		/// </summary>
		public string PartText { get; set; }

		/// <summary>
		///		Where in the string is this part located
		/// </summary>
		public CharacterLocation Location { get; set; }

		/// <inheritdoc />
		public bool Equals(ExpressionStringConstPart other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return PartText == other.PartText && Location.Equals(other.Location);
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

			return Equals((ExpressionStringConstPart)obj);
		}

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				return ((PartText != null ? PartText.GetHashCode() : 0) * 397) ^ (Location != null ? Location.GetHashCode() : 0);
			}
		}
	}
}