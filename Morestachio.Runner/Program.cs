using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Morestachio.Formatter.Framework;
using Morestachio.Framework.Context;
using Morestachio.Framework.Context.Resolver;
using Morestachio.Framework.Expression;
using Morestachio.Helper.Localization;
using Morestachio.Helper.FileSystem;
using Morestachio.Linq;
using Morestachio.Newtonsoft.Json;
using Morestachio.Rendering;
using Newtonsoft.Json;

namespace Morestachio.Runner
{
	public class Program
	{
		static async Task<int> Main(string[] args)
		{
			var prog = new Program();
			var result = await prog.RootHandler(args);
			return result;
		}

		public Program()
		{

		}

		public class FormatterData
		{
			public Type DeclaringType { get; set; }
			public IList<FormatterMethod> Methods { get; set; }

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

				return Equals((ServicePropertyType) obj);
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
				MorestachioFormatterService formatterService,
				ICollection<ServicePropertyType> servicePropertyTypes)
			{
				var type = new ServicePropertyType();
				type.Type = csType;
				type.IsFrameworkType = true;
				servicePropertyTypes.Add(type);
				foreach (var formatterServiceFormatter in formatterService
					.Formatters
					.SelectMany(f => f.Value)
					.Where(e => e.LinkFunctionTarget)
					.Where(e => e.Function.DeclaringType == csType)
					.GroupBy(e => e.Function.DeclaringType))
				{
					type.Formatter = EnumerateFormatters(formatterServiceFormatter, true);
				}

				foreach (var propertyInfo in csType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
				{
					var prop = new ServiceProperty();
					prop.Name = propertyInfo.Name;
					
					if (propertyInfo.PropertyType.Namespace.StartsWith("Morestachio"))
					{
						prop.PropType = EnumerateObject(propertyInfo.PropertyType, formatterService, servicePropertyTypes);
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

			foreach (var formatterServiceFormatter in formatterService
				.Formatters
				.SelectMany(f => f.Value)
				.GroupBy(e => e.Function.DeclaringType))
			{
				var data = EnumerateFormatters(formatterServiceFormatter, false);
				if (data.Methods.Any())
				{
					formatterTypes.Add(data);
				}
			}

			var services = new List<ServiceData>();
			values["ServiceData"] = services;
			foreach (var service in formatterService.Services.Enumerate())
			{
				var serviceData = new ServiceData();
				serviceData.ServiceName = service.Key.Name;
				serviceData.Types = new HashSet<ServicePropertyType>();
				EnumerateObject(service.Key, formatterService, serviceData.Types);
				services.Add(serviceData);
			}

			var constants = new List<ServiceData>();
			values["ConstData"] = constants;
			foreach (var service in formatterService.Constants)
			{
				var serviceData = new ServiceData();
				serviceData.ServiceName = service.Key;
				serviceData.Types = new HashSet<ServicePropertyType>();
				EnumerateObject(service.Value.GetType(), formatterService, serviceData.Types);
				constants.Add(serviceData);
			}

			return values;
		}

		public async Task<int> RootHandler(string[] args)
		{
			return await GetCommands().InvokeAsync(args);
		}

		public RootCommand GetCommands()
		{
			var rootCommand = new RootCommand()
			{
				new Option<SourceTypes>("--source-type", "The Data string related to the --source-type")
				{
					IsRequired = true
				},
				new Option<string>("--source-data", "The Data string related to the --source-type")
				{
					IsRequired = true
				},
				new Option<string>("--source-data-net-type", "if --source-type is NetFunction then it is expected that this is the fully quallified path to the type to be executed"),
				new Option<string>("--source-data-net-function", "if --source-type is NetFunction then it is expected that this is the path to the method to be executed"),
				new Option<string>("--template-data", "The path to the template that should be processed")
				{
					IsRequired = true
				},
				new Option<string>("--target-path", "The full path including the filename where the result should be stored")
				{
					IsRequired = true
				},
				new Option<string>("--build-log", "If set a log of everything written to the console will be saved at the location and the console will never break")
				{

				},
			};
			rootCommand.Handler = CommandHandler.Create<SourceTypes, string, string, string, string, string, string>(Invoke);
			return rootCommand;
		}

		public StreamWriter BuildLog { get; set; }

		public void WriteLine(string logEntry)
		{
			BuildLog?.WriteLine(logEntry);
			Console.WriteLine(logEntry);
		}

		public void WriteErrorLine(string logEntry)
		{
			var color = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			BuildLog?.WriteLine(logEntry);
			Console.WriteLine(logEntry);
			Console.ForegroundColor = color;
		}

		public void CloseMessage()
		{
			if (BuildLog == null)
			{
				Console.WriteLine("Press any key to close the program...");
				//Console.ReadKey();
			}
		}

		public void Hr()
		{
			WriteLine(Enumerable.Repeat("-", Console.BufferWidth).Aggregate((e, f) => e + f));
		}

		public void WriteHeader()
		{
			Hr();
			WriteLine("Morestachio.Runner");
			WriteLine("\t Runner Version: " + typeof(Program).Assembly.GetName().Version);
			WriteLine("\t Morestachio Version: " + typeof(Parser).Assembly.GetName().Version);
			WriteLine("\t Morestachio.Linq Version: " + typeof(DynamicLinq).Assembly.GetName().Version);
			WriteLine("\t Morestachio.Newtonsoft.Json Version: " + typeof(JsonNetValueResolver).Assembly.GetName().Version);
			//Console.WriteLine("\t Usage:");
			//Console.WriteLine("\t\t Define a --source-type. This can be any of " 
			//                  + Enum.GetValues(typeof(SourceTypes))
			//	                  .OfType<SourceTypes>()
			//	                  .Select(f => f.ToString())
			//	                  .Aggregate((e, f) => e + "," + f));
			//Console.WriteLine($"\t\t If you are using {SourceTypes.Json} or {SourceTypes.Xml}[Currently Unsupported]");
			Hr();
		}

		IDictionary<string, PerformanceMetric> PerformanceMetrics { get; set; }

		class PerformanceMetric : IDisposable
		{
			public PerformanceMetric()
			{
				Stopwatch = Stopwatch.StartNew();
			}
			public string Name { get; set; }
			public Stopwatch Stopwatch { get; set; }

			public void Dispose()
			{
				Stopwatch.Stop();
			}
		}

		private IDisposable StartMetric(string name)
		{
			var metric = new PerformanceMetric()
			{
				Name = name
			};
			PerformanceMetrics[name] = metric;
			return metric;
		}

		private async Task<int> Invoke(SourceTypes sourceType, string sourceData, string templateData, string targetPath, string buildLog, string sourceDataNetType, string sourceDataNetFunction)
		{
			PerformanceMetrics = new Dictionary<string, PerformanceMetric>();
			using (StartMetric("Whole Operation"))
			{
				try
				{
					ContextObject.DefaultFormatter.AddFromType(typeof(DynamicLinq));
					if (buildLog != null)
					{
						BuildLog = new StreamWriter(new FileStream(buildLog, FileMode.OpenOrCreate));
					}

					WriteHeader();
					WriteLine("- Take '" + sourceType + "' from '" + sourceData + "', put it into '" + templateData + "' and store the result at '" + targetPath + "'");

					if (!File.Exists(sourceData))
					{
						WriteErrorLine($"- The source file at '{sourceData}' does not exist");
						CloseMessage();
						return -1;
					}
					if (!File.Exists(templateData))
					{
						WriteErrorLine($"- The template file at '{templateData}' does not exist");
						CloseMessage();
						return -1;
					}

					object data = null;
					IValueResolver resolver = null;
					using (StartMetric("Get Data"))
					{
						try
						{
							switch (sourceType)
							{
								case SourceTypes.Json:
									WriteLine($"- Load file '{sourceData}' and parse Json from it");
									data = JsonConvert.DeserializeObject(File.ReadAllText(sourceData));
									resolver = new JsonNetValueResolver();
									break;
								case SourceTypes.Xml:
									throw new NotSupportedException("The XML deserialisation is currently not supported");
								case SourceTypes.NetFunction:
									Console.WriteLine($"- Load Assembly '{sourceData}', search for type '{sourceDataNetType}'" +
									                  $" and run public static object {sourceDataNetFunction}(); to obtain data");
									if (sourceDataNetType == null)
									{
										WriteErrorLine("- Expected the --source-data-net-type argument to contain an valid type");
										CloseMessage();
										return -1;
									}
									if (sourceDataNetFunction == null)
									{
										WriteErrorLine("- Expected the --source-data-net-function argument to contain an valid type");
										CloseMessage();
										return -1;
									}

									var assembly = Assembly.LoadFrom(sourceData);
									var type = assembly.GetType(sourceDataNetType);

									if (type == null)
									{
										WriteErrorLine($"- The type '{sourceDataNetType}' was not found");
										CloseMessage();
										return -1;
									}

									var method = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
										.Where(e => e.ReturnType != typeof(void))
										.Where(e => e.GetParameters().Length == 0)
										.FirstOrDefault(e => e.Name.Equals(sourceDataNetFunction));

									if (method == null)
									{
										WriteErrorLine($"- The method '{sourceDataNetFunction}' was not found in type '{type}'. Expected to find a public static void Method without parameters.");
										CloseMessage();
										return -1;
									}

									data = method.Invoke(null, null);
									//File.WriteAllText("documentation.json", JsonConvert.SerializeObject(data));
									break;
								default:
									throw new ArgumentOutOfRangeException(nameof(sourceType), sourceType, null);
							}
						}
						catch (Exception e)
						{
							WriteErrorLine("- Error while loading the source file");
							WriteErrorLine("----");
							WriteErrorLine(e.ToString());
							CloseMessage();
							return -1;
						}
					}

					string template;
					using (StartMetric("Get Template Content"))
					{
						WriteLine($"- Read all contents from file '{templateData}'");
						template = File.ReadAllText(templateData);
						WriteLine($"- Read '{template.Length}' chars from file");
					}
					using (var sourceFs = new FileStream(targetPath, FileMode.OpenOrCreate))
					{
						MorestachioDocumentInfo document;
						WriteLine("- Parse the template");
						using (StartMetric("Parse Template"))
						{
							document = await Parser.ParseWithOptionsAsync(new ParserOptions(template, () => sourceFs, Encoding.UTF8, true)
							{
								Timeout = TimeSpan.FromMinutes(1),
								ValueResolver = resolver,
							});
						}
						
						if (document.Errors.Any())
						{
							var sb = new StringBuilder();
							sb.AppendLine("- Template Errors: ");
							foreach (var morestachioError in document.Errors)
							{
								morestachioError.Format(sb);
								sb.AppendLine();
								sb.AppendLine("----");
							}
							WriteErrorLine(sb.ToString());
							CloseMessage();
							return -1;
						}

						WriteLine("- Execute the parsed template");
						using (StartMetric("Create Document"))
						{
							await document.CreateRenderer().RenderAsync(data);	
						}

						WriteLine("- Done");
						Hr();
					}
				}
				finally
				{
					BuildLog?.Dispose();
				}
			}

			WriteLine("Performance Metrics: ");
			foreach (var performanceMetric in PerformanceMetrics)
			{
				WriteLine("\t Name: " + performanceMetric.Key);
				WriteLine("\t Time: " + performanceMetric.Value.Stopwatch.Elapsed.ToString("g"));
				Hr();
			}
			CloseMessage();
			return 1;
		}
	}

	public enum SourceTypes
	{
		Json,
		Xml,
		NetFunction
	}
}
