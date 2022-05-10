using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Framework.Converter;

namespace Morestachio;

/// <summary>
///		Contains methods for adding formatters in a structured way to an <see cref="IParserOptionsBuilder"/>
/// </summary>
public static class ParserOptionsBuilderFormatterExtensions
{
	/// <summary>
	///		Adds a new Named constant
	/// </summary>
	public static IParserOptionsBuilder WithConstant(this IParserOptionsBuilder builder, string name, object value)
	{
		return builder.WithConfig(c =>
		{
			c.Formatters.Constants[name] = value;
			return c;
		});
	}

	/// <summary>
	///		Adds a new Named constant
	/// </summary>
	public static IParserOptionsBuilder WithConstant(this IParserOptionsBuilder builder, Func<(string, object)> value)
	{
		return builder.WithConfig(c =>
		{
			var (key, val) = value();
			c.Formatters.Constants[key] = val;
			return c;
		});
	}

	/// <summary>
	///		Adds a new Service
	/// </summary>
	public static IParserOptionsBuilder WithService<TService>(this IParserOptionsBuilder builder, TService value)
	{
		return builder.WithConfig(c =>
		{
			c.Formatters.Services.AddService(value);
			return c;
		});
	}

	/// <summary>
	///		Adds a new Service
	/// </summary>
	public static IParserOptionsBuilder WithService<TService>(this IParserOptionsBuilder builder, Func<TService> value)
	{
		return builder.WithConfig(c =>
		{
			c.Formatters.Services.AddService(value);
			return c;
		});
	}

	/// <summary>
	///		Adds a new Service
	/// </summary>
	public static IParserOptionsBuilder WithService<TServiceType, TServiceInstance>(this IParserOptionsBuilder builder, 
																					TServiceInstance value)
		where TServiceInstance : class, TServiceType
	{
		return builder.WithConfig(c =>
		{
			c.Formatters.Services.AddService<TServiceType>(value);
			return c;
		});
	}

	/// <summary>
	///		Adds a new Service
	/// </summary>
	public static IParserOptionsBuilder WithService<TServiceType, TServiceInstance>(this IParserOptionsBuilder builder, 
																					Func<TServiceInstance> value)
		where TServiceInstance : class, TServiceType
	{
		return builder.WithConfig(c =>
		{
			c.Formatters.Services.AddService<TServiceType>(value);
			return c;
		});
	}

	/// <summary>
	///		Adds formatters from an type
	/// </summary>
	public static IParserOptionsBuilder WithAllParametersAllDefaultValue(this IParserOptionsBuilder builder, bool value)
	{
		return builder.WithConfig(c =>
		{
			c.Formatters.AllParametersAllDefaultValue = value;
			return c;
		});
	}

	/// <summary>
	///		Adds formatters from an type
	/// </summary>
	public static IParserOptionsBuilder WithValueConverter(this IParserOptionsBuilder builder, IFormatterValueConverter valueConverter)
	{
		return builder.WithConfig(c =>
		{
			c.Formatters.ValueConverter.Add(valueConverter);
			return c;
		});
	}

	/// <summary>
	///		Adds formatters from an type
	/// </summary>
	public static IParserOptionsBuilder WithFormatters<T>(this IParserOptionsBuilder builder)
	{
		return builder.WithConfig(c =>
		{
			c.Formatters.AddFromType<T>();
			return c;
		});
	}

	/// <summary>
	///		Adds formatters from an type
	/// </summary>
	public static IParserOptionsBuilder WithFormatters(this IParserOptionsBuilder builder, Type type)
	{
		return builder.WithConfig(c =>
		{
			c.Formatters.AddFromType(type);
			return c;
		});
	}

	/// <summary>
	///		Adds an formatter
	/// </summary>
	public static IParserOptionsBuilder WithFormatter(this IParserOptionsBuilder builder, Delegate @delegate, string name)
	{
		return builder.WithConfig(c =>
		{
			c.Formatters.AddSingle(@delegate, name);
			return c;
		});
	}

	/// <summary>
	///		Adds an formatter
	/// </summary>
	public static IParserOptionsBuilder WithGlobalFormatter(this IParserOptionsBuilder builder, Delegate @delegate, string name)
	{
		return builder.WithConfig(c =>
		{
			c.Formatters.AddSingleGlobal(@delegate, name);
			return c;
		});
	}

	#region Action Overloads

	/// <summary>
	///		Adds a new Function to the list of formatters
	/// </summary>
	public static IParserOptionsBuilder WithFormatter(this IParserOptionsBuilder builder, Action function, string name)
	{
		return builder.WithFormatter((Delegate)function, name);
	}

	/// <summary>
	///		Adds a new Function to the list of formatters
	/// </summary>
	public static IParserOptionsBuilder WithFormatter<T>(this IParserOptionsBuilder builder, Action<T> function, string name)
	{
		return builder.WithFormatter((Delegate)function, name);
	}

	/// <summary>
	///		Adds a new Function to the list of formatters
	/// </summary>
	public static IParserOptionsBuilder WithFormatter<T, T1>(this IParserOptionsBuilder builder, Action<T, T1> function, string name)
	{
		return builder.WithFormatter((Delegate)function, name);
	}

	/// <summary>
	///		Adds a new Function to the list of formatters
	/// </summary>
	public static IParserOptionsBuilder WithFormatter<T, T1, T2>(this IParserOptionsBuilder builder, Action<T, T1, T2> function, string name)
	{
		return builder.WithFormatter((Delegate)function, name);
	}

	/// <summary>
	///		Adds a new Function to the list of formatters
	/// </summary>
	public static IParserOptionsBuilder WithFormatter<T, T1, T2, T3>(this IParserOptionsBuilder builder, Action<T, T1, T2, T3> function, string name)
	{
		return builder.WithFormatter((Delegate)function, name);
	}

