using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Morestachio.Document.Contracts;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Expression.Visitors;

namespace Morestachio.Analyzer.DataAccess;

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
			reportUsage.ReportUsage(data);

			return new UsageResult()
			{
				UseTree = data.Root
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
	public UsageDataItem UseTree { get; set; }

	/// <summary>
	///		Provides a Text display of all usage data.
	///		Property access is separated by a dot "."
	///		Array access is indicated by a pair of brackets "[]"
	/// </summary>
	/// <returns></returns>
	public string[] AsText()
	{
		var hashMap = new HashSet<string>();
		var lookupStack = new Stack<UsageDataItem>();
		lookupStack.Push(UseTree);

		UsageDataItem current;

		while (lookupStack.Any())
		{
			current = lookupStack.Pop();

			if (current.Dependents.Count == 0)
			{
				var formatItem = FormatItem(current);

				if (!string.IsNullOrWhiteSpace(formatItem))
				{
					hashMap.Add(formatItem);
				}
			}

			foreach (var currentDependent in current.Dependents)
			{
				lookupStack.Push(currentDependent);
			}
		}

		return hashMap.ToArray();
	}

	private string FormatItem(UsageDataItem current)
	{
		return current.RenderPath();
	}
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
	public static UsageDataItem GetInferedExpressionUsage(this IMorestachioExpression expression,
														UsageData usageData)
	{
		var visitor = new DataAccessExpressionVisitor(usageData);
		visitor.Visit(expression);
		return visitor.UsageDataItem;
	}
}

internal class DataAccessExpressionVisitor : MorestachioExpressionVisitorBase
{
	private readonly UsageData _usageData;
	public UsageDataItem UsageDataItem { get; private set; }

	public DataAccessExpressionVisitor(UsageData usageData)
	{
		_usageData = usageData;
	}

	public override void Visit(MorestachioExpression expression)
	{
		var source = _usageData.CurrentPath with { };

		var pathParts = expression.PathParts.ToArray();

		if (pathParts.Any())
		{
			var index = 0;

			if (_usageData.VariableSource.TryGetValue(pathParts[0].Key, out var variable))
			{
				var variableItem = variable.Peek();

				if (variableItem is null)
				{
					return;
				}

				source = variableItem with { };
				index = 1;
			}

			for (; index < pathParts.Length; index++)
			{
				var pathPart = pathParts[index];

				switch (pathPart.Value)
				{
					case PathType.RootSelector:
						source = _usageData.Root with { };
						break;
					case PathType.ParentSelector:
						source = source.Parent;
						break;
					case PathType.DataPath:
						source = source.AddDependent(new UsageDataItem(pathPart.Key, UsageDataItemTypes.DataPath,
							source));
						break;
				}
			}
		}

		UsageDataItem = source;

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