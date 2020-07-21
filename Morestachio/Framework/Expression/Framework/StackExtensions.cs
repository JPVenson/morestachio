using System.Collections.Generic;

namespace Morestachio.Framework.Expression.Framework
{
	public static class StackExtensions
	{
		public static T TryPeek<T>(this Stack<T> stack)
		{
			if (stack.Count > 0)
			{
				return stack.Peek();
			}

			return default;
		}
	}
}