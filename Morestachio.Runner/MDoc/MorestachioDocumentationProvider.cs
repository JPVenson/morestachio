using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Morestachio.Document;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Predefined;
using Morestachio.Framework.Expression;
using Morestachio.Helper;
using Morestachio.Helper.FileSystem;
using Morestachio.Helper.Localization;
using Morestachio.Util.StaticBinding;

namespace Morestachio.Runner.MDoc
{
	public static class MorestachioDocumentationProvider
	{
		public class FormatterData
		{
			public Type DeclaringType { get; set; }
			public IList<FormatterMethod> Methods { get; set; }
			public string Description { get; set; }

			public class FormatterMethod
			{
				public string MethodName { get; set; }
				public Type Returns { get; set; }
				public IList<MethodFunction> Functions { get; set; }
				public IList<MethodParameter> Parameters { get; set; }

				public class MethodFunction
				{
					public string FormatterName { get; set; }
					public Type Returns { get; set; }
					public string Description { get; set; }
					public bool IsOperator { get; set; }
					public bool IsInstanceFunction { get; set; }
				}

				public class MethodParameter
				{
					public string Name { get; set; }
					public Type Type { get; set; }
					public bool IsOptional { get; set; }
					public bool IsSourceObject { get; set; }
					public bool IsInjected { get; set; }
					public bool IsRestObject { get; set; }
				}
			}
		}

		public class ServiceData
		{
			public string ServiceName { get; set; }
			public ICollection<ServicePropertyType> Types { get; set; }
			public string Description { get; set; }
		}

		public class ServicePropertyType : IEquatable<ServicePropertyType>
		{
			public ServicePropertyType()
			{
				Properties = new List<ServiceProperty>();
				Formatter = new FormatterData();
			}

			public FormatterData Formatter { get; set; }
			public Type Type { get; set; }
			public IList<ServiceProperty> Properties { get; set; }

			public bool IsFrameworkType { get; set; }
			public string Description { get; set; }

			public bool Equals(ServicePropertyType other)
			{
				if (ReferenceEquals(null, other))
				{
					return false;
				}

				if (ReferenceEquals(this, other))
				{
					return true;
				}

				return Equals(Type, other.Type);
			}

			public override bool Equals(object obj)
			{
				if (ReferenceEquals(null, obj))
				{
					return false;
				}

				if (ReferenceEquals(this, obj))
				{
					return true;
				}

				if (obj.GetType() != this.GetType())
				{
					return false;
				}

				return Equals((ServicePropertyType)obj);
			}

			public override int GetHashCode()
			{
				return (Type != null ? Type.GetHashCode() : 0);
			}
		}

		public class ServiceProperty
		{
			public string Name { get; set; }
			public ServicePropertyType PropType { get; set; }
		}

