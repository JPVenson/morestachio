#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Morestachio.Document.Custom;
using Morestachio.Formatter.Framework;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Context.FallbackResolver;
using Morestachio.Framework.Context.Options;
using Morestachio.Framework.Context.Resolver;
using Morestachio.Framework.Error;
using Morestachio.Framework.IO;
using Morestachio.Framework.IO.SingleStream;
using Morestachio.Helper.Logging;
using Morestachio.TemplateContainers;
using Morestachio.Util;
using Morestachio.Util.Sealing;

#endregion

namespace Morestachio;

/// <summary>
///     Options for Parsing run
/// </summary>
public class ParserOptions : SealedBase
{
	private IMorestachioFormatterService _formatters;
	private ICustomDocumentList _customDocumentItemProviders;
	private IPartialsStore _partialsStore;
	private bool _profileExecution;
	private IValueResolver _valueResolver;
	private IFallbackValueResolver _fallbackValueResolver;
	private CultureInfo _cultureInfo;
	private uint _partialStackSize;
	private ScopingBehavior _scopingBehavior;
	private PartialStackOverflowBehavior _stackOverflowBehavior;
	private UnmatchedFormatterBehavior _unmatchedFormatterBehavior;
	private TimeSpan _timeout;
	private ITemplateContainer _template;
	private bool _disableContentEscaping;
	private long _maxSize;
	private ByteCounterFactory _streamFactory;
	private Encoding _encoding;
	private string _null;
	private bool _handleDictionaryAsObject;
	private ILogger _logger;
	private UnmatchedTagBehavior _unmatchedTagBehavior;
	private bool _strictExecution;

	/// <summary>
	///		Creates a new Empty ParserOptions object. Use the <see cref="ParserOptionsBuilder"/> to create a new ParserOptions object.
	/// </summary>
	internal ParserOptions()
	{
		_customDocumentItemProviders = new CustomDocumentList();
	}

	///// <summary>
	/////     Creates a new object without any template
	///// </summary>
	//public ParserOptions() : this(new StringTemplateContainer(""))
	//{
	//}

	///// <summary>
	/////     ctor
	///// </summary>
	///// <param name="template"></param>
	//public ParserOptions(ITemplateContainer template)
	//	: this(template, (Func<Stream>)null)
	//{
	//}

	///// <summary>
	/////     ctor
	///// </summary>
	///// <param name="template"></param>
	///// <param name="sourceStream">The factory that is used for each template generation</param>
	//public ParserOptions(ITemplateContainer template,
	//					Func<Stream> sourceStream)
	//	: this(template, sourceStream, null)
	//{
	//}

	///// <summary>
	/////     Initializes a new instance of the <see cref="ParserOptions" /> class.
	///// </summary>
	///// <param name="template">The template.</param>
	///// <param name="sourceStream">The source stream.</param>
	///// <param name="encoding">The encoding.</param>
	//public ParserOptions(ITemplateContainer template,
	//					Func<Stream> sourceStream,
	//					Encoding encoding) 
	//	: this(template, ByteCounterFactory.GetDefaultByteCounter(sourceStream), encoding)
	//{
	//}

	///// <summary>
	/////     Initializes a new instance of the <see cref="ParserOptions" /> class.
	///// </summary>
	///// <param name="template">The template.</param>
	///// <param name="sourceStream">The source stream.</param>
	///// <param name="encoding">The encoding.</param>
	//public ParserOptions(ITemplateContainer template,
	//					Func<ParserOptions, IByteCounterStream> sourceStream,
	//					Encoding encoding)
	//{
	//	Template = template ?? new StringTemplateContainer("");
	//	StreamFactory = new ByteCounterFactory(sourceStream);
	//	Encoding = encoding ?? Encoding.UTF8;
	//	_formatters = new MorestachioFormatterService(false);
	//	Null = string.Empty;
	//	MaxSize = 0;
	//	DisableContentEscaping = false;
	//	Timeout = TimeSpan.Zero;
	//	PartialStackSize = 255;
	//	_customDocumentItemProviders = new CustomDocumentList();
	//	CultureInfo = CultureInfo.CurrentCulture;
	//	UnmatchedTagBehavior = UnmatchedTagBehavior.ThrowError | UnmatchedTagBehavior.LogWarning;
	//	FallbackValueResolver = GetDefaultResolver();
	//}

