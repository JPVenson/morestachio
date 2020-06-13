using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Morestachio.ParserErrors;

namespace Morestachio.Framework.Expression
{
	public class TokenzierContext
	{
		public TokenzierContext(int[] lines)
		{
			Lines = lines;
			Errors = new MorestachioErrorCollection();
			Character = 0;
		}

		public static TokenzierContext FromText(string expression)
		{
			var tokenzierContext = new TokenzierContext(
				Tokenizer.NewlineFinder.Matches(expression).OfType<Match>().Select(k => k.Index)
					.ToArray());
			tokenzierContext.SetLocation(0);
			return tokenzierContext;
		}

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

	public class MorestachioErrorCollection : ICollection<IMorestachioError>
	{
		private ICollection<IMorestachioError> _base;
		public MorestachioErrorCollection()
		{
			_base = new List<IMorestachioError>();
		}

		public string GetErrorText()
		{
			var sb = new StringBuilder();
			foreach (var err in this)
			{
				err.Format(sb);
				sb.AppendLine();
			}

			return sb.ToString();
		}

		public IEnumerator<IMorestachioError> GetEnumerator()
		{
			return _base.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return ((IEnumerable) _base).GetEnumerator();
		}

		public void Add(IMorestachioError item)
		{
			//#if DEBUG
			//throw item.GetException();
			//#endif
			_base.Add(item);
		}

		public void Clear()
		{
			_base.Clear();
		}

		public bool Contains(IMorestachioError item)
		{
			return _base.Contains(item);
		}

		public void CopyTo(IMorestachioError[] array, int arrayIndex)
		{
			_base.CopyTo(array, arrayIndex);
		}

		public bool Remove(IMorestachioError item)
		{
			return _base.Remove(item);
		}

		public int Count
		{
			get { return _base.Count; }
		}

		public bool IsReadOnly
		{
			get { return _base.IsReadOnly; }
		}
	}
}
