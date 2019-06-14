using System;
using System.Collections.ObjectModel;
using System.Windows;
using JPB.Mustachio.Client.Contacts.Contracts;
using JPB.Mustachio.Client.Data.CSharp.DataSourceProvider;
using JPB.Mustachio.Client.Wpf.Services;
using JPB.WPFBase.MVVM.ViewModel;

namespace JPB.Mustachio.Client.Wpf.ViewModels
{
	public class DataEditorViewModel : AsyncViewModelBase
	{
		private readonly TemplateServiceProvider _templateServiceProvider;

		public DataEditorViewModel(TemplateServiceProvider templateServiceProvider)
		{
			_templateServiceProvider = templateServiceProvider;
			DataSourceProviders = new ObservableCollection<IDataSourceProvider>();
			DataSourceProviders.Add(new CSharpCompilerViewModel());

			foreach (var dataSourceProvider in DataSourceProviders)
			{
				var typeName = dataSourceProvider.GetType().Assembly.GetName().Name;
				App.Current.Resources.MergedDictionaries.Add(new ResourceDictionary()
				{
					Source = new Uri($"pack://application:,,,/{typeName};component/Resources/DataTemplates.xaml")
				});
			}
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
