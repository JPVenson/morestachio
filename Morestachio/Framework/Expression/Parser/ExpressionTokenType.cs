namespace Morestachio.Framework.Expression.Parser
{
	/// <summary>
	///		Defines all possible tokens that can be tokenized by the <see cref="ExpressionParser"/>
	/// </summary>
	public enum ExpressionTokenType
	{
		/// <summary>
		///		An path pointing to a property or formatter. 	
		/// </summary>
		Path,

		/// <summary>
		///		Defines a seperator for two formatter arguments
		/// </summary>
		ArgumentSeperator,

		/// <summary>
		///		Defines the start of an named formatter argument
		/// </summary>
		Argument,

		/// <summary>
		///		Defines a bracket used ether for seperation or a formatter call 
		/// </summary>
		Bracket,

		/// <summary>
		///		Defines a number
		/// </summary>
		Number,

		/// <summary>
		///		Defines a string
		/// </summary>
		String,

		/// <summary>
		///		Defines the use of an operator
		/// </summary>
		Operator,

		/// <summary>
		///		Defines the use of an operator
		/// </summary>
		LambdaOperator,
	}
}