using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Morestachio.Framework;

namespace Morestachio.Formatter
{
	/// <summary>
	///		An Argument for a Formatter
	/// </summary>
	[DebuggerDisplay("[{Name ?? 'Unnamed'}] {Argument}")]
	public class FormatterToken
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="FormatterToken"/> class.
		/// </summary>
		/// <param name="name">The name.</param>
		/// <param name="argument">The argument.</param>
		public FormatterToken(string name, FormatExpression argument)
		{
			Name = name;
			Argument = argument ?? new FormatExpression();
		}

		/// <summary>
		///		Ether the Name of the Argument or Null
		/// </summary>
		[CanBeNull]
		public string Name { get; set; }

		/// <summary>
		///		The value of the Argument
		/// </summary>
		[NotNull]
		public FormatExpression Argument { get; set; }
	}

	public class FormatExpression
	{
		public FormatExpression()
		{
		}

		public string OrigialString { get; set; }
		internal IFormatterArgumentType ParsedArguments { get; set; }
	}

	internal class ConstFormatterArgumentValue : IFormatterArgumentType
	{
		private readonly string _argument;

		public ConstFormatterArgumentValue(string argument)
		{
			_argument = argument;
		}

		public IEnumerable<TokenPair> GetValue()
		{
			yield return new TokenPair(TokenType.Content, _argument, new Tokenizer.CharacterLocation());
		}
	}

	internal class PathFormatterArgumentValue : IFormatterArgumentType
	{
		private readonly IEnumerable<TokenPair> _tokenizeString;

		public PathFormatterArgumentValue(IEnumerable<TokenPair> tokenizeString)
		{
			_tokenizeString = tokenizeString;
		}

		public IEnumerable<TokenPair> GetValue()
		{
			return _tokenizeString;
		}
	}
}