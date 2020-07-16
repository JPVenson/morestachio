using System.Collections.ObjectModel;
using System.Linq;
using JPB.Mustachio.Client.Wpf.Core.ClientDataProvider;
using JPB.Mustachio.Client.Wpf.Core.Contracts;
using JPB.Mustachio.Client.Wpf.Core.Services;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.Mustachio.Client.Wpf.Core.ViewModels
{
	public class DataEditorViewModel : AsyncViewModelBase
	{
		private readonly TemplateServiceProvider _templateServiceProvider;

		public DataEditorViewModel(TemplateServiceProvider templateServiceProvider)
		{
			_templateServiceProvider = templateServiceProvider;
			DataSourceProviders = new ObservableCollection<IDataSourceProvider>();
			DataSourceProviders.Add(new JsonDataProvider());
			//DataSourceProviders.Add(new CSharpCompilerViewModel());

			foreach (var dataSourceProvider in DataSourceProviders)
			{
				var typeName = dataSourceProvider.GetType().Assembly.GetName().Name;
				//try
				//{
				//	App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
				//	{
				//		Source = new Uri($"pack://application:,,,/{typeName};component/Resources/DataTemplates.xaml")
				//	});
				//}
				//catch (Exception e)
				//{
					
				//}
			}

			SelectedDataSourceProvider = DataSourceProviders.First();
		}

		private ObservableCollection<IDataSourceProvider> _dataSourceProviders;
		private IDataSourceProvider _selectedDataSourceProvider;

		public IDataSourceProvider SelectedDataSourceProvider
		{
			get { return _selectedDataSourceProvider; }
			set
			{
				SendPropertyChanging(() => SelectedDataSourceProvider);
				_selectedDataSourceProvider = value;
				_templateServiceProvider.OnDataProviderChanged(value);
				SendPropertyChanged(() => SelectedDataSourceProvider);
			}
		}
		public ObservableCollection<IDataSourceProvider> DataSourceProviders
		{
			get { return _dataSourceProviders; }
			set
			{
				SendPropertyChanging(() => DataSourceProviders);
				_dataSourceProviders = value;
				SendPropertyChanged(() => DataSourceProviders);
			}
		}
	}
}
