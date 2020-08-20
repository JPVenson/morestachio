using System;
using System.Collections.Generic;
using Morestachio.Framework.Expression;

namespace Morestachio.Fluent.Expression
{
	/// <summary>
	///		Gets the methods for adding arguments to an formatter call
	/// </summary>
	public class MorestachioArgumentExpressionBuilder
	{
		internal MorestachioArgumentExpressionBuilder()
		{
			Arguments = new List<KeyValuePair<string, IMorestachioExpression>>();
		}
		internal List<KeyValuePair<string, IMorestachioExpression>> Arguments { get; set; }

		/// <summary>
		///		Adds an Argument to the formatter call
		/// </summary>
		/// <param name="name"></param>
		/// <param name="argValue"></param>
		public MorestachioArgumentExpressionBuilder Argument(string name, Func<MorestachioExpressionBuilderBaseRootApi, MorestachioExpressionBuilder> argValue)
		{
			Arguments.Add(new KeyValuePair<string, IMorestachioExpression>(name, argValue(new MorestachioExpressionBuilderBaseRootApi()).Compile()));
			return this;
		}
	}
}