using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Framework.Expression;

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

		[MorestachioFormatter("Where", "Filteres the collection based on the predicate")]
		public static IEnumerable<T> Where<T>(IEnumerable<T> items, MorestachioTemplateExpression expression)
		{
			return items.Where(expression.AsFunc<T, bool>());
		}

		[MorestachioFormatter("Select", "Selects items from the collection based on the predicate")]
		public static IEnumerable<TE> Select<T, TE>(IEnumerable<T> items, MorestachioTemplateExpression expression)
		{
			return items.Select(expression.AsFunc<T, TE>());
		}

		[MorestachioFormatter("SelectMany", "Selects a list of items from the collection based on the predicate and flattens them")]
		public static IEnumerable<TE> SelectMany<T, TE>(IEnumerable<T> items, MorestachioTemplateExpression expression)
		{
			return items.SelectMany(expression.AsFunc<T, IEnumerable<TE>>());
		}

		[MorestachioFormatter("TakeWhile", "Takes items from the collection as long as the predicate is true")]
		public static IEnumerable<T> TakeWhile<T>(IEnumerable<T> items, MorestachioTemplateExpression expression)
		{
			return items.TakeWhile(expression.AsFunc<T, bool>());
		}

		[MorestachioFormatter("SkipWhile", "Skips items from the collection as long as the predicate is true")]
		public static IEnumerable<T> SkipWhile<T>(IEnumerable<T> items, MorestachioTemplateExpression expression)
		{
			return items.SkipWhile(expression.AsFunc<T, bool>());
		}

		[MorestachioFormatter("OrderBy", "Orders the list descending")]
		public static IOrderedEnumerable<T> OrderBy<T>(IEnumerable<T> items, MorestachioTemplateExpression expression)
		{
			return items.OrderBy(expression.AsFunc<T, bool>());
		}

		[MorestachioFormatter("OrderBy", "Orders the list descending")]
		public static IOrderedEnumerable<T> OrderBy<T>(IEnumerable<T> items)
		{
			return items.OrderBy(e => e);
		}

		[MorestachioFormatter("ThenBy", "Orders the list descending")]
		public static IOrderedEnumerable<T> ThenBy<T>(IOrderedEnumerable<T> items)
		{
			return items.ThenBy(e => e);
		}

		[MorestachioFormatter("ThenBy", "Orders the list descending")]
		public static IOrderedEnumerable<T> ThenBy<T>(IOrderedEnumerable<T> items, MorestachioTemplateExpression expression)
		{
			return items.ThenBy(expression.AsFunc<T, bool>());
		}

		[MorestachioFormatter("GroupBy", "Groups a list")]
		public static IEnumerable<IGrouping<TE, T>> GroupBy<T, TE>(IEnumerable<T> items, MorestachioTemplateExpression expression)
		{
			return items.GroupBy(expression.AsFunc<T, TE>());
		}

		[MorestachioFormatter("First", "Selected the first item that matches the condition")]
		public static T First<T>(IEnumerable<T> items, MorestachioTemplateExpression expression)
		{
			return items.First(expression.AsFunc<T, bool>());
		}

		[MorestachioFormatter("FirstOrDefault", "Selected the first item that matches the condition")]
		public static T FirstOrDefault<T>(IEnumerable<T> items, MorestachioTemplateExpression expression)
		{
			return items.FirstOrDefault(expression.AsFunc<T, bool>());
		}

		[MorestachioFormatter("Last", "Gets the Last item in the list that matches the predicate")]
		public static T Last<T>(IEnumerable<T> items, MorestachioTemplateExpression expression)
		{
			return items.Last(expression.AsFunc<T, bool>());
		}

		[MorestachioFormatter("Last", "Gets the Last item in the list that matches the predicate")]
		public static T LastOrDefault<T>(IEnumerable<T> items, MorestachioTemplateExpression expression)
		{
			return items.LastOrDefault(expression.AsFunc<T, bool>());
		}

		[MorestachioFormatter("Single", "Gets the only item in the list that matches the predicate")]
		public static T Single<T>(IEnumerable<T> items, MorestachioTemplateExpression expression)
		{
			return items.Single(expression.AsFunc<T, bool>());
		}

		[MorestachioFormatter("SingleOrDefault", "Gets the only item in the list that matches the predicate")]
		public static T SingleOrDefault<T>(IEnumerable<T> items)
		{
			return items.SingleOrDefault();
		}

		[MorestachioFormatter("SingleOrDefault", "Gets the only item in the list that matches the predicate")]
		public static T SingleOrDefault<T>(IEnumerable<T> items, MorestachioTemplateExpression expression)
		{
			return items.SingleOrDefault(expression.AsFunc<T, bool>());
		}

		[MorestachioFormatter("Any", "returns if Any elements in the collection matches the condition")]
		public static bool Any<T>(IEnumerable<T> items, MorestachioTemplateExpression expression)
		{
			return items.Any(expression.AsFunc<T, bool>());
		}

		[MorestachioFormatter("All", "returns if All elements in the collection matches the condition")]
		public static bool All<T>(IEnumerable<T> items, MorestachioTemplateExpression expression)
		{
			return items.All(expression.AsFunc<T, bool>());
		}

		[MorestachioFormatter("Count", "Counts all items that matches the predicate")]
		public static int Count<T>(IEnumerable<T> items, MorestachioTemplateExpression expression)
		{
			return items.Count(expression.AsFunc<T, bool>());
		}

		[MorestachioFormatter("Aggregate", "Counts all items that matches the predicate")]
		public static T Aggregate<T>(IEnumerable<T> items, MorestachioTemplateExpression expression)
		{
			return items.Aggregate(expression.AsFunc<T, T, T>());
		}
	}
}
