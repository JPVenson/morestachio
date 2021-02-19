using System;
using System.Linq;
using System.Text;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Items.SwitchCase;
using Morestachio.Document.TextOperations;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Visitors;

namespace Morestachio.Document.Visitor
{
	/// <summary>
	///		Reconstructs the DocumentTree to a Template string
	/// </summary>
	public class ToParsableStringDocumentVisitor : IDocumentItemVisitor
	{
		private readonly ParserOptions _contextOptions;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="contextOptions"></param>
		public ToParsableStringDocumentVisitor(ParserOptions contextOptions)
		{
			_contextOptions = contextOptions;
			StringBuilder = new StringBuilder();
		}

		/// <summary>
		///		Contains the reconstructed DocumentTree
		/// </summary>
		public StringBuilder StringBuilder { get; set; }

		/// <inheritdoc />
		public void Visit(AliasDocumentItem documentItem)
		{
			VisitChildren(documentItem);
		}

		/// <inheritdoc />
		public void Visit(ContentDocumentItem documentItem)
		{
			StringBuilder.Append(documentItem.Value);
		}

		/// <summary>
		///		Parses an expression for use in an original template
		/// </summary>
		/// <param name="expression"></param>
		/// <returns></returns>
		public string ReparseExpression(IMorestachioExpression expression)
		{
			var visitor = new ToParsableStringExpressionVisitor();
			expression.Accept(visitor);
			return visitor.StringBuilder.ToString();
		}

		/// <summary>
		///		Loops through all the document items children
		/// </summary>
		/// <param name="documentItem"></param>
		public void VisitChildren(IDocumentItem documentItem)
		{
			if (!(documentItem is IBlockDocumentItem blockItem))
			{
				return;
			}

			foreach (var documentItemChild in blockItem.Children)
			{
				documentItemChild.Accept(this);
			}
		}

		public void CheckForInlineTagLineBreakAtStart(IDocumentItem documentItem)
		{
			if (documentItem.TagCreationOptions?.FirstOrDefault(e => e.Name == "Embedded.TrimLeading")?.Value is bool valSingle && valSingle)
			{
				StringBuilder.Append("-| ");
			}
			if (documentItem.TagCreationOptions?.FirstOrDefault(e => e.Name == "Embedded.TrimAllLeading")?.Value is bool valAll && valAll)
			{
				StringBuilder.Append("--| ");
			}
		}

		public void CheckForInlineTagLineBreakAtEnd(IDocumentItem documentItem)
		{
			if (documentItem.TagCreationOptions?.FirstOrDefault(e => e.Name == "Embedded.TrimTailing")?.Value is bool valSingle && valSingle)
			{
				StringBuilder.Append(" |-");
			}
			if (documentItem.TagCreationOptions?.FirstOrDefault(e => e.Name == "Embedded.TrimAllTailing")?.Value is bool valAll && valAll)
			{
				StringBuilder.Append(" |--");
			}
		}

		public void CheckForInlineBlockLineBreakAtStart(IBlockDocumentItem documentItem)
		{
			if (documentItem.BlockClosingOptions?.FirstOrDefault(e => e.Name == "Embedded.TrimLeading")?.Value is bool valSingle && valSingle)
			{
				StringBuilder.Append("-| ");
			}
			if (documentItem.BlockClosingOptions?.FirstOrDefault(e => e.Name == "Embedded.TrimAllLeading")?.Value is bool valAll && valAll)
			{
				StringBuilder.Append("--| ");
			}
		}

		public void CheckForInlineBlockLineBreakAtEnd(IBlockDocumentItem documentItem)
		{
			if (documentItem.BlockClosingOptions?.FirstOrDefault(e => e.Name == "Embedded.TrimTailing")?.Value is bool valSingle && valSingle)
			{
				StringBuilder.Append(" |-");
			}
			if (documentItem.BlockClosingOptions?.FirstOrDefault(e => e.Name == "Embedded.TrimAllTailing")?.Value is bool valAll && valAll)
			{
				StringBuilder.Append(" |--");
			}
		}

