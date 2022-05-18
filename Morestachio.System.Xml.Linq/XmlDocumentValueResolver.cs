using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Morestachio.Document;
using Morestachio.Formatter.Framework.Converter;
using Morestachio.Framework.Context;
using Morestachio.Framework.Context.Resolver;

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
								.WithValueConverter(new XmlDocumentFassadeValueConverter());
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
			if (!(value is XContainer container))
			{
				return value;
			}

			var hasChild = container.Elements().Where(e => e.Name.LocalName == path).ToArray();

			if (hasChild.Length > 1)
			{
				return new XmlElementListFassade(container, hasChild);
			}

			if (hasChild.Any())
			{
				return new XmlElementFassade(hasChild[0]);
			}

			return value;
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