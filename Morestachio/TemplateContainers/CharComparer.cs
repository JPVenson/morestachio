using System.Collections.Generic;

namespace Morestachio.TemplateContainers
{
	/// <summary>
	///		Compares two chars based only on its numeric value 
	/// </summary>
	public class CharComparer : IEqualityComparer<char>
	{
		/// <inheritdoc />
		public bool Equals(char x, char y)
		{
			return ((int)x) == ((int)y);
		}

		/// <inheritdoc />
		public int GetHashCode(char obj)
		{
			return obj.GetHashCode();
		}
	}
}