		private void RenderTagHead(ExpressionDocumentItemBase documentItem, string tag, string cmdChar = "#")
		{
			StringBuilder.Append("{{");
			CheckForInlineTagLineBreakAtStart(documentItem);
			StringBuilder.Append(cmdChar);
			StringBuilder.Append(tag);
			StringBuilder.Append(ReparseExpression(documentItem.MorestachioExpression));
			var aliasDocumentItem = documentItem.Children.FirstOrDefault() as AliasDocumentItem;
			if (!(aliasDocumentItem is null))
			{
				StringBuilder.Append(" AS ");
				StringBuilder.Append(aliasDocumentItem.Value);
			}

			CheckForInlineTagLineBreakAtEnd(documentItem);
			StringBuilder.Append("}}");
		}

		private void RenderBlockFooter(ExpressionDocumentItemBase documentItem, string tag)
		{
			StringBuilder.Append("{{");
			CheckForInlineBlockLineBreakAtStart(documentItem);
			StringBuilder.Append("/");
			StringBuilder.Append(tag.Trim());
			CheckForInlineBlockLineBreakAtEnd(documentItem);
			StringBuilder.Append("}}");
		}

		/// <summary>
		///		Writes the tag with the leading char as well as all of the documentItems children
		/// </summary>
		/// <param name="documentItem"></param>
		/// <param name="tag"></param>
		/// <param name="cmdChar"></param>
		public void Visit(ExpressionDocumentItemBase documentItem, string tag, string cmdChar = "#")
		{
			RenderTagHead(documentItem, tag, cmdChar);
			if (documentItem.Children.Any())
			{
				VisitChildren(documentItem);
				RenderBlockFooter(documentItem, tag);
			}
		}

		/// <summary>
		///		Writes the Tag and all of the documentItems children
		/// </summary>
		/// <param name="documentItem"></param>
		/// <param name="tag"></param>
		public void Visit(DocumentItemBase documentItem, string tag)
		{
			StringBuilder.Append("{{");
			CheckForInlineTagLineBreakAtStart(documentItem);
			StringBuilder.Append("#");
			StringBuilder.Append(tag);
			CheckForInlineTagLineBreakAtEnd(documentItem);
			StringBuilder.Append("}}");

			if (documentItem is IBlockDocumentItem blockItem && blockItem.Children.Any())
			{
				VisitChildren(documentItem);

				StringBuilder.Append("{{");
				CheckForInlineBlockLineBreakAtStart(blockItem);
				StringBuilder.Append("/");
				StringBuilder.Append(tag);
				CheckForInlineBlockLineBreakAtEnd(blockItem);
				StringBuilder.Append("}}");
			}
		}


		/// <inheritdoc />
		public void Visit(DoLoopDocumentItem documentItem)
		{
			Visit(documentItem, "DO ");
		}

		/// <inheritdoc />
		public void Visit(EachDocumentItem documentItem)
		{
			Visit(documentItem, "EACH ");
		}

		/// <inheritdoc />
		public void Visit(ElseExpressionScopeDocumentItem documentItem)
		{
			Visit(documentItem, "ELSE");
		}

		/// <inheritdoc />
		public void Visit(EvaluateVariableDocumentItem documentItem)
		{
			StringBuilder.Append("{{");
			CheckForInlineTagLineBreakAtStart(documentItem);
			StringBuilder.Append("#");
			StringBuilder.Append(documentItem.IdVariableScope == 0 ? "VAR " : "LET ");
			StringBuilder.Append(documentItem.Value);
			StringBuilder.Append(" = ");
			StringBuilder.Append(ReparseExpression(documentItem.MorestachioExpression));
			CheckForInlineTagLineBreakAtEnd(documentItem);
			StringBuilder.Append("}}");
		}

