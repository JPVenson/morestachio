using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Morestachio.Document.Contracts;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Expression.Visitors;

namespace Morestachio.Analyzer.DataAccess
{

	/// <summary>
	///		Allows to get all references to the data structure
	/// </summary>
	public class DataAccessAnalyzer
	{
		private readonly IDocumentItem _document;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="document"></param>
		public DataAccessAnalyzer(IDocumentItem document)
		{
			_document = document;
		}

		/// <summary>
		///		Gets all paths used in the template
		/// </summary>
		/// <returns></returns>
		public UsageResult GetUsageFromDeclared()
		{
			if (_document is IReportUsage reportUsage)
			{
				var data = new UsageData();
				var usages = reportUsage.Usage(data).Distinct().ToArray();

				return new UsageResult()
				{
					UsedProperties = new HashSet<string>(usages)
				};
			}

			return null;
		}
	}

	/// <summary>
	///		Holds all paths that lead to a property within the template
	/// </summary>
	public class UsageResult
	{
		/// <summary>
		///		The paths
		/// </summary>
		public HashSet<string> UsedProperties { get; set; }
	}

	/// <summary>
	/// 
	/// </summary>
	public static class DataAccessAnalyzerGenerationExtensions
	{
		/// <summary>
		///		Visit all path parts and get a list of all accessed properties
		/// </summary>
		/// <param name="expression"></param>
		/// <param name="usageData"></param>
		/// <returns></returns>
		public static IEnumerable<string> InferExpressionUsage(this IMorestachioExpression expression,
			UsageData usageData)
		{
			var visitor = new DataAccessExpressionVisitor(usageData);
			visitor.Visit(expression);
			return visitor.Parts;
		}
	}

	internal class DataAccessExpressionVisitor : MorestachioExpressionVisitorBase
	{
		private readonly UsageData _usageData;

		public DataAccessExpressionVisitor(UsageData usageData)
		{
			_usageData = usageData;
			Parts = new HashSet<string>();
		}

		public HashSet<string> Parts { get; set; }
		
		public override void Visit(MorestachioExpression expression)
		{
			var pathParts = expression.PathParts.ToArray().Where(e => e.Value == PathType.DataPath).ToArray();
			if (pathParts.Any())
			{
				string path;
				if (_usageData.VariableSource.TryGetValue(pathParts[0].Key, out var variable))
				{
					if (variable == null)
					{
						return;
					}

					path = variable;
					if (pathParts.Length > 1)
					{
						path = path.Trim('.') + "." + pathParts.Skip(1).Select(f => f.Key).Aggregate((e, f) => e + "." + f);
					}
				}
				else
				{
					path = _usageData.CurrentPath + pathParts.Select(f => f.Key).Aggregate((e, f) => e + "." + f);	
				}
				Parts.Add(path);
			}
			
			foreach (var expressionArgument in expression.Formats)
			{
				Visit(expressionArgument);
			}
		}

		public override void Visit(MorestachioLambdaExpression expression)
		{
			this.Visit(expression.Expression);
		}
	}
}
