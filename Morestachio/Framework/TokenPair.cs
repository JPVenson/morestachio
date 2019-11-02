using System;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Morestachio.Formatter;

namespace Morestachio.Framework
{
	/// <summary>
	///     The token that has been lexed out of template content.
	/// </summary>
	[DebuggerTypeProxy(typeof(TokenPairDebuggerProxy))]
	internal class TokenPair
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
				if (_pair.Format != null && _pair.Format.FormatString.Any())
				{
					return $"{Type} {Value}.{_pair.Format.FormatterName}({_pair.Format.FormatString.Select(e => e.ToString()).Aggregate((e, f) => e + "," + f)})";
				}
				return $"{Type} {Value}";
			}
		}

		public TokenPair(TokenType type, string value, CharacterLocation tokenLocation)
		{
			Type = type;
			Value = value;
			TokenLocation = tokenLocation;
		}

		public TokenType Type { get; set; }

		internal Tokenizer.FormattableToken Format { get; set; }
		
		[CanBeNull]
		public string Value { get; set; }

		public CharacterLocation TokenLocation { get; set; }
	}
}