using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Morestachio.ParserErrors;

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
