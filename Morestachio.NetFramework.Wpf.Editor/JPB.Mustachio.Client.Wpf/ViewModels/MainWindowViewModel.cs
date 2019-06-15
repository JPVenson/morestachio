using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ICSharpCode.AvalonEdit.Utils;
using JPB.Mustachio.Client.Wpf.Services;
using JPB.Mustachio.Client.Wpf.ViewModels.Formatting;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;
using Newtonsoft.Json;

namespace JPB.Mustachio.Client.Wpf.ViewModels
{
	public class MainWindowViewModel : AsyncViewModelBase
	{
		public MainWindowViewModel()
		{
			var service = new TemplateServiceProvider();
			TemplateResultViewModel = new TemplateResultViewModel(service);
			TemplateEditorViewModel = new TemplateEditorViewModel(service);
			DataEditorViewModel = new DataEditorViewModel(service);
			FormatterEditorViewModel = new FormatterEditorViewModel(service);
			StoreData = new ThreadSaveObservableCollection<StoreMetaModel>();


			LoadStoreDataCommand = new DelegateCommand<StoreMetaModel>(LoadStoreDataExecute, CanLoadStoreDataExecute);
			SaveStoreDataCommand = new DelegateCommand(SaveStoreDataExecute, CanSaveStoreDataExecute);
			if (!Directory.Exists(StorePath))
			{
				Directory.CreateDirectory(StorePath);
			}
			EnumerateStoreFiles();
		}

		private static string StorePath = Path.Combine(Directory.GetCurrentDirectory(), "Store\\");

		private void EnumerateStoreFiles()
		{
			StoreData.Clear();
			var enumerateFiles = Directory.EnumerateFiles(StorePath, "*.mustachioPackage");
			foreach (var enumerateFile in enumerateFiles)
			{
				StoreData.Add(new StoreMetaModel()
				{
					FileName = enumerateFile,
					Name = Path.GetFileName(enumerateFile)
				});
			}
		}

		private DataEditorViewModel _dataEditorViewModel;
		private TemplateEditorViewModel _templateEditorViewModel;
		private TemplateResultViewModel _templateResultViewModel;
		private FormatterEditorViewModel _formatterEditorViewModel;
		public ThreadSaveObservableCollection<StoreMetaModel> StoreData { get; set; }

		public DelegateCommand<StoreMetaModel> LoadStoreDataCommand { get; private set; }
		public DelegateCommand SaveStoreDataCommand { get; private set; }

		private void SaveStoreDataExecute(object sender)
		{
			using (var fs = new FileStream(Path.Combine(StorePath, DateTime.Now.ToString("s").Replace(':',' ') + ".mustachioPackage"), 
				FileMode.Create))
			{
				var jsonSerializer = JsonSerializer.Create();
				var storeModel = new StoreModel();

				storeModel.MustachioTemplate = TemplateEditorViewModel.Template;
				if (DataEditorViewModel.SelectedDataSourceProvider != null)
				{
					storeModel.CodeProviderType = DataEditorViewModel.SelectedDataSourceProvider.Name;
					storeModel.CodeProviderData = DataEditorViewModel.SelectedDataSourceProvider.StoreProviderData();
				}

				using (var streamWriter = new StreamWriter(fs))
				{
					using (var jsonTextWriter = new JsonTextWriter(streamWriter))
					{
						jsonSerializer.Serialize(jsonTextWriter, storeModel);
					}
				}
			}
			EnumerateStoreFiles();
		}

		private bool CanSaveStoreDataExecute(object sender)
		{
			return true;
		}

		private void LoadStoreDataExecute(StoreMetaModel sender)
		{
			using (var fs = new FileStream(sender.FileName, FileMode.Open))
			{
				var jsonSerializer = JsonSerializer.Create();
				StoreModel storeModel;
				using (var streamReader = new StreamReader(fs))
				{
					using (var jsonTextReader = new JsonTextReader(streamReader))
					{
						storeModel = jsonSerializer.Deserialize<StoreModel>(jsonTextReader);
					}
				}

				TemplateEditorViewModel.Template = storeModel.MustachioTemplate;
				DataEditorViewModel.SelectedDataSourceProvider =
					DataEditorViewModel.DataSourceProviders.FirstOrDefault(f => f.Name == storeModel.CodeProviderType);
				if (DataEditorViewModel.SelectedDataSourceProvider != null)
				{
					DataEditorViewModel.SelectedDataSourceProvider.StoreProviderData(storeModel.CodeProviderData);
				}
			}
		}

		private bool CanLoadStoreDataExecute(StoreMetaModel sender)
		{
			return true;
		}
		
		public FormatterEditorViewModel FormatterEditorViewModel
		{
			get { return _formatterEditorViewModel; }
			set
			{
				SendPropertyChanging(() => FormatterEditorViewModel);
				_formatterEditorViewModel = value;
				SendPropertyChanged(() => FormatterEditorViewModel);
			}
		}

		public TemplateResultViewModel TemplateResultViewModel
		{
			get { return _templateResultViewModel; }
			set
			{
				SendPropertyChanging(() => TemplateResultViewModel);
				_templateResultViewModel = value;
				SendPropertyChanged(() => TemplateResultViewModel);
			}
		}
		public TemplateEditorViewModel TemplateEditorViewModel
		{
			get { return _templateEditorViewModel; }
			set
			{
				SendPropertyChanging(() => TemplateEditorViewModel);
				_templateEditorViewModel = value;
				SendPropertyChanged(() => TemplateEditorViewModel);
			}
		}

		public DataEditorViewModel DataEditorViewModel
		{
			get { return _dataEditorViewModel; }
			set
			{
				SendPropertyChanging(() => DataEditorViewModel);
				_dataEditorViewModel = value;
				SendPropertyChanged(() => DataEditorViewModel);
			}
		}
	}
}
