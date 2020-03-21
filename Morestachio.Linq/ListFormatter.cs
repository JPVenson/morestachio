using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic;
using System.Linq.Dynamic.Core;
using JetBrains.Annotations;
using Morestachio.Attributes;
using Morestachio.Formatter.Framework;

namespace Morestachio.Linq
{
	/// <summary>
	///		This class wraps the System.Linq.Dynamic.Core package for Morstachio
	/// </summary>
	[PublicAPI]
	public static class DynamicLinq
	{
		[MorestachioFormatter("first or default", "Selects the First item in the list")]
		public static T FirstOrDefault<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.FirstOrDefault();
		}

		[MorestachioFormatter("order desc", "Orders the list descending")]
		public static IEnumerable<T> OrderDesc<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.OrderByDescending(e => e);
		}

		[MorestachioFormatter("OrderAsc", "Orders the list ascending")]
		public static IEnumerable<T> OrderAsc<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.OrderBy(e => e);
		}

		[MorestachioFormatter("order", "Orders the list ascending")]
		public static IEnumerable<T> Order<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.OrderBy(e => e);
		}

		[MorestachioFormatter("reverse", "Reverses the order of the list")]
		public static IEnumerable<T> Reverse<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.Reverse();
		}

		[MorestachioFormatter("max", "Called on a list of numbers it returns the biggest")]
		public static T Max<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.Max();
		}

		[MorestachioFormatter("min", "Called on a list of numbers it returns the smallest")]
		public static T Min<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.Min();
		}

		[MorestachioFormatter("contains", "Searches in the list for that the argument")]
		[MorestachioFormatterInput("Must be ether a fixed value or an reference $other$")]
		public static bool Contains<T>(IEnumerable<T> sourceCollection, object arguments)
		{
			return sourceCollection.Any(e => e.Equals(arguments));
		}

		[MorestachioFormatter("element at", "Gets the item in the list on the position")]
		[MorestachioFormatterInput("Must be a number")]
		public static T ElementAt<T>(IEnumerable<T> sourceCollection, string arguments)
		{
			return sourceCollection.ElementAtOrDefault(int.Parse(arguments));
		}

		[MorestachioFormatter("order by asc", "Orders the list by the argument")]
		[MorestachioFormatterInput("Must be Expression to property")]
		public static IEnumerable<T> OrderBy<T>(IEnumerable<T> sourceCollection, 
			string expression, 
			params object[] arguments)
		{
			return sourceCollection.AsQueryable().OrderBy(expression, arguments);
		}
		
		[MorestachioFormatter("count", "Gets the count of the list")]
		public static decimal Count<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.Count();
		}

		[MorestachioFormatter("distinct", "Gets a new list that contains not duplicates")]
		public static IEnumerable<T> Distinct<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.Distinct();
		}

		[MorestachioFormatter("group by", "Groups the list be the argument.",
			ReturnHint = "List with Key. Can be listed with #each")]
		[MorestachioFormatterInput("Must be Expression to property")]
		public static IEnumerable GroupBy<T>(IEnumerable<T> sourceCollection, 
			string expression = "it", 
			params object[] arguments)
		{
			return sourceCollection.AsQueryable()
				.GroupBy(expression, arguments);
		}

		[MorestachioFormatter("flat group", "Flattens the Group returned by group by",
			ReturnHint = "Can be listed with #each")]
		[MorestachioFormatterInput("Must be Expression to property")]
		public static IEnumerable<T> GroupByList<TKey, T>(IGrouping<TKey, T> sourceCollection)
		{
			return sourceCollection.ToList();
		}

	
		[MorestachioFormatter("any", "Returns ether true or false if the expression in the argument is fulfilled by any item")]
		[MorestachioFormatterInput("Must be Expression to property")]
		public static bool Any(IEnumerable sourceCollection)
		{
			return sourceCollection.AsQueryable().Any();
		}

		[MorestachioFormatter("take", "Takes the ammount of items in argument")]
		[MorestachioFormatterInput("number")]
		public static object Take(IEnumerable sourceCollection, int arguments)
		{
			return sourceCollection.AsQueryable().Take(arguments);
		}

		[MorestachioFormatter("Select", "Takes the ammount of items in argument")]
		public static IEnumerable Select<T>(IEnumerable<T> sourceCollection, string query, [RestParameter]object[] arguments)
		{
			return sourceCollection.AsQueryable().Select(query, arguments);
		}

		[MorestachioFormatter("sum", "Aggreates the property in the argument and returns it")]
		public static T Sum<T>(IEnumerable<T> sourceCollection)
		{
			return (T)sourceCollection.AsQueryable().Sum();
		}

		[MorestachioFormatter("all", "returns if all elements in the collection matches the condition")]
		public static bool All<T>(IEnumerable<T> sourceCollection, string expression, params object[] arguments)
		{
			return sourceCollection.AsQueryable().All(expression, arguments);
		}

		[MorestachioFormatter("Any", "returns if there are any elements in the collection")]
		public static bool Any<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.AsQueryable().Any();
		}

		[MorestachioFormatter("Any", "returns if Any elements in the collection matches the condition")]
		public static bool Any<T>(IEnumerable<T> sourceCollection, string expression, params object[] arguments)
		{
			return sourceCollection.AsQueryable().Any(expression, arguments);
		}

		[MorestachioFormatter("Average", "returns the Average of all items in the collection")]
		public static double Average<T>(IEnumerable<T> sourceCollection, string expression, params object[] arguments)
		{
			return sourceCollection.AsQueryable().Average(expression, arguments);
		}

		[MorestachioFormatter("Cast", "casts all elements in the collection into another type")]
		public static IQueryable Cast<T>(IEnumerable<T> sourceCollection, string type)
		{
			return sourceCollection.AsQueryable().Cast(type);
		}

		[MorestachioFormatter("Contains", "returns if the collection contains the given item")]
		public static bool Contains<T>(IEnumerable<T> sourceCollection, T item)
		{
			return sourceCollection.AsQueryable().Contains(item);
		}


	}
}
