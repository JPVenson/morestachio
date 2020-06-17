using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Visitor;
using Morestachio.Framework;

namespace Morestachio.Document
{
	/// <summary>
	///		Defines a document that can be rendered. Does only store its Children
	/// </summary>
	[System.Serializable]
	public sealed class MorestachioDocument : DocumentItemBase, 
		IEquatable<MorestachioDocument>
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
		internal MorestachioDocument()
		{
			MorestachioVersion = GetMorestachioVersion();
		}
		
		/// <inheritdoc />
		[UsedImplicitly]
		public MorestachioDocument(SerializationInfo info, StreamingContext c) : base(info, c)
		{
			MorestachioVersion = info.GetValue(nameof(MorestachioVersion), typeof(Version)) as Version;
			//var serializedHashCode = info.GetInt32(nameof(GetHashCode));
			//if (serializedHashCode != GetHashCode())
			//{
			//	throw new InvalidOperationException("The hashcode check for the Morestachio document failed");
			//}
		}

		/// <inheritdoc />
		protected override void SerializeBinaryCore(SerializationInfo info, StreamingContext context)
		{
			info.AddValue(nameof(MorestachioVersion), MorestachioVersion.ToString());
			//info.AddValue(nameof(GetHashCode), GetHashCode());
			base.SerializeBinaryCore(info, context);
		}

		/// <inheritdoc />
		protected override void DeSerializeXml(XmlReader reader)
		{
			MorestachioVersion = Version.Parse(reader.GetAttribute(nameof(MorestachioVersion)));
			//var attribute = reader.GetAttribute(nameof(GetHashCode));
			//if (!string.IsNullOrWhiteSpace(attribute))
			//{
			//	DeserializedHashCode = int.Parse(attribute);
			//}
			
			base.DeSerializeXml(reader);
		}
		
		/// <inheritdoc />
		protected override void SerializeXml(XmlWriter writer)
		{
			writer.WriteAttributeString(nameof(MorestachioVersion), MorestachioVersion.ToString());
			//writer.WriteAttributeString(nameof(GetHashCode), GetHashCode().ToString());
			base.SerializeXml(writer);
		}

		///// <summary>
		/////		Will only be set in case of an Serialization. Can be used to check if the current object (this.GetHashCode()) and the written hashcode are the same
		///// </summary>
		//public int DeserializedHashCode { get; private set; }

		/// <summary>
		///		Gets the Version of Morestachio that this Document was parsed with
		/// </summary>
		public Version MorestachioVersion { get; private set; }

		/// <inheritdoc />
		public override string Kind { get; } = "Document";

		/// <inheritdoc />
		public override async Task<IEnumerable<DocumentItemExecution>> Render(IByteCounterStream outputStream, ContextObject context,
			ScopeData scopeData)
		{
			await Task.CompletedTask;
			return Children.WithScope(context);
		}

		/// <summary>
		///		Processes the items and children.
		/// </summary>
		/// <param name="documentItems">The document items.</param>
		/// <param name="outputStream">The output stream.</param>
		/// <param name="context">The context.</param>
		/// <param name="scopeData">The scope data.</param>
		/// <returns></returns>
		public static async Task ProcessItemsAndChildren(IEnumerable<IDocumentItem> documentItems, 
			IByteCounterStream outputStream, 
			ContextObject context,
			ScopeData scopeData)
		{
			//we do NOT use a recursive loop to avoid stack overflows. 

			var processStack = new Stack<DocumentItemExecution>(); //deep search. create a stack to go deeper into the tree without loosing work left on other branches

			foreach (var documentItem in documentItems.TakeWhile(e => ContinueBuilding(outputStream, context))) //abort as soon as the cancellation is requested OR the template size is reached
			{
				processStack.Push(new DocumentItemExecution(documentItem, context));
				while (processStack.Any() && ContinueBuilding(outputStream, context))
				{
					var currentDocumentItem = processStack.Pop();//take the current branch
					var next = await currentDocumentItem.DocumentItem.Render(outputStream, currentDocumentItem.ContextObject, scopeData);
					foreach (var item in next.Reverse()) //we have to reverse the list as the logical first item returned must be the last inserted to be the next that pops out
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

			return Equals((MorestachioDocument) obj);
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
}