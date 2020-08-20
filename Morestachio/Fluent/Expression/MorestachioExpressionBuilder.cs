using System;
using System.Collections.Generic;
using System.Linq;
using Morestachio.Framework;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Fluent.Expression
{
	/// <summary>
	///		Defines the methods available in all stages in an expression
	/// </summary>
	public class MorestachioExpressionBuilder
	{
		internal IList<IMorestachioExpression> ExpressionParts { get; set; }
		internal int Column { get; set; }

		internal MorestachioExpressionBuilder()
		{
			ExpressionParts = new List<IMorestachioExpression>();
		}

		/// <summary>
		///		Adds one or more .(dot) separated property paths to the expression or creates a new one
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public PropertyMorestachioExpressionBuilder Property(string property)
		{
			var parts = property.Split('.');
			var pathParts = new PathTokenizer.PathPartsCollection();
			foreach (var part in parts)
			{
				for (var index = 0; index < part.Length; index++)
				{
					var pathPartChar = part[index];
					if (!Tokenizer.IsSingleExpressionPathChar(pathPartChar) && pathPartChar != '$')
					{
						throw new InvalidOperationException("The property " + part + " contains invalid chars at " + index);
					}
				}
				pathParts.Add(part, PathType.DataPath);
			}
			Column += property.Length;

			if (ExpressionParts.LastOrDefault() is MorestachioExpression exp && exp.FormatterName == null)
			{
				exp.PathParts = exp.PathParts.Expand(pathParts.GetList());
			}
			else
			{
				var morestachioExpression = new MorestachioExpression(CharacterLocation.Unknown);
				morestachioExpression.PathParts = new Traversable(pathParts.GetList());
				ExpressionParts.Add(morestachioExpression);
			}

			return new PropertyMorestachioExpressionBuilder(ExpressionParts, Column);
		}

		/// <summary>
		///		Adds the .? Path type and compiles the list
		/// </summary>
		/// <returns></returns>
		public IMorestachioExpression SelectObjectAsList()
		{	
			if (ExpressionParts.LastOrDefault() is MorestachioExpression exp && exp.FormatterName == null)
			{
				exp.PathParts = exp.PathParts.Expand(new []
				{
					new KeyValuePair<string, PathType>(null, PathType.ObjectSelector), 
				});
			}
			else
			{
				var morestachioExpression = new MorestachioExpression(CharacterLocation.Unknown);
				morestachioExpression.PathParts = new Traversable(new []
				{
					new KeyValuePair<string, PathType>(null, PathType.ObjectSelector), 
				});
				ExpressionParts.Add(morestachioExpression);
			}
			return Compile();
		}

		/// <summary>
		///		Gets the build expression
		/// </summary>
		/// <returns></returns>
		public IMorestachioExpression Compile()
		{
			if (ExpressionParts.Count > 1)
			{
				return new MorestachioExpressionList(ExpressionParts, CharacterLocation.Unknown);
			}

			return ExpressionParts.FirstOrDefault();
		}
	}
}