	/// <summary>
	///		Adds a new Function to the list of formatters
	/// </summary>
	public static IParserOptionsBuilder WithFormatter<T, T1, T2, T3, T4>(this IParserOptionsBuilder builder, Action<T, T1, T2, T3, T4> function, string name)
	{
		return builder.WithFormatter((Delegate)function, name);
	}

	#endregion

	#region Function Overloads

	/// <summary>
	///		Adds a new Function to the list of formatters
	/// </summary>
	public static IParserOptionsBuilder WithFormatter<T>(this IParserOptionsBuilder builder, Func<T> function, string name)
	{
		return builder.WithFormatter((Delegate)function, name);
	}

	/// <summary>
	///		Adds a new Function to the list of formatters
	/// </summary>
	public static IParserOptionsBuilder WithFormatter<T, T1>(this IParserOptionsBuilder builder, Func<T, T1> function, string name)
	{
		return builder.WithFormatter((Delegate)function, name);
	}

	/// <summary>
	///		Adds a new Function to the list of formatters
	/// </summary>
	public static IParserOptionsBuilder WithFormatter<T, T1, T2>(this IParserOptionsBuilder builder, Func<T, T1, T2> function, string name)
	{
		return builder.WithFormatter((Delegate)function, name);
	}

	/// <summary>
	///		Adds a new Function to the list of formatters
	/// </summary>
	public static IParserOptionsBuilder WithFormatter<T, T1, T2, T3>(this IParserOptionsBuilder builder, Func<T, T1, T2, T3> function, string name)
	{
		return builder.WithFormatter((Delegate)function, name);
	}

	/// <summary>
	///		Adds a new Function to the list of formatters
	/// </summary>
	public static IParserOptionsBuilder WithFormatter<T, T1, T2, T3, T4>(this IParserOptionsBuilder builder, Func<T, T1, T2, T3, T4> function, string name)
	{
		return builder.WithFormatter((Delegate)function, name);
	}

	#endregion

	#region Action Overloads

	/// <summary>
	///		Adds a new Function to the list of formatters
	/// </summary>
	public static IParserOptionsBuilder WithGlobalFormatter(this IParserOptionsBuilder builder, Action function, string name)
	{
		return builder.WithGlobalFormatter((Delegate)function, name);
	}

	/// <summary>
	///		Adds a new Function to the list of formatters
	/// </summary>
	public static IParserOptionsBuilder WithGlobalFormatter<T>(this IParserOptionsBuilder builder, Action<T> function, string name)
	{
		return builder.WithGlobalFormatter((Delegate)function, name);
	}

	/// <summary>
	///		Adds a new Function to the list of formatters
	/// </summary>
	public static IParserOptionsBuilder WithGlobalFormatter<T, T1>(this IParserOptionsBuilder builder, Action<T, T1> function, string name)
	{
		return builder.WithGlobalFormatter((Delegate)function, name);
	}

	/// <summary>
	///		Adds a new Function to the list of formatters
	/// </summary>
	public static IParserOptionsBuilder WithGlobalFormatter<T, T1, T2>(this IParserOptionsBuilder builder, Action<T, T1, T2> function, string name)
	{
		return builder.WithGlobalFormatter((Delegate)function, name);
	}

	/// <summary>
	///		Adds a new Function to the list of formatters
	/// </summary>
	public static IParserOptionsBuilder WithGlobalFormatter<T, T1, T2, T3>(this IParserOptionsBuilder builder, Action<T, T1, T2, T3> function, string name)
	{
		return builder.WithGlobalFormatter((Delegate)function, name);
	}

	/// <summary>
	///		Adds a new Function to the list of formatters
	/// </summary>
	public static IParserOptionsBuilder WithGlobalFormatter<T, T1, T2, T3, T4>(this IParserOptionsBuilder builder, Action<T, T1, T2, T3, T4> function, string name)
	{
		return builder.WithGlobalFormatter((Delegate)function, name);
	}

	#endregion

	#region Function Overloads

	/// <summary>
	///		Adds a new Function to the list of formatters
	/// </summary>
	public static IParserOptionsBuilder WithGlobalFormatter<T>(this IParserOptionsBuilder builder, Func<T> function, string name)
	{
		return builder.WithGlobalFormatter((Delegate)function, name);
	}

	/// <summary>
	///		Adds a new Function to the list of formatters
	/// </summary>
	public static IParserOptionsBuilder WithGlobalFormatter<T, T1>(this IParserOptionsBuilder builder, Func<T, T1> function, string name)
	{
		return builder.WithGlobalFormatter((Delegate)function, name);
	}

	/// <summary>
	///		Adds a new Function to the list of formatters
	/// </summary>
	public static IParserOptionsBuilder WithGlobalFormatter<T, T1, T2>(this IParserOptionsBuilder builder, Func<T, T1, T2> function, string name)
	{
		return builder.WithGlobalFormatter((Delegate)function, name);
	}

	/// <summary>
	///		Adds a new Function to the list of formatters
	/// </summary>
	public static IParserOptionsBuilder WithGlobalFormatter<T, T1, T2, T3>(this IParserOptionsBuilder builder, Func<T, T1, T2, T3> function, string name)
	{
		return builder.WithGlobalFormatter((Delegate)function, name);
	}

	/// <summary>
	///		Adds a new Function to the list of formatters
	/// </summary>
	public static IParserOptionsBuilder WithGlobalFormatter<T, T1, T2, T3, T4>(this IParserOptionsBuilder builder, Func<T, T1, T2, T3, T4> function, string name)
	{
		return builder.WithGlobalFormatter((Delegate)function, name);
	}

	#endregion
}
