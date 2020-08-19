using System;
using Morestachio.Document.Items;
using Morestachio.Document.TextOperations;
using Morestachio.Framework.Expression;

namespace Morestachio.Fluent
{
	public static class MorestachioDocumentFluentApiExtensions
	{
		/// <summary>
		///		Adds a new <see cref="ContentDocumentItem"/>.
		/// </summary>
		public static MorestachioDocumentFluentApi AddContent(this MorestachioDocumentFluentApi api, string content)
		{
			return api.AddChild(f => new ContentDocumentItem(content));
		}

		/// <summary>
		///		Adds a new <see cref="DoLoopDocumentItem"/> and enters it.
		/// </summary>
		public static MorestachioDocumentFluentApi AddDoLoopAndEnter(this MorestachioDocumentFluentApi api,
			Func<MorestachioExpressionBuilderBaseRootApi, IMorestachioExpression> condition)
		{
			return api.AddChildAndEnter(builder => new DoLoopDocumentItem(condition(builder)));
		}

		/// <summary>
		///		Adds a new <see cref="EachDocumentItem"/> and enters it.
		/// </summary>
		public static MorestachioDocumentFluentApi AddEachLoopAndEnter(this MorestachioDocumentFluentApi api,
			Func<MorestachioExpressionBuilderBaseRootApi, IMorestachioExpression> condition)
		{
			return api.AddChildAndEnter(builder => new EachDocumentItem(condition(builder)));
		}

		/// <summary>
		///		Adds a new <see cref="ElseExpressionScopeDocumentItem"/> and enters it.
		/// </summary>
		public static MorestachioDocumentFluentApi AddElseAndEnter(this MorestachioDocumentFluentApi api)
		{
			return api
				.AddChildAndEnter(builder => new ElseExpressionScopeDocumentItem());
		}

		/// <summary>
		///		Adds a new <see cref="ElseExpressionScopeDocumentItem"/> and enters it.
		/// </summary>
		public static MorestachioDocumentFluentApi AddGlobalVariable(this MorestachioDocumentFluentApi api,
			string name,
			Func<MorestachioExpressionBuilderBaseRootApi, IMorestachioExpression> condition)
		{
			return api.AddChild(builder => new EvaluateVariableDocumentItem(name, condition(builder)));
		}

		/// <summary>
		///		Adds a new <see cref="ExpressionScopeDocumentItem"/> and enters it.
		/// </summary>
		public static MorestachioDocumentFluentApi AddScopeAndEnter(this MorestachioDocumentFluentApi api,
			Func<MorestachioExpressionBuilderBaseRootApi, IMorestachioExpression> condition)
		{
			return api.AddChildAndEnter(builder => new ExpressionScopeDocumentItem(condition(builder)));
		}

		/// <summary>
		///		Adds a new <see cref="IfExpressionScopeDocumentItem"/> and enters it.
		/// </summary>
		public static MorestachioDocumentFluentApi AddIfAndEnter(this MorestachioDocumentFluentApi api,
			Func<MorestachioExpressionBuilderBaseRootApi, IMorestachioExpression> condition)
		{
			return api.AddChildAndEnter(builder => new IfExpressionScopeDocumentItem(condition(builder)));
		}

		/// <summary>
		///		Adds a new <see cref="IfNotExpressionScopeDocumentItem"/> and enters it.
		/// </summary>
		public static MorestachioDocumentFluentApi AddInvertedIfAndEnter(this MorestachioDocumentFluentApi api,
			Func<MorestachioExpressionBuilderBaseRootApi, IMorestachioExpression> condition)
		{
			return api.AddChildAndEnter(builder => new IfNotExpressionScopeDocumentItem(condition(builder)));
		}

		/// <summary>
		///		Adds a new <see cref="InvertedExpressionScopeDocumentItem"/> and enters it.
		/// </summary>
		public static MorestachioDocumentFluentApi AddInvertedScopeAndEnter(this MorestachioDocumentFluentApi api,
			Func<MorestachioExpressionBuilderBaseRootApi, IMorestachioExpression> condition)
		{
			return api.AddChildAndEnter(builder => new InvertedExpressionScopeDocumentItem(condition(builder)));
		}

		/// <summary>
		///		Adds a new <see cref="PartialDocumentItem"/> and enters it.
		/// </summary>
		public static MorestachioDocumentFluentApi AddPartial(this MorestachioDocumentFluentApi api,
			string name,
			Func<MorestachioDocumentFluentApi, MorestachioDocumentFluentApi> factory)
		{
			return api.AddChild(builder => new PartialDocumentItem(name,
				factory(new MorestachioDocumentFluentApi(new MorestachioDocumentInfo(api.Context.Options,
					new MorestachioDocument()))).Context.RootNode.Item));
		}

		/// <summary>
		///		Adds a new <see cref="PathDocumentItem"/>
		/// </summary>
		public static MorestachioDocumentFluentApi AddPath(this MorestachioDocumentFluentApi api,
			Func<MorestachioExpressionBuilderBaseRootApi, IMorestachioExpression> condition,
			bool escapeValue = false)
		{
			return api.AddChild(builder => new PathDocumentItem(condition(builder), escapeValue));
		}

		/// <summary>
		///		Adds a new <see cref="RenderPartialDocumentItem"/>
		/// </summary>
		public static MorestachioDocumentFluentApi AddInclude(this MorestachioDocumentFluentApi api,
			string name,
			Func<MorestachioExpressionBuilderBaseRootApi, IMorestachioExpression> condition = null)
		{
			return api.AddChild(builder => new RenderPartialDocumentItem(name, condition?.Invoke(builder)));
		}

		/// <summary>
		///		Adds a new <see cref="TextEditDocumentItem"/>
		/// </summary>
		public static MorestachioDocumentFluentApi AddTextModification(this MorestachioDocumentFluentApi api,
			ITextOperation textOperation)
		{
			return api.AddChild(builder => new TextEditDocumentItem(textOperation));
		}

		/// <summary>
		///		Adds a new <see cref="WhileLoopDocumentItem"/>
		/// </summary>
		public static MorestachioDocumentFluentApi AddWhileLoopAndEnter(this MorestachioDocumentFluentApi api,
			Func<MorestachioExpressionBuilderBaseRootApi, IMorestachioExpression> condition)
		{
			return api.AddChildAndEnter(builder => new WhileLoopDocumentItem(condition(builder)));
		}
	}
}