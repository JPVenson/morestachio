#region

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using JetBrains.Annotations;
using Morestachio.Document.Custom;
using Morestachio.Formatter;
using Morestachio.Formatter.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Context.Options;
using Morestachio.Framework.Context.Resolver;
using Morestachio.Framework.IO;

#endregion

namespace Morestachio
{
	/// <summary>
	///     Options for Parsing run
	/// </summary>
	[PublicAPI]
	public class ParserOptions
	{
		[NotNull]
		private IMorestachioFormatterService _formatters;

		/// <summary>
		///		The store for PreParsed Partials
		/// </summary>
		[CanBeNull]
		public IPartialsStore PartialsStore { get; set; }

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
			CustomDocumentItemProviders = new List<CustomDocumentItemProvider>();
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

		/// <summary>
		///		The list of provider that emits custom document items
		/// </summary>
		public IList<CustomDocumentItemProvider> CustomDocumentItemProviders { get; private set; }

		/// <summary>
		///		If set to True morestachio will profile the execution and report the result in both <seealso cref="MorestachioDocumentInfo"/>
		/// </summary>
		public bool ProfileExecution { get; set; }

		/// <summary>
		///		Can be used to resolve values from custom objects
		/// </summary>
		public IValueResolver ValueResolver { get; set; }

		/// <summary>
		///		Gets or Sets the Culture in which the template should be rendered
		/// </summary>
		public CultureInfo CultureInfo { get; set; }

		/// <summary>
		///		Can be used to observe unresolved paths
		/// </summary>
		public event InvalidPath UnresolvedPath;

		///// <summary>
		/////		See <see cref="IPartialTemplateProvider"/>
		///// </summary>
		//public IPartialTemplateProvider PartialTemplateProvider { get; set; }

		/// <summary>
		///     Adds an Formatter overwrite or new Formatter for an Type
		/// </summary>
		[NotNull]
		public IMorestachioFormatterService Formatters
		{
			get { return _formatters; }
			set
			{
				_formatters = value ?? throw new InvalidOperationException("You must set the Formatters matcher");
			}
		}

		/// <summary>
		///		Gets or sets the max Stack size for nested Partials in execution. Recommended to be not exceeding 2000. Defaults to 255.
		/// </summary>
		public uint PartialStackSize { get; set; }

		/// <summary>
		///		Defines how blocks should behave when a scope is present
		/// </summary>
		public ScopingBehavior ScopingBehavior { get; set; }

		/// <summary>
		///		Defines how the Parser should behave when encountering a the PartialStackSize to be exceeded.
		///		Default is <see cref="PartialStackOverflowBehavior.FailWithException"/>
		/// </summary>
		public PartialStackOverflowBehavior StackOverflowBehavior { get; set; }

		/// <summary>
		///		Defines how the Parser should behave when encountering a formatter call that cannot be resolved
		///		Default is <see cref="UnmatchedFormatterBehavior.ParentValue"/>
		/// </summary>
		public UnmatchedFormatterBehavior UnmatchedFormatterBehavior { get; set; }

		/// <summary>
		///		Gets or sets the timeout. After the timeout is reached and the Template has not finished Processing and Exception is thrown.
		///		For no timeout use <code>TimeSpan.Zero</code>
		/// </summary>
		/// <value>
		/// The timeout.
		/// </value>
		public TimeSpan Timeout { get; set; }

		/// <summary>
		///     The template content to parse.
		/// </summary>
		[NotNull]
		public string Template { get; private set; }

		/// <summary>
		///     In some cases, content should not be escaped (such as when rendering text bodies and subjects in emails).
		///     By default, we use no content escaping, but this parameter allows it to be enabled. Default is False
		/// </summary>
		public bool DisableContentEscaping { get; private set; }

		/// <summary>
		///     Defines a Max size for the Generated Template.
		///     Zero for unlimited
		/// </summary>
		public long MaxSize { get; private set; }

		/// <summary>
		///     SourceFactory can be used to create a new stream for each template. Default is
		///     <code>() => new MemoryStream()</code>
		/// </summary>
		[NotNull]
		public ByteCounterFactory StreamFactory { get; private set; }

		/// <summary>
		///     In what encoding should the text be written
		///     Default is <code>Encoding.Utf8</code>
		/// </summary>
		[NotNull]
		public Encoding Encoding { get; private set; }

		/// <summary>
		///     Defines how NULL values are exposed to the Template default is <code>String.Empty</code>
		/// </summary>
		[NotNull]
		public string Null { get; set; }

		/// <summary>
		///		If enabled all instances of IDictionary{string, object} will be processed as normal objects.
		/// <value>Default is false</value>
		/// </summary>
		public bool HandleDictionaryAsObject { get; set; }

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

		public ParserOptions CopyWithTemplate(string template)
		{
			return new ParserOptions(template)
			{
				Null = Null,
				StackOverflowBehavior = StackOverflowBehavior,
				Formatters = Formatters,
				Timeout = Timeout,
				CustomDocumentItemProviders = CustomDocumentItemProviders,
				MaxSize = MaxSize,
				DisableContentEscaping = DisableContentEscaping,
				Encoding = Encoding,
				PartialStackSize = PartialStackSize,
				PartialsStore = PartialsStore,
				ProfileExecution = ProfileExecution,
				Template = template,
				ValueResolver = ValueResolver,
				UnresolvedPath = UnresolvedPath,
				StreamFactory = StreamFactory,
				CultureInfo = CultureInfo
			};
		}

		internal void OnUnresolvedPath(InvalidPathEventArgs args)
		{
			UnresolvedPath?.Invoke(args);
		}
	}
}