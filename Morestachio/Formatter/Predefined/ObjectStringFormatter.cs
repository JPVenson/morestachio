using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Framework.Context.Resolver;
using Morestachio.Helper.Logging;

namespace Morestachio.Formatter.Predefined
{
#pragma warning disable CS1591
	/// <summary>
	///		Contains the basic Formatting operations
	/// </summary>
	public static class ObjectStringFormatter
	{
		[MorestachioFormatter("ToString", "Formats a value according to the structure set by the argument")]
		public static string Formattable(IFormattable source, string argument, [ExternalData]ParserOptions options)
		{
			return source.ToString(argument, options.CultureInfo);
		}

		[MorestachioFormatter(null, null)]
		public static string FormattableObsolete(IFormattable source, string argument, [ExternalData]ParserOptions options)
		{
			options.Logger.LogWarn(LoggingFormatter.FormatterObsoleteEventId, "The null ToString formatter is obsolete. Please use the named 'ToString()' formatter");
			return source.ToString(argument, options.CultureInfo);
		}

		[MorestachioFormatter("ToString", "Formats the value according to the build in rules for that object")]
		public static string Formattable(object source)
		{
			return source.ToString();
		}

		[MorestachioFormatter(null, null)]
		public static string FormattableObsolete(object source, [ExternalData]ILogger logger)
		{
			logger?.LogWarn(LoggingFormatter.FormatterObsoleteEventId, "The null ToString formatter is obsolete. Please use the named 'ToString()' formatter");
			return source.ToString();
		}
		
		[MorestachioFormatter("ToXml", null)]
		public static string ToXml(object source, [ExternalData]ParserOptions options)
		{
			var xmlSerializer = new XmlSerializer(source.GetType());
			using (var xmlStream = new MemoryStream())
			{
				xmlSerializer.Serialize(xmlStream, source);
				return options.Encoding.GetString(xmlStream.ToArray());
			}
		}
		
		[MorestachioFormatter("Get", "Gets a specific property from an object or IDictionary")]
		public static object Get(object source, string propertyName, [ExternalData]ParserOptions options)
		{
			if (options.ValueResolver?.CanResolve(source.GetType(), source, propertyName, null) == true)
			{
				return options.ValueResolver.Resolve(source.GetType(), source, propertyName, null);
			}

			if (!options.HandleDictionaryAsObject && source is IDictionary<string, object> dict)
			{
				if (dict.TryGetValue(propertyName, out var val))
				{
					return val;
				}

				return null;
			}

			if (source is IMorestachioPropertyResolver cResolver)
			{
				if (cResolver.TryGetValue(propertyName, out var value))
				{
					return value;
				}

				return null;
			}

			if (source is ICustomTypeDescriptor ctype)
			{
				return ctype.GetProperties().Find(propertyName, false)?.GetValue(source);
			}

			return source.GetType().GetProperty(propertyName)?.GetValue(source);
		}
	}
}