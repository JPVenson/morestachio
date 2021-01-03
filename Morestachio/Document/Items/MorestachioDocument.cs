#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.ValueTask;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
using Promise = System.Threading.Tasks.Task;
#endif
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml;
using JetBrains.Annotations;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.IO;
using Morestachio.Framework.Tokenizing;
using Morestachio.Helper;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Defines a document that can be rendered. Does only store its Children
	/// </summary>
	[Serializable]
	public sealed class MorestachioDocument : BlockDocumentItemBase,
		IEquatable<MorestachioDocument>, ISupportCustomCompilation
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
		public MorestachioDocument(CharacterLocation location,
			IEnumerable<ITokenOption> tagCreationOptions) : base(location,tagCreationOptions)
		{
			MorestachioVersion = GetMorestachioVersion();
		}

		/// <inheritdoc />
		[UsedImplicitly]
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
		protected override void DeSerializeXml(XmlReader reader)
		{
			var versionAttribute = reader.GetAttribute(nameof(MorestachioVersion));

			if (!Version.TryParse(versionAttribute, out var version))
			{
				throw new XmlException($"Error while serializing '{nameof(MorestachioDocument)}'. " +
									   $"The value for '{nameof(MorestachioVersion)}' is expected to be an version string in form of 'x.x.x.x' .");
			}

			MorestachioVersion = version;
			base.DeSerializeXml(reader);
		}

		/// <inheritdoc />
		protected override void SerializeXml(XmlWriter writer)
		{
			writer.WriteAttributeString(nameof(MorestachioVersion), MorestachioVersion.ToString());
			base.SerializeXml(writer);
		}

		/// <summary>
		///		Gets the Version of Morestachio that this Document was parsed with
		/// </summary>
		public Version MorestachioVersion { get; private set; }

		/// <inheritdoc />
		public Compilation Compile()
		{
			var compilation = CompileItemsAndChildren(Children);
			return async (stream, context, data) =>
			{
				await compilation(stream, context, data);
			};
		}

		/// <inheritdoc />
		public override ItemExecutionPromise Render(IByteCounterStream outputStream, ContextObject context,
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

		/// <summary>
		///		Compiles all <see cref="IDocumentItem"/> and their children. If the <see cref="IDocumentItem"/> supports the <see cref="ISupportCustomCompilation"/> it is used otherwise
		///		the items <see cref="IDocumentItem.Render"/> method is wrapped
		/// </summary>
		/// <param name="documentItems"></param>
		/// <returns></returns>
		public static Compilation CompileItemsAndChildren(IEnumerable<IDocumentItem> documentItems)
		{
			var docs = documentItems.ToArray();
			var actions = new Compilation[docs.Length];

			for (var index = 0; index < docs.Length; index++)
			{
				var documentItem = docs[index];
				var document = documentItem;
				if (document is ISupportCustomCompilation customCompilation)
				{
					actions[index] = (customCompilation.Compile());
				}
				else
				{
					actions[index] = (async (IByteCounterStream outputStream,
						ContextObject context,
						ScopeData scopeData) =>
					{
						var children = await document.Render(outputStream, context, scopeData);

						foreach (var documentItemExecution in children)
						{
							await ProcessItemsAndChildren(new IDocumentItem[]
							{
								documentItemExecution.DocumentItem
							}, outputStream, documentItemExecution.ContextObject, scopeData);
						}
					});
				}
			}

			return FastExecuteItems(actions);
		}

		private static Compilation FastExecuteItems(Compilation[] actions)
		{
			async Promise ExecuteTenItems(IByteCounterStream stream, ContextObject context, ScopeData data)
			{
				if (!ContinueBuilding(stream, context))
				{
					return;
				}

				await actions[0](stream, context, data);
				if (!ContinueBuilding(stream, context))
				{
					return;
				}

				await actions[1](stream, context, data);
				if (!ContinueBuilding(stream, context))
				{
					return;
				}

				await actions[2](stream, context, data);
				if (!ContinueBuilding(stream, context))
				{
					return;
				}

				await actions[3](stream, context, data);
				if (!ContinueBuilding(stream, context))
				{
					return;
				}

				await actions[4](stream, context, data);
				if (!ContinueBuilding(stream, context))
				{
					return;
				}

				await actions[5](stream, context, data);
				if (!ContinueBuilding(stream, context))
				{
					return;
				}

				await actions[6](stream, context, data);
				if (!ContinueBuilding(stream, context))
				{
					return;
				}

				await actions[7](stream, context, data);
				if (!ContinueBuilding(stream, context))
				{
					return;
				}

				await actions[8](stream, context, data);
				if (!ContinueBuilding(stream, context))
				{
					return;
				}

				await actions[9](stream, context, data);
			}

			if (actions.Length == 0)
			{
				return async (stream, context, data) =>
				{
					await AsyncHelper.FakePromise();
				};
			}
			else if (actions.Length == 1)
			{
				return async (stream, context, data) =>
				{
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[0](stream, context, data);
				};
			}
			else if (actions.Length == 2)
			{
				return async (stream, context, data) =>
				{
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[0](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[1](stream, context, data);
				};
			}
			else if (actions.Length == 3)
			{
				return async (stream, context, data) =>
				{
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[0](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[1](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[2](stream, context, data);
				};
			}
			else if (actions.Length == 4)
			{
				return async (stream, context, data) =>
				{
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[0](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[1](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[2](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[3](stream, context, data);
				};
			}
			else if (actions.Length == 5)
			{
				return async (stream, context, data) =>
				{
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[0](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[1](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[2](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[3](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[4](stream, context, data);
				};
			}
			else if (actions.Length == 6)
			{
				return async (stream, context, data) =>
				{
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[0](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[1](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[2](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[3](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[4](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[5](stream, context, data);
				};
			}
			else if (actions.Length == 7)
			{
				return async (stream, context, data) =>
				{
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[0](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[1](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[2](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[3](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[4](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[5](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[6](stream, context, data);
				};
			}
			else if (actions.Length == 8)
			{
				return async (stream, context, data) =>
				{
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[0](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[1](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[2](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[3](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[4](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[5](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[6](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[7](stream, context, data);
				};
			}
			else if (actions.Length == 9)
			{
				return async (stream, context, data) =>
				{
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[0](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[1](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[2](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[3](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[4](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[5](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[6](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[7](stream, context, data);
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[8](stream, context, data);
				};
			}
			else
			{
				if (actions.Length == 10)
				{
					return ExecuteTenItems;
				}
			}

			return async (stream, context, data) =>
			{
				await ExecuteTenItems(stream, context, data);
				for (int i = 10; i < actions.Length; i++)
				{
					if (!ContinueBuilding(stream, context))
					{
						return;
					}
					await actions[i](stream, context, data);	
				}
			};
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
}