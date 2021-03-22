using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Morestachio.Document.Custom;
using Morestachio.Framework.Tokenizing;
using Morestachio.Util.Sealing;

namespace Morestachio.Util
{
	internal class CustomDocumentList : SealableList<CustomDocumentItemProvider>, ICustomDocumentList
	{
		public virtual CustomDocumentItemProvider FindTokenProvider(string token)
		{
			return this.FirstOrDefault(e => e.ShouldTokenize(token));
		}
		public virtual CustomDocumentItemProvider FindTokenProvider(TokenPair currentToken, ParserOptions options, IEnumerable<ITokenOption> tokenOptions)
		{
			return this.FirstOrDefault(e => e.ShouldParse(currentToken, options, tokenOptions));
		}
	}

	/// <summary>
	///		Stores a number of <see cref="CustomDocumentItemProvider"/> and allows searching for matching providers for tokens
	/// </summary>
	public interface ICustomDocumentList : ISealed, IList<CustomDocumentItemProvider>
	{
		/// <summary>
		///		Searches for a provider that can tokenize a token
		/// </summary>
		CustomDocumentItemProvider FindTokenProvider(string token);

		/// <summary>
		///		Searches for a provider that can parse a token
		/// </summary>
		CustomDocumentItemProvider FindTokenProvider(TokenPair currentToken, ParserOptions options, IEnumerable<ITokenOption> tokenOptions);
	}
}
