using System;
using System.Collections.Generic;

namespace Morestachio.Framework.Tokenizing
{
	public class MorestachioDefaultRollingArray : RollingArray<char>
	{
		public MorestachioDefaultRollingArray() : base(3)
		{
		}

		public override bool EndsWith(char[] elements, IEqualityComparer<char> comparer = null)
		{
			if (elements.Length > Buffer.Length)
			{
				throw new IndexOutOfRangeException("The number of elements exceeds the size of the array");
			}

			return elements[elements.Length - 1] == Get(Buffer.Length - 1) 
			       && elements[elements.Length - 2] == Get(Buffer.Length - 2);
		}
	}
}