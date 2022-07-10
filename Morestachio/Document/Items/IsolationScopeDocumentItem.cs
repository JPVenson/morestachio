using System.Security.Permissions;
using System.Xml;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Framework.Expression.Parser;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper.Serialization;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Document.Items;

/// <summary>
///		Document item for Isolation
/// </summary>
[Serializable]
public class IsolationScopeDocumentItem : BlockDocumentItemBase, ISupportCustomAsyncCompilation
{
	/// <inheritdoc />
	public IsolationScopeDocumentItem()
	{

	}

	/// <inheritdoc />
	public IsolationScopeDocumentItem(TextRange location, IsolationOptions isolationOptions,
									IMorestachioExpression morestachioExpression,
									IEnumerable<ITokenOption> tagCreationOptions) : base(location, tagCreationOptions)
	{
		Isolation = isolationOptions;
		ScopeIsolationExpression = morestachioExpression;
	}

	/// <summary>
	///		Creates a new DocumentItemBase from a Serialization context
	/// </summary>
	/// <param name="info"></param>
	/// <param name="c"></param>
	protected IsolationScopeDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
	{
		Isolation = (IsolationOptions)info.GetValue(nameof(Isolation), typeof(IsolationOptions));
		ScopeIsolationExpression = info.GetValueOrDefault<IMorestachioExpression>(c, nameof(ScopeIsolationExpression));
	}

	/// <inheritdoc />
	protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
	{
		info.AddValue(nameof(Isolation), Isolation);
		info.AddValue(nameof(ScopeIsolationExpression), ScopeIsolationExpression);
		base.SerializeBinaryCore(info, context);
	}

	/// <inheritdoc />
	protected override void SerializeXmlHeaderCore(XmlWriter writer)
	{
		base.SerializeXmlHeaderCore(writer);
		foreach (var flag in Isolation.GetFlags())
		{
			writer.WriteAttributeString(flag.ToString(), "true");
		}
	}

	/// <inheritdoc />
	protected override void DeSerializeXmlHeaderCore(XmlReader reader)
	{
		base.DeSerializeXmlHeaderCore(reader);

		foreach (IsolationOptions option in Enum.GetValues(typeof(IsolationOptions)))
		{
			if (reader.GetAttribute(option.ToString()) == "true")
			{
				Isolation |= option;
			}
		}
	}

	/// <inheritdoc />
	protected override void SerializeXmlBodyCore(XmlWriter writer)
	{
		base.SerializeXmlBodyCore(writer);

		if (ScopeIsolationExpression != null)
		{
			writer.WriteStartElement(nameof(ScopeIsolationExpression));
			writer.WriteExpressionToXml(ScopeIsolationExpression);
			writer.WriteEndElement();
		}
	}

	/// <inheritdoc />
	protected override void DeSerializeXmlBodyCore(XmlReader reader)
	{
		base.DeSerializeXmlBodyCore(reader);

		if (reader.Name == nameof(ScopeIsolationExpression))
		{
			reader.ReadStartElement();
			var subtree = reader.ReadSubtree();
			subtree.Read();
			ScopeIsolationExpression = subtree.ParseExpressionFromKind();
			reader.Skip();
			reader.ReadEndElement();
		}
	}

	/// <summary>
	///		The type of isolation enforced
	/// </summary>
	public IsolationOptions Isolation { get; private set; }

	/// <summary>
	///		Is set, defines the path that should be isolated to
	/// </summary>
	public IMorestachioExpression ScopeIsolationExpression { get; private set; }

	/// <inheritdoc />
	public override async ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context, ScopeData scopeData)
	{
		if (ScopeIsolationExpression != null)
		{
			context = await ScopeIsolationExpression.GetValue(context, scopeData).ConfigureAwait(false);
			context = new ContextObject(context.Key, null, context.Value);
		}

		return Children.WithScope(context);
	}

	/// <inheritdoc />
	public override void Accept(IDocumentItemVisitor visitor)
	{
		visitor.Visit(this);
	}

	/// <param name="compiler"></param>
	/// <param name="parserOptions"></param>
	/// <inheritdoc />
	public CompilationAsync Compile(IDocumentCompiler compiler, ParserOptions parserOptions)
	{
		var children = compiler.Compile(Children, parserOptions);
		if (ScopeIsolationExpression != null)
		{
			var compiledExpression = ScopeIsolationExpression.Compile(parserOptions);
			return async (stream, context, data) =>
			{
				context = await compiledExpression(context, data).ConfigureAwait(false);
				context = new ContextObject(context.Key, null, context.Value);
				await children(stream, context, data).ConfigureAwait(false);
			};
		}


		return children;
	}
}