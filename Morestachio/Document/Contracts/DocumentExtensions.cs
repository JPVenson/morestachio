using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Morestachio.Framework.Context;

namespace Morestachio.Document.Contracts
{
	/// <summary>
	///		Helper Functions for Document creation
	/// </summary>
	public static class DocumentExtensions
	{
		static DocumentExtensions()
		{
			DocumentItems = typeof(DocumentExtensions)
				.Assembly
				.GetTypes()
				.Where(e => e.IsClass && !e.IsAbstract)
				.Where(e => typeof(IDocumentItem).IsAssignableFrom(e))
				.Where(e => e.GetCustomAttribute<SerializableAttribute>(false) != null)
				.ToDictionary(e => e.Name, type =>
				{
					var ctor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
						null, Type.EmptyTypes, null);
					if (ctor == null)
					{
						ctor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance,
							null, Type.EmptyTypes, null);
					}
					if (ctor == null)
					{
						throw new InvalidOperationException("There is no ether public or private constructor that has no parameter for " + type.Name);
					}

					return new Func<IDocumentItem>(() => ctor.Invoke(null) as IDocumentItem);
				});
		}
		
		internal static IDictionary<string, Func<IDocumentItem>> DocumentItems { get; private set; }

		internal static IDocumentItem CreateDocumentItemInstance(string name)
		{
			var docItem =
				DocumentItems.FirstOrDefault(e => e.Key.Equals(name, StringComparison.OrdinalIgnoreCase))
					.Value;
			if (docItem == null)
			{
				var type = AppDomain.CurrentDomain.GetAssemblies().SelectMany(f => f.GetTypes())
					.FirstOrDefault(e => e.IsClass && e.Name == name + "DocumentItem");
				if (type != null)
				{
					var ctor = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance,
						null, Type.EmptyTypes, null);
					if (ctor == null)
					{
						ctor = type.GetConstructor(BindingFlags.Public | BindingFlags.Instance,
							null, Type.EmptyTypes, null);
					}
					if (ctor == null)
					{
						throw new InvalidOperationException("There is no ether public or private constructor that has no parameter");
					}

					docItem = DocumentItems[type.Name] = () => ctor.Invoke(null) as IDocumentItem;
				}
				else
				{
					throw new InvalidOperationException($"There is no known DocumentType with the name of '{name}'");	
				}
			}

			return docItem();
		}

		/// <summary>
		///		Wraps the Document items with an scope they should be executed with
		/// </summary>
		public static IEnumerable<DocumentItemExecution> WithScope(this IEnumerable<IDocumentItem> items, ContextObject contextObject)
		{
			return items.Select(e => new DocumentItemExecution(e, contextObject));
		}
	}
}