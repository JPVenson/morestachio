using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Morestachio.Tests
{
	/// <summary>
	///     Used to create an Example for what the Formatter can be used for
	/// </summary>
	public class EnumerableFormatter
	{
		static EnumerableFormatter()
		{
			Formatter = new Dictionary<string, Func<IEnumerable<object>, string, object>>();

			Formatter.Add("order by desc ", (collection, arg) => collection.OrderByDescending(PropExpression(arg)));
			Formatter.Add("order by ", (collection, arg) => collection.OrderBy(PropExpression(arg)));

			Formatter.Add("order desc", (collection, arg) => collection.OrderByDescending(e => e));
			Formatter.Add("order", (collection, arg) => collection.OrderBy(e => e));

			Formatter.Add("contains ", (collection, arg) => collection.Any(e => e.Equals(arg)));
			Formatter.Add("count", (collection, arg) => collection.Count());
			Formatter.Add("element at ", (collection, arg) => collection.ElementAt(int.Parse(arg)));
			Formatter.Add("distinct", (collection, arg) => collection.Distinct());
			Formatter.Add("first or default", (collection, arg) => collection.FirstOrDefault());
			Formatter.Add("group by ", (collection, arg) => collection.GroupBy(PropExpression(arg)));
			Formatter.Add("max ", (collection, arg) => collection.Max(PropExpression(arg)));
			Formatter.Add("max", (collection, arg) => collection.Max());
			Formatter.Add("min ", (collection, arg) => collection.Min(PropExpression(arg)));
			Formatter.Add("min", (collection, arg) => collection.Min());

			Formatter.Add("reverse", (collection, arg) => collection.Reverse());
			Formatter.Add("select ", (collection, arg) => collection.Select(PropExpression(arg)));
			Formatter.Add("take ", (collection, arg) => collection.Take(int.Parse(arg)));
		}

		public static IDictionary<string, Func<IEnumerable<object>, string, object>> Formatter { get; set; }

		public static Func<object, object> PropExpression(string propName)
		{
			var parameterExpression = Expression.Parameter(typeof(object));
			var propCall = Expression.Property(parameterExpression, propName);
			return Expression.Lambda<Func<object, object>>(propCall, parameterExpression).Compile();
		}

		public object FormatArgument(IEnumerable sourceCollection, string arguments)
		{
			var formatter = Formatter.FirstOrDefault(e => arguments.StartsWith(e.Key));

			if (formatter.Value != null)
			{
				return formatter.Value(sourceCollection.Cast<object>(), arguments.Remove(0, formatter.Key.Length));
			}

			return sourceCollection;
		}
	}
}