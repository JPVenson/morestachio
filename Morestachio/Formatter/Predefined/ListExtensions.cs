using System.Collections.Generic;
using Morestachio.Formatter.Framework.Attributes;

namespace Morestachio.Formatter.Predefined
{
	/// <summary>
	/// 
	/// </summary>
	public static class ListExtensions
	{
		[MorestachioFormatter("Add", "Adds the values to the SourceCollection")]
		public static IList<T> Add<T>(IList<T> sourceCollection, [RestParameter] params object[] toBeAdded)
		{
			foreach (T o in toBeAdded)
			{
				sourceCollection.Add(o);
			}

			return sourceCollection;
		}

		[MorestachioFormatter("Remove", "Removes an element from the SourceCollection")]
		public static IList<T> Remove<T>(IList<T> sourceCollection, [RestParameter] params object[] toBeAdded)
		{
			foreach (T o in toBeAdded)
			{
				sourceCollection.Remove(o);
			}

			return sourceCollection;
		}

		[MorestachioFormatter("Insert", "Inserts a value at the specified index in the SourceCollection.")]
		public static IList<T> Insert<T>(IList<T> sourceCollection, int index, [RestParameter] params object[] toBeInserted)
		{
			foreach (T item in toBeInserted)
			{
				sourceCollection.Insert(index, item);
			}
			return sourceCollection;
		}

		[MorestachioFormatter("RemoveAt", "Removes an element at the specified index from the input list")]
		public static IList<T> RemoveAt<T>(IList<T> sourceCollection, int index)
		{
			sourceCollection.RemoveAt(index);
			return sourceCollection;
		}
	}
}