	///// <summary>
	/////     Initializes a new instance of the <see cref="ParserOptions" /> class.
	///// </summary>
	///// <param name="template">The template.</param>
	///// <param name="sourceStream">The source stream.</param>
	///// <param name="encoding">The encoding.</param>
	//public ParserOptions(ITemplateContainer template,
	//					Func<ParserOptions, IByteCounterStream> sourceStream)
	//	: this(template, sourceStream, null)
	//{
	//}

	///// <summary>
	/////     Initializes a new instance of the <see cref="ParserOptions" /> class.
	///// </summary>
	///// <param name="template">The template.</param>
	///// <param name="sourceStream">The source stream.</param>
	///// <param name="encoding">The encoding.</param>
	///// <param name="maxSize">The maximum size.</param>
	///// <param name="disableContentEscaping">if set to <c>true</c> [disable content escaping].</param>
	//public ParserOptions(ITemplateContainer template,
	//					Func<Stream> sourceStream,
	//					Encoding encoding,
	//					long maxSize,
	//					bool disableContentEscaping = false)
	//	: this(template, sourceStream, encoding)
	//{
	//	MaxSize = maxSize;
	//	DisableContentEscaping = disableContentEscaping;
	//}

	///// <summary>
	/////     Initializes a new instance of the <see cref="ParserOptions" /> class.
	///// </summary>
	///// <param name="template">The template.</param>
	///// <param name="sourceStream">The source stream.</param>
	///// <param name="encoding">The encoding.</param>
	///// <param name="disableContentEscaping">if set to <c>true</c> [disable content escaping].</param>
	//public ParserOptions(ITemplateContainer template,
	//					Func<Stream> sourceStream,
	//					Encoding encoding,
	//					bool disableContentEscaping = false)
	//	: this(template, sourceStream, encoding, 0, disableContentEscaping)
	//{
	//}

	///// <summary>
	/////     ctor
	///// </summary>
	///// <param name="template"></param>
	//public ParserOptions(string template)
	//	: this(template, null)
	//{
	//}

	///// <summary>
	/////     ctor
	///// </summary>
	///// <param name="template"></param>
	///// <param name="sourceStream">The factory that is used for each template generation</param>
	//public ParserOptions(string template,
	//					Func<Stream> sourceStream)
	//	: this(template, sourceStream, null)
	//{
	//}

	///// <summary>
	/////     Initializes a new instance of the <see cref="ParserOptions" /> class.
	///// </summary>
	///// <param name="template">The template.</param>
	///// <param name="sourceStream">The source stream.</param>
	///// <param name="encoding">The encoding.</param>
	//public ParserOptions(string template,
	//					Func<Stream> sourceStream,
	//					Encoding encoding)
	//	: this(new StringTemplateContainer(template), sourceStream, encoding)
	//{
	//}

	///// <summary>
	/////     Initializes a new instance of the <see cref="ParserOptions" /> class.
	///// </summary>
	///// <param name="template">The template.</param>
	///// <param name="sourceStream">The source stream.</param>
	///// <param name="encoding">The encoding.</param>
	///// <param name="maxSize">The maximum size.</param>
	///// <param name="disableContentEscaping">if set to <c>true</c> [disable content escaping].</param>
	//public ParserOptions(string template,
	//					Func<Stream> sourceStream,
	//					Encoding encoding,
	//					long maxSize,
	//					bool disableContentEscaping = false)
	//	: this(template, sourceStream, encoding)
	//{
	//	MaxSize = maxSize;
	//	DisableContentEscaping = disableContentEscaping;
	//}

	///// <summary>
	/////     Initializes a new instance of the <see cref="ParserOptions" /> class.
	///// </summary>
	///// <param name="template">The template.</param>
	///// <param name="sourceStream">The source stream.</param>
	///// <param name="encoding">The encoding.</param>
	///// <param name="disableContentEscaping">if set to <c>true</c> [disable content escaping].</param>
	//public ParserOptions(string template,
	//					Func<Stream> sourceStream,
	//					Encoding encoding,
	//					bool disableContentEscaping = false)
	//	: this(template, sourceStream, encoding, 0, disableContentEscaping)
	//{
	//}

	/// <inheritdoc />
	public override void Seal()
	{
		base.Seal();
		_customDocumentItemProviders.Seal();
	}

	/// <summary>
	///		When enabled, adds checks to ensure that a property resolving operation will always obtain a value. When enabled and a property cannot be resolved, an exception is thrown, otherwise null is returned.
	/// </summary>
	public bool StrictExecution
	{
		get { return _strictExecution; }
		set
		{
			CheckSealed();
			_strictExecution = value;
		}
	}