		/// <inheritdoc />
		public void Visit(ExpressionScopeDocumentItem documentItem)
		{
			if (documentItem.TagCreationOptions?.Any(f => f.Name == "Render.LegacyStyle") == true)
			{
				VisitExpressionScope(documentItem, '#');
			}
			else
			{
				Visit(documentItem, "SCOPE ");
			}
		}

		private void VisitExpressionScope(ExpressionDocumentItemBase documentItem, char prefix)
		{
			StringBuilder.Append("{{");
			CheckForInlineTagLineBreakAtStart(documentItem);
			StringBuilder.Append(prefix);
			StringBuilder.Append(ReparseExpression(documentItem.MorestachioExpression));
			var children = documentItem.Children.ToList();
			var aliasDocumentItem = children.FirstOrDefault() as AliasDocumentItem;
			if (!(aliasDocumentItem is null))
			{
				StringBuilder.Append(" AS ");
				StringBuilder.Append(aliasDocumentItem.Value);
			}

			CheckForInlineTagLineBreakAtEnd(documentItem);
			StringBuilder.Append("}}");

			if (children.Any())
			{
				VisitChildren(documentItem);

				StringBuilder.Append("{{/");
				CheckForInlineBlockLineBreakAtStart(documentItem);
				if (!(aliasDocumentItem is null))
				{
					StringBuilder.Append(aliasDocumentItem.Value);
				}
				else
				{
					StringBuilder.Append(ReparseExpression(documentItem.MorestachioExpression));
				}
				CheckForInlineBlockLineBreakAtEnd(documentItem);
				StringBuilder.Append("}}");
			}
		}

		/// <inheritdoc />
		public void Visit(IfExpressionScopeDocumentItem documentItem)
		{

			if (documentItem.Inverted)
			{
				RenderTagHead(documentItem, "IF ", "^");
			}
			else
			{
				RenderTagHead(documentItem, "IF ");
			}
			
			if (documentItem.Children.Any())
			{
				VisitChildren(documentItem);
			}

			if (documentItem.Else != null)
			{
				Visit(documentItem.Else as ElseExpressionScopeDocumentItem);
			}
			else
			{
				RenderBlockFooter(documentItem, "IF");	
			}
		}

		/// <inheritdoc />
		public void Visit(InvertedExpressionScopeDocumentItem documentItem)
		{
			if (documentItem.TagCreationOptions?.Any(f => f.Name == "Render.LegacyStyle") == true)
			{
				VisitExpressionScope(documentItem, '^');
			}
			else
			{
				Visit(documentItem, "SCOPE ", "^");
			}
		}

		/// <inheritdoc />
		public void Visit(SwitchDocumentItem documentItem)
		{
			StringBuilder.Append("{{");
			CheckForInlineTagLineBreakAtStart(documentItem);
			StringBuilder.Append("#SWITCH ");
			StringBuilder.Append(ReparseExpression(documentItem.MorestachioExpression));
			if (documentItem.ScopeToValue)
			{
				StringBuilder.Append(" #SCOPE");
			}

			CheckForInlineTagLineBreakAtEnd(documentItem);
			StringBuilder.Append("}}");

			VisitChildren(documentItem);

			StringBuilder.Append("{{");
			CheckForInlineBlockLineBreakAtStart(documentItem);
			StringBuilder.Append("/SWITCH");
			CheckForInlineBlockLineBreakAtEnd(documentItem);
			StringBuilder.Append("}}");
		}

		/// <inheritdoc />
		public void Visit(SwitchCaseDocumentItem documentItem)
		{
			Visit(documentItem, "CASE ");
		}

		/// <inheritdoc />
		public void Visit(SwitchDefaultDocumentItem documentItem)
		{
			Visit(documentItem, "DEFAULT");
		}

		/// <inheritdoc />
		public void Visit(MorestachioDocument documentItem)
		{
			VisitChildren(documentItem);
		}

