using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Framework;

namespace Morestachio.Document
{
	/// <summary>
	///		An single Value expression
	/// </summary>
	[System.Serializable]
	public class PathDocumentItem : ValueDocumentItemBase, IValueDocumentItem, IEquatable<PathDocumentItem>
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

		protected override void SerializeXml(XmlWriter writer)
		{
			writer.WriteAttributeString(nameof(EscapeValue), EscapeValue.ToString());
			base.SerializeXml(writer);
		}

		protected override void DeSerializeXml(XmlReader reader)
		{
			EscapeValue = reader.GetAttribute(nameof(EscapeValue)) == Boolean.TrueString;
			base.DeSerializeXml(reader);
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

		public bool Equals(PathDocumentItem other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return base.Equals(other) && EscapeValue == other.EscapeValue;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != this.GetType())
			{
				return false;
			}

			return Equals((PathDocumentItem) obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = base.GetHashCode();
				hashCode = (hashCode * 397) ^ EscapeValue.GetHashCode();
				return hashCode;
			}
		}
	}
}