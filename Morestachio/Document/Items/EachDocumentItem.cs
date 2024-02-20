using Morestachio.Analyzer.DataAccess;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Error;
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Document.Items;

/// <summary>
///     Emits N items that are in the collection
/// </summary>
[Serializable]
public class EachDocumentItem : BlockExpressionDocumentItemBase, ISupportCustomAsyncCompilation
{
	/// <summary>
	///     Used for XML Serialization
	/// </summary>
	internal EachDocumentItem()
	{
	}

	/// <inheritdoc />
	public EachDocumentItem(TextRange location,
							IMorestachioExpression value,
							IEnumerable<ITokenOption> tagCreationOptions)
		: base(location, value, tagCreationOptions)
	{
	}

	/// <inheritdoc />
	protected EachDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
	{
	}

	/// <param name="compiler"></param>
	/// <param name="parserOptions"></param>
	/// <inheritdoc />
	public CompilationAsync Compile(IDocumentCompiler compiler, ParserOptions parserOptions)
	{
		var expression = MorestachioExpression.Compile(parserOptions);
		var children = compiler.Compile(Children, parserOptions);
		return async (outputStream, context, scopeData) =>
		{
			await CoreAction(outputStream, await expression(context, scopeData).ConfigureAwait(false), scopeData,
				async o => { await children(outputStream, o, scopeData).ConfigureAwait(false); }).ConfigureAwait(false);
		};
	}

	/// <exception cref="IndexedParseException"></exception>
	/// <inheritdoc />
	public override async ItemExecutionPromise Render(IByteCounterStream outputStream,
													ContextObject context,
													ScopeData scopeData)
	{
		await CoreAction(outputStream,
			await MorestachioExpression.GetValue(context, scopeData).ConfigureAwait(false)
			, scopeData, async itemContext =>
			{
				await MorestachioDocument.ProcessItemsAndChildren(Children, outputStream, itemContext, scopeData)
					.ConfigureAwait(false);
				//contexts.AddRange(Children.WithScope(itemContext));
			}).ConfigureAwait(false);
		return Enumerable.Empty<DocumentItemExecution>();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private async Promise CoreAction(IByteCounterStream outputStream,
									ContextObject c,
									ScopeData scopeData,
									Func<ContextObject, Promise> onItem)
	{
		if (!c.Exists())
		{
			return;
		}

		if (!(c.Value is IEnumerable value) || value is string || value is IDictionary<string, object>)
		{
			var path = new Stack<string>();
			var parent = c.Parent;

			while (parent != null)
			{
				path.Push(parent.Key);
				parent = parent.Parent;
			}

			throw new IndexedParseException(Location,
				string.Format(
					"{1}'{0}' is used like an array by the template, but is a scalar value or object in your model." +
					" Complete Expression until Error:{2}",
					MorestachioExpression.AsStringExpression(),
					Location,
					(path.Count == 0 ? "Empty" : path.Aggregate((e, f) => e + "\r\n" + f))));
		}

		if (value is IList lst)
		{
			await LoopList(outputStream, c, scopeData, onItem, lst).ConfigureAwait(false);
		}
		else if (value is ICollection col)
		{
			await LoopCollection(outputStream, c, scopeData, onItem, col).ConfigureAwait(false);
		}
		else
		{
			await LoopEnumerable(outputStream, c, scopeData, onItem, value).ConfigureAwait(false);
		}
	}

	private static async Promise LoopList(IByteCounterStream outputStream,
										ContextObject context,
										ScopeData scopeData,
										Func<ContextObject, Promise> onItem,
										IList value)
	{
		var innerContext =
			new ContextCollection(0, false, $"[]", context, null)
				.MakeNatural() as ContextCollection;

		for (var i = 0; i < value.Count; i++)
		{
			if (!ContinueBuilding(outputStream, scopeData))
			{
				return;
			}

			innerContext.Index = i;
			innerContext.Last = i + 1 == value.Count;
			innerContext.Value = value[i];
			await onItem(innerContext).ConfigureAwait(false);
		}
	}

	private static async Promise LoopCollection(IByteCounterStream outputStream,
												ContextObject context,
												ScopeData scopeData,
												Func<ContextObject, Promise> onItem,
												ICollection value)
	{
		var index = 0;
		var innerContext =
			new ContextCollection(index, false, $"[]", context, null)
				.MakeNatural() as ContextCollection;

		foreach (var item in value)
		{
			if (!ContinueBuilding(outputStream, scopeData))
			{
				return;
			}

			innerContext.Index = index;
			innerContext.Last = index + 1 == value.Count;
			innerContext.Value = item;
			await onItem(innerContext).ConfigureAwait(false);
			index++;
		}
	}

	private static async Promise LoopEnumerable(IByteCounterStream outputStream,
												ContextObject context,
												ScopeData scopeData,
												Func<ContextObject, Promise> onItem,
												IEnumerable value)
	{
		//Use this "lookahead" enumeration to allow the $last keyword
		var index = 0;
		var enumerator = value.GetEnumerator();

		if (!enumerator.MoveNext())
		{
			return;
		}

		var current = enumerator.Current;

		var innerContext =
			new ContextCollection(index, false, $"[]", context, current)
				.MakeNatural() as ContextCollection;

		do
		{
			var next = enumerator.MoveNext() ? enumerator.Current : null;
			innerContext.Value = current;
			innerContext.Index = index;
			innerContext.Last = next == null;
			await onItem(innerContext).ConfigureAwait(false);
			index++;
			current = next;
		} while (current != null && ContinueBuilding(outputStream, scopeData));
	}

	/// <inheritdoc />
	public override void Accept(IDocumentItemVisitor visitor)
	{
		visitor.Visit(this);
	}

	/// <inheritdoc />
	public override void ReportUsage(UsageData data)
	{
		var dataScope = data.AddAndScopeTo(MorestachioExpression.GetInferedExpressionUsage(data));
		var arrScope
			= data.AddAndScopeTo(new UsageDataItem(string.Empty, UsageDataItemTypes.ArrayAccess, data.CurrentPath));

		foreach (var usage in Children.OfType<IReportUsage>())
		{
			usage.ReportUsage(data);
		}

		data.PopScope(arrScope);
		data.PopScope(dataScope);
	}
}