using Morestachio.Document.Contracts;
using Morestachio.Document.Items;
using Morestachio.Document.Items.SwitchCase;

namespace Morestachio.Document.Visitor;

/// <summary>
///		A visitor for a tree of document items
/// </summary>
public interface IDocumentItemVisitor
{
#pragma warning disable
	void Visit(AliasDocumentItem documentItem);
	void Visit(ContentDocumentItem documentItem);
	void Visit(DoLoopDocumentItem documentItem);
	void Visit(EachDocumentItem documentItem);
	void Visit(ForEachDocumentItem documentItem);
	void Visit(EvaluateVariableDocumentItem documentItem);
	void Visit(EvaluateLetVariableDocumentItem documentItem);
	void Visit(ExpressionScopeDocumentItem documentItem);
	void Visit(IfExpressionScopeDocumentItem documentItem);
	void Visit(ElseExpressionScopeDocumentItem documentItem);
	void Visit(ElseIfExpressionScopeDocumentItem documentItem);
	void Visit(InvertedExpressionScopeDocumentItem documentItem);
	void Visit(SwitchDocumentItem documentItem);
	void Visit(SwitchCaseDocumentItem documentItem);
	void Visit(SwitchDefaultDocumentItem documentItem);
	void Visit(MorestachioDocument documentItem);
	void Visit(PartialDocumentItem documentItem);
	void Visit(PathDocumentItem documentItem);
	void Visit(RemoveAliasDocumentItem documentItem);
	void Visit(RenderPartialDocumentItem documentItem);
	void Visit(ImportPartialDocumentItem documentItem);
	void Visit(RenderPartialDoneDocumentItem documentItem);
	void Visit(WhileLoopDocumentItem documentItem);
	void Visit(RepeatDocumentItem documentItem);
	void Visit(TextEditDocumentItem documentItem);
	void Visit(IsolationScopeDocumentItem documentItem);
	void Visit(CommentDocumentItem documentItem);
	void Visit(NoPrintDocumentItem documentItem);
	void Visit(IDocumentItem documentItem);
#pragma warning restore
}