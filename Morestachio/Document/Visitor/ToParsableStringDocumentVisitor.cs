using System;
using System.Linq;
using System.Text;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Items.SwitchCase;
using Morestachio.Document.TextOperations;
using Morestachio.Framework.Context.Options;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Visitors;

namespace Morestachio.Document.Visitor
{
	/// <summary>
	///		Reconstructs the DocumentTree to a Template string
	/// </summary>
	public class ToParsableStringDocumentVisitor : IDocumentItemVisitor
	{
		/// <summary>
		/// 
		/// </summary>
		public ToParsableStringDocumentVisitor()
		{
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

		public LineBreakTrimDirection RenderLineBreak { get; set; } = LineBreakTrimDirection.None;

		/// <summary>
		///		Loops through all the document items children
		/// </summary>
		/// <param name="documentItem"></param>
		public void VisitChildren(IDocumentItem documentItem)
		{
			var documentItemChildren = documentItem
				.Children
				.SkipWhile(f => f is TextEditDocumentItem txtOp && txtOp.EmbeddedState == EmbeddedState.Next)
				.Reverse()
				.SkipWhile(f => f is TextEditDocumentItem txtOp && txtOp.EmbeddedState == EmbeddedState.Previous)
				.Reverse()
				.ToArray();
			for (var index = 0; index < documentItemChildren.Length; index++)
			{
				var documentItemChild = documentItemChildren[index];
				if (documentItemChild is TextEditDocumentItem txtOp && txtOp.EmbeddedState != EmbeddedState.None)
				{
					continue;
				}

				if (index - 1 >= 0)
				{
					if (documentItemChildren[index - 1] is TextEditDocumentItem ntxtOp)
					{
						if (ntxtOp.Operation is TrimLineBreakTextOperation tl &&
						    (tl.LineBreakTrimDirection != LineBreakTrimDirection.Begin && index != 0))
						{
							RenderLineBreak |= tl.LineBreakTrimDirection;
						}
					}
				}

				if (index + 1 < documentItemChildren.Length)
				{
					if (documentItemChildren[index + 1] is TextEditDocumentItem ntxtOp)
					{
						if (ntxtOp.Operation is TrimLineBreakTextOperation tl &&
						    tl.LineBreakTrimDirection != LineBreakTrimDirection.End)
						{
							RenderLineBreak |= tl.LineBreakTrimDirection;
						}
					}
				}

				documentItemChild.Accept(this);
			}
		}
		

		/// <summary>
		///		Writes the tag with the leading char as well as all of the documentItems children
		/// </summary>
		/// <param name="documentItem"></param>
		/// <param name="tag"></param>
		/// <param name="cmdChar"></param>
		public void Visit(ExpressionDocumentItemBase documentItem, string tag, string cmdChar = "#")
		{
			StringBuilder.Append("{{");
			if (RenderLineBreak.HasFlagFast(LineBreakTrimDirection.End))
			{
				StringBuilder.Append("- | ");
			}
			StringBuilder.Append(cmdChar);
			StringBuilder.Append(tag);
			StringBuilder.Append(ReparseExpression(documentItem.MorestachioExpression));
			var children = documentItem.Children.ToList();
			var aliasDocumentItem = children.FirstOrDefault() as AliasDocumentItem;
			if (!(aliasDocumentItem is null))
			{
				StringBuilder.Append(" AS ");
				StringBuilder.Append(aliasDocumentItem.Value);
			}
			
			if (RenderLineBreak.HasFlagFast(LineBreakTrimDirection.Begin)
			||
			(children.Any() && 
			 children.First() is TextEditDocumentItem txtDoc && 
			 txtDoc.Operation is TrimLineBreakTextOperation trim && 
			 trim.LineBreakTrimDirection == LineBreakTrimDirection.Begin))
			{
				StringBuilder.Append(" | -");
			}

			RenderLineBreak = LineBreakTrimDirection.None;
			StringBuilder.Append("}}");

			if (children.Any())
			{
				VisitChildren(documentItem);

				StringBuilder.Append("{{");

				var preLastItem = children.ElementAtOrDefault(children.Count - 2);
				if(preLastItem  is TextEditDocumentItem lastTxtDoc && 
				   lastTxtDoc.Operation is TrimLineBreakTextOperation lastTrim && 
				   lastTrim.LineBreakTrimDirection == LineBreakTrimDirection.End)
				{
					StringBuilder.Append("- | ");
				}

				StringBuilder.Append("/");
				if (!(aliasDocumentItem is null))
				{
					StringBuilder.Append(aliasDocumentItem.Value);
				}
				else
				{
					StringBuilder.Append(tag.Trim());
				}
				StringBuilder.Append("}}");
			}
		}
		
		/// <summary>
		///		Writes the Tag and all of the documentItems children
		/// </summary>
		/// <param name="documentItem"></param>
		/// <param name="tag"></param>
		public void Visit(DocumentItemBase documentItem, string tag)
		{
			StringBuilder.Append("{{#");
			StringBuilder.Append(tag);
			StringBuilder.Append("}}");

			if (documentItem.Children.Any())
			{
				VisitChildren(documentItem);

				StringBuilder.Append("{{/");
				StringBuilder.Append(tag);
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
			StringBuilder.Append("{{#");
			StringBuilder.Append(documentItem.IdVariableScope == 0 ? "VAR " : "LET ");
			StringBuilder.Append(documentItem.Value);
			StringBuilder.Append(" = ");
			StringBuilder.Append(ReparseExpression(documentItem.MorestachioExpression));
			StringBuilder.Append("}}");
		}

		/// <inheritdoc />
		public void Visit(ExpressionScopeDocumentItem documentItem)
		{
			VisitExpressionScope(documentItem, '#');
		}

		private void VisitExpressionScope(ExpressionDocumentItemBase documentItem, char prefix)
		{
			StringBuilder.Append("{{");
			StringBuilder.Append(prefix);
			StringBuilder.Append(ReparseExpression(documentItem.MorestachioExpression));
			var children = documentItem.Children.ToList();
			var aliasDocumentItem = children.FirstOrDefault() as AliasDocumentItem;
			if (!(aliasDocumentItem is null))
			{
				StringBuilder.Append(" AS ");
				StringBuilder.Append(aliasDocumentItem.Value);
			}

			StringBuilder.Append("}}");

			if (children.Any())
			{
				VisitChildren(documentItem);

				StringBuilder.Append("{{/");
				if (!(aliasDocumentItem is null))
				{
					StringBuilder.Append(aliasDocumentItem.Value);
				}
				else
				{
					StringBuilder.Append(ReparseExpression(documentItem.MorestachioExpression));
				}
				StringBuilder.Append("}}");
			}
		}
		
		/// <inheritdoc />
		public void Visit(IfExpressionScopeDocumentItem documentItem)
		{
			Visit(documentItem, "IF ");
		}
		
		/// <inheritdoc />
		public void Visit(IfNotExpressionScopeDocumentItem documentItem)
		{
			Visit(documentItem, "IF ", "^");
		}
		
		/// <inheritdoc />
		public void Visit(InvertedExpressionScopeDocumentItem documentItem)
		{
			VisitExpressionScope(documentItem, '^');
		}

		public void Visit(SwitchDocumentItem documentItem)
		{
			StringBuilder.Append("{{#SWITCH ");
			StringBuilder.Append(ReparseExpression(documentItem.MorestachioExpression));
			if (documentItem.ScopeToValue)
			{
				StringBuilder.Append(" #SCOPE");
			}

			StringBuilder.Append("}}");

			VisitChildren(documentItem);

			StringBuilder.Append("{{/SWITCH}}");
		}

		public void Visit(SwitchCaseDocumentItem documentItem)
		{
			Visit(documentItem, "CASE ");
		}

		public void Visit(SwitchDefaultDocumentItem documentItem)
		{
			Visit(documentItem, "DEFAULT ");
		}

		/// <inheritdoc />
		public void Visit(MorestachioDocument documentItem)
		{
			VisitChildren(documentItem);
		}
		
		/// <inheritdoc />
		public void Visit(PartialDocumentItem documentItem)
		{
			StringBuilder.Append("{{#DECLARE ");
			StringBuilder.Append(documentItem.Value);
			StringBuilder.Append("}}");

			Visit(documentItem.Partial as MorestachioDocument);

			StringBuilder.Append("{{/DECLARE}}");
		}
		
		/// <inheritdoc />
		public void Visit(PathDocumentItem documentItem)
		{
			Visit(documentItem, "", "");
		}
		
		/// <inheritdoc />
		public void Visit(RemoveAliasDocumentItem documentItem)
		{
		}
		
		/// <inheritdoc />
		public void Visit(RenderPartialDocumentItem documentItem)
		{
			StringBuilder.Append("{{#INCLUDE ");
			StringBuilder.Append(documentItem.Value);
			if (documentItem.Context != null)
			{
				StringBuilder.Append(" WITH ");
				StringBuilder.Append(ReparseExpression(documentItem.Context));
			}
			StringBuilder.Append("}}");
		}

		public void Visit(ImportPartialDocumentItem documentItem)
		{
			StringBuilder.Append("{{#IMPORT ");
			StringBuilder.Append(ReparseExpression(documentItem.MorestachioExpression));
			if (documentItem.Context != null)
			{
				StringBuilder.Append(" #WITH ");
				StringBuilder.Append(ReparseExpression(documentItem.Context));
			}
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
