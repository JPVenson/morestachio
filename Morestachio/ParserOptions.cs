#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using Morestachio.Document.Custom;
using Morestachio.Formatter.Framework;
using Morestachio.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Context.Options;
using Morestachio.Framework.Context.Resolver;
using Morestachio.Framework.IO;
using Morestachio.Util.Sealing;

#endregion

namespace Morestachio
{


	/// <summary>
	///     Options for Parsing run
	/// </summary>
	[PublicAPI]
	public class ParserOptions : SealedBase
	{
		[NotNull]
		private IMorestachioFormatterService _formatters;

		private SealableList<CustomDocumentItemProvider> _customDocumentItemProviders;
		[CanBeNull] private IPartialsStore _partialsStore;
		private bool _profileExecution;
		private IValueResolver _valueResolver;
		private CultureInfo _cultureInfo;
		private uint _partialStackSize;
		private ScopingBehavior _scopingBehavior;
		private PartialStackOverflowBehavior _stackOverflowBehavior;
		private UnmatchedFormatterBehavior _unmatchedFormatterBehavior;
		private TimeSpan _timeout;
		[NotNull] private string _template;
		private bool _disableContentEscaping;
		private long _maxSize;
		[NotNull] private ByteCounterFactory _streamFactory;
		[NotNull] private Encoding _encoding;
		[NotNull] private string _null;
		private bool _handleDictionaryAsObject;

		/// <summary>
		///		Creates a new object without any template
		/// </summary>
		public ParserOptions() : this(string.Empty)
		{
			
		}

		/// <summary>
		///     ctor
		/// </summary>
		/// <param name="template"></param>
		public ParserOptions([NotNull]string template)
			: this(template, null)
		{
		}

		/// <summary>
		///     ctor
		/// </summary>
		/// <param name="template"></param>
		/// <param name="sourceStream">The factory that is used for each template generation</param>
		public ParserOptions([NotNull]string template,
			[CanBeNull]Func<Stream> sourceStream)
			: this(template, sourceStream, null)
		{
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ParserOptions" /> class.
		/// </summary>
		/// <param name="template">The template.</param>
		/// <param name="sourceStream">The source stream.</param>
		/// <param name="encoding">The encoding.</param>
		public ParserOptions([NotNull]string template,
			[CanBeNull]Func<Stream> sourceStream,
			[CanBeNull]Encoding encoding)
		{
			Template = template ?? "";
			StreamFactory = new ByteCounterFactory(sourceStream);
			Encoding = encoding ?? Encoding.UTF8;
			_formatters = new MorestachioFormatterService();
			Null = string.Empty;
			MaxSize = 0;
			DisableContentEscaping = false;
			Timeout = TimeSpan.Zero;
			PartialStackSize = 255;
			_customDocumentItemProviders = new SealableList<CustomDocumentItemProvider>();
			CultureInfo = CultureInfo.CurrentCulture;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ParserOptions" /> class.
		/// </summary>
		/// <param name="template">The template.</param>
		/// <param name="sourceStream">The source stream.</param>
		/// <param name="encoding">The encoding.</param>
		/// <param name="maxSize">The maximum size.</param>
		/// <param name="disableContentEscaping">if set to <c>true</c> [disable content escaping].</param>
		public ParserOptions([NotNull]string template,
			[CanBeNull]Func<Stream> sourceStream,
			[CanBeNull]Encoding encoding,
			long maxSize,
			bool disableContentEscaping = false)
			: this(template, sourceStream, encoding)
		{
			MaxSize = maxSize;
			DisableContentEscaping = disableContentEscaping;
		}

		/// <summary>
		///     Initializes a new instance of the <see cref="ParserOptions" /> class.
		/// </summary>
		/// <param name="template">The template.</param>
		/// <param name="sourceStream">The source stream.</param>
		/// <param name="encoding">The encoding.</param>
		/// <param name="disableContentEscaping">if set to <c>true</c> [disable content escaping].</param>
		public ParserOptions([NotNull]string template,
			[CanBeNull]Func<Stream> sourceStream,
			[CanBeNull]Encoding encoding,
			bool disableContentEscaping = false)
			: this(template, sourceStream, encoding, 0, disableContentEscaping)
		{
		}

		/// <inheritdoc />
		public override void Seal()
		{
			base.Seal();
			_customDocumentItemProviders.Seal();
		}

		/// <summary>
		///		The store for PreParsed Partials
		/// </summary>
		[CanBeNull]
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
		///		The list of provider that emits custom document items
		/// </summary>
		public IList<CustomDocumentItemProvider> CustomDocumentItemProviders
		{
			get { return _customDocumentItemProviders; }
		}

		/// <summary>
		///		[Experimental]If set to True morestachio will profile the execution and report the result in both <seealso cref="MorestachioDocumentInfo"/>
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
		///		Can be used to resolve values from custom objects
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
		///		Gets or Sets the Culture in which the template should be rendered
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
		///		Can be used to observe unresolved paths
		/// </summary>
		public event InvalidPath UnresolvedPath;
		
		/// <summary>
		///     Adds an Formatter overwrite or new Formatter for an Type
		/// </summary>
		[NotNull]
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
		///		Gets or sets the max Stack size for nested Partials in execution. Recommended to be not exceeding 2000. Defaults to 255.
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
		///		Defines how blocks should behave when a scope is present
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
		///		Defines how the Parser should behave when encountering a the PartialStackSize to be exceeded.
		///		Default is <see cref="PartialStackOverflowBehavior.FailWithException"/>
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
		///		Defines how the Parser should behave when encountering a formatter call that cannot be resolved
		///		Default is <see cref="UnmatchedFormatterBehavior.ParentValue"/>
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
		///		Gets or sets the timeout. After the timeout is reached and the Template has not finished Processing and Exception is thrown.
		///		For no timeout use <code>TimeSpan.Zero</code>
		/// </summary>
		/// <value>
		/// The timeout.
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
		[NotNull]
		public string Template
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
		[NotNull]
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
		[NotNull]
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
		///     Defines how NULL values are exposed to the Template default is <code>String.Empty</code>
		/// </summary>
		[NotNull]
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
		///		If enabled all instances of IDictionary{string, object} will be processed as normal objects.
		/// <value>Default is false</value>
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
		///		Allows the creation of an custom Context object
		/// </summary>
		/// <param name="key"></param>
		/// <param name="token"></param>
		/// <param name="value"></param>
		/// <param name="parent"></param>
		/// <returns></returns>
		[NotNull]
		public virtual ContextObject CreateContextObject(string key,
			CancellationToken token,
			object value,
			ContextObject parent = null)
		{
			return new ContextObject(this, key, parent, value)
			{
				CancellationToken = token,
			};
		}

		internal void OnUnresolvedPath(InvalidPathEventArgs args)
		{
			UnresolvedPath?.Invoke(args);
		}
	}
}