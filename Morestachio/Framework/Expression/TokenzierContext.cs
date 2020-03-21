using System.Collections.Generic;
using Morestachio.ParserErrors;

namespace Morestachio.Framework.Expression
{
	public class TokenzierContext
	{
		public TokenzierContext(int[] lines)
		{
			Lines = lines;
			Errors = new List<IMorestachioError>();
			Character = 0;
		}

		public int Character { get; private set; }
		public int[] Lines { get; private set; }

		public CharacterLocation CurrentLocation { get; set; }
		public ICollection<IMorestachioError> Errors { get; set; }

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
