using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace Morestachio.Example.Base
{
	public abstract partial class MorestachioExampleBase
	{
		public virtual ParserOptions Configurate(string templateText, Encoding encoding, bool shouldEscape)
		{
			var options = ParserOptionsBuilder.New()
											.WithTemplate(templateText)
											.WithEncoding(encoding)
											.WithDisableContentEscaping(shouldEscape)
											.WithTimeout(TimeSpan.FromSeconds(5));

			return options.Build();
		}

		public abstract object GetData();

		public virtual async Task<MorestachioDocumentInfo> Parse(ParserOptions parserOptions)
		{
			return await Parser.ParseWithOptionsAsync(parserOptions);
		}

		public virtual string SerializeToXmlText(IDocumentItem obj)
		{
			var devidedTypes = typeof(MorestachioDocument).Assembly
									.GetTypes()
									.Where(e => e.IsClass)
									.Where(e => typeof(IDocumentItem)
											.IsAssignableFrom(e))
									.ToArray();
			var xmlSerializer = new XmlSerializer(obj.GetType(), devidedTypes);

			using (var ms = new MemoryStream())
			{
				xmlSerializer.Serialize(ms, obj);
				ms.Position = 0;
				using var reader = new StreamReader(ms, true);
				var xmlText = reader.ReadToEnd(); // no exception on `LoadXml`

				return PrettifyXML(xmlText);
			}

			static string PrettifyXML(string xml)
			{
				using var mStream = new MemoryStream();
				using var writer = new XmlTextWriter(mStream, Encoding.UTF8);
				var document = new XmlDocument();
				try
				{
					// Load the XmlDocument with the XML.
					document.LoadXml(xml);

					writer.Formatting = System.Xml.Formatting.Indented;

					// Write the XML into a formatting XmlTextWriter
					document.WriteContentTo(writer);

					writer.Flush();
					mStream.Flush();

					// Have to rewind the MemoryStream in order to read
					// its contents.
					mStream.Position = 0;

					// Read MemoryStream contents into a StreamReader.
					var sReader = new StreamReader(mStream);

					// Extract the text from the StreamReader.
					var formattedXml = sReader.ReadToEnd();

					return formattedXml;
				}
				catch (XmlException e)
				{
					return e.Message;
					// Handle the exception
				}
			}
		}

		public virtual string SerializeToJsonText(IDocumentItem obj)
		{
			var jsonSerializerSettings = new JsonSerializerSettings();
			jsonSerializerSettings.Formatting = Formatting.Indented;
			jsonSerializerSettings.TypeNameHandling = TypeNameHandling.Objects;
			return JsonConvert.SerializeObject(obj, jsonSerializerSettings);
		}
	}
}
