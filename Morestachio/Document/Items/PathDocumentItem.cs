#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#endif
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.Serialization;
using System.Xml;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		An single Value expression
	/// </summary>
	[Serializable]
	public class PathDocumentItem : ExpressionDocumentItemBase, ISupportCustomCompilation
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal PathDocumentItem()
		{

		}

		/// <inheritdoc />
		public PathDocumentItem(CharacterLocation location,  IMorestachioExpression value, bool escapeValue,
			IEnumerable<ITokenOption> tagCreationOptions) 
			: base(location, value,tagCreationOptions)
		{
			EscapeValue = escapeValue;
		}

		/// <inheritdoc />
		
		protected PathDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			EscapeValue = info.GetBoolean(nameof(EscapeValue));
		}

		/// <inheritdoc />
		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			base.SerializeBinaryCore(info, context);
			info.AddValue(nameof(EscapeValue), EscapeValue);
		}

		/// <inheritdoc />
		protected override void SerializeXml(XmlWriter writer)
		{
			writer.WriteAttributeString(nameof(EscapeValue), EscapeValue.ToString());
			base.SerializeXml(writer);
		}

		/// <inheritdoc />
		protected override void DeSerializeXml(XmlReader reader)
		{
			EscapeValue = reader.GetAttribute(nameof(EscapeValue)) == Boolean.TrueString;
			base.DeSerializeXml(reader);
		}
		
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
		public Compilation Compile()
		{
			var children = MorestachioDocument.CompileItemsAndChildren(Children);
			var expression = MorestachioExpression.Compile();
			return async (outputStream, context, scopeData) =>
			{
				//try to locate the value in the context, if it exists, append it.
				var contextObject = context != null ? (await expression(context, scopeData)) : null;
				if (contextObject != null)
				{
					//await contextObject.EnsureValue();
					if (EscapeValue && !context.Options.DisableContentEscaping)
					{
						outputStream.Write(HtmlEncodeString(await contextObject.RenderToString()));
					}
					else
					{
						outputStream.Write(await contextObject.RenderToString());
					}
				}

				await children(outputStream, contextObject, scopeData);
			};
		}
		
		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
		{
			//try to locate the value in the context, if it exists, append it.
			var contextObject = context != null ? (await MorestachioExpression.GetValue(context, scopeData)) : null;
			if (contextObject != null)
			{
				//await contextObject.EnsureValue();
				if (EscapeValue && !context.Options.DisableContentEscaping)
				{
					outputStream.Write(HtmlEncodeString(await contextObject.RenderToString()));
				}
				else
				{
					outputStream.Write(await contextObject.RenderToString());
				}
			}
			
			return Children.WithScope(contextObject);
		}
		
		/// <inheritdoc />
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

		/// <inheritdoc />
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

		/// <inheritdoc />
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = base.GetHashCode();
				hashCode = (hashCode * 397) ^ EscapeValue.GetHashCode();
				return hashCode;
			}
		}

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}