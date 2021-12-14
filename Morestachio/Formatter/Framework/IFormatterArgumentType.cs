using System.Collections.Generic;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Formatter.Framework;

internal interface IFormatterArgumentType
{
	IEnumerable<TokenPair> GetValue();
}