	/// <summary>
	///     If set to true, the tokenizer will add an token for each comment found. Default is <code>false</code>
	/// </summary>
	public bool TokenizeComments
	{
		get { return _tokenizeComments; }
		set
		{
			CheckSealed();
			_tokenizeComments = value;
		}
	}

	/// <summary>
	///     The store for PreParsed Partials
	/// </summary>
	public IPartialsStore PartialsStore
	{
		get { return _partialsStore; }
		set
		{
			CheckSealed();
			_partialsStore = value;
		}
	}

	/// <summary>
	///     The list of provider that emits custom document items
	/// </summary>
	public ICustomDocumentList CustomDocumentItemProviders
	{
		get { return _customDocumentItemProviders; }
	}

	/// <summary>
	///     [Experimental]If set to True morestachio will profile the execution and report the result in both
	///     <seealso cref="MorestachioDocumentInfo" />
	/// </summary>
	public bool ProfileExecution
	{
		get { return _profileExecution; }
		set
		{
			CheckSealed();
			_profileExecution = value;
		}
	}

	/// <summary>
	///     Can be used to resolve values from custom objects
	/// </summary>
	public IValueResolver ValueResolver
	{
		get { return _valueResolver; }
		set
		{
			CheckSealed();
			_valueResolver = value;
		}
	}

	/// <summary>
	///     The Value Resolver that is called when the <see cref="ValueResolver"/> does not support an object and its not an <see cref="IDictionary{T,TE}"/>
	/// </summary>
	/// <value>
	///		Default depends on executing .Net runtime
	/// </value>
	public IFallbackValueResolver FallbackValueResolver
	{
		get { return _fallbackValueResolver; }
		set
		{
			CheckSealed();
			_fallbackValueResolver = value;
		}
	}

	/// <summary>
	///     Gets or Sets the Culture in which the template should be rendered
	/// </summary>
	public CultureInfo CultureInfo
	{
		get { return _cultureInfo; }
		set
		{
			CheckSealed();
			_cultureInfo = value ?? throw new ArgumentException();
		}
	}

	/// <summary>
	///     Can be used to observe unresolved paths
	/// </summary>
	public event InvalidPath UnresolvedPath;

	/// <summary>
	///     Adds an Formatter overwrite or new Formatter for an Type
	/// </summary>

	public IMorestachioFormatterService Formatters
	{
		get { return _formatters; }
		set
		{
			CheckSealed();
			_formatters = value ?? throw new InvalidOperationException("You must set the Formatters matcher");
		}
	}

	/// <summary>
	///     Gets or sets the max Stack size for nested Partials in execution. Recommended to be not exceeding 2000. Defaults to
	///     255.
	/// </summary>
	public uint PartialStackSize
	{
		get { return _partialStackSize; }
		set
		{
			CheckSealed();
			_partialStackSize = value;
		}
	}

	/// <summary>
	///     Defines how blocks should behave when a scope is present
	/// </summary>
	public ScopingBehavior ScopingBehavior
	{
		get { return _scopingBehavior; }
		set
		{
			CheckSealed();
			_scopingBehavior = value;
		}
	}

	/// <summary>
	///     Defines how the Parser should behave when encountering a the PartialStackSize to be exceeded.
	///     Default is <see cref="PartialStackOverflowBehavior.FailWithException" />
	/// </summary>
	public PartialStackOverflowBehavior StackOverflowBehavior
	{
		get { return _stackOverflowBehavior; }
		set
		{
			CheckSealed();
			_stackOverflowBehavior = value;
		}
	}

	/// <summary>
	///     Defines how the Parser should behave when encountering a formatter call that cannot be resolved
	///     Default is <see cref="UnmatchedFormatterBehavior.ParentValue" />
	/// </summary>
	public UnmatchedFormatterBehavior UnmatchedFormatterBehavior
	{
		get { return _unmatchedFormatterBehavior; }
		set
		{
			CheckSealed();
			_unmatchedFormatterBehavior = value;
		}
	}

	/// <summary>
	///     Defines the behavior how the tokenizer should react to tags it does not recognizance
	/// </summary>
	public UnmatchedTagBehavior UnmatchedTagBehavior
	{
		get { return _unmatchedTagBehavior; }
		set
		{
			CheckSealed();
			_unmatchedTagBehavior = value;
		}
	}

	/// <summary>
	///     Gets or sets the timeout. After the timeout is reached and the Template has not finished Processing an Exception is
	///     thrown.
	///     For no timeout use <code>TimeSpan.Zero</code>
	/// </summary>
	/// <value>
	///     The timeout.
	/// </value>
	public TimeSpan Timeout
	{
		get { return _timeout; }
		set
		{
			CheckSealed();
			_timeout = value;
		}
	}

