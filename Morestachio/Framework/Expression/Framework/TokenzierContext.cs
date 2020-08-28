using System;
using System.Globalization;
using Morestachio.Document;
using Morestachio.Framework.Context;
using Morestachio.Framework.Tokenizing;
using Morestachio.Parsing.ParserErrors;
#if ValueTask
using ContextObjectPromise = System.Threading.Tasks.ValueTask<Morestachio.Framework.Context.ContextObject>;
using Promise = System.Threading.Tasks.ValueTask;
#else
using ContextObjectPromise = System.Threading.Tasks.Task<Morestachio.Framework.Context.ContextObject>;
using Promise = System.Threading.Tasks.Task;
#endif

namespace Morestachio.Framework.Expression.Framework
{
	/// <summary>
	///		The context for all Tokenizer operations
	/// </summary>
	public class TokenzierContext
	{
		/// <summary>
		///		Creates a new <see cref="TokenzierContext"/>
		/// </summary>
		public TokenzierContext(int[] lines, CultureInfo culture)
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
				Tokenizer.FindNewLines(expression).ToArray(), culture);
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
		public int[] Lines { get; private set; }

		/// <summary>
		///		The current location responding to the Character
		/// </summary>
		public CharacterLocation CurrentLocation { get; private set; }

		/// <summary>
		///		The list of all tokenizer errors
		/// </summary>
		public MorestachioErrorCollection Errors { get; private set; }

		/// <summary>
		///		Gets or sets the starting of an Token
		/// </summary>
		public string PrefixToken { get; set; } = "{{";

		/// <summary>
		///		Gets or sets the ending of an Token
		/// </summary>
		public string SuffixToken { get; set; } = "}}";

		internal int CommentIntend { get; set; } = 0;

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

		public async Promise SetOption(string name, IMorestachioExpression value, ParserOptions parserOptions)
		{
			var val = (await value.GetValue(new ContextObject(parserOptions, ".", null, new object()), new ScopeData()))
				.Value;
			if (name.Equals("TokenPrefix", StringComparison.InvariantCultureIgnoreCase))
			{
				if (val == null)
				{
					Errors.Add(new MorestachioSyntaxError(CurrentLocation.AddWindow(new CharacterSnippedLocation()),
						"SET OPTION", "VALUE", $"The expression returned null for option '{name}' that does not accept a null value"));
					return;
				}
				PrefixToken = val.ToString();
			}
			if (name.Equals("TokenSuffix", StringComparison.InvariantCultureIgnoreCase))
			{
				if (val == null)
				{
					Errors.Add(new MorestachioSyntaxError(CurrentLocation.AddWindow(new CharacterSnippedLocation()),
						"SET OPTION", "VALUE", $"The expression returned null for option '{name}' that does not accept a null value"));
					return;
				}
				SuffixToken = val.ToString();
			}
		}
	}
}
