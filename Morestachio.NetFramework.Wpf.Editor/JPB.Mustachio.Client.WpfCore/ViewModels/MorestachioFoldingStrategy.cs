using System.Collections.Generic;
using System.Linq;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Folding;
using Morestachio.Document;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items;
using Morestachio.Document.Visitor;

namespace JPB.Mustachio.Client.Wpf.Core.ViewModels
{
	public class MorestachioFoldingStrategy
	{
		public static void UpdateFolding(FoldingManager manager, TextDocument document, IDocumentItem morestachioDocument)
		{
			var foldings = CreateNewFoldings(document, morestachioDocument);
			manager.UpdateFoldings(foldings, -1);
		}

		private static IEnumerable<NewFolding> CreateNewFoldings(TextDocument document,
			IDocumentItem morestachioDocument)
		{
			var morestachioDocumentChildren = morestachioDocument
				.Children
				.ToArray();
			for (var index = 0; index < morestachioDocumentChildren.Length; index++)
			{
				var morestachioDocumentChild = morestachioDocumentChildren[index];
				var start = document.GetOffset(
					morestachioDocumentChild.ExpressionStart.Line,
					morestachioDocumentChild.ExpressionStart.Character - 2);

				var end = document.Text.Length;
				if (index + 1 < morestachioDocumentChildren.Length)
				{
					var documentChild = morestachioDocumentChildren[index + 1];
					end = document.GetOffset(documentChild.ExpressionStart.Line,
						documentChild.ExpressionStart.Character);
				}

				var builder = new ToParsableStringDocumentVisitor();

				if (morestachioDocumentChild.Children.Any())
				{
					if (morestachioDocumentChild is ExpressionScopeDocumentItem expScope)
					{
						builder.Visit(expScope);
					}
					else if (morestachioDocumentChild is EachDocumentItem eachScope)
					{
						builder.Visit(eachScope);
					}
					else if (morestachioDocumentChild is DoLoopDocumentItem doScope)
					{
						builder.Visit(doScope);
					}
					else if (morestachioDocumentChild is IfExpressionScopeDocumentItem ifScope)
					{
						builder.Visit(ifScope);
					}
					else if (morestachioDocumentChild is IfNotExpressionScopeDocumentItem ifNotScope)
					{
						builder.Visit(ifNotScope);
					}
					else if (morestachioDocumentChild is InvertedExpressionScopeDocumentItem invertScope)
					{
						builder.Visit(invertScope);
					}
					else if (morestachioDocumentChild is PartialDocumentItem partial)
					{
						builder.Visit(partial);
					}
					else if (morestachioDocumentChild is WhileLoopDocumentItem whileScope)
					{
						builder.Visit(whileScope);
					}

					yield return new NewFolding(start, end)
					{
						Name = builder.StringBuilder.ToString(),
					};
				}

				foreach (var newFolding in CreateNewFoldings(document, morestachioDocumentChild))
				{
					yield return newFolding;
				}
			}
		}
	}
}