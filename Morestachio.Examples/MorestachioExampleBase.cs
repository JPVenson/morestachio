using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items;
using Morestachio.Parsing.ParserErrors;
using Morestachio.Rendering;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

// ReSharper disable once CheckNamespace
namespace Morestachio.Example.Base;

/// <summary>
///		DTO object used to communicate with the UI
/// </summary>
public class MorestachioRunResult
{
	/// <summary>
	///		When set will indicate errors in the template and should be displayed in UI
	/// </summary>
	public IEnumerable<IMorestachioError> Errors { get; set; }

	/// <summary>
	///		The stringified version of the generated template
	/// </summary>
	public string TemplateResult { get; set; }

	/// <summary>
	///		Tracking information
	/// </summary>
	public IEnumerable<KeyValuePair<string, TimeSpan>> Times { get; set; }

	/// <summary>
	///		The document as an Xml document
	/// </summary>
	public string XmlDocument { get; set; }

	/// <summary>
	///		The document as an Xml document
	/// </summary>
	public string JsonDocument { get; set; }
}

/// <summary>
///		Base class for generating Templates with the use of the Morestachio Demo
/// </summary>
public abstract class MorestachioExampleBase
{
	///  <summary>
	/// 		External call point. This is where the UI will enter the Custom document generation
	///  </summary>
	///  <param name="templateText">The Template as provided by the UI</param>
	///  <param name="encoding">Currently expected to be <see cref="Encoding.UTF8"/></param>
	///  <param name="shouldEscape">The toggle from UI "Html Escaped" </param>
	///  <param name="serviceProvider">The runtimes service provider</param>
	///  <returns></returns>
	public ValueTask<MorestachioRunResult> Run(string templateText, 
												Encoding encoding, 
												bool shouldEscape,
												IServiceProvider serviceProvider)
	{
		return RunCore(templateText, encoding, shouldEscape, serviceProvider);
	}

	protected virtual async ValueTask<MorestachioRunResult> RunCore(
		string templateText,
		Encoding encoding,
		bool shouldEscape,
		IServiceProvider serviceProvider
	)
	{
		//for debugging purposes
		var times = new Dictionary<string, TimeSpan>();

		//Helper method for profiling a step in template execution
		T Evaluate<T>(Func<T> fnc, string name)
		{
			var sw = Stopwatch.StartNew();

			try
			{
				return fnc();
			}
			finally
			{
				sw.Stop();
				times[name] = sw.Elapsed;
			}
		}

		//Helper method for profiling a step in template execution
		async Task<T> EvaluateAsync<T>(Func<Task<T>> fnc, string name)
		{
			var sw = Stopwatch.StartNew();

			try
			{
				return await fnc();
			}
			finally
			{
				sw.Stop();
				times[name] = sw.Elapsed;
			}
		}

		//Call the Configure method to get either a user custom defined parser options object or a default one
		var options = Evaluate(() => Configure(templateText, encoding, shouldEscape, serviceProvider), "Configure");

		//parse the template
		var documentInfo = await EvaluateAsync(async () => await Parse(options), "Parse");

		if (documentInfo.Errors.Any())
		{
			//when there are any errors just return them there is no point in further progressing with the template
			return new MorestachioRunResult()
			{
				Errors = documentInfo.Errors
			};
		}

		//get the custom data object
		var data = Evaluate(GetData, "GetData");

		//create a renderer. This might be changed to support an compiled renderer
		var renderer = Evaluate(() => CreateRenderer(documentInfo), "Create Render");

		//render the template
		var result = await EvaluateAsync(async () => (await renderer.RenderAndStringifyAsync(data, CancellationToken.None)), "Render");

		//serialization
		var jsonResult = Evaluate(() => SerializeToJsonText(documentInfo.Document), "Json Serialization");
		var xmlResult = Evaluate(() => SerializeToXmlText(documentInfo.Document), "Xml Serialization");

		return new MorestachioRunResult()
		{
			JsonDocument = jsonResult,
			XmlDocument = xmlResult,
			TemplateResult = result,
			Times = times
		};
	}

	/// <summary>
	///		Gets a renderer for an <see cref="MorestachioDocumentInfo"/>
	/// </summary>
	/// <param name="morestachioDocumentInfo"></param>
	/// <returns></returns>
	protected virtual IRenderer CreateRenderer(MorestachioDocumentInfo morestachioDocumentInfo)
	{
		return morestachioDocumentInfo.CreateRenderer();
	}

	///  <summary>
	/// 		Gets a Parser options object that can be used to process <see cref="GetData"/>
	///  </summary>
	///  <param name="templateText"></param>
	///  <param name="encoding"></param>
	///  <param name="shouldEscape"></param>
	///  <param name="serviceProvider"></param>
	///  <returns></returns>
	public virtual ParserOptions Configure(
		string templateText,
		Encoding encoding,
		bool shouldEscape,
		IServiceProvider serviceProvider
	)
	{
		return ParserOptionsBuilder.New()
									.WithTemplate(templateText)
									.WithEncoding(encoding)
									.WithDisableContentEscaping(shouldEscape)
									.WithTimeout(TimeSpan.FromSeconds(5))
									.WithServiceProvider(serviceProvider)
									.Build();
	}

	/// <summary>
	///		Gets the data object used to process the template
	/// </summary>
	/// <returns></returns>
	public abstract object GetData();

	/// <summary>
	///		Parses the Morestachio template in a processable form
	/// </summary>
	/// <param name="parserOptions"></param>
	/// <returns></returns>
	protected virtual async Task<MorestachioDocumentInfo> Parse(ParserOptions parserOptions)
	{
		return await Parser.ParseWithOptionsAsync(parserOptions);
	}

	/// <summary>
	///		Serializes the <see cref="IDocumentItem"/> to a XML document
	/// </summary>
	/// <param name="obj"></param>
	/// <returns></returns>
	protected virtual string SerializeToXmlText(IDocumentItem obj)
	{
		var documentItemTypes = typeof(MorestachioDocument).Assembly
															.GetTypes()
															.Where(e => e.IsClass)
															.Where(e => typeof(IDocumentItem)
																.IsAssignableFrom(e))
															.ToArray();
		var xmlSerializer = new XmlSerializer(obj.GetType(), documentItemTypes);

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

	/// <summary>
	///		Serializes the <see cref="IDocumentItem"/> to a Json Document
	/// </summary>
	/// <param name="obj"></param>
	/// <returns></returns>
	protected virtual string SerializeToJsonText(IDocumentItem obj)
	{
		var jsonSerializerSettings = new JsonSerializerSettings();
		jsonSerializerSettings.Formatting = Formatting.Indented;
		jsonSerializerSettings.TypeNameHandling = TypeNameHandling.Objects;
		return JsonConvert.SerializeObject(obj, jsonSerializerSettings);
	}
}