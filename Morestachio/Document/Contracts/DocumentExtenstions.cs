using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Morestachio.Framework;

namespace Morestachio.Document.Contracts
{
	/// <summary>
	///		Helper Functions for Document creation
	/// </summary>
	public static class DocumentExtenstions
	{
		static DocumentExtenstions()
		{
			DocumentItems = typeof(DocumentExtenstions)
				.Assembly
				.GetTypes()
				.Where(e => e.IsClass)
				.Where(e => typeof(IDocumentItem).IsAssignableFrom(e))
				.ToDictionary(e => e, type =>
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

					return new Func<IDocumentItem>(() => ctor.Invoke(null) as IDocumentItem);
				});
		}

		public static IDictionary<Type, Func<IDocumentItem>> DocumentItems { get; private set; }

		internal static IDocumentItem CreateDocumentItemInstance(string name)
		{
			var docItem =
				DocumentItems.FirstOrDefault(e => e.Key.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase))
					.Value;
			if (docItem == null)
			{
				throw new InvalidOperationException($"There is no known DocumentType with the name of '{name}'");
			}

			return docItem();
		}

		/// <summary>
		///		
		/// </summary>
		public static IEnumerable<DocumentItemExecution> WithScope(this IEnumerable<IDocumentItem> items, ContextObject contextObject)
		{
			foreach (var documentItem in items)
			{
				yield return new DocumentItemExecution(documentItem, contextObject);
			}
		}
	}
}