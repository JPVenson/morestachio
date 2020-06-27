using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Visitor;
using Morestachio.Framework;

namespace Morestachio.Document
{
	/// <summary>
	///		Prints a partial
	/// </summary>
	[System.Serializable]
	public class RenderPartialDocumentItem : ValueDocumentItemBase
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal RenderPartialDocumentItem()
		{

		}

		/// <inheritdoc />
		public RenderPartialDocumentItem(string value)
		{
			Value = value;
		}

		/// <inheritdoc />
		[UsedImplicitly]
		protected RenderPartialDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}

		/// <inheritdoc />
		public override string Kind { get; } = "Include";
		
		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, 
			ContextObject context,
			ScopeData scopeData)
		{
			await Task.CompletedTask;
			string partialName = Value;
			var currentPartial = partialName + "_" + scopeData.PartialDepth.Count;
			scopeData.PartialDepth.Push(currentPartial);
			if (scopeData.PartialDepth.Count >= context.Options.PartialStackSize)
			{
				switch (context.Options.StackOverflowBehavior)
				{
					case PartialStackOverflowBehavior.FailWithException:
						throw new MustachioStackOverflowException(
							$"You have exceeded the maximum stack Size for nested Partial calls of '{context.Options.PartialStackSize}'. See Data for call stack")
						{
							Data =
							{
								{"Callstack", scopeData.PartialDepth}
							}
						};
					case PartialStackOverflowBehavior.FailSilent:

						break;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			if (scopeData.Partials.TryGetValue(partialName, out var partial))
			{
				return new[]
				{
					new DocumentItemExecution(partial, context),
				};
			}

			partial = context.Options.PartialsStore?.GetPartial(partialName)?.Document;

			if (partial != null)
			{
				return new[]
				{
					new DocumentItemExecution(partial, context),
				};
			}

			throw new MorestachioRuntimeException($"Could not obtain a partial named '{partialName}' from the template nor the Partial store");
		}
		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}