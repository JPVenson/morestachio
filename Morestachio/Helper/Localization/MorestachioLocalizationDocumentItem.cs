using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Morestachio.Document;
using Morestachio.Document.Contracts;
using Morestachio.Document.Items.Base;
using Morestachio.Document.Visitor;
using Morestachio.Framework.Context;
using Morestachio.Framework.Expression;
using Morestachio.Framework.IO;

#if ValueTask
using ItemExecutionPromise = System.Threading.Tasks.ValueTask<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#else
using ItemExecutionPromise = System.Threading.Tasks.Task<System.Collections.Generic.IEnumerable<Morestachio.Document.Contracts.DocumentItemExecution>>;
#endif
namespace Morestachio.Helper.Localization
{
	/// <summary>
	///		Allows the usage of {{#loc expression}} in combination with an <see cref="IMorestachioLocalizationService"/>
	/// </summary>
	[System.Serializable]
	public class MorestachioLocalizationDocumentItem : ExpressionDocumentItemBase
	{
		internal MorestachioLocalizationDocumentItem()
		{
			
		}

		/// <inheritdoc />
		public MorestachioLocalizationDocumentItem(IMorestachioExpression value) : base()
		{
			base.MorestachioExpression = value;
		}

		/// <inheritdoc />
		protected MorestachioLocalizationDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}
		
		/// <inheritdoc />
		public override async ItemExecutionPromise Render(IByteCounterStream outputStream,
			ContextObject context,
			ScopeData scopeData)
		{
			var service = context.Options.Formatters.GetService(typeof(IMorestachioLocalizationService)) as IMorestachioLocalizationService;
			if (service == null)
			{
				outputStream.Write("IMorestachioLocalizationService not registered");
				return Enumerable.Empty<DocumentItemExecution>();
			}

			var valueContext = await MorestachioExpression.GetValue(context, scopeData);

			var culture = context.Options.CultureInfo;
			if (scopeData.CustomData.TryGetValue(MorestachioCustomCultureLocalizationDocumentItem.LocalizationCultureKey, out var customCulture) && customCulture is CultureInfo culInfo)
			{
				culture = culInfo;
			}

			var translationOrNull = service.GetTranslationOrNull(await valueContext.RenderToString(), culture);
			outputStream.Write(translationOrNull?.ToString());
			return Enumerable.Empty<DocumentItemExecution>();
		}
		
		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}