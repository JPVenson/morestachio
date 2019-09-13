using System.Collections.Generic;
using Morestachio.Framework;

namespace Morestachio.Formatter
{
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
}