using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Morestachio.Formatter.Framework;
using Morestachio.Framework.Context.Options;
using Morestachio.Framework.Context.Resolver;
using Morestachio.Rendering;

namespace Morestachio.AspNetCore
{
	public abstract class MorestachioTemplateBase : IMorestachioTemplate
	{
		protected MorestachioTemplateBase()
		{
			MorestachioFormatterService = new MorestachioFormatterService(false);
			Timeout = TimeSpan.FromMinutes(1);
		}

		public IMorestachioFormatterService MorestachioFormatterService { get; set; }
		public CultureInfo Culture { get; set; }
		public Encoding Encoding { get; set; }
		public IValueResolver ValueResolver { get; set; }
		public uint PartialStackSize { get; set; } = 255;
		public PartialStackOverflowBehavior StackOverflowBehavior { get; set; }
		public UnmatchedFormatterBehavior UnmatchedFormatterBehavior { get; set; }
		public TimeSpan Timeout { get; set; }
		public bool DisableContentEscaping { get; set; }
		public long MaxSize { get; set; }
		public string Null { get; set; }
		public bool HandleDictionaryAsObject { get; set; }

		public bool AddHttpContext { get; set; }

		public abstract ValueTask<IRenderer> GetTemplateCore(HttpContext context);
		public abstract ValueTask<object> GetDataCore(HttpContext context);

		public virtual ParserOptions CreateOptions(string template, Encoding encodingFromRequest)
		{
			var parserOptions = new ParserOptions(template, null, Encoding ?? encodingFromRequest, MaxSize, DisableContentEscaping);
			parserOptions.Formatters = MorestachioFormatterService ?? parserOptions.Formatters;
			parserOptions.CultureInfo = Culture ?? parserOptions.CultureInfo;
			parserOptions.ValueResolver = ValueResolver ?? parserOptions.ValueResolver;
			parserOptions.PartialStackSize = PartialStackSize;
			parserOptions.StackOverflowBehavior = StackOverflowBehavior;
			parserOptions.UnmatchedFormatterBehavior = UnmatchedFormatterBehavior;
			parserOptions.Timeout = Timeout;
			parserOptions.Null = Null;
			parserOptions.HandleDictionaryAsObject = HandleDictionaryAsObject;
			return parserOptions;
		}

		public abstract bool Matches(HttpContext context);

		public virtual async ValueTask<IRenderer> GetTemplate(HttpContext context)
		{
			return await GetTemplateCore(context);
		}

		public virtual  async ValueTask<object> GetData(HttpContext context)
		{
			var data = new AspMorestachioData();
			data.Data = await GetDataCore(context);
			if (AddHttpContext)
			{
				data.Context = context;
			}

			return data;
		}
	}
}