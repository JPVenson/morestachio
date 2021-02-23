using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Framework.Attributes;

namespace Morestachio.Formatter.Predefined
{
#pragma warning disable CS1591
	/// <summary>
	/// 
	/// </summary>
	public static class ListExtensions
	{
		[MorestachioGlobalFormatter("With", "Creates a new List object with the given values")]
		public static IList With([RestParameter] params object[] toBeAdded)
		{
			if (toBeAdded.Length == 0)
			{
				return new List<object>();
			}

			var fodType = toBeAdded.First().GetType();
			if (toBeAdded.All(e => e.GetType() == fodType))
			{
				var instance = Activator.CreateInstance(typeof(List<>).MakeGenericType(fodType)) as IList;
				foreach (var o in toBeAdded)
				{
					instance.Add(o);
				}

				return instance;
			}

			return new List<object>(toBeAdded);
		}


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

		[MorestachioGlobalFormatter("ToList", "Gets any number of elements and returns a new list containing those elements")]
		public static IList<T> RemoveAt<T>(params object[] items)
		{
			var itemsList = new List<T>();
			itemsList.AddRange(items.OfType<T>());
			return itemsList;
		}


	}
}
