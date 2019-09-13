using System.Collections.Generic;
using Morestachio.Framework;

namespace Morestachio.Formatter
{
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