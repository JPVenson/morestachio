using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using JPB.Mustachio.Client.Wpf.Services;
using JPB.Mustachio.Client.Wpf.Services.AvalonEditTextMarker;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;
using Morestachio.ParserErrors;

namespace JPB.Mustachio.Client.Wpf.ViewModels
{
	public class TemplateEditorViewModel : AsyncViewModelBase
	{
		private readonly TemplateServiceProvider _templateServiceProvider;

		public TemplateEditorViewModel(TemplateServiceProvider templateServiceProvider)
		{
			_templateServiceProvider = templateServiceProvider;
			TemplateChangedCommand = new DelegateCommand(TemplateChangedExecute, CanTemplateChangedExecute);
			_template = new TextDocument();
			_template.TextChanged += _template_TextChanged;
			var service = _template.ServiceProvider.GetService(typeof(IServiceContainer)) as IServiceContainer;
			TextMarkerService = new TextMarkerService(Template);

			service.AddService(typeof(ITextMarkerService), TextMarkerService);
			string path = null;

			if (File.Exists("../../MorestachioHightlight.xml"))
			{
				path = "../../MorestachioHightlight.xml";
			}
			else if (File.Exists("MorestachioHightlight.xml"))
			{
				path = "MorestachioHightlight.xml";
			}

			if (path != null)
			{
				var xshdSyntaxDefinition = HighlightingLoader.Load(
					new XmlTextReader(new FileStream(path, FileMode.Open)), null);
				HighlightingManager.Instance.RegisterHighlighting("Morestachio", new string[0], xshdSyntaxDefinition);
				MorestachioSyntax = xshdSyntaxDefinition;
			}

			_templateServiceProvider.TemplateCreated += TemplateServiceProviderOnTemplateCreated;
		}

		private void TemplateServiceProviderOnTemplateCreated(object sender, GeneratedTemplateInfos e)
		{
			BeginThreadSaveAction(() =>
			{
				TextMarkerService.RemoveAll(f => true);
				foreach (var morestachioError in e.Errors ?? new IMorestachioError[0])
				{
					var charOffset = Template.GetOffset(morestachioError.Location.Line, Math.Max(morestachioError.Location.Snipped.Snipped.Length - morestachioError.Location.Character - 1, morestachioError.Location.Character - 1));
					var textMarker = TextMarkerService.Create(charOffset, morestachioError.Location.Snipped.Snipped.Length);
					textMarker.MarkerColor = Colors.Red;
					textMarker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
				}
			});
		}

		private void _template_TextChanged(object sender, EventArgs e)
		{
			_templateServiceProvider.OnTemplateChanged(Template.Text);
		}

		public event EventHandler<string> TemplateChanged;
		public TextMarkerService TextMarkerService { get; set; }

		public DelegateCommand TemplateChangedCommand { get; private set; }

		private void TemplateChangedExecute(object sender)
		{
			OnTemplateChanged(Template.Text);
		}

		private bool CanTemplateChangedExecute(object sender)
		{
			return true;
		}

		private TextDocument _template;
		private IHighlightingDefinition _morestachioSyntax;
		private TextArea _textArea;

		public IHighlightingDefinition MorestachioSyntax
		{
			get { return _morestachioSyntax; }
			set
			{
				SendPropertyChanging(() => MorestachioSyntax);
				_morestachioSyntax = value;
				SendPropertyChanged(() => MorestachioSyntax);
			}
		}

		public TextDocument Template
		{
			get { return _template; }
			set
			{
				SendPropertyChanging(() => Template);
				_template = value;
				SendPropertyChanged(() => Template);
			}
		}

		public TextArea TextArea
		{
			get { return _textArea; }
			set
			{
				_textArea = value;
				TextArea.TextView.BackgroundRenderers.Add(TextMarkerService);
				TextArea.TextView.LineTransformers.Add(TextMarkerService);
			}
		}

		protected virtual void OnTemplateChanged(string e)
		{
			TemplateChanged?.Invoke(this, e);
		}
	}
}
