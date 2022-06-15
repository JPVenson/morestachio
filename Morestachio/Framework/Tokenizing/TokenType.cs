namespace Morestachio.Framework.Tokenizing;

/// <summary>
///     The type of token produced in the lexing stage of template compilation.
/// </summary>
[Flags]
public enum TokenType : long
{
	/// <summary>
	///		A path that should be printed. If it contains any html tags, those will be escaped
	/// </summary>
	EscapedSingleValue = 1 << 0,

	/// <summary>
	///		A path that should be printed. 
	/// </summary>
	UnescapedSingleValue = 1 << 1,

	/// <summary>
	///		Defines the start of a scope that will only be applied if the value of the path is falsy
	/// </summary>
	InvertedElementOpen = 1 << 2,
	/// <summary>
	///		Defines the start of a scope
	/// </summary>
	ElementOpen = 1 << 3,
	/// <summary>
	///		Defines the end of a scope
	/// </summary>
	ElementClose = 1 << 4,

	/// <summary>
	///		A comment inside. Will not be printed
	/// </summary>
	Comment = 1 << 5,
	/// <summary>
	///		A comment inside. Will not be printed
	/// </summary>
	BlockComment = 1 << 6,

	/// <summary>
	///		Plain content. No further processing
	/// </summary>
	Content = 1 << 7,

	/// <summary>
	///		The start of a collection loop
	/// </summary>
	CollectionOpen = 1 << 8,

	/// <summary>
	///		The end of a collection loop
	/// </summary>
	CollectionClose = 1 << 9,

	/// <summary>
	///     Contains information about the formatting of the values. Must be followed by PrintFormatted or CollectionOpen
	/// </summary>
	Format = 1 << 10,

	/// <summary>
	///     Is used to "print" the current formatted value to the output
	/// </summary>
	Print = 1 << 11,

	/// <summary>
	///		A Partial that is inserted into the one or multiple places in the Template
	/// </summary>
	PartialDeclarationOpen = 1 << 12,

	/// <summary>
	///		End of a Partial
	/// </summary>
	PartialDeclarationClose = 1 << 13,

	/// <summary>
	///		Defines the place for rendering a single partial [Obsolete]
	/// </summary>
	RenderPartial = 1 << 14,

	/// <summary>
	///		Defines the place for rendering a single partial
	/// </summary>
	ImportPartial = 1 << 15,

	/// <summary>
	///		Defines the context an import should be set to
	/// </summary>
	ImportPartialContext = 1 << 16,

	/// <summary>
	///		Defines the current Context as the be accessed by an alias
	/// </summary>
	Alias = 1 << 17,

	/// <summary>
	///		Defines an switch statement. Must be used in conjunction with an nested case statement.
	/// </summary>
	SwitchOpen = 1 << 18,

	/// <summary>
	///		Defines the #ScopeTo keyword inline with a switch
	/// </summary>
	SwitchOptionScopeTo = 1 << 19,

	/// <summary>
	///		Defines an case statement. Must be used in conjunction with an enclosing Switch statement.
	/// </summary>
	SwitchCaseOpen = 1 << 20,
		
	/// <summary>
	///		Defines an default statement. Must be used in conjunction with an enclosing Switch statement.
	/// </summary>
	SwitchDefaultOpen = 1 << 21,

	/// <summary>
	///		Defines the end of a switch-scope
	/// </summary>
	SwitchClose = 1 << 22,

	/// <summary>
	///		Defines the end of a switch-case-scope
	/// </summary>
	SwitchCaseClose = 1 << 23,
		
	/// <summary>
	///		Defines the end of a switch-default-scope
	/// </summary>
	SwitchDefaultClose = 1 << 24,

	/// <summary>
	///		Defines an if. It Works the same as the "#" keyword but does not scope its body to it.
	/// </summary>
	If = 1 << 25,
	/// <summary>
	///		Defines the end of a if-scope
	/// </summary>
	IfClose = 1 << 26,
	/// <summary>
	///		Defines an inverted If. Works the same as the "^" keyword but does not scope its body to it
	/// </summary>
	IfNot = 1 << 27,
	/// <summary>
	///		Defines an Else. An else can only be used when its an direct descendent of an if
	/// </summary>
	Else = 1 << 28,
	/// <summary>
	///		Defines the end of an else-scope
	/// </summary>
	ElseClose = 1 << 29,
	/// <summary>
	///		Defines an if within the Else of an if. An elseif can only be used when its an direct descendent of an if
	/// </summary>
	ElseIf = 1 << 30,
	/// <summary>
	///		Defines the end of an else-scope
	/// </summary>
	ElseIfClose = 1L << 31,
	/// <summary>
	///		Defines the current Context as the be accessed by an alias
	/// </summary>
	VariableVar = 1L << 32,
	/// <summary>
	///		Defines the current Context as the be accessed by an alias
	/// </summary>
	VariableLet = 1L << 33,

	/// <summary>
	///		The start of a while loop
	/// </summary>
	WhileLoopOpen = 1L << 34,

	/// <summary>
	///		The end of a while loop
	/// </summary>
	WhileLoopClose = 1L << 35,

	/// <summary>
	///		The start of a while loop
	/// </summary>
	DoLoopOpen = 1L << 36,

	/// <summary>
	///		The end of a while loop
	/// </summary>
	DoLoopClose = 1L << 37,

	/// <summary>
	///		Writes a linebreak on the given position
	/// </summary>
	WriteLineBreak = 1L << 38,

	/// <summary>
	///		Trims one linebreak from the following content at its start
	/// </summary>
	TrimLineBreak = 1L << 39,

	/// <summary>
	///		Trims all occuring linebreaks from the following content at its start
	/// </summary>
	TrimLineBreaks = 1L << 40,

	/// <summary>
	///		Trims all occuring linebreaks from the following content at its start
	/// </summary>
	TrimPrependedLineBreaks = 1L << 41,

	/// <summary>
	///		Trims all occuring \t &amp; \r\n 
	/// </summary>
	TrimEverything = 1L << 42,

	///// <summary>
	/////		stops Trimming all occuring \t & \r\n 
	///// </summary>
	//StopTrimEverything,
		

	/// <summary>
	///		The start of a while loop
	/// </summary>
	RepeatLoopOpen = 1L << 43,

	/// <summary>
	///		The end of a while loop
	/// </summary>
	RepeatLoopClose = 1L << 44,

	/// <summary>
	///		The start of an Isolation scope
	/// </summary>
	IsolationScopeOpen = 1L << 45,

	/// <summary>
	///		The end of an Isolation Scope
	/// </summary>
	IsolationScopeClose = 1L << 46,

	/// <summary>
	///		The start of a Foreach collection loop
	/// </summary>
	ForeachCollectionOpen = 1L << 47,

	/// <summary>
	///		The end of a Foreach collection loop
	/// </summary>
	ForeachCollectionClose = 1L << 48,

	/// <summary>
	///		The start of a NOPRINT block
	/// </summary>
	NoPrintOpen = 1L << 49,
	/// <summary>
	///		The end of a NOPRINT block
	/// </summary>
	NoPrintClose = 1L << 50,
}

/// <summary>
///		Helper Methods for enum type <see cref="TokenType"/>
/// </summary>
public static class TokenTypeExtensions
{
	/// <summary>
	///		Checks for flag presence in value
	/// </summary>
	/// <param name="value"></param>
	/// <param name="flag"></param>
	/// <returns></returns>
	public static bool HasFlagFast(this TokenType value, TokenType flag)
	{
		return (value & flag) != 0;
	}
}