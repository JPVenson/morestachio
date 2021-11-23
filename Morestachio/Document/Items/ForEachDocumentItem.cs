using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;
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

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Emits N items that are in the collection
	/// </summary>
	[Serializable]
	public class ForEachDocumentItem : ExpressionDocumentItemBase, ISupportCustomAsyncCompilation
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal ForEachDocumentItem()
		{

		}

		/// <inheritdoc />
		public ForEachDocumentItem(CharacterLocation location,
			IMorestachioExpression value,
			string itemVariableName,
			IEnumerable<ITokenOption> tagCreationOptions)
			: base(location, value, tagCreationOptions)
		{
		}

		/// <inheritdoc />

		protected ForEachDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			ItemVariableName = info.GetString(nameof(ItemVariableName));
		}

		/// <summary>
		///		The name of the Variable for each item
		/// </summary>
		public string ItemVariableName { get; set; }

		/// <param name="compiler"></param>
		/// <inheritdoc />
		public CompilationAsync Compile(IDocumentCompiler compiler)
		{
			var expression = MorestachioExpression.Compile();
			var children = compiler.Compile(Children);
			return async (outputStream, context, scopeData) =>
			{
				await CoreAction(outputStream, await expression(context, scopeData), scopeData,
					async o =>
					{
						await children(outputStream, o, scopeData);
					});
			};
		}

		/// <exception cref="IndexedParseException"></exception>
		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context,
			ScopeData scopeData)
		{
			await CoreAction(outputStream,
				await MorestachioExpression.GetValue(context, scopeData)
				, scopeData, async itemContext =>
				{
					await MorestachioDocument.ProcessItemsAndChildren(Children, outputStream, itemContext, scopeData);
				});
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

				throw new IndexedParseException(CharacterLocationExtended.Empty,
					string.Format(
						"{1}'{0}' is used like an array by the template, but is a scalar value or object in your model." +
						" Complete Expression until Error:{2}",
						MorestachioExpression, ExpressionStart,
						(path.Count == 0 ? "Empty" : path.Aggregate((e, f) => e + "\r\n" + f))));
			}

			if (value is ICollection col)
			{
				await LoopCollection(outputStream, c, scopeData, onItem, col);
			}
			else
			{
				await LoopEnumerable(outputStream, c, scopeData, onItem, value);
			}

		}

		private async Promise LoopCollection(IByteCounterStream outputStream, ContextObject parentContext, ScopeData scopeData,
			Func<ContextObject, Promise> onItem, ICollection value)
		{
			var index = 0;
			var innerContext =
				new ContextCollection(index, false, $"[{index}]", parentContext, null)
					.MakeNatural() as ContextCollection;
			scopeData.AddVariable(ItemVariableName, (e, cx) => innerContext, 999999);

			foreach (var item in value)
			{
				if (!ContinueBuilding(outputStream, scopeData))
				{
					return;
				}
				innerContext.Index = index;
				innerContext.Last = index + 1 == value.Count;
				innerContext.Value = item;
				await onItem(parentContext);
				index++;
			}

			scopeData.RemoveVariable(ItemVariableName, 999999);
		}

		private async Promise LoopEnumerable(IByteCounterStream outputStream, ContextObject loopContext, ScopeData scopeData,
			Func<ContextObject, Promise> onItem, IEnumerable value)
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
				new ContextCollection(index, false, $"[{index}]", loopContext, current)
					.MakeNatural() as ContextCollection;
			scopeData.AddVariable(ItemVariableName, (e, cx) => innerContext, 999999);

			do
			{
				var next = enumerator.MoveNext() ? enumerator.Current : null;
				innerContext.Value = current;
				innerContext.Index = index;
				innerContext.Last = next == null;
				await onItem(loopContext);
				index++;
				current = next;
			} while (current != null && ContinueBuilding(outputStream, scopeData));

			scopeData.RemoveVariable(ItemVariableName, 999999);
		}

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}

		/// <inheritdoc />
		public override IEnumerable<string> Usage(UsageData data)
		{
			var path = MorestachioExpression.InferExpressionUsage(data).ToArray();
			var mainPath = path.FirstOrDefault();
			if (mainPath != null)
			{
				mainPath = mainPath.TrimEnd('.') + ".[].";
				data.ScopeTo(mainPath);
			}

			foreach (var usage in path)
			{
				yield return usage;
			}

			foreach (var usage in Children.OfType<IReportUsage>().SelectMany(f => f.Usage(data)))
			{
				yield return usage;
			}
			data.PopScope(mainPath);
		}
	}
}