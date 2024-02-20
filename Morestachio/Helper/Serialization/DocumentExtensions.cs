using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Morestachio.Document.Contracts;
using Morestachio.Framework.Context;

namespace Morestachio.Helper.Serialization;

/// <summary>
///		Contains helper for serialization and deserialization
/// </summary>
public static class SerializationHelper
{
	static SerializationHelper()
	{
		DocumentItems = typeof(DocumentExtensions)
			.Assembly
			.GetTypes()
			.Where(e => e.IsClass && !e.IsAbstract)
			.Where(e => typeof(IDocumentItem).IsAssignableFrom(e))
			.Where(e => e.GetCustomAttribute<SerializableAttribute>(false) != null)
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
					throw new InvalidOperationException(
						"There is no ether public or private constructor that has no parameter for " + type.Name);
				}

				return new Func<IDocumentItem>(() => ctor.Invoke(null) as IDocumentItem);
			});
	}

	internal static IDictionary<Type, Func<IDocumentItem>> DocumentItems { get; private set; }

	/// <summary>
	///		Creates a new Document from its Name by using its internal register
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	public static IDocumentItem CreateDocumentItemInstance(string name)
	{
		var factory = GetFactory(name);
		return factory.Value();
	}

	private static bool TestTypeName(Type type, string name)
	{
		return type.Name == name ||
			type.FullName == name ||
			type.Name == name + "DocumentItem" ||
			type.ToString() == name;
	}

	private static KeyValuePair<Type, Func<IDocumentItem>> GetFactory(string name)
	{
		var docItem =
			DocumentItems.FirstOrDefault(e => TestTypeName(e.Key, name));

		if (docItem.Key == null)
		{
			var type = AppDomain.CurrentDomain
				.GetAssemblies()
				.SelectMany(f => f.GetTypes().Where(e => e.IsAssignableFrom(typeof(IDocumentItem))))
				.Where(e => e.IsClass)
				.FirstOrDefault(e => TestTypeName(e, name));

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
					throw new InvalidOperationException(
						"There is no ether public or private constructor that has no parameter");
				}

				docItem = new KeyValuePair<Type, Func<IDocumentItem>>(type,
					DocumentItems[type] = () => ctor.Invoke(null) as IDocumentItem);
			}
			else
			{
				throw new InvalidOperationException($"There is no known DocumentType with the name of '{name}'");
			}
		}

		return docItem;
	}

	/// <summary>
	///		Creates a new Document from its Name by using its internal register
	/// </summary>
	/// <param name="name"></param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	public static Type GetDocumentItemType(string name)
	{
		return GetFactory(name).Key;
	}

	/// <summary>
	///		Gets the serializable identifier name
	/// </summary>
	/// <param name="arg"></param>
	/// <returns></returns>
	public static string GetDocumentItemName(Type arg)
	{
		return arg.ToString();
	}
}

/// <summary>
///		Helper Functions for Document creation
/// </summary>
public static class DocumentExtensions
{
	/// <summary>
	///		Wraps the Document items with an scope they should be executed with
	/// </summary>
	public static IEnumerable<DocumentItemExecution> WithScope(this IEnumerable<IDocumentItem> items,
																ContextObject contextObject)
	{
		return items.Select(e => new DocumentItemExecution(e, contextObject));
	}
}