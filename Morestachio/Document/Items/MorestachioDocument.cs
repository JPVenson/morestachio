﻿using System.Xml;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper;
using Morestachio.Helper.Serialization;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Document.Items;

/// <summary>
///		Defines a document that can be rendered. Does only store its Children
/// </summary>
[Serializable]
public sealed class MorestachioDocument : BlockDocumentItemBase,
										IEquatable<MorestachioDocument>, ISupportCustomAsyncCompilation
{
	/// <summary>
	///		Gets the current version of Morestachio
	/// </summary>
	/// <returns></returns>
	public static Version GetMorestachioVersion()
	{
		return typeof(MorestachioDocument).Assembly.GetName().Version;
	}

	/// <summary>
	///		Used for XML Serialization
	/// </summary>
	public MorestachioDocument()
	{
		MorestachioVersion = GetMorestachioVersion();
	}

	/// <summary>
	///		Used for XML Serialization
	/// </summary>
	public MorestachioDocument(TextRange location,
								IEnumerable<ITokenOption> tagCreationOptions) : base(location, tagCreationOptions)
	{
		MorestachioVersion = GetMorestachioVersion();
	}

	/// <inheritdoc />
	public MorestachioDocument(SerializationInfo info, StreamingContext c) : base(info, c)
	{
		MorestachioVersion = info.GetValue(nameof(MorestachioVersion), typeof(Version)) as Version;
	}

	/// <inheritdoc />
	protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
	{
		info.AddValue(nameof(MorestachioVersion), MorestachioVersion.ToString());
		base.SerializeBinaryCore(info, context);
	}

	/// <inheritdoc />
	protected override void SerializeXmlHeaderCore(XmlWriter writer)
	{
		base.SerializeXmlHeaderCore(writer);
		writer.WriteAttributeString(nameof(MorestachioVersion), MorestachioVersion.ToString());
	}

	/// <inheritdoc />
	protected override void DeSerializeXmlHeaderCore(XmlReader reader)
	{
		base.DeSerializeXmlHeaderCore(reader);

		var versionAttribute = reader.GetAttribute(nameof(MorestachioVersion));

		if (!Version.TryParse(versionAttribute, out var version))
		{
			throw new XmlException($"Error while serializing '{nameof(MorestachioDocument)}'. " +
				$"The value for '{nameof(MorestachioVersion)}' is expected to be an version string in form of 'x.x.x.x' .");
		}

		MorestachioVersion = version;
	}

	/// <summary>
	///		Gets the Version of Morestachio that this Document was parsed with
	/// </summary>
	public Version MorestachioVersion { get; private set; }

	/// <param name="compiler"></param>
	/// <param name="parserOptions"></param>
	/// <inheritdoc />
	public CompilationAsync Compile(IDocumentCompiler compiler, ParserOptions parserOptions)
	{
		var compilation = compiler.Compile(Children, parserOptions);
		return async (stream, context, data) => { await compilation(stream, context, data).ConfigureAwait(false); };
	}

	/// <inheritdoc />
	public override ItemExecutionPromise Render(IByteCounterStream outputStream,
												ContextObject context,
												ScopeData scopeData)
	{
		return Children.WithScope(context).ToPromise();
	}

	/// <summary>
	///		Processes the items and children.
	/// </summary>
	/// <param name="documentItems">The document items.</param>
	/// <param name="outputStream">The output stream.</param>
	/// <param name="context">The context.</param>
	/// <param name="scopeData">The scope data.</param>
	/// <returns></returns>
	public static async Promise ProcessItemsAndChildren(IEnumerable<IDocumentItem> documentItems,
														IByteCounterStream outputStream,
														ContextObject context,
														ScopeData scopeData)
	{
		//we do NOT use a recursive loop to avoid stack overflows. 

		var processStack
			= new Stack<DocumentItemExecution>(); //deep search. create a stack to go deeper into the tree without loosing work left on other branches

		foreach (var documentItem in documentItems)
		{
			//abort as soon as the cancellation is requested OR the template size is reached
			if (scopeData.IsOutputLimited && !ContinueBuilding(outputStream, scopeData))
			{
				break;
			}

			processStack.Push(new DocumentItemExecution(documentItem, context));

			while (processStack.Any() && ContinueBuilding(outputStream, scopeData))
			{
				var currentDocumentItem = processStack.Pop(); //take the current branch
				var next = await currentDocumentItem.DocumentItem
					.Render(outputStream, currentDocumentItem.ContextObject, scopeData).ConfigureAwait(false);

				foreach (var item in
						next.Reverse()) //we have to reverse the list as the logical first item returned must be the last inserted to be the next that pops out
				{
					processStack.Push(item);
				}
			}
		}
	}

	/// <inheritdoc />
	public override void Accept(IDocumentItemVisitor visitor)
	{
		visitor.Visit(this);
	}

	/// <inheritdoc />
	public bool Equals(MorestachioDocument other)
	{
		if (ReferenceEquals(null, other))
		{
			return false;
		}

		if (ReferenceEquals(this, other))
		{
			return true;
		}

		return base.Equals(other) &&
			Equals(MorestachioVersion, other.MorestachioVersion);
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

		return Equals((MorestachioDocument)obj);
	}

	/// <inheritdoc />
	public override int GetHashCode()
	{
		unchecked
		{
			return ((MorestachioVersion != null ? MorestachioVersion.GetHashCode() : 0) * 397) ^
				base.GetHashCode();
		}
	}
}