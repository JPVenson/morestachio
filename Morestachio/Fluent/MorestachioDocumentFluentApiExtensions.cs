using System;
using System.Linq;
using Morestachio.Document.Items;
using Morestachio.Document.TextOperations;
using Morestachio.Fluent.Expression;
using Morestachio.Framework;
using Morestachio.Framework.Context.Options;
using Morestachio.Framework.Tokenizing;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Fluent;

/// <summary>
///		Contains the extension methods for creating the BuildIn Document items
/// </summary>
public static class MorestachioDocumentFluentApiExtensions
{
	/// <summary>
	///		Adds a new <see cref="ContentDocumentItem"/>.
	/// </summary>
	public static MorestachioDocumentFluentApi AddContent(this MorestachioDocumentFluentApi api, string content)
	{
		return api.AddChild(f => new ContentDocumentItem(TextRange.Unknown, content, Enumerable.Empty<ITokenOption>()));
	}

	/// <summary>
	///		Adds a new <see cref="DoLoopDocumentItem"/> and enters it.
	/// </summary>
	public static MorestachioDocumentFluentApi AddDoLoopAndEnter(this MorestachioDocumentFluentApi api,
																Func<MorestachioExpressionBuilderBaseRootApi,
																	MorestachioExpressionBuilder> condition)
	{
		return api.AddChildAndEnter(builder =>
			new DoLoopDocumentItem(TextRange.Unknown, condition(builder).Compile(), Enumerable.Empty<ITokenOption>()));
	}

	/// <summary>
	///		Adds a new <see cref="EachDocumentItem"/> and enters it.
	/// </summary>
	public static MorestachioDocumentFluentApi AddEachLoopAndEnter(this MorestachioDocumentFluentApi api,
																	Func<MorestachioExpressionBuilderBaseRootApi,
																		MorestachioExpressionBuilder> condition)
	{
		return api.AddChildAndEnter(builder =>
			new EachDocumentItem(TextRange.Unknown, condition(builder).Compile(), Enumerable.Empty<ITokenOption>()));
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
																Func<MorestachioExpressionBuilderBaseRootApi,
																	MorestachioExpressionBuilder> condition)
	{
		return api.AddChild(builder => new EvaluateVariableDocumentItem(TextRange.Unknown, name,
			condition(builder).Compile(), Enumerable.Empty<ITokenOption>()));
	}

	/// <summary>
	///		Adds a new <see cref="ExpressionScopeDocumentItem"/> and enters it.
	/// </summary>
	public static MorestachioDocumentFluentApi AddScopeAndEnter(this MorestachioDocumentFluentApi api,
																Func<MorestachioExpressionBuilderBaseRootApi,
																	MorestachioExpressionBuilder> condition)
	{
		return api.AddChildAndEnter(builder => new ExpressionScopeDocumentItem(TextRange.Unknown,
			condition(builder).Compile(), Enumerable.Empty<ITokenOption>()));
	}

	/// <summary>
	///		Adds a new <see cref="IfExpressionScopeDocumentItem"/> and enters it.
	/// </summary>
	public static MorestachioDocumentFluentApi AddIfAndEnter(this MorestachioDocumentFluentApi api,
															Func<MorestachioExpressionBuilderBaseRootApi,
																MorestachioExpressionBuilder> condition)
	{
		return api.AddChildAndEnter(builder => new IfExpressionScopeDocumentItem(TextRange.Unknown,
			condition(builder).Compile(), Enumerable.Empty<ITokenOption>(), false));
	}

	/// <summary>
	///		Adds a new <see cref="IfExpressionScopeDocumentItem"/> and enters it.
	/// </summary>
	public static MorestachioDocumentFluentApi AddInvertedIfAndEnter(this MorestachioDocumentFluentApi api,
																	Func<MorestachioExpressionBuilderBaseRootApi,
																		MorestachioExpressionBuilder> condition)
	{
		return api.AddChildAndEnter(builder => new IfExpressionScopeDocumentItem(TextRange.Unknown,
			condition(builder).Compile(), Enumerable.Empty<ITokenOption>(), true));
	}

	/// <summary>
	///		Adds a new <see cref="InvertedExpressionScopeDocumentItem"/> and enters it.
	/// </summary>
	public static MorestachioDocumentFluentApi AddInvertedScopeAndEnter(this MorestachioDocumentFluentApi api,
																		Func<MorestachioExpressionBuilderBaseRootApi,
																			MorestachioExpressionBuilder> condition)
	{
		return api.AddChildAndEnter(builder => new InvertedExpressionScopeDocumentItem(TextRange.Unknown,
			condition(builder).Compile(), Enumerable.Empty<ITokenOption>()));
	}

	/// <summary>
	///		Adds a new <see cref="PartialDocumentItem"/> and enters it.
	/// </summary>
	public static MorestachioDocumentFluentApi AddPartial(this MorestachioDocumentFluentApi api,
														string name,
														Func<MorestachioDocumentFluentApi, MorestachioDocumentFluentApi>
															factory)
	{
		return api.AddChild(builder =>
			new PartialDocumentItem(TextRange.Unknown, name, Enumerable.Empty<ITokenOption>())
			{
				Children =
				{
					factory(new MorestachioDocumentFluentApi(new MorestachioDocumentInfo(api.Context.Options,
						new MorestachioDocument()))).Context.RootNode.Item
				}
			});
	}

	/// <summary>
	///		Adds a new <see cref="PathDocumentItem"/>
	/// </summary>
	public static MorestachioDocumentFluentApi AddPath(this MorestachioDocumentFluentApi api,
														Func<MorestachioExpressionBuilderBaseRootApi,
															MorestachioExpressionBuilder> condition,
														bool escapeValue = true)
	{
		return api.AddChild(builder => new PathDocumentItem(TextRange.Unknown, condition(builder).Compile(),
			escapeValue, Enumerable.Empty<ITokenOption>()));
	}

	/// <summary>
	///		Adds a new <see cref="RenderPartialDocumentItem"/>
	/// </summary>
	[Obsolete("Use the #Import keyword")]
	public static MorestachioDocumentFluentApi AddInclude(this MorestachioDocumentFluentApi api,
														string name,
														Func<MorestachioExpressionBuilderBaseRootApi,
															MorestachioExpressionBuilder> condition = null)
	{
		return api.AddChild(builder => new RenderPartialDocumentItem(TextRange.Unknown, name,
			condition?.Invoke(builder).Compile(), Enumerable.Empty<ITokenOption>()));
	}

	/// <summary>
	///		Adds a new <see cref="TextEditDocumentItem"/>
	/// </summary>
	public static MorestachioDocumentFluentApi AddTextModification(this MorestachioDocumentFluentApi api,
																	ITextOperation textOperation)
	{
		return api.AddChild(builder => new TextEditDocumentItem(TextRange.Unknown, textOperation,
			EmbeddedInstructionOrigin.Self, Enumerable.Empty<ITokenOption>()));
	}

	/// <summary>
	///		Adds a new <see cref="WhileLoopDocumentItem"/>
	/// </summary>
	public static MorestachioDocumentFluentApi AddWhileLoopAndEnter(this MorestachioDocumentFluentApi api,
																	Func<MorestachioExpressionBuilderBaseRootApi,
																		MorestachioExpressionBuilder> condition)
	{
		return api.AddChildAndEnter(builder =>
			new WhileLoopDocumentItem(TextRange.Unknown, condition(builder).Compile(),
				Enumerable.Empty<ITokenOption>()));
	}
}