		public static IDictionary<string, object> GetMorestachioFormatterDocumentation()
		{
			FormatterData EnumerateFormatters(IGrouping<Type, MorestachioFormatterModel> formatterServiceFormatter, bool includeInstanceMethods)
			{
				var formatter = new FormatterData();
				formatter.DeclaringType = formatterServiceFormatter.Key;

				var methods = new List<FormatterData.FormatterMethod>();
				formatter.Methods = methods;
				foreach (var formatterMethod in formatterServiceFormatter
					.Where(e => e.LinkFunctionTarget == includeInstanceMethods)
					.GroupBy(e => e.Function))
				{
					foreach (var morestachioFormatterModel in formatterMethod.GroupBy(e => e.IsGlobalFormatter))
					{
						var methodMeta = new FormatterData.FormatterMethod();
						methodMeta.MethodName = formatterMethod.Key.Name;

						var methodFunctions = new List<FormatterData.FormatterMethod.MethodFunction>();
						foreach (var fncGrouped in morestachioFormatterModel)
						{
							var function = new FormatterData.FormatterMethod.MethodFunction();
							function.FormatterName = string.IsNullOrWhiteSpace(fncGrouped.Name) ? "{Null}" : fncGrouped.Name;
							var mOperator = MorestachioOperator.Operators
								.FirstOrDefault(f => ("op_" + f.Key.ToString()) == function.FormatterName).Value;
							if (mOperator != null)
							{
								function.FormatterName = mOperator.OperatorText;
								function.IsOperator = true;
							}

							function.Returns = fncGrouped.OutputType ?? fncGrouped.Function.ReturnType;
							function.Description = fncGrouped.Description;
							function.IsInstanceFunction = fncGrouped.LinkFunctionTarget;
							methodFunctions.Add(function);
						}


						methodMeta.Functions = methodFunctions;
						var parameters = new List<FormatterData.FormatterMethod.MethodParameter>();
						var metaData = morestachioFormatterModel.First();
						methodMeta.Returns = metaData.Function.ReturnType;
						foreach (var inputDescription in metaData.MetaData)
						{
							var paramter = new FormatterData.FormatterMethod.MethodParameter();
							paramter.Name = inputDescription.Name;
							paramter.Type = inputDescription.ParameterType;
							paramter.IsOptional = inputDescription.IsOptional;
							paramter.IsSourceObject = inputDescription.IsSourceObject;
							paramter.IsInjected = inputDescription.IsInjected;
							paramter.IsRestObject = inputDescription.IsRestObject;
							parameters.Add(paramter);
						}

						methodMeta.Parameters = parameters;
						methods.Add(methodMeta);
					}
				}

				return formatter;
			}

			ServicePropertyType EnumerateObject(Type csType,
				IDictionary<string, IList<MorestachioFormatterModel>> formatters,
				ICollection<ServicePropertyType> servicePropertyTypes)
			{
				var any = servicePropertyTypes.FirstOrDefault(f => f.Type == csType);
				if (any != null)
				{
					return any;
				}

				var type = new ServicePropertyType();
				type.Type = csType;
				type.IsFrameworkType = true;
				type.Description = csType.GetCustomAttribute<MorestachioExtensionSetupAttribute>()?.Description;

				servicePropertyTypes.Add(type);
				foreach (var formatterServiceFormatter in formatters
					.SelectMany(f => f.Value)
					.Where(e => e.LinkFunctionTarget)
					.Where(e => e.Function.DeclaringType == csType)
					.GroupBy(e => e.Function.DeclaringType))
				{
					type.Formatter = EnumerateFormatters(formatterServiceFormatter, true);
				}

				foreach (var propertyInfo in csType.GetProperties())
				{
					var prop = new ServiceProperty();
					prop.Name = propertyInfo.Name;

					if (propertyInfo.PropertyType.Namespace.StartsWith("Morestachio"))
					{
						prop.PropType = EnumerateObject(propertyInfo.PropertyType, formatters, servicePropertyTypes);
					}
					else
					{
						prop.PropType = new ServicePropertyType()
						{
							Type = propertyInfo.PropertyType,
							IsFrameworkType = false
						};
					}

					type.Properties.Add(prop);
				}

				return type;
			}

			var values = new Dictionary<string, object>();
			var formatterTypes = new List<FormatterData>();
			values["FormatterData"] = formatterTypes;

			var parserOptions = new ParserOptions();

			parserOptions
				.RegisterFileSystem(() => new FileSystemService())
				.RegisterLocalizationService(() => new MorestachioLocalizationService());

			var formatterService = parserOptions.Formatters as MorestachioFormatterService;

			var sourceServices = ObjectFormatter.Combine(formatterService.Services.Enumerate(), MorestachioFormatterService.Default.Services.Enumerate());
			var sourceFormatters = ObjectFormatter.Combine(formatterService.Formatters,
				(MorestachioFormatterService.Default as MorestachioFormatterService).Formatters);
			var sourceConstants = ObjectFormatter.Combine(formatterService.Constants,
				MorestachioFormatterService.Default.Constants);

			foreach (var formatterServiceFormatter in sourceFormatters
				.SelectMany(f => f.Value)
				.GroupBy(e => e.Function.DeclaringType))
			{
				var data = EnumerateFormatters(formatterServiceFormatter, false);
				data.Description = data.DeclaringType.GetCustomAttribute<MorestachioExtensionSetupAttribute>()
					?.Description;
				if (data.Methods.Any())
				{
					formatterTypes.Add(data);
				}
			}

			var services = new List<ServiceData>();
			values["ServiceData"] = services;
			foreach (var service in sourceServices)
			{
				var serviceData = new ServiceData();
				serviceData.ServiceName = service.Key.GetCustomAttribute<ServiceNameAttribute>()?.Name ?? service.Key.Name;
				serviceData.Types = new HashSet<ServicePropertyType>();
				serviceData.Description = service.Key.GetCustomAttribute<MorestachioExtensionSetupAttribute>()?.Description;
				EnumerateObject(service.Key, sourceFormatters, serviceData.Types);
				services.Add(serviceData);
			}

			var constants = new List<ServiceData>();
			values["ConstData"] = constants;
			foreach (var service in sourceConstants)
			{
				var serviceData = new ServiceData();
				serviceData.ServiceName = service.Key;
				serviceData.Types = new HashSet<ServicePropertyType>();
				if (service.Value is Static typeAccessor)
				{
					serviceData.Description = typeAccessor.Type.GetCustomAttribute<MorestachioExtensionSetupAttribute>()?.Description;
					EnumerateObject(typeAccessor.Type, sourceFormatters, serviceData.Types);
				}
				else if (service.Value is Type staticType)
				{
					serviceData.Description = staticType.GetCustomAttribute<MorestachioExtensionSetupAttribute>()?.Description;
					EnumerateObject(staticType, sourceFormatters, serviceData.Types);
				}
				else
				{
					var csType = service.Value.GetType();
					serviceData.Description = csType.GetCustomAttribute<MorestachioExtensionSetupAttribute>()?.Description;
					EnumerateObject(csType, sourceFormatters, serviceData.Types);
				}


				constants.Add(serviceData);
			}

			return values;
		}
	}
}
