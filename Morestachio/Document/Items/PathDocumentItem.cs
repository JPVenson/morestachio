using System.Net;
using System.Xml;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper.Serialization;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Document.Items;

/// <summary>
///		An single Value expression
/// </summary>
[Serializable]
public class PathDocumentItem : ExpressionDocumentItemBase, ISupportCustomAsyncCompilation
{
	/// <summary>
	///		Used for XML Serialization
	/// </summary>
	internal PathDocumentItem()
	{

	}

	/// <inheritdoc />
	public PathDocumentItem(TextRange location,  IMorestachioExpression value, bool escapeValue,
							IEnumerable<ITokenOption> tagCreationOptions) 
		: base(location, value, tagCreationOptions)
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
	protected override void SerializeXmlHeaderCore(XmlWriter writer)
	{
		base.SerializeXmlHeaderCore(writer);
		writer.WriteAttributeString(nameof(EscapeValue), EscapeValue.ToString());
	}

	/// <inheritdoc />
	protected override void DeSerializeXmlHeaderCore(XmlReader reader)
	{
		base.DeSerializeXmlHeaderCore(reader);
		EscapeValue = reader.GetAttribute(nameof(EscapeValue)) == bool.TrueString;
	}
		
	/// <summary>
	/// Gets a value indicating whether [escape value].
	/// </summary>
	/// <value>
	///   <c>true</c> if [escape value]; otherwise, <c>false</c>.
	/// </value>
	public bool EscapeValue { get; private set; }
	
#if Span
	private static ReadOnlySpan<char> HtmlEncodeString(ReadOnlySpan<char> context)
	{
		return WebUtility.HtmlEncode(context.ToString()).AsSpan();
	}
#else
	private static string HtmlEncodeString(string context)
	{
		return WebUtility.HtmlEncode(context);
	}
#endif

	/// <param name="compiler"></param>
	/// <param name="parserOptions"></param>
	/// <inheritdoc />
	public CompilationAsync Compile(IDocumentCompiler compiler, ParserOptions parserOptions)
	{
		//var children = compiler.Compile(Children, parserOptions);
		var expression = MorestachioExpression.Compile(parserOptions);
			
		//try to locate the value in the context, if it exists, append it.
		if (EscapeValue && !parserOptions.DisableContentEscaping)
		{
			return async (outputStream, context, scopeData) =>
			{
				var contextObject = await expression(context, scopeData).ConfigureAwait(false);
				outputStream.Write(HtmlEncodeString(contextObject.RenderToString(scopeData)));
			};
		}

		return async (outputStream, context, scopeData) =>
		{
			var contextObject = await expression(context, scopeData).ConfigureAwait(false);
			outputStream.Write(contextObject.RenderToString(scopeData));
		};
	}
		
	/// <inheritdoc />
	public override async ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
	{
		//try to locate the value in the context, if it exists, append it.
		var contextObject = context != null ? (await MorestachioExpression.GetValue(context, scopeData).ConfigureAwait(false)) : null;
		if (contextObject != null)
		{
			//await contextObject.EnsureValue();
			if (EscapeValue && !scopeData.ParserOptions.DisableContentEscaping)
			{
				outputStream.Write(HtmlEncodeString(contextObject.RenderToString(scopeData)));
			}
			else
			{
				outputStream.Write(contextObject.RenderToString(scopeData));
			}
		}

		return Enumerable.Empty<DocumentItemExecution>();
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