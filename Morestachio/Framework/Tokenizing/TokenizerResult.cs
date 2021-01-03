using System;
using System.Collections;
using System.Collections.Generic;

namespace Morestachio.Framework.Tokenizing
{
	/// <summary>
	///		Contains the result of an Tokenizer
	/// </summary>
	public class TokenizerResult : ITokenizerResult
	{
		private TokenPair _current;

		/// <summary>
		/// 
		/// </summary>
		public TokenizerResult(IList<TokenPair> tokens)
		{
			Tokens = tokens;
		}

		/// <summary>
		///		The Tokenized template
		/// </summary>
		IList<TokenPair> Tokens { get; set; }

		int CurrentIndex { get; set; }

		internal TokenPair? Next
		{
			get
			{
				if (CurrentIndex >= Tokens.Count)
				{
					return null;
				}

				return Tokens[CurrentIndex];
			}
		}

		internal TokenPair? Previous
		{
			get
			{
				if (CurrentIndex - 2 < 0)
				{
					return null;
				}

				return Tokens[CurrentIndex - 2];
			}
		}

		internal void MoveCursor(int by)
		{
			CurrentIndex += by;
		}

		IEnumerator<TokenPair> IEnumerable<TokenPair>.GetEnumerator()
		{
			((IEnumerator)this).Reset();
			return this;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			((IEnumerator)this).Reset();
			return this;
		}

		bool IEnumerator.MoveNext()
		{
			if (CurrentIndex >= Tokens.Count)
			{
				return false;
			}

			_current = Tokens[CurrentIndex];
			CurrentIndex++;
			return true;
		}

		void IEnumerator.Reset()
		{
			CurrentIndex = 0;
		}

		TokenPair IEnumerator<TokenPair>.Current
		{
			get { return _current; }
		}

		object IEnumerator.Current
		{
			get { return ((IEnumerator<TokenPair>)this).Current; }
		}

		void IDisposable.Dispose()
		{
		}
	}
}