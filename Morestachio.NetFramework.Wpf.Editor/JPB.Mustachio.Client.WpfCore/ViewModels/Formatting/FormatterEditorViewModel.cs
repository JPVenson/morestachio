using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using JPB.Mustachio.Client.Wpf.Core.Services;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;
using Microsoft.Win32;
using Morestachio.Formatter.Framework;

namespace JPB.Mustachio.Client.Wpf.Core.ViewModels.Formatting
{
	public class FormatterEditorViewModel : AsyncViewModelBase
	{
		private readonly TemplateServiceProvider _service;

		public FormatterEditorViewModel(TemplateServiceProvider service)
		{
			_service = service;
			RemoveFormatterCommand =
				new DelegateCommand<FormatterGroup>(RemoveFormatterExecute, CanRemoveFormatterExecute);
			AddFormatterCommand = new DelegateCommand(AddFormatterExecute, CanAddFormatterExecute);
			LoadFromExternalCommand = new DelegateCommand(LoadFromExternalExecute, CanLoadFromExternalExecute);
			FormatterGroups = new ThreadSaveObservableCollection<FormatterGroup>();

			var dllToLoad = Path.Combine(Directory.GetCurrentDirectory(), "Morestachio.Formatter.Linq.dll");
			if (File.Exists(dllToLoad))
			{
				LoadAndAddFromExternal(dllToLoad);
			}
		}

		private void RemoveFormatterExecute(FormatterGroup sender)
		{
			FormatterGroups.Remove(sender);
		}

		private bool CanRemoveFormatterExecute(FormatterGroup sender)
		{
			return true;
		}

		public ThreadSaveObservableCollection<FormatterGroup> FormatterGroups { get; set; }
		public DelegateCommand<FormatterGroup> RemoveFormatterCommand { get; }
		public DelegateCommand AddFormatterCommand { get; private set; }
		public DelegateCommand LoadFromExternalCommand { get; private set; }

		private void LoadFromExternalExecute(object sender)
		{
			var fileLoader = new OpenFileDialog();
			fileLoader.Multiselect = false;
			fileLoader.Filter = "assembly|*.dll";
			if (fileLoader.ShowDialog() == true)
			{
				var dllToLoad = fileLoader.FileName;
				if (File.Exists(dllToLoad))
				{
					LoadAndAddFromExternal(dllToLoad);
				}
			}
		}

		private string _probePath;

		private void LoadAndAddFromExternal(string dllToLoad)
		{
			_probePath = Path.GetDirectoryName(dllToLoad);
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
			try
			{
				var group = new FormatterGroup();
				group.Name = Path.GetFileName(dllToLoad);
				FormatterGroups.Add(group);

				var assembly = Assembly.Load(File.ReadAllBytes(dllToLoad));
				var enumerable = assembly.GetTypes().Where(e => e.GetMethods().Any(w => w.CustomAttributes.Any(f =>
					f.AttributeType.Namespace == typeof(MorestachioFormatterAttribute).Namespace &&
					f.AttributeType.Name == typeof(MorestachioFormatterAttribute).Name)));

				foreach (var type in enumerable)
				{
					foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
					{
						foreach (var customAttribute in method.GetCustomAttributes<MorestachioFormatterAttribute>())
						{
							if (customAttribute != null)
							{
								var name = customAttribute.Name;
								var description = customAttribute.Description;
								var parameterType = method.GetParameters().FirstOrDefault()?.ParameterType;
								var outputType = customAttribute.OutputType;
								if ((object) outputType == null)
								{
									outputType = method.ReturnType;
								}

								var array = method
									.GetCustomAttributes<MorestachioFormatterInputAttribute>().Select(e =>
										new InputDescription(e.Description, e.OutputType, e.Example)).ToArray();
								var returnHint = customAttribute.ReturnHint;
								var function = method;
								var formatterViewModel = new ExternalFormatterViewModel(_service, dllToLoad, function);
								formatterViewModel.Name = name;
								formatterViewModel.InputType = parameterType;
								formatterViewModel.OutputType = outputType;
								group.Formatters.Add(formatterViewModel);
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				MessageBox.Show(e.Message, $"Could not load dll '{dllToLoad}'");
			}
			finally
			{
				AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
			}
		}

		private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			var assamName = new AssemblyName(args.Name);

			var path = Path.Combine(_probePath, assamName.Name + ".dll");

			if (File.Exists(path))
			{
				return Assembly.Load(File.ReadAllBytes(path));
			}

			return null;
		}

		private bool CanLoadFromExternalExecute(object sender)
		{
			return true;
		}

		private void AddFormatterExecute(object sender)
		{
			var formatterViewModel = new FormatterViewModel(_service);
			//Formatters.Add(formatterViewModel);
		}

		private bool CanAddFormatterExecute(object sender)
		{
			return true;
		}
	}
}