		/// <inheritdoc />
		public void Visit(PartialDocumentItem documentItem)
		{
			StringBuilder.Append("{{");
			CheckForInlineTagLineBreakAtStart(documentItem);
			StringBuilder.Append("#DECLARE ");
			StringBuilder.Append(documentItem.Value);
			CheckForInlineTagLineBreakAtEnd(documentItem);
			StringBuilder.Append("}}");

			Visit(documentItem.Partial as MorestachioDocument);

			StringBuilder.Append("{{");
			CheckForInlineBlockLineBreakAtStart(documentItem);
			StringBuilder.Append("/DECLARE");
			CheckForInlineBlockLineBreakAtEnd(documentItem);
			StringBuilder.Append("}}");
		}

		/// <inheritdoc />
		public void Visit(PathDocumentItem documentItem)
		{
			Visit(documentItem, "", documentItem.EscapeValue ? "" : "&");
		}

		/// <inheritdoc />
		public void Visit(RemoveAliasDocumentItem documentItem)
		{
		}

		/// <inheritdoc />
		public void Visit(RenderPartialDocumentItem documentItem)
		{
			StringBuilder.Append("{{");
			CheckForInlineTagLineBreakAtStart(documentItem);
			StringBuilder.Append("#INCLUDE ");
			StringBuilder.Append(documentItem.Value);
			if (documentItem.Context != null)
			{
				StringBuilder.Append(" WITH ");
				StringBuilder.Append(ReparseExpression(documentItem.Context));
			}
			CheckForInlineTagLineBreakAtEnd(documentItem);
			StringBuilder.Append("}}");
		}

		/// <inheritdoc />
		public void Visit(ImportPartialDocumentItem documentItem)
		{
			StringBuilder.Append("{{");
			CheckForInlineTagLineBreakAtStart(documentItem);
			StringBuilder.Append("#IMPORT ");
			StringBuilder.Append(ReparseExpression(documentItem.MorestachioExpression));
			if (documentItem.Context != null)
			{
				StringBuilder.Append(" #WITH ");
				StringBuilder.Append(ReparseExpression(documentItem.Context));
			}
			CheckForInlineTagLineBreakAtEnd(documentItem);
			StringBuilder.Append("}}");
		}

		/// <inheritdoc />
		public void Visit(RenderPartialDoneDocumentItem documentItem)
		{
		}

		/// <inheritdoc />
		public void Visit(WhileLoopDocumentItem documentItem)
		{
			Visit(documentItem, "WHILE ");
		}

		/// <inheritdoc />
		public void Visit(RepeatDocumentItem documentItem)
		{
			Visit(documentItem, "REPEAT ");
		}

		/// <inheritdoc />
		public void Visit(TextEditDocumentItem documentItem)
		{
			switch (documentItem.Operation)
			{
				case AppendLineBreakTextOperation _:
					StringBuilder.Append("{{#NL}}");
					break;
				case TrimLineBreakTextOperation op:
					if (op.LineBreaks == 1)
					{
						StringBuilder.Append("{{#TNL}}");
					}
					else if (op.LineBreaks == -1)
					{
						StringBuilder.Append("{{#TNLS}}");
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <inheritdoc />
		public void Visit(IDocumentItem documentItem)
		{
			if (documentItem is IStringVisitor renderer)
			{
				renderer.Render(this);
			}
			else
			{
				throw new InvalidOperationException(
					$"The type '{documentItem.GetType()}' cannot be rendered with the '{nameof(ToParsableStringDocumentVisitor)}'" +
					$" as its an custom document item." +
					$" Please implement '{nameof(IStringVisitor)}'");
			}
		}

		/// <summary>
		///		Defines custom rendering logic for custom <see cref="IDocumentItem"/>
		/// </summary>
		public interface IStringVisitor
		{
			/// <summary>
			///		Renders the object
			/// </summary>
			void Render(ToParsableStringDocumentVisitor visitor);
		}
	}
}
