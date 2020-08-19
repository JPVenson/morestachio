using Morestachio.Document.Contracts;
using Morestachio.Document.Items;

namespace Morestachio.Document.Visitor
{
	/// <summary>
	///		A visitor for a tree of document items
	/// </summary>
	public interface IDocumentItemVisitor
	{
		void Visit(AliasDocumentItem documentItem);
		void Visit(ContentDocumentItem documentItem);
		void Visit(DoLoopDocumentItem documentItem);
		void Visit(EachDocumentItem documentItem);
		void Visit(ElseExpressionScopeDocumentItem documentItem);
		void Visit(EvaluateVariableDocumentItem documentItem);
		void Visit(ExpressionScopeDocumentItem documentItem);
		void Visit(IfExpressionScopeDocumentItem documentItem);
		void Visit(IfNotExpressionScopeDocumentItem documentItem);
		void Visit(InvertedExpressionScopeDocumentItem documentItem);
		void Visit(MorestachioDocument documentItem);
		void Visit(PartialDocumentItem documentItem);
		void Visit(PathDocumentItem documentItem);
		void Visit(RemoveAliasDocumentItem documentItem);
		void Visit(RenderPartialDocumentItem documentItem);
		void Visit(RenderPartialDoneDocumentItem documentItem);
		void Visit(WhileLoopDocumentItem documentItem);
		void Visit(TextEditDocumentItem documentItem);
		void Visit(IDocumentItem documentItem);
	}
}
