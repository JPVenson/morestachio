using System.Globalization;
using Morestachio.Document;
using Morestachio.Framework.Context;
using Morestachio.Framework.Tokenizing;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Framework.Expression.Framework
{
	/// <summary>
	///		The context for all Tokenizer operations
	/// </summary>
	public class TokenzierContext
	{
		internal char[] _prefixToken = new[] { '{', '{' };

		/// <summary>
		///		Creates a new <see cref="TokenzierContext"/>
		/// </summary>
		public TokenzierContext(List<int> lines, CultureInfo culture)
		{
			Lines = lines;
			Errors = new MorestachioErrorCollection();
			Character = 0;
			Culture = culture;
		}

		/// <summary>
		///		Indexes the expression or template and creates a new Context for the given text by indexing all linebreaks
		/// </summary>
		/// <returns></returns>
		public static TokenzierContext FromText(string expression, CultureInfo culture = null)
		{
			var tokenzierContext = new TokenzierContext(
				Tokenizer.FindNewLines(expression), culture);
			tokenzierContext.SetLocation(0);
			return tokenzierContext;
		}

		/// <summary>
		///		The culture of the current template
		/// </summary>
		public CultureInfo Culture { get; private set; }

		/// <summary>
		///		The current total character
		/// </summary>
		public int Character { get; private set; }

		/// <summary>
		///		Indexes of new lines
		/// </summary>
		public List<int> Lines { get; private set; }

		/// <summary>
		///		The current location responding to the Character
		/// </summary>
		public CharacterLocation CurrentLocation { get; private set; }

		/// <summary>
		///		The list of all tokenizer errors
		/// </summary>
		public MorestachioErrorCollection Errors { get; private set; }

		/// <summary>
		///		Gets the level of Isolation currently enforced
		/// </summary>
		public IsolationOptions Isolation { get; private set; }

		/// <summary>
		///		Gets or sets the starting of an Token
		/// </summary>
		public char[] PrefixToken
		{
			get { return _prefixToken; }
			set { _prefixToken = value; }
		}

		/// <summary>
		///		Gets or sets the ending of an Token
		/// </summary>
		public char[] SuffixToken { get; set; } = new[] { '}', '}' };

		internal int CommentIntend { get; set; } = 0;

		internal bool TokenizeComments;
		//internal int EscapeIntend { get; set; } = 0;

		/// <summary>
		///		Gets or sets the option for trimming all leading whitespaces and newlines
		/// </summary>
		public bool TrimLeading { get; set; }

		/// <summary>
		///		Gets or sets the option for trimming all leading whitespaces and newlines
		/// </summary>
		public bool TrimAllLeading { get; set; }
		/// <summary>
		///		Gets or sets the option for trimming all tailing whitespaces and newlines
		/// </summary>
		public bool TrimTailing { get; set; }
		/// <summary>
		///		Gets or sets the option for trimming all tailing whitespaces and newlines
		/// </summary>
		public bool TrimAllTailing { get; set; }

		internal CharacterLocation Location(int chars)
		{
			return Tokenizer.HumanizeCharacterLocation(chars, Lines);
		}

		/// <summary>
		///		Advances the current location by the number of chars
		/// </summary>
		public void AdvanceLocation(int chars)
		{
			Character += chars;
			CurrentLocation = Tokenizer.HumanizeCharacterLocation(Character, Lines);
		}

		/// <summary>
		///		sets the current location
		/// </summary>
		public void SetLocation(int chars)
		{
			Character = chars;
			CurrentLocation = Tokenizer.HumanizeCharacterLocation(Character, Lines);
		}

		///  <summary>
		/// 		Sets an Option that was requested from template
		///  </summary>
		///  <param name="name"></param>
		///  <param name="value"></param>
		///  <param name="parserOptions"></param>
		///  <returns></returns>
		public async Promise SetOption(string name, IMorestachioExpression value, ParserOptions parserOptions)
		{
			var val = (await value.GetValue(new ContextObject(".", null, new object()), new ScopeData(parserOptions)))
				.Value;

			if (name.Equals("TrimTailing", StringComparison.OrdinalIgnoreCase))
			{
				if (val == null)
				{
					Errors.Add(new MorestachioSyntaxError(CurrentLocation.AddWindow(new CharacterSnippedLocation()),
						"SET OPTION", "VALUE", $"The expression returned null for option '{name}' that does not accept a null value"));
					return;
				}
				if (!(val is bool valBool))
				{
					Errors.Add(new MorestachioSyntaxError(CurrentLocation.AddWindow(new CharacterSnippedLocation()),
						"SET OPTION", "VALUE", $"The expression returned '{val.GetType()}' for option '{name}' but expected and value of type '{typeof(bool)}'"));
					return;
				}
				TrimTailing = valBool;
			}
			if (name.Equals("TrimLeading", StringComparison.OrdinalIgnoreCase))
			{
				if (val == null)
				{
					Errors.Add(new MorestachioSyntaxError(CurrentLocation.AddWindow(new CharacterSnippedLocation()),
						"SET OPTION", "VALUE", $"The expression returned null for option '{name}' that does not accept a null value"));
					return;
				}
				if (!(val is bool valBool))
				{
					Errors.Add(new MorestachioSyntaxError(CurrentLocation.AddWindow(new CharacterSnippedLocation()),
						"SET OPTION", "VALUE", $"The expression returned '{val.GetType()}' for option '{name}' but expected and value of type '{typeof(bool)}'"));
					return;
				}
				TrimLeading = valBool;
			}
			if (name.Equals("TrimAllTailing", StringComparison.OrdinalIgnoreCase))
			{
				if (val == null)
				{
					Errors.Add(new MorestachioSyntaxError(CurrentLocation.AddWindow(new CharacterSnippedLocation()),
						"SET OPTION", "VALUE", $"The expression returned null for option '{name}' that does not accept a null value"));
					return;
				}
				if (!(val is bool valBool))
				{
					Errors.Add(new MorestachioSyntaxError(CurrentLocation.AddWindow(new CharacterSnippedLocation()),
						"SET OPTION", "VALUE", $"The expression returned '{val.GetType()}' for option '{name}' but expected and value of type '{typeof(bool)}'"));
					return;
				}
				TrimAllTailing = valBool;
			}
			if (name.Equals("TrimAllLeading", StringComparison.OrdinalIgnoreCase))
			{
				if (val == null)
				{
					Errors.Add(new MorestachioSyntaxError(CurrentLocation.AddWindow(new CharacterSnippedLocation()),
						"SET OPTION", "VALUE", $"The expression returned null for option '{name}' that does not accept a null value"));
					return;
				}
				if (!(val is bool valBool))
				{
					Errors.Add(new MorestachioSyntaxError(CurrentLocation.AddWindow(new CharacterSnippedLocation()),
						"SET OPTION", "VALUE", $"The expression returned '{val.GetType()}' for option '{name}' but expected and value of type '{typeof(bool)}'"));
					return;
				}
				TrimAllLeading = valBool;
			}
		}
	}
}
