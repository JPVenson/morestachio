namespace Morestachio.Framework
{
	/// <summary>
	///     The type of token produced in the lexing stage of template compilation.
	/// </summary>
	internal enum TokenType
	{
		EscapedSingleValue,
		UnescapedSingleValue,
		InvertedElementOpen,
		/// <summary>
		///		Defines the start of a scope
		/// </summary>
		ElementOpen,
		/// <summary>
		///		Defines the end of a scope
		/// </summary>
		ElementClose,
		Comment,
		Content,
		CollectionOpen,
		CollectionClose,

		/// <summary>
		///     Contains information about the formatting of the values. Must be followed by PrintFormatted or CollectionOpen
		/// </summary>
		Format,

		/// <summary>
		///     Is used to "print" the current formatted value to the output
		/// </summary>
		Print,

		/// <summary>
		///		A Partial that is inserted into the one or multiple places in the Template
		/// </summary>
		PartialDeclarationOpen,

		/// <summary>
		///		End of a Partial
		/// </summary>
		PartialDeclarationClose,

		/// <summary>
		///		Defines the place for rendering a single partial
		/// </summary>
		RenderPartial,

		/// <summary>
		///		Defines the current Context as the be accessed by an alias
		/// </summary>
		Alias,

		/// <summary>
		///		Defines an if. It Works the same as the "#" keyword but does not scope its body to it.
		/// </summary>
		If,
		/// <summary>
		///		Defines the end of a if-scope
		/// </summary>
		IfClose,
	}
}