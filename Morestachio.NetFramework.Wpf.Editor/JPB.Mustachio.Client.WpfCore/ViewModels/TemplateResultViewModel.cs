using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JPB.Mustachio.Client.Wpf.Core.Contracts;
using JPB.Mustachio.Client.Wpf.Core.Services;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;
using Morestachio;
using Morestachio.Framework.Context;
using Morestachio.Framework.Context.Resolver;
using Morestachio.Parsing.ParserErrors;
using Newtonsoft.Json.Linq;

namespace JPB.Mustachio.Client.Wpf.Core.ViewModels
{
	public class TemplateResultViewModel : AsyncViewModelBase
	{
		private readonly TemplateServiceProvider _templateServiceProvider;

		public TemplateResultViewModel(TemplateServiceProvider templateServiceProvider)
		{
			_templateServiceProvider = templateServiceProvider;
			_templateServiceProvider.TemplateChanged += _templateServiceProvider_TemplateChanged;
			GenerateTemplateCommand = new DelegateCommand(GenerateTemplateExecute, CanGenerateTemplateExecute);
		}

		private void _templateServiceProvider_TemplateChanged(object sender, string e)
		{
			GenerateTemplate();
		}

		private GeneratedTemplateInfos _generatedTemplate;
		public GeneratedTemplateInfos GeneratedTemplate
		{
			get { return _generatedTemplate; }
			set
			{
				SendPropertyChanging(() => GeneratedTemplate);
				_generatedTemplate = value;
				SendPropertyChanged(() => GeneratedTemplate);
			}
		}

		public DelegateCommand GenerateTemplateCommand { get; private set; }

		private void GenerateTemplateExecute(object sender)
		{
			GenerateTemplate();
		}

		private bool CanGenerateTemplateExecute(object sender)
		{
			return IsNotWorking;
		}

		private bool _generateTemplateCallGuardFlag;
		public void GenerateTemplate()
		{
			if (IsWorking)
			{
				_generateTemplateCallGuardFlag = true;
				return;
			}

			SimpleWorkAsync(async () =>
			{
				var sw = Stopwatch.StartNew();
				var generatedTemplateInfos = await GenerateTemplateExecute(_templateServiceProvider.LastTemplate,
					_templateServiceProvider.LastProvider);
				sw.Stop();
				if (_generateTemplateCallGuardFlag)
				{
					_generateTemplateCallGuardFlag = false;
					GenerateTemplate();
					return;
				}

				generatedTemplateInfos.RenderTime = sw.Elapsed;
				GeneratedTemplate = generatedTemplateInfos;
			});
		}

		public class JObjectResolver : IValueResolver
		{
			public object Resolve(Type type, object value, string path, ContextObject context)
			{
				return (value as JObject)[path];
			}

			public bool CanResolve(Type type, object value, string path, ContextObject context)
			{
				return type == typeof(JObject);
			}

			public bool IsSealed { get; }
			public void Seal()
			{
				
			}
		}

		public async Task<GeneratedTemplateInfos> GenerateTemplateExecute(string template, IDataSourceProvider dataSourceProvider)
		{
			if (template == null)
			{
				return null;
			}

			var parsingOptions = new ParserOptions(template,
				() => new MemoryStream(),
				Encoding.Default,
				_templateServiceProvider.ParserOptions.MaxSize,
				_templateServiceProvider.ParserOptions.DisableContentEscaping);
			parsingOptions.Timeout = _templateServiceProvider.ParserOptions.Timeout;
			parsingOptions.Null = _templateServiceProvider.ParserOptions.NullSubstitute;
			parsingOptions.StackOverflowBehavior = _templateServiceProvider.ParserOptions.PartialStackOverflowBehavior;
			parsingOptions.PartialStackSize = _templateServiceProvider.ParserOptions.PartialStackSize;
			parsingOptions.ValueResolver = new JObjectResolver();

			//if (_templateServiceProvider.ParserOptions.EnableExtendedMissingPathOutput)
			//{
			//	parsingOptions.UnresolvedPath += ParsingOptionsOnUnresolvedPath;
			//}

			//var formatterService = new MorestachioFormatterService();
			//var formatters = _templateServiceProvider.ObtainFormatters();

			//foreach (var formatter in formatters)
			//{
			//	var morestachioFormatterModel = formatter.CreateFormatter();
			//	formatterService.Add(morestachioFormatterModel.Function, morestachioFormatterModel);
			//}

			
			MorestachioDocumentInfo extendedParseInformation;

			try
			{
				extendedParseInformation = Parser.ParseWithOptions(parsingOptions);
			}
			catch (Exception e)
			{
				return new GeneratedTemplateInfos()
				{
					Errors = new IMorestachioError[]
					{
						new MorestachioSyntaxError(new CharacterLocationExtended(), "Error ", "", "", e.Message), 
					}
				};
			}

			if (extendedParseInformation.Errors.Any())
			{
				var generatedTemplateInfos = new GeneratedTemplateInfos()
				{
					InferredTemplateModel = extendedParseInformation.Document,
					Errors = extendedParseInformation.Errors.ToArray()
				};
				_templateServiceProvider.OnTemplateCreated(generatedTemplateInfos);
				return generatedTemplateInfos;
			}

			var result = await dataSourceProvider.Fetch();
			if (result == null)
			{
				return null;
			}

			var generateTemplateExecute = new GeneratedTemplateInfos()
			{
				Result = await extendedParseInformation.CreateAndStringifyAsync(result),
				InferredTemplateModel = extendedParseInformation.Document
			};
			_templateServiceProvider.OnTemplateCreated(generateTemplateExecute);
			return generateTemplateExecute;
		}

		private void ParsingOptionsOnUnresolvedPath(string path, Type type)
		{
			
		}
	}
}