	/// <summary>
	///     The template content to parse.
	/// </summary>

	public ITemplateContainer Template
	{
		get { return _template; }
		set
		{
			CheckSealed();
			_template = value ?? throw new ArgumentException();
		}
	}

	/// <summary>
	///     In some cases, content should not be escaped (such as when rendering text bodies and subjects in emails).
	///     By default, we use no content escaping, but this parameter allows it to be enabled. Default is False
	/// </summary>
	public bool DisableContentEscaping
	{
		get { return _disableContentEscaping; }
		set
		{
			CheckSealed();
			_disableContentEscaping = value;
		}
	}

	/// <summary>
	///     Defines a Max size for the Generated Template.
	///     Zero for unlimited
	/// </summary>
	public long MaxSize
	{
		get { return _maxSize; }
		set
		{
			CheckSealed();
			_maxSize = value;
		}
	}

	/// <summary>
	///     SourceFactory can be used to create a new stream for each template. Default is
	///     <code>() => new MemoryStream()</code>
	/// </summary>
	public ByteCounterFactory StreamFactory
	{
		get { return _streamFactory; }
		set
		{
			CheckSealed();
			_streamFactory = value ?? throw new ArgumentException();
		}
	}

	/// <summary>
	///     In what encoding should the text be written
	///     Default is <code>Encoding.Utf8</code>
	/// </summary>

	public Encoding Encoding
	{
		get { return _encoding; }
		set
		{
			CheckSealed();
			_encoding = value ?? throw new ArgumentException();
		}
	}

	/// <summary>
	///     The Logger used for this template
	/// </summary>
	public ILogger Logger
	{
		get { return _logger; }
		set
		{
			CheckSealed();
			_logger = value;
		}
	}

	/// <summary>
	///     Defines how NULL values are exposed to the Template default is <code>String.Empty</code>
	/// </summary>

	public string Null
	{
		get { return _null; }
		set
		{
			CheckSealed();
			_null = value ?? throw new ArgumentException();
		}
	}

	/// <summary>
	///     If enabled all instances of IDictionary{string, object} will be processed as normal objects.
	///     <value>Default is false</value>
	/// </summary>
	public bool HandleDictionaryAsObject
	{
		get { return _handleDictionaryAsObject; }
		set
		{
			CheckSealed();
			_handleDictionaryAsObject = value;
		}
	}

	/// <summary>
	///     Copies all values from one parser options to a new instance
	/// </summary>
	/// <returns></returns>
	public ParserOptions Copy()
	{
		return new ParserOptions()
		{
			Logger = Logger,
			Formatters = Formatters,
			_customDocumentItemProviders = _customDocumentItemProviders,
			CultureInfo = CultureInfo,
			DisableContentEscaping = DisableContentEscaping,
			Encoding = Encoding,
			HandleDictionaryAsObject = HandleDictionaryAsObject,
			MaxSize = MaxSize,
			Null = Null,
			PartialStackSize = PartialStackSize,
			PartialsStore = PartialsStore,
			ProfileExecution = ProfileExecution,
			ScopingBehavior = ScopingBehavior,
			StackOverflowBehavior = StackOverflowBehavior,
			StreamFactory = StreamFactory,
			Template = Template,
			Timeout = Timeout,
			UnmatchedFormatterBehavior = UnmatchedFormatterBehavior,
			UnmatchedTagBehavior = UnmatchedTagBehavior,
			ValueResolver = ValueResolver
		};
	}

	private ContextObject _nullContext;
	private ContextObject _trueContext;
	private ContextObject _falseContext;
	private bool _tokenizeComments;

	/// <summary>
	///     Allows the creation of an custom Context object
	/// </summary>
	/// <param name="key"></param>
	/// <param name="value"></param>
	/// <param name="parent"></param>
	/// <returns></returns>
	public virtual ContextObject CreateContextObject(string key,
													object value,
													ContextObject parent = null)
	{
		if (key == "x:null")
		{
			return _nullContext ??= new ContextObject("x:null", null, null);
		}

		if (value is not bool val)
		{
			return new ContextObject(key, parent, value);
		}

		if (val)
		{
			return _trueContext ??= new ContextObject("x:true", null, true);
		}

		return _falseContext ??= new ContextObject("x:false", null, false);
	}

	internal void OnUnresolvedPath(InvalidPathEventArgs args)
	{
		UnresolvedPath?.Invoke(args);

		if (StrictExecution)
		{
			throw new UnresolvedPathException(args);
		}
	}
}