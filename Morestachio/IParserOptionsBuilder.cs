using System.Globalization;
using System.IO;
using System.Text;
using Morestachio.Document.Custom;
using Morestachio.Formatter.Framework;
using Morestachio.Framework;
using Morestachio.Framework.Context.FallbackResolver;
using Morestachio.Framework.Context.Options;
using Morestachio.Framework.Context.Resolver;
using Morestachio.Framework.IO;
using Morestachio.Helper.Logging;
using Morestachio.TemplateContainers;

namespace Morestachio;

/// <summary>
///		Contains methods for creating a new ParserOptions object
/// </summary>
public interface IParserOptionsBuilder
{
	IParserOptionsBuilder WithTemplate(string value);
	IParserOptionsBuilder WithTemplate(Func<string> value);
	IParserOptionsBuilder WithTemplate(ITemplateContainer value);
	IParserOptionsBuilder WithTemplate(Func<ITemplateContainer> value);

	IParserOptionsBuilder WithStrictExecution(bool value);
	IParserOptionsBuilder WithStrictExecution(Func<bool> value);
	IParserOptionsBuilder WithTokenizeComments(bool value);
	IParserOptionsBuilder WithTokenizeComments(Func<bool> value);
	IParserOptionsBuilder WithPartialsStore(IPartialsStore value);
	IParserOptionsBuilder WithPartialsStore(Func<IPartialsStore> value);
	IParserOptionsBuilder AddCustomDocument(CustomDocumentItemProvider value);
	IParserOptionsBuilder AddCustomDocument(Func<CustomDocumentItemProvider> value);
	IParserOptionsBuilder WithValueResolver(IValueResolver value);
	IParserOptionsBuilder WithValueResolver(Func<IValueResolver> value);
	IParserOptionsBuilder WithFallbackValueResolver(IFallbackValueResolver value);
	IParserOptionsBuilder WithFallbackValueResolver(Func<IFallbackValueResolver> value);
	IParserOptionsBuilder WithCultureInfo(CultureInfo value);
	IParserOptionsBuilder WithCultureInfo(Func<CultureInfo> value);
	IParserOptionsBuilder WithFormatterService(IMorestachioFormatterService value);
	IParserOptionsBuilder WithFormatterService(Func<IMorestachioFormatterService> value);
	IParserOptionsBuilder WithPartialStackSize(uint value);
	IParserOptionsBuilder WithPartialStackSize(Func<uint> value);
	IParserOptionsBuilder WithScopingBehavior(ScopingBehavior value);
	IParserOptionsBuilder WithScopingBehavior(Func<ScopingBehavior> value);
	IParserOptionsBuilder WithStackOverflowBehavior(PartialStackOverflowBehavior value);
	IParserOptionsBuilder WithStackOverflowBehavior(Func<PartialStackOverflowBehavior> value);
	IParserOptionsBuilder WithUnmatchedFormatterBehavior(UnmatchedFormatterBehavior value);
	IParserOptionsBuilder WithUnmatchedFormatterBehavior(Func<UnmatchedFormatterBehavior> value);
	IParserOptionsBuilder WithUnmatchedTagBehavior(UnmatchedTagBehavior value);
	IParserOptionsBuilder WithUnmatchedTagBehavior(Func<UnmatchedTagBehavior> value);
	IParserOptionsBuilder WithTimeout(TimeSpan value);
	IParserOptionsBuilder WithTimeout(Func<TimeSpan> value);
	IParserOptionsBuilder WithDisableContentEscaping(bool value);
	IParserOptionsBuilder WithDisableContentEscaping(Func<bool> value);
	IParserOptionsBuilder WithMaxSize(long value);
	IParserOptionsBuilder WithMaxSize(Func<long> value);
	IParserOptionsBuilder WithStreamFactory(ByteCounterFactory value);
	IParserOptionsBuilder WithStreamFactory(Func<ByteCounterFactory> value);
	IParserOptionsBuilder WithTargetStream(IByteCounterStream value);
	IParserOptionsBuilder WithTargetStream(Func<IByteCounterStream> value);
	IParserOptionsBuilder WithTargetStream(Func<ParserOptions, IByteCounterStream> value);
	IParserOptionsBuilder WithTargetStream(Stream value);
	IParserOptionsBuilder WithTargetStream(Func<Stream> value);
	IParserOptionsBuilder WithEncoding(Encoding value);
	IParserOptionsBuilder WithEncoding(Func<Encoding> value);
	IParserOptionsBuilder WithLogger(ILogger value);
	IParserOptionsBuilder WithLogger(Func<ILogger> value);
	IParserOptionsBuilder WithNull(string value);
	IParserOptionsBuilder WithNull(Func<string> value);
	IParserOptionsBuilder WithHandleDictionaryAsObject(bool value);
	IParserOptionsBuilder WithHandleDictionaryAsObject(Func<bool> value);

	/// <summary>
	///		Applies all settings to the given <see cref="ParserOptions"/>
	/// </summary>
	/// <param name="options"></param>
	/// <returns></returns>
	ParserOptions Apply(ParserOptions options);

	/// <summary>
	///		Creates a new default <see cref="ParserOptions"/> from <see cref="ParserOptionsDefaultBuilder"/> and applies all custom settings
	/// </summary>
	/// <returns></returns>
	ParserOptions Build();

	/// <summary>
	///		Creates an copy that is detached from this one but contains all settings
	/// </summary>
	/// <returns></returns>
	IParserOptionsBuilder Copy();
}