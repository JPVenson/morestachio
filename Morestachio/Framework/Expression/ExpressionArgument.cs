using System;
using System.Text;
using System.Threading.Tasks;
using Morestachio.Framework.Expression.Renderer;

namespace Morestachio.Framework.Expression
{
	/// <summary>
	///		Defines an Argument used within a formatter
	/// </summary>
	public class ExpressionArgument : IEquatable<ExpressionArgument>
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="location"></param>
		public ExpressionArgument(CharacterLocation location)
		{
			Location = location;
		}

		/// <summary>
		///		The name of the Argument
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		///		The value of the Argument
		/// </summary>
		public IMorestachioExpression MorestachioExpression { get; set; }

		/// <summary>
		///		The Location within the Template
		/// </summary>
		public CharacterLocation Location { get; set; }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="contextObject"></param>
		/// <param name="scopeData"></param>
		/// <returns></returns>
		public async Task<ContextObject> GetValue(ContextObject contextObject, ScopeData scopeData)
		{
			return await MorestachioExpression.GetValue(contextObject, scopeData);
		}


		/// <inheritdoc />
		public override string ToString()
		{
			var sb = new StringBuilder();
			ExpressionRenderer.RenderExpression(this, sb);
			return sb.ToString();
		}
		
		/// <inheritdoc />
		public bool Equals(ExpressionArgument other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Name == other.Name && MorestachioExpression.Equals(other.MorestachioExpression) && Location.Equals(other.Location);
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

			return Equals((ExpressionArgument) obj);
		}
		
		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = (Name != null ? Name.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (MorestachioExpression != null ? MorestachioExpression.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Location != null ? Location.GetHashCode() : 0);
				return hashCode;
			}
		}
	}
}