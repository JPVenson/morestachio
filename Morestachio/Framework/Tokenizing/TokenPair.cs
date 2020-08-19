using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Morestachio.Framework.Expression;

namespace Morestachio.Framework.Tokenizing
{
	/// <summary>
	///     The token that has been lexed out of template content.
	/// </summary>
	[DebuggerTypeProxy(typeof(TokenPairDebuggerProxy))]
	public readonly struct TokenPair
	{
		[PublicAPI]
		private class TokenPairDebuggerProxy
		{
			private readonly TokenPair _pair;

			public TokenPairDebuggerProxy(TokenPair pair)
			{
				_pair = pair;
			}

			public string Type
			{
				get { return _pair.Type.ToString(); }
			}

			public string Value
			{
				get { return _pair.Value; }
			}

			public override string ToString()
			{
				if (_pair.MorestachioExpression != null)
				{
					return $"{Type} {_pair.MorestachioExpression}";
				}
				return $"{Type} {Value}";
			}
		}

		/// <summary>
		///		Creates a new Token Pair
		/// </summary>
		public TokenPair(IComparable type, string value, CharacterLocation tokenLocation, bool noScope = false)
			: this(type, value, null, tokenLocation, noScope)
		{
			Value = value;
		}


		/// <summary>
		///		Creates a new Token Pair
		/// </summary>
		public TokenPair(IComparable type, IMorestachioExpression expression, CharacterLocation tokenLocation, bool noScope = false)
			: this(type, null, expression, tokenLocation, noScope)
		{
		}


		/// <summary>
		///		Creates a new Token Pair
		/// </summary>
		public TokenPair(IComparable type, string value, IMorestachioExpression expression, CharacterLocation tokenLocation, bool noScope = false)
		{
			Type = type;
			MorestachioExpression = expression;
			TokenLocation = tokenLocation;
			NoScope = noScope;
			Value = value;
		}

		/// <summary>
		///		The type of this Token
		/// </summary>
		public IComparable Type { get; }

		/// <summary>
		///		With what format should this token be evaluated
		/// </summary>
		internal IMorestachioExpression MorestachioExpression { get; }

		/// <summary>
		///		What is the Value of this token
		/// </summary>
		[CanBeNull]
		public string Value { get; }

		/// <summary>
		///		Where does this token occure in the Template
		/// </summary>
		public CharacterLocation TokenLocation { get; }

		/// <summary>
		///		IF set an otherwise scopeing block does not perform the scopeing
		/// </summary>
		public bool NoScope { get; }
	}
}