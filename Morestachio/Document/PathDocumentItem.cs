using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Framework;

namespace Morestachio.Document
{
	/// <summary>
	///		An single Value expression
	/// </summary>
	[System.Serializable]
	public class PathDocumentItem : ValueDocumentItemBase, IValueDocumentItem
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal PathDocumentItem()
		{

		}

		/// <inheritdoc />
		public PathDocumentItem(string value, bool escapeValue)
		{
			Value = value;
			EscapeValue = escapeValue;
		}

		[UsedImplicitly]
		protected PathDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			EscapeValue = info.GetBoolean(nameof(EscapeValue));
		}

		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			base.SerializeBinaryCore(info, context);
			info.AddValue(nameof(EscapeValue), EscapeValue);
		}

		/// <inheritdoc />
		public override string Kind { get; } = "Expression";

		/// <summary>
		/// Gets a value indicating whether [escape value].
		/// </summary>
		/// <value>
		///   <c>true</c> if [escape value]; otherwise, <c>false</c>.
		/// </value>
		public bool EscapeValue { get; private set; }

		private static string HtmlEncodeString(string context)
		{
			return WebUtility.HtmlEncode(context);
		}
		
		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			//try to locate the value in the context, if it exists, append it.
			var contextObject = context != null ? (await context.GetContextForPath(Value, scopeData)) : null;
			if (contextObject != null)
			{
				await contextObject.EnsureValue();
				if (EscapeValue && !context.Options.DisableContentEscaping)
				{
					ContentDocumentItem.WriteContent(outputStream, HtmlEncodeString(await contextObject.RenderToString()), contextObject);
				}
				else
				{
					ContentDocumentItem.WriteContent(outputStream, await contextObject.RenderToString(), contextObject);
				}
			}
			
			return Children.WithScope(contextObject);
		}

		public async Task<ContextObject> GetValue(ContextObject context, ScopeData scopeData)
		{
			await Task.CompletedTask;
			return await context.GetContextForPath(Value, scopeData);
		}
	}
}