using System.Collections.Generic;

namespace Morestachio.TemplateContainers
{
	/// <summary>
	///		Compares two chars based only on its numeric value 
	/// </summary>
	public class OrdinalCharComparer : IEqualityComparer<char>
	{
		public static OrdinalCharComparer Comparer = new OrdinalCharComparer(false);
		public static OrdinalCharComparer ComparerIgnoreCase = new OrdinalCharComparer(true);

		private readonly bool _ignoreCase;

		/// <summary>
		/// 
		/// </summary>
		internal OrdinalCharComparer(bool ignoreCase)
		{
			_ignoreCase = ignoreCase;
		}

		/// <inheritdoc />
		public bool Equals(char x, char y)
		{
			if (!_ignoreCase)
			{
				return ((int)x) == ((int)y);
			}

			if (!char.IsLetter(x) || !char.IsLetter(y))
			{
				return ((int)x) == ((int)y);
			}

			if (char.IsUpper(x))
			{
				if (char.IsUpper(y))
				{
					return ((int)x) == ((int)y);
				}
				return ((int)x) == ((int)char.ToUpper(y));
			}
			if (char.IsUpper(y))
			{
				return ((int)char.ToUpper(x)) == ((int)y);
			}

			return ((int)x) == ((int)y);
		}

		/// <inheritdoc />
		public int GetHashCode(char obj)
		{
			return obj.GetHashCode();
		}
	}
}