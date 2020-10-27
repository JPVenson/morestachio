using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Morestachio.Framework.Context.Options;
using Morestachio.Framework.Expression;

namespace Morestachio.Framework.Tokenizing
{
	/// <summary>
	///     The token that has been lexed out of template content.
	/// </summary>
	[DebuggerTypeProxy(typeof(TokenPairDebuggerProxy))]
	public readonly struct TokenPair
	{
		/// <summary>
		///		Creates a new Token Pair
		/// </summary>
		public TokenPair(IComparable type, string value, CharacterLocation tokenLocation, EmbeddedState isEmbeddedToken = EmbeddedState.None)
			: this(type, value, tokenLocation, null, isEmbeddedToken, null)
		{
			Value = value;
		}


		/// <summary>
		///		Creates a new Token Pair
		/// </summary>
		public TokenPair(IComparable type, CharacterLocation tokenLocation, IMorestachioExpression expression, EmbeddedState isEmbeddedToken = EmbeddedState.None, ScopingBehavior? noScope = null)
			: this(type, null, tokenLocation, expression, isEmbeddedToken, noScope)
		{
		}


		/// <summary>
		///		Creates a new Token Pair
		/// </summary>
		public TokenPair(IComparable type, string value, CharacterLocation tokenLocation,
			IMorestachioExpression expression, EmbeddedState isEmbeddedToken = EmbeddedState.None, ScopingBehavior? noScope = null)
		{
			Type = type;
			MorestachioExpression = expression;
			IsEmbeddedToken = isEmbeddedToken;
			TokenLocation = tokenLocation;
			ScopeBehavior = noScope;
			Value = value;
		}

		/// <summary>
		///		The type of this Token
		/// </summary>
		public EmbeddedState IsEmbeddedToken { get; }

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
		public ScopingBehavior? ScopeBehavior { get; }

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

			public IMorestachioExpression Expression
			{
				get { return _pair.MorestachioExpression; }
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

	}


	internal struct ImportData
	{
		public string ImportNameExpression { get; set; }
		public string ImportContext { get; set; }


	}
}