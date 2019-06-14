using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using JPB.Mustachio.Client.Wpf.Services;
using JPB.WPFBase.MVVM.DelegateCommand;
using JPB.WPFBase.MVVM.ViewModel;
using Microsoft.Win32;
using Morestachio.Formatter.Framework;

namespace JPB.Mustachio.Client.Wpf.ViewModels
{
	public class FormatterEditorViewModel : AsyncViewModelBase
	{
		private readonly TemplateServiceProvider _service;

		public FormatterEditorViewModel(TemplateServiceProvider service)
		{
			_service = service;
			RemoveFormatterCommand =
				new DelegateCommand<FormatterViewModel>(RemoveFormatterExecute, CanRemoveFormatterExecute);
			AddFormatterCommand = new DelegateCommand(AddFormatterExecute, CanAddFormatterExecute);
			LoadFromExternalCommand = new DelegateCommand(LoadFromExternalExecute, CanLoadFromExternalExecute);
		}

		private void RemoveFormatterExecute(FormatterViewModel sender)
		{
			Formatters.Remove(sender);
			sender.Dispose();
		}

		private bool CanRemoveFormatterExecute(FormatterViewModel sender)
		{
			return true;
		}

		public ThreadSaveObservableCollection<FormatterViewModel> Formatters { get; set; }

		public DelegateCommand RemoveFormatterCommand { get; }
		public DelegateCommand AddFormatterCommand { get; private set; }
		public DelegateCommand LoadFromExternalCommand { get; private set; }

		private void LoadFromExternalExecute(object sender)
		{
			var fileLoader = new OpenFileDialog();
			fileLoader.Multiselect = false;
			fileLoader.Filter = "*.dll";
			if (fileLoader.ShowDialog() == true)
			{
				var dllToLoad = fileLoader.FileName;
				if (File.Exists(dllToLoad))
				{
					LoadAndAddFromExternal(dllToLoad);
				}
			}
		}

		private void LoadAndAddFromExternal(string dllToLoad)
		{
			AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
			try
			{
				var assembly = Assembly.Load(File.ReadAllBytes(dllToLoad));
				var enumerable = assembly.GetTypes().Where(e => e.GetMethods().Any(w => w.CustomAttributes.Any(f =>
					f.AttributeType.Namespace == typeof(MorestachioFormatterAttribute).Namespace &&
					f.AttributeType.Name == typeof(MorestachioFormatterAttribute).Name)));

				foreach (var type in enumerable)
				{
					foreach (MethodInfo method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
					{
						foreach (MorestachioFormatterAttribute customAttribute in method.GetCustomAttributes<MorestachioFormatterAttribute>())
						{
							if (customAttribute != null)
							{
								string name = customAttribute.Name;
								string description = customAttribute.Description;
								Type parameterType = method.GetParameters().FirstOrDefault()?.ParameterType;
								Type outputType = customAttribute.OutputType;
								if ((object)outputType == null)
									outputType = method.ReturnType;
								InputDescription[] array = method
									.GetCustomAttributes<MorestachioFormatterInputAttribute>().Select(e =>
										new InputDescription(e.Description, e.OutputType, e.Example)).ToArray();
								string returnHint = customAttribute.ReturnHint;
								MethodInfo function = method;
								var formatterViewModel = new ExternalFormatterViewModel(_service, dllToLoad, function);
								formatterViewModel.Name = name;
								formatterViewModel.InputType = parameterType;
								formatterViewModel.OutputType = outputType;
								Formatters.Add(formatterViewModel);
							}
						}
					}
				}
			}
			finally
			{
				AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
			}
		}

		private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
		{
			if (File.Exists(args.Name))
			{
				return Assembly.Load(File.ReadAllBytes(args.Name));
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
			Formatters.Add(formatterViewModel);
		}

		private bool CanAddFormatterExecute(object sender)
		{
			return true;
		}
	}
}