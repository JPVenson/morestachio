using System;
using System.Collections.Generic;
using System.Text;
using Morestachio.Document;
using Morestachio.Framework.Expression;

namespace Morestachio.Framework.Context.FallbackResolver
{
	/// <summary>
	///     Defines methods for resolving an unknown type
	/// </summary>
	public interface IFallbackValueResolver
	{
		/// <summary>
		///     Should return ether NULL or an object as result.
		///     this CAN return a Task that has a result. The task will be awaited if needed.
		/// </summary>
		/// <returns></returns>
		object Resolve(ContextObject source,
						string path, 
						ScopeData scopeData,
						IMorestachioExpression morestachioExpression);
	}
}