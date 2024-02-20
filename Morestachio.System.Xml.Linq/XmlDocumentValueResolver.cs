using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;
using Morestachio.Document;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Formatter.Framework.Converter;
using Morestachio.Framework.Context;
using Morestachio.Framework.Context.Resolver;
using Morestachio.Helper;

namespace Morestachio.System.Xml.Linq
{
	/// <summary>
	///		Extension method for adding the <see cref="XDocument"/> Resolver
	/// </summary>
	public static class XmlDocumentValueResolverExtensions
	{
		/// <summary>
		///		Sets or Adds the <see cref="XDocument"/> Value resolver
		/// </summary>
		/// <param name="optionsBuilder"></param>
		/// <returns></returns>
		public static IParserOptionsBuilder WithXmlDocumentValueResolver(this IParserOptionsBuilder optionsBuilder)
		{
			return optionsBuilder.WithValueResolver(new XmlDocumentValueResolver())
				.WithValueConverter(new XmlDocumentFassadeValueConverter())
				.WithFormatters(typeof(XPathFormatter))
				.WithFormatters(typeof(XContainerSelectorFormatter));
		}
	}

	/// <summary>
	///		Contains methods for accessing an <see cref="XElement"/>.
	/// </summary>
	[MorestachioExtensionSetup(
		"Must be added via Nuget package 'Morestachio.System.Xml.Linq' and added via 'ParserOptionsBuilder.WithXmlDocumentValueResolver()'")]
	public static class XContainerSelectorFormatter
	{
		[MorestachioFormatter("[MethodName]", "Gets an attribute value or null")]
		public static string GetAttribute(XElement element, string name)
		{
			return element.Attribute(name)?.Value;
		}

		[MorestachioFormatter("[MethodName]", "Return true if there is an attribute on that element, otherwise false.")]
		public static bool HasAttribute(XElement element, string name)
		{
			return element.Attribute(name) is not null;
		}

		[MorestachioFormatter("[MethodName]",
			"Returns the child element with this name or null if there is no child element with a matching name")]
		public static XContainer GetElement(XContainer element, string name)
		{
			return element.Element(name);
		}

		[MorestachioFormatter("[MethodName]", "Returns the child elements of this container.")]
		public static IEnumerable<XContainer> GetElements(XContainer element)
		{
			return element.Elements();
		}

		[MorestachioFormatter("[MethodName]",
			"Returns the child elements of this container that match the name passed in.")]
		public static IEnumerable<XContainer> GetElements(XContainer element, string name)
		{
			return element.Elements(name);
		}
	}

	/// <summary>
	///		Allows access to an <see cref="XContainer"/> via XPath syntax.
	/// </summary>
	[MorestachioExtensionSetup(
		"Must be added via Nuget package 'Morestachio.System.Xml.Linq' and added via 'ParserOptionsBuilder.WithXmlDocumentValueResolver()'")]
	public static class XPathFormatter
	{
		[MorestachioFormatter("[MethodName]", "Evaluates an XPath expression")]
		public static object Evaluate(XContainer container, string expression)
		{
			return container.XPathEvaluate(expression);
		}

		[MorestachioFormatter("[MethodName]", "Select an XElement using a XPath expression")]
		public static XElement SelectElement(XContainer container, string expression)
		{
			return container.XPathSelectElement(expression);
		}

		[MorestachioFormatter("[MethodName]", "Select a set of XElement using a XPath expression")]
		public static IEnumerable<XElement> SelectElements(XContainer container, string expression)
		{
			return container.XPathSelectElements(expression);
		}
	}

	/// <summary>
	///		Allows the conversion of an <see cref="XmlElementFassade"/>s content to a string
	/// </summary>
	public class XmlDocumentFassadeValueConverter : IFormatterValueConverter
	{
		/// <inheritdoc />
		public bool CanConvert(Type sourceType, Type requestedType)
		{
			return sourceType == typeof(XmlElementFassade) && requestedType == typeof(string);
		}

		/// <inheritdoc />
		public object Convert(object value, Type requestedType)
		{
			var fassade = value as XmlElementFassade;
			return (fassade.XContainer as XElement)?.Value;
		}
	}

	/// <summary>
	///		Defines a Value resolver that can resolve values from an <see cref="XDocument"/> or one of its children.
	/// </summary>
	public class XmlDocumentValueResolver : IValueResolver
	{
		/// <inheritdoc />
		public bool IsSealed { get; private set; }

		/// <inheritdoc />
		public void Seal()
		{
			IsSealed = true;
		}

		/// <inheritdoc />
		public object Resolve(
			Type type,
			object value,
			string path,
			ContextObject context,
			ScopeData scopeData
		)
		{
			if (value is not XContainer container)
			{
				return value;
			}

			var hasChild = container.Elements().Where(e => e.Name.LocalName == path).ToArray();

			if (hasChild.Length > 1)
			{
				return new XmlElementListFassade(container, hasChild);
			}

			return hasChild.Length == 1 ? new XmlElementFassade(hasChild[0]) : value;
		}

		/// <inheritdoc />
		public bool CanResolve(
			Type type,
			object value,
			string path,
			ContextObject context,
			ScopeData scopeData
		)
		{
			return typeof(XContainer).IsAssignableFrom(type);
		}
	}

	/// <summary>
	///		Allows the lazy access to a <see cref="XContainer"/>
	/// </summary>
	public class XmlElementListFassade : XmlElementFassade, IEnumerable<XmlElementFassade>
	{
		public XContainer[] Children { get; }

		public XmlElementListFassade(XContainer parent, XContainer[] children) : base(parent)
		{
			Children = children;
		}

		/// <inheritdoc />
		IEnumerator<XmlElementFassade> IEnumerable<XmlElementFassade>.GetEnumerator()
		{
			return Children.Select(e => new XmlElementFassade(e)).GetEnumerator();
		}

		/// <inheritdoc />
		public IEnumerator GetEnumerator()
		{
			return (this as IEnumerable<XmlElementFassade>).GetEnumerator();
		}
	}

	/// <summary>
	///		Allows the lazy access to a <see cref="XContainer"/>
	/// </summary>
	public class XmlElementFassade : IMorestachioPropertyResolver
	{
		public XContainer XContainer { get; }

		public XmlElementFassade(XContainer xContainer)
		{
			XContainer = xContainer;
		}

		/// <inheritdoc />
		public override string ToString()
		{
			return (XContainer as XElement)?.Value ?? XContainer.ToString();
		}

		/// <inheritdoc />
		public bool TryGetValue(string name, out object found)
		{
			found = TryBuildFassade(name, out var foundValue);
			return foundValue;
		}

		private object TryBuildFassade(string name, out bool found)
		{
			var hasElement = XContainer.Elements().Where(e => e.Name.LocalName == name).ToArray();

			if (hasElement.Length > 0)
			{
				found = true;

				if (hasElement.Length > 1)
				{
					return new XmlElementListFassade(XContainer, hasElement);
				}

				return new XmlElementFassade(hasElement[0]);
			}

			if (XContainer is XElement element)
			{
				var attribute = element.Attributes().FirstOrDefault(e => e.Name.LocalName == name);

				if (attribute != null)
				{
					found = true;
					return attribute.Value;
				}

				found = false;
				return null;
			}

			found = false;
			return null;
		}
	}
}