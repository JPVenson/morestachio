using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using JPB.Mustachio.Client.Contacts.Contracts;
using JPB.Mustachio.Client.Wpf.Services;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Morestachio;
using Morestachio.Formatter.Framework;
using Morestachio.Framework;
using Morestachio.Helper;

namespace JPB.Mustachio.Client.Wpf.ViewModels
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
				var generatedTemplateInfos = await GenerateTemplateExecute(_templateServiceProvider.LastTemplate,
					_templateServiceProvider.LastProvider);
				if (_generateTemplateCallGuardFlag)
				{
					_generateTemplateCallGuardFlag = false;
					GenerateTemplate();
					return;
				}

				GeneratedTemplate = generatedTemplateInfos;
			});
		}

		public async Task<GeneratedTemplateInfos> GenerateTemplateExecute(string template, IDataSourceProvider dataSourceProvider)
		{
			if (template == null)
			{
				return null;
			}

			var parsingOptions = new ParserOptions(template,
				() => new MemoryStream(),
				Encoding.Default, false, true);

			var formatterService = new MorestachioFormatterService();
			var formatters = _templateServiceProvider.ObtainFormatters();

			foreach (var formatter in formatters)
			{
				formatterService.GlobalFormatterModels.Add(formatter.CreateFormatter());
			}
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
						new MorestachioSyntaxError(new Tokenizer.CharacterLocation(), "Error ", "", "", e.Message), 
					}
				};
			}

			if (extendedParseInformation.Errors.Any())
			{
				return new GeneratedTemplateInfos()
				{
					InferredTemplateModel = extendedParseInformation.Document,
					Errors = extendedParseInformation.Errors.ToArray()
				};
			}

			var result = await dataSourceProvider.Fetch();
			if (result == null)
			{
				return null;
			}
			
			return new GeneratedTemplateInfos()
			{
				Result = extendedParseInformation.Create(result).Stringify(true, Encoding.Default),
				InferredTemplateModel = extendedParseInformation.Document
			};
		}
	}
}
