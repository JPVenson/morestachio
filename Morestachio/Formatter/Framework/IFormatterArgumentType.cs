using System.Collections.Generic;
using Morestachio.Framework;

namespace Morestachio.Formatter
{
	internal interface IFormatterArgumentType
	{
		IEnumerable<TokenPair> GetValue();
	}
}