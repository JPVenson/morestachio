using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Framework.Expression;
using Morestachio.Helper;

namespace Morestachio.Formatter.Predefined;
#pragma warning disable CS1591
public static class LinqFormatter
{
	[MorestachioFormatter("Concat", "Concats two lists together")]
	public static IEnumerable Concat<T>(IEnumerable<T> sourceCollection,
										IEnumerable<T> targetCollection)
	{
		return sourceCollection.Concat(targetCollection);
	}

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

	[MorestachioFormatter("First", "Selects the First item in the list")]
	public static T First<T>(IEnumerable<T> sourceCollection, MorestachioTemplateExpression expression)
	{
		return sourceCollection.First(expression.AsFunc<T, bool>());
	}

	[MorestachioFormatter("FirstOrDefault", "Gets the first item in the list that matches the predicate")]
	public static T FirstOrDefault<T>(IEnumerable<T> sourceCollection)
	{
		return sourceCollection.FirstOrDefault();
	}

	[MorestachioFormatter("FirstOrDefault", "Gets the first item in the list that matches the predicate")]
	public static T FirstOrDefault<T>(IEnumerable<T> sourceCollection, MorestachioTemplateExpression expression)
	{
		return sourceCollection.FirstOrDefault(expression.AsFunc<T, bool>());
	}

	[MorestachioFormatter("Last", "Selects the Last item in the list")]
	public static T Last<T>(IEnumerable<T> sourceCollection)
	{
		return sourceCollection.Last();
	}

	[MorestachioFormatter("Last", "Selects the Last item in the list")]
	public static T Last<T>(IEnumerable<T> sourceCollection, MorestachioTemplateExpression expression)
	{
		return sourceCollection.Last(expression.AsFunc<T, bool>());
	}

	[MorestachioFormatter("LastOrDefault", "Gets the Last item in the list that matches the predicate")]
	public static T LastOrDefault<T>(IEnumerable<T> sourceCollection)
	{
		return sourceCollection.LastOrDefault();
	}

	[MorestachioFormatter("LastOrDefault", "Gets the Last item in the list that matches the predicate")]
	public static T LastOrDefault<T>(IEnumerable<T> sourceCollection, MorestachioTemplateExpression expression)
	{
		return sourceCollection.LastOrDefault(expression.AsFunc<T, bool>());
	}

	[MorestachioFormatter("Single", "Selects the only item in the list")]
	public static T Single<T>(IEnumerable<T> sourceCollection)
	{
		return sourceCollection.Single();
	}

	[MorestachioFormatter("Single", "Selects the only item in the list")]
	public static T Single<T>(IEnumerable<T> sourceCollection, MorestachioTemplateExpression expression)
	{
		return sourceCollection.Single(expression.AsFunc<T, bool>());
	}

	[MorestachioFormatter("ElementAt", "Gets the item in the list on the position")]
	public static T ElementAt<T>(IEnumerable<T> sourceCollection, Number index)
	{
		return sourceCollection.ElementAt(index);
	}

	[MorestachioFormatter("ElementAtOrDefault", "Gets the item in the list on the position")]
	public static T ElementAtOrDefault<T>(IEnumerable<T> sourceCollection, Number index)
	{
		return sourceCollection.ElementAtOrDefault(index);
	}

	[MorestachioFormatter("Range", "Generates a list of numbers")]
	public static IEnumerable<int> Range(Number start, Number count)
	{
		return Enumerable.Range(start, count);
	}

	[MorestachioFormatter("Repeat", "Creates a list of the given item")]
	public static IEnumerable<T> Repeat<T>(T element, Number count)
	{
		return Enumerable.Repeat(element, count);
	}

	[MorestachioFormatter("Any", "returns if there are any elements in the collection")]
	public static bool Any<T>(IEnumerable<T> sourceCollection)
	{
		return sourceCollection.AsQueryable().Any();
	}

	[MorestachioFormatter("Count", "Gets the count of the list")]
	public static int Count<T>(IEnumerable<T> sourceCollection)
	{
		return sourceCollection.Count();
	}

	[MorestachioFormatter("Contains", "Searches in the list for that the argument")]
	public static bool Contains<T>(IEnumerable<T> sourceCollection, T arguments)
	{
		return sourceCollection.Contains(arguments);
	}

	[MorestachioFormatter("Min", "Called on a list of numbers it returns the smallest")]
	public static T Min<T>(IEnumerable<T> sourceCollection)
	{
		return sourceCollection.Min();
	}

	[MorestachioFormatter("Max", "Called on a list of numbers it returns the biggest")]
	public static T Max<T>(IEnumerable<T> sourceCollection)
	{
		return sourceCollection.Max();
	}

	[MorestachioFormatter("Aggregate",
		"Joins the element of a list separated by a delimiter string and return the concatenated string.")]
	public static string Aggregate<T>(IEnumerable<T> sourceCollection, string delimiter)
	{
		return string.Join(delimiter, sourceCollection.Select(f => f.ToString()));
	}

	[MorestachioFormatter("FlatGroup", "Flattens the Group returned by group by")]
	[MorestachioFormatterInput("Must be Expression to property")]
	public static IEnumerable<T> GroupByList<TKey, T>(IGrouping<TKey, T> sourceCollection)
	{
		return sourceCollection.ToList();
	}

	[MorestachioFormatter("Partition", "Splits the source into a list of lists equals the size of size")]
	public static IEnumerable<List<T>> Partition<T>(IEnumerable<T> source, Number size)
	{
		IList<T> target;

		if (source is IList<T>)
		{
			target = source as IList<T>;
		}
		else
		{
			target = source.ToArray();
		}

		var sizeInt = size.ToInt32(null);

		for (var i = 0; i < Math.Ceiling(target.Count / (double)sizeInt); i++)
		{
			yield return new List<T>(target.Skip((int)(sizeInt * i)).Take((int)sizeInt));
		}
	}

	[MorestachioFormatter("Compact", "Removes any non-null values from the input list.")]
	public static IEnumerable<T> Compact<T>(IEnumerable<T> sourceCollection)
	{
		return sourceCollection.Where(e => e != null);
	}

	[MorestachioFormatter("Take", "Takes the amount of items in argument")]
	public static IEnumerable<T> Take<T>(IEnumerable<T> sourceCollection, Number arguments)
	{
		return sourceCollection.Take(arguments.ToInt32(null));
	}

	[MorestachioFormatter("TakeWhile", "Takes items as long as the condition is true")]
	public static IEnumerable<T> TakeWhile<T>(IEnumerable<T> sourceCollection, MorestachioTemplateExpression expression)
	{
		return sourceCollection.TakeWhile(expression.AsFunc<T, bool>());
	}

	[MorestachioFormatter("SkipWhile", "Skips the amount of items in argument")]
	public static IEnumerable<T> SkipWhile<T>(IEnumerable<T> sourceCollection, MorestachioTemplateExpression expression)
	{
		return sourceCollection.SkipWhile(expression.AsFunc<T, bool>());
	}

	[MorestachioFormatter("Skip", "Skips the amount of items in argument")]
	public static IEnumerable<T> Skip<T>(IEnumerable<T> sourceCollection, Number arguments)
	{
		return sourceCollection.Skip(arguments.ToInt32(null));
	}

	[MorestachioFormatter("Where", "Filteres the collection based on the predicate")]
	public static IEnumerable<T> Where<T>(IEnumerable<T> items, MorestachioTemplateExpression expression)
	{
		return items.Where(expression.AsFunc<T, bool>()).ToArray();
	}

	[MorestachioFormatter("Select", "Selects items from the collection based on the predicate")]
	public static IEnumerable<TE> Select<T, TE>(IEnumerable<T> items, MorestachioTemplateExpression expression)
	{
		return items.Select(expression.AsFunc<T, TE>()).ToArray();
	}

	[MorestachioFormatter("SelectMany",
		"Selects a list of items from the collection based on the predicate and flattens them")]
	public static IEnumerable<TE> SelectMany<T, TE>(IEnumerable<T> items, MorestachioTemplateExpression expression)
	{
		return items.SelectMany(expression.AsFunc<T, IEnumerable<TE>>()).ToArray();
	}

	[MorestachioFormatter("OrderBy", "Orders the list descending")]
	public static IEnumerable<T> OrderBy<T>(IEnumerable<T> items, MorestachioTemplateExpression expression)
	{
		return items.OrderBy(expression.AsFunc<T, object>()).ToArray();
	}

	[MorestachioFormatter("OrderBy", "Orders the list descending")]
	public static IEnumerable<T> OrderBy<T>(IEnumerable<T> items)
	{
		return items.OrderBy(e => e).ToArray();
	}

	[MorestachioFormatter("ThenBy", "Orders the list descending")]
	public static IEnumerable<T> ThenBy<T>(IOrderedEnumerable<T> items)
	{
		return items.ThenBy(e => e).ToArray();
	}

	[MorestachioFormatter("ThenBy", "Orders the list descending")]
	public static IEnumerable<T> ThenBy<T>(IOrderedEnumerable<T> items, MorestachioTemplateExpression expression)
	{
		return items.ThenBy(expression.AsFunc<T, bool>());
	}

	[MorestachioFormatter("GroupBy", "Groups a list")]
	public static IEnumerable<IGrouping<TE, T>> GroupBy<T, TE>(IEnumerable<T> items,
																MorestachioTemplateExpression expression)
	{
		return items.GroupBy(expression.AsFunc<T, TE>()).ToArray();
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