using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Morestachio.Framework.Expression.Renderer
{
	public static class ExpressionRenderer
	{
		/// <summary>
		///		Renders a Expression into the StringBuilder
		/// </summary>
		/// <param name="morestachioExpression"></param>
		/// <param name="sb"></param>
		public static void RenderExpression(IMorestachioExpression morestachioExpression, StringBuilder sb)
		{
			switch (morestachioExpression)
			{
				case MorestachioExpression expression1:
					RenderExpression(expression1, sb);
					break;
				case ExpressionArgument expressionArgument:
					RenderExpression(expressionArgument, sb);
					break;
				case MorestachioExpressionList expressionList:
					RenderExpression(expressionList, sb);
					break;
				case MorestachioExpressionString expressionString:
					RenderExpression(expressionString, sb);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(morestachioExpression));
			}
		}
		
		/// <summary>
		///		Renders the Argument like this:
		///		IF HAS NAME	-> "[" + NAME + "]" -> END + RenderExpression(Expression)
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="sb"></param>
		public static void RenderExpression(ExpressionArgument expression, StringBuilder sb)
		{
			if (!string.IsNullOrWhiteSpace(expression.Name))
			{
				sb.Append("[");
				sb.Append(expression.Name);
				sb.Append("] ");
			}

			RenderExpression(expression.MorestachioExpression, sb);
		}

		/// <summary>
		///		Renders the Expression like this:
		///		EACH PATHPART -> [Path + "."], "~", "../", "." END + IF HAS FORMATTER -> IF NOT SELFASSIGNMENT -> "." END -> FormatterName + "(" + [EACH ARG -> RenderExpression(ExpressionArgument ARG) + ", "] + ")" END
		/// </summary>
		/// <param name="morestachioExpression"></param>
		/// <param name="sb"></param>
		public static void RenderExpression(MorestachioExpression morestachioExpression, StringBuilder sb)
		{
			var isSelfAssignment = false;
			for (var index = 0; index < morestachioExpression.PathParts.Count; index++)
			{
				var expressionPathPart = morestachioExpression.PathParts[index];
				switch (expressionPathPart.Value)
				{
					case PathTokenizer.PathType.DataPath:
						sb.Append(expressionPathPart.Key);

						if (index != morestachioExpression.PathParts.Count - 1)
						{
							sb.Append(".");
						}
						break;
					case PathTokenizer.PathType.RootSelector:
						sb.Append("~");
						break;
					case PathTokenizer.PathType.ParentSelector:
						sb.Append("../");
						break;
					case PathTokenizer.PathType.SelfAssignment:
						sb.Append(".");
						isSelfAssignment = true;
						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}
			if (morestachioExpression.FormatterName != null)
			{
				if (!isSelfAssignment)
				{
					sb.Append(".");
				}

				sb.Append(morestachioExpression.FormatterName);
				sb.Append("(");

				if (morestachioExpression.Formats.Any())
				{
					for (var index = 0; index < morestachioExpression.Formats.Count; index++)
					{
						var expressionArgument = morestachioExpression.Formats[index];
						RenderExpression(expressionArgument, sb);
						if (index != morestachioExpression.Formats.Count - 1)
						{
							sb.Append(", ");
						}
					}
				}
				sb.Append(")");
			}
		}

		/// <summary>
		///		Renders the Expression like this:
		///		EACH EXP -> IF EXP NOT ONLY SelfAssignment -> "." END + RenderExpression(IExpression EXP) -> END
		/// </summary>
		/// <param name="morestachioExpression"></param>
		/// <param name="sb"></param>
		public static void RenderExpression(MorestachioExpressionList morestachioExpression, StringBuilder sb)
		{
			for (var index = 0; index < morestachioExpression.Expressions.Count; index++)
			{
				var expressionExpression = morestachioExpression.Expressions[index];
				if (index != 0 && 
				    (expressionExpression as MorestachioExpression)?.PathParts.All(f => f.Value == PathTokenizer.PathType.SelfAssignment) == false)
				{
					sb.Append(".");
				}
				RenderExpression(expressionExpression, sb);
			}
		}

		public static void RenderExpression(MorestachioExpressionString morestachioExpression, StringBuilder sb)
		{
			sb.Append(morestachioExpression.Delimiter +
					  string.Join("", morestachioExpression.StringParts.Select(f => f.PartText))
					  + morestachioExpression.Delimiter);
		}
	}
}
