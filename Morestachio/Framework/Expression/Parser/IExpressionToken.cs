namespace Morestachio.Framework.Expression.Parser
{
	/// <summary>
	///		Represents a Expression token
	/// </summary>
	public interface IExpressionToken
	{
		/// <summary>
		///		Defines the type of token
		/// </summary>
		ExpressionTokenType TokenType { get; }

		/// <summary>
		///		Defines the location within the template of this token
		/// </summary>
		CharacterLocation Location { get; }
	}
}