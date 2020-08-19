using System.Globalization;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Framework.Expression.Framework
{
	/// <summary>
	///		The context for all Tokenizer operations
	/// </summary>
	public class TokenzierContext
	{
		/// <summary>
		///		The indices of all linebreaks
		/// </summary>
		/// <param name="lines"></param>
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
		public CharacterLocation CurrentLocation { get; set; }

		/// <summary>
		///		The list of all tokenizer errors
		/// </summary>
		public MorestachioErrorCollection Errors { get; set; }

		public void AdvanceLocation(int chars)
		{
			Character += chars;
			CurrentLocation = Tokenizer.HumanizeCharacterLocation(Character, Lines);
		}

		public void SetLocation(int chars)
		{
			Character = chars;
			CurrentLocation = Tokenizer.HumanizeCharacterLocation(Character, Lines);
		}
	}
}
