using System;
using System.ComponentModel.Design;
using System.IO;
using System.Windows.Media;
using System.Xml;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.CodeCompletion;
using ICSharpCode.AvalonEdit.Document;
using ICSharpCode.AvalonEdit.Editing;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Highlighting.Xshd;
using JPB.Mustachio.Client.Wpf.Core.Services;
using JPB.Mustachio.Client.Wpf.Core.Services.AvalonEditTextMarker;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;
using Morestachio.ParserErrors;

namespace JPB.Mustachio.Client.Wpf.Core.ViewModels
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
		CompletionWindow _completionWindow;


		private void TextArea_TextEntered(object sender, System.Windows.Input.TextCompositionEventArgs e)
		{
			if (TextArea.Caret.Offset < 2)
			{
				return;
			}
			var caret = Template.GetText(TextArea.Caret.Offset - 2, 2);
			if (caret == "{{")
			{
				_completionWindow = new CompletionWindow(TextArea);
				var data = _completionWindow.CompletionList.CompletionData;
				data.Add(new SyntaxCompletionData("Do Loop", @"#DO $selection_start$condition$selection_end$}}

{{/DO}}"));
				data.Add(new SyntaxCompletionData("Each", @"#EACH $selection_start$Data$selection_end$}}

{{/EACH}}"));
				data.Add(new SyntaxCompletionData("Variable", @"#VAR $selection_start$name$selection_end$ = expression}}"));	
				data.Add(new SyntaxCompletionData("If", @"#IF $selection_start$condition$selection_end$}}

{{/IF}}"));		
				data.Add(new SyntaxCompletionData("If Not", @"^IF $selection_start$condition$selection_end$}}

{{/IF}}"));
				data.Add(new SyntaxCompletionData("While", @"#WHILE $selection_start$condition$selection_end$}}

{{/WHILE}}"));
				data.Add(new SyntaxCompletionData("Scope", @"#$selection_start$path$selection_end$}}

{{/PATH}}"));
				data.Add(new SyntaxCompletionData("Partial", @"#DECLARE $selection_start$name$selection_end$}}

{{/DECLARE}}"));
				data.Add(new SyntaxCompletionData("Include Partial", @"#INCLUDE $selection_start$name$selection_end$}}"));
				data.Add(new SyntaxCompletionData("While Loop", @"#While $selection_start$condition$selection_end$}}

{{/While}}"));
				_completionWindow.Show();
				_completionWindow.Closed += delegate {
					_completionWindow = null;
				};
			}
		}

		public class SyntaxCompletionData : ICompletionData
		{
			private readonly string _text;

			public SyntaxCompletionData(string header, string text)
			{
				_text = text;
				Text = header;
				Content = header;
			}

			public const string SEL_START_PLACEHOLDER = "$selection_start$";
			public const string SEL_END_PLACEHOLDER = "$selection_end$";

			public void Complete(TextArea textArea, ISegment completionSegment, EventArgs insertionRequestEventArgs)
			{
				int startIndex = -1;
				int endIndex = -1;
				var text = _text;
				if (text.Contains(SEL_START_PLACEHOLDER))
				{
					startIndex = completionSegment.Offset + text.IndexOf(SEL_START_PLACEHOLDER);
					text = text.Replace(SEL_START_PLACEHOLDER, "");

					if (text.Contains(SEL_END_PLACEHOLDER))
					{
						endIndex = completionSegment.Offset + text.IndexOf(SEL_END_PLACEHOLDER);
						text = text.Replace(SEL_END_PLACEHOLDER, "");
					}
				}

				textArea.Document.Replace(completionSegment, text);
				
				if (startIndex != -1)
				{
					if (endIndex != -1)
					{
						textArea.Caret.Position = new TextViewPosition(textArea.Document.GetLocation(startIndex));
						textArea.Selection = Selection.Create(textArea, startIndex, endIndex);
					}
					else
					{
						textArea.Caret.Position = new TextViewPosition(textArea.Document.GetLocation(startIndex));
					}
				}
			}

			public ImageSource Image { get; }
			public string Text { get; }
			public object Content { get; }
			public object Description { get; }
			public double Priority { get; }
		}

		private void TemplateServiceProviderOnTemplateCreated(object sender, GeneratedTemplateInfos e)
		{
			BeginThreadSaveAction(() =>
			{
				TextMarkerService.RemoveAll(f => true);
				foreach (var morestachioError in e.Errors ?? new IMorestachioError[0])
				{
					//var column = Math.Max(
					//	morestachioError.Location.Snipped.Snipped.Length - morestachioError.Location.Character - 1,
					//	morestachioError.Location.Character - 1);
					var charOffset = Template.GetOffset(morestachioError.Location.Line,
						morestachioError.Location.Character - 1);
					var textMarker = TextMarkerService.Create(charOffset, 1);
					textMarker.MarkerColor = Colors.Red;
					textMarker.MarkerTypes = TextMarkerTypes.SquigglyUnderline;
				}

				if (e.InferredTemplateModel != null)
				{
					MorestachioFoldingStrategy.UpdateFolding(FoldingManager, Template, e.InferredTemplateModel);
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
				TextArea.TextEntered += TextArea_TextEntered;


				FoldingManager = FoldingManager.Install(TextArea);
			}
		}

		public FoldingManager FoldingManager { get; set; }

		protected virtual void OnTemplateChanged(string e)
		{
			TemplateChanged?.Invoke(this, e);
		}
	}
}
