using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using JetBrains.Annotations;
using Morestachio.Formatter.Framework.Attributes;

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

		//[MorestachioFormatter("Zip", "Zips two lists together")]
		//public static IEnumerable Zip<T>(IEnumerable<T> sourceCollection,
		//	IEnumerable<T> targetCollection,
		//	string predicate,
		//	[RestParameter]params object[] arguments)
		//{
		//	return sourceCollection.AsQueryable().Zip(targetCollection, predicate);
		//}
		
		[MorestachioFormatter("First", "Gets the first item in the list that matches the predicate")]
		public static T First<T>(IEnumerable<T> sourceCollection,
			string predicate,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection.AsQueryable().Where(predicate, arguments).First();
		}

		[MorestachioFormatter("FirstOrDefault", "Gets the first item in the list that matches the predicate")]
		public static T FirstOrDefault<T>(IEnumerable<T> sourceCollection,
			string predicate,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection.AsQueryable().Where(predicate, arguments).FirstOrDefault();
		}

		[MorestachioFormatter("Last", "Gets the Last item in the list that matches the predicate")]
		public static T Last<T>(IEnumerable<T> sourceCollection,
			string predicate,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection.AsQueryable().Where(predicate, arguments).Last();
		}


		[MorestachioFormatter("LastOrDefault", "Gets the Last item in the list that matches the predicate")]
		public static T LastOrDefault<T>(IEnumerable<T> sourceCollection,
			string predicate,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection.AsQueryable().Where(predicate, arguments).LastOrDefault();
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

		[MorestachioFormatter("Count", "Counts all items that matches the predicate")]
		public static decimal Count<T>(IEnumerable<T> sourceCollection,
			string predicate,
			[RestParameter] params object[] arguments)
		{
			return sourceCollection.AsQueryable().Count(predicate, arguments);
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