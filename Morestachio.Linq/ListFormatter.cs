using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using JetBrains.Annotations;
using Morestachio.Attributes;
using Morestachio.Formatter.Framework;

namespace Morestachio.Linq
{
	/// <summary>
	///     This class wraps the System.Linq.Dynamic.Core package for Morstachio
	/// </summary>
	[PublicAPI]
	public static class DynamicLinq
	{
		[MorestachioFormatter("Where",
			"Filteres the collection based on the predicate. Accepts any number of additonal arguments")]
		public static IEnumerable<T> Where<T>(IEnumerable<T> sourceCollection,
			string predicate,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection.AsQueryable().Where(predicate, arguments);
		}

		[MorestachioFormatter("Select", "Selects items from the collection based on the predicate")]
		public static IEnumerable Select<T>(IEnumerable<T> sourceCollection,
			string predicate,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection.AsQueryable().Select(predicate, arguments);
		}

		[MorestachioFormatter("Select",
			"Selects a list of items from the collection based on the predicate and flattens them")]
		public static IEnumerable SelectMany<T>(IEnumerable<T> sourceCollection,
			string predicate,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection.AsQueryable().SelectMany(predicate, arguments);
		}

		[MorestachioFormatter("Take", "Takes the amount of items in argument")]
		public static IEnumerable<T> Take<T>(IEnumerable<T> sourceCollection, int arguments)
		{
			return sourceCollection.AsQueryable().Take(arguments);
		}

		[MorestachioFormatter("TakeWhile", "Takes items from the collection as long as the predicate is true")]
		public static IEnumerable<T> TakeWhile<T>(IEnumerable<T> sourceCollection,
			string predicate,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection.AsQueryable().TakeWhile(predicate, arguments).OfType<T>();
		}

		[MorestachioFormatter("Skip", "Skips the amount of items in argument")]
		public static IEnumerable<T> Skip<T>(IEnumerable<T> sourceCollection, int arguments)
		{
			return sourceCollection.AsQueryable().Skip(arguments);
		}

		[MorestachioFormatter("SkipWhile", "Skips items from the collection as long as the predicate is true")]
		public static IEnumerable<T> SkipWhile<T>(IEnumerable<T> sourceCollection,
			string predicate,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection.AsQueryable().SkipWhile(predicate, arguments).OfType<T>();
		}

		[MorestachioFormatter("Join", "Joins two collections together")]
		public static IEnumerable Join(IEnumerable sourceCollection,
			IEnumerable targetCollection,
			string outerKeySelector,
			string innerKeySelector,
			string resultSelector,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection
				.AsQueryable().Join(targetCollection, outerKeySelector, innerKeySelector, resultSelector, arguments);
		}

		[MorestachioFormatter("GroupJoin", "Joins two collections together")]
		public static IEnumerable GroupJoin(IEnumerable sourceCollection,
			IEnumerable targetCollection,
			string outerKeySelector,
			string innerKeySelector,
			string resultSelector,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection
				.AsQueryable().GroupJoin(targetCollection, outerKeySelector, innerKeySelector, resultSelector,
					arguments);
		}

		[MorestachioFormatter("OrderBy", "Orders the list descending")]
		public static IEnumerable<T> OrderBy<T>(IEnumerable<T> sourceCollection,
			string predicate,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection.AsQueryable().OrderBy(predicate, arguments);
		}

		[MorestachioFormatter("OrderBy", "Orders the list descending")]
		public static IEnumerable<T> OrderBy<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.OrderBy(f => f);
		}

		[MorestachioFormatter("ThenBy", "Orders an list previusly ordered with 'OrderBy'")]
		public static IEnumerable<T> ThenBy<T>(IOrderedQueryable<T> sourceCollection,
			string predicate,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection.ThenBy(predicate, arguments);
		}

		[MorestachioFormatter("GroupBy", "Groups a list")]
		public static IEnumerable GroupBy<T>(IEnumerable<T> sourceCollection,
			string predicate,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection.AsQueryable().GroupBy(predicate, arguments);
		}

		[MorestachioFormatter("Concat", "Concats two lists together")]
		public static IEnumerable Concat<T>(IEnumerable<T> sourceCollection,
			IEnumerable<T> targetCollection)
		{
			return sourceCollection.Concat(targetCollection);
		}

		//[MorestachioFormatter("Zip", "Zips two lists together")]
		//public static IEnumerable Zip<T>(IEnumerable<T> sourceCollection,
		//	IEnumerable<T> targetCollection,
		//	string predicate,
		//	[RestParameter]params object[] arguments)
		//{
		//	return sourceCollection.AsQueryable().Zip(targetCollection, predicate);
		//}

		[MorestachioFormatter("Distinct", "Filters duplicates from the list")]
		public static IEnumerable Distinct<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.Distinct();
		}

		[MorestachioFormatter("Union", "Creates a new list that contains all unqiue items from both lists")]
		public static IEnumerable Union<T>(IEnumerable<T> sourceCollection,
			IEnumerable<T> targetCollection)
		{
			return sourceCollection.Union(targetCollection);
		}

		[MorestachioFormatter("Intersect", "Gets the list of all duplicates from both lists")]
		public static IEnumerable Intersect<T>(IEnumerable<T> sourceCollection,
			IEnumerable<T> targetCollection)
		{
			return sourceCollection.Intersect(targetCollection);
		}

		[MorestachioFormatter("Except", "Gets all items from the source list except for all items in the target list")]
		public static IEnumerable Except<T>(IEnumerable<T> sourceCollection,
			IEnumerable<T> targetCollection)
		{
			return sourceCollection.Intersect(targetCollection);
		}

		[MorestachioFormatter("Reverse", "Reverses the order of all items in the list")]
		public static IEnumerable Reverse<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.Reverse();
		}

		[MorestachioFormatter("SequenceEqual", "Checks for all items of both lists for equality")]
		public static bool SequenceEqual<T>(IEnumerable<T> sourceCollection, IEnumerable<T> targetCollection)
		{
			return sourceCollection.SequenceEqual(targetCollection);
		}

		[MorestachioFormatter("DefaultIfEmpty", "If the source list is empty, the parameter will be returned instead")]
		public static IEnumerable<T> DefaultIfEmpty<T>(IEnumerable<T> sourceCollection, T defaultValue)
		{
			return sourceCollection.DefaultIfEmpty(defaultValue);
		}

		[MorestachioFormatter("First", "Selects the First item in the list")]
		public static T First<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.First();
		}

		[MorestachioFormatter("First", "Gets the first item in the list that matches the predicate")]
		public static T First<T>(IEnumerable<T> sourceCollection,
			string predicate,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection.AsQueryable().Where(predicate, arguments).First();
		}


		[MorestachioFormatter("FirstOrDefault", "Gets the first item in the list that matches the predicate")]
		public static T FirstOrDefault<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.FirstOrDefault();
		}

		[MorestachioFormatter("FirstOrDefault", "Gets the first item in the list that matches the predicate")]
		public static T FirstOrDefault<T>(IEnumerable<T> sourceCollection,
			string predicate,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection.AsQueryable().Where(predicate, arguments).FirstOrDefault();
		}

		[MorestachioFormatter("Last", "Selects the Last item in the list")]
		public static T Last<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.Last();
		}

		[MorestachioFormatter("Last", "Gets the Last item in the list that matches the predicate")]
		public static T Last<T>(IEnumerable<T> sourceCollection,
			string predicate,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection.AsQueryable().Where(predicate, arguments).Last();
		}


		[MorestachioFormatter("LastOrDefault", "Gets the Last item in the list that matches the predicate")]
		public static T LastOrDefault<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.LastOrDefault();
		}

		[MorestachioFormatter("LastOrDefault", "Gets the Last item in the list that matches the predicate")]
		public static T LastOrDefault<T>(IEnumerable<T> sourceCollection,
			string predicate,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection.AsQueryable().Where(predicate, arguments).LastOrDefault();
		}

		[MorestachioFormatter("Single", "Selects the only item in the list")]
		public static T Single<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.Single();
		}

		[MorestachioFormatter("Single", "Gets the only item in the list that matches the predicate")]
		public static T Single<T>(IEnumerable<T> sourceCollection,
			string predicate,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection.AsQueryable().Where(predicate, arguments).Single();
		}

		[MorestachioFormatter("SingleOrDefault", "Gets the only item in the list that matches the predicate")]
		public static T SingleOrDefault<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.LastOrDefault();
		}

		[MorestachioFormatter("SingleOrDefault", "Gets the only item in the list that matches the predicate")]
		public static T SingleOrDefault<T>(IEnumerable<T> sourceCollection,
			string predicate,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection.AsQueryable().Where(predicate, arguments).SingleOrDefault();
		}

		[MorestachioFormatter("ElementAt", "Gets the item in the list on the position")]
		public static T ElementAt<T>(IEnumerable<T> sourceCollection, int index)
		{
			return sourceCollection.ElementAt(index);
		}

		[MorestachioFormatter("ElementAtOrDefault", "Gets the item in the list on the position")]
		public static T ElementAtOrDefault<T>(IEnumerable<T> sourceCollection, int index)
		{
			return sourceCollection.ElementAtOrDefault(index);
		}

		[MorestachioFormatter("Range", "Generates a list of numbers")]
		public static IEnumerable<int> Range(int start, int count)
		{
			return Enumerable.Range(start, count);
		}

		[MorestachioFormatter("Repeat", "Creates a list of the given item")]
		public static IEnumerable<T> Repeat<T>(T element, int count)
		{
			return Enumerable.Repeat(element, count);
		}

		//[MorestachioFormatter("Empty", "Creates a list of the given item")]
		//public static IEnumerable<T> Empty<T>()
		//{
		//	return Enumerable.Repeat(element, count);
		//}

		[MorestachioFormatter("Any", "returns if there are any elements in the collection")]
		public static bool Any<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.AsQueryable().Any();
		}

		[MorestachioFormatter("Any", "returns if Any elements in the collection matches the condition")]
		public static bool Any<T>(IEnumerable<T> sourceCollection,
			string predicate,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection.AsQueryable().Any(predicate, arguments);
		}

		[MorestachioFormatter("All", "returns if All elements in the collection matches the condition")]
		public static bool All<T>(IEnumerable<T> sourceCollection,
			string predicate,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection.AsQueryable().All(predicate, arguments);
		}

		[MorestachioFormatter("Count", "Gets the count of the list")]
		public static decimal Count<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.Count();
		}

		[MorestachioFormatter("Count", "Counts all items that matches the predicate")]
		public static decimal Count<T>(IEnumerable<T> sourceCollection,
			string predicate,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection.AsQueryable().Count(predicate, arguments);
		}

		[MorestachioFormatter("Contains", "Searches in the list for that the argument")]
		public static bool Contains<T>(IEnumerable<T> sourceCollection, T arguments)
		{
			return sourceCollection.Contains(arguments);
		}

		[MorestachioFormatter("Aggregate", "Aggregate all items in the list")]
		public static object Aggregate<T>(IEnumerable<T> sourceCollection, string function, string member)
		{
			return sourceCollection.AsQueryable().Aggregate(function, member);
		}

		[MorestachioFormatter("sum", "Aggreates the property in the argument and returns it")]
		public static T Sum<T>(IEnumerable<T> sourceCollection)
		{
			return (T)sourceCollection.AsQueryable().Sum();
		}

		[MorestachioFormatter("min", "Called on a list of numbers it returns the smallest")]
		public static T Min<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.Min();
		}

		[MorestachioFormatter("max", "Called on a list of numbers it returns the biggest")]
		public static T Max<T>(IEnumerable<T> sourceCollection)
		{
			return sourceCollection.Max();
		}

		[MorestachioFormatter("flat group", "Flattens the Group returned by group by",
			ReturnHint = "Can be listed with #each")]
		[MorestachioFormatterInput("Must be Expression to property")]
		public static IEnumerable<T> GroupByList<TKey, T>(IGrouping<TKey, T> sourceCollection)
		{
			return sourceCollection.ToList();
		}

		[MorestachioFormatter("Average", "returns the Average of all items in the collection")]
		public static double Average<T>(IEnumerable<T> sourceCollection, 
			string predicate, 
			params object[] arguments)
		{
			return sourceCollection.AsQueryable().Average(predicate, arguments);
		}

		[MorestachioFormatter("any",
			"Returns ether true or false if the expression in the argument is fulfilled by any item")]
		[MorestachioFormatterInput("Must be Expression to property")]
		public static bool Any(IEnumerable sourceCollection)
		{
			return sourceCollection.AsQueryable().Any();
		}

		[MorestachioFormatter("Cast", "casts all elements in the collection into another type")]
		public static IQueryable Cast<T>(IEnumerable<T> sourceCollection, string type)
		{
			return sourceCollection.AsQueryable().Cast(type);
		}
	}
}