using System.Collections.Generic;

namespace Morestachio.Framework.Tokenizing
{
	/// <summary>
	///		Contains the result of an Tokenizer
	/// </summary>
	public class TokenizerResult
	{
		/// <summary>
		/// 
		/// </summary>
		public TokenizerResult(IEnumerable<TokenPair> tokens)
		{
			Tokens = tokens;
		}

		/// <summary>
		///		The Tokenized template
		/// </summary>
		public IEnumerable<TokenPair> Tokens { get; set; }
	}
}