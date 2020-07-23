using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Predefined;
using Morestachio.Framework;
using Morestachio.Framework.Expression;
using Morestachio.Helper;
using Morestachio.Linq;
using Morestachio.Newtonsoft.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Morestachio.Runner
{
	public class Program
	{
		static int Main(string[] args)
		{
			var prog = new Program();
			var result =  prog.RootHandler(args);
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

				public class MethodFunction
				{
					public string FormatterName { get; set; }
					public Type Returns { get; set; }
					public string Description { get; set; }
				}

				public IList<MethodParameter> Parameters { get; set; }

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

		public static IDictionary<string, object> GetMorestachioFormatterDocumentation()
		{
			var values = new Dictionary<string, object>();
			var formatterTypes = new List<FormatterData>();
			values["Data"] = formatterTypes;

			var formatterService = ContextObject.DefaultFormatter as MorestachioFormatterService;
			foreach (var formatterServiceFormatter in formatterService
				.Formatters
				.SelectMany(f => f.Value)
				.GroupBy(e => e.Function.DeclaringType))
			{
				var formatter = new FormatterData();
				formatter.DeclaringType = formatterServiceFormatter.Key;
				
				var methods = new List<FormatterData.FormatterMethod>();
				formatter.Methods = methods;
				foreach (var formatterMethod in formatterServiceFormatter
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
							function.FormatterName = string.IsNullOrWhiteSpace(fncGrouped.Name) ? "{Null}" : fncGrouped.Name ;
							var mOperator = MorestachioOperator.Operators.FirstOrDefault(f => ("op_" + f.Key.ToString()) == function.FormatterName).Value;
							if (mOperator != null)
							{
								function.FormatterName = mOperator.OperatorText;
							}
							function.Returns = fncGrouped.OutputType ?? fncGrouped.Function.ReturnType;
							function.Description = fncGrouped.Description;
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
				formatterTypes.Add(formatter);
			}

			return values;
		}

		public int RootHandler(string[] args)
		{
			return GetCommands().Invoke(args);
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

		public void CloseMessage()
		{
			if (BuildLog == null)
			{
				Console.WriteLine();
				Console.ReadKey();
			}
		}

		private int Invoke(SourceTypes sourceType, string sourceData, string templateData, string targetPath, string buildLog, string sourceDataNetType, string sourceDataNetFunction)
		{
			try
			{
				ContextObject.DefaultFormatter.AddFromType(typeof(DynamicLinq));
				if (buildLog != null)
				{
					BuildLog = new StreamWriter(new FileStream(buildLog, FileMode.OpenOrCreate));
				}
				WriteLine("Take '" + sourceType + "' from '" + sourceData + "', put it into '" + templateData + "' and store the result at '" + targetPath + "'");

				if (!File.Exists(sourceData))
				{
					WriteLine($"The source file at '{sourceData}' does not exist");
					CloseMessage();
					return -1;
				}
				if (!File.Exists(templateData))
				{
					WriteLine($"The template file at '{templateData}' does not exist");
					CloseMessage();
					return -1;
				}

				object data = null;
				IValueResolver resolver = null;
				try
				{
					switch (sourceType)
					{
						case SourceTypes.Json:
							data = JsonConvert.DeserializeObject(File.ReadAllText(sourceData));
							resolver = new JsonNetValueResolver();
							break;
						case SourceTypes.Xml:
							throw new NotSupportedException("The XML deserialisation is currently not supported");
							break;
						case SourceTypes.NetFunction:
							if (sourceDataNetType == null)
							{
								WriteLine("Expected the --source-data-net-type argument to contain an valid type");
								CloseMessage();
								return -1;
							}
							if (sourceDataNetFunction == null)
							{
								WriteLine("Expected the --source-data-net-function argument to contain an valid type");
								CloseMessage();
								return -1;
							}

							var assembly = Assembly.LoadFrom(sourceData);
							var type = assembly.GetType(sourceDataNetType);

							if (type == null)
							{
								WriteLine($"The type '{sourceDataNetType}' was not found");
								CloseMessage();
								return -1;
							}

							var method = type.GetMethods(BindingFlags.Public | BindingFlags.Static)
								.Where(e => e.ReturnType != typeof(void))
								.Where(e => e.GetParameters().Length == 0)
								.FirstOrDefault(e => e.Name.Equals(sourceDataNetFunction));

							if (method == null)
							{
								WriteLine($"The method '{sourceDataNetFunction}' was not found in type '{type}'. Expected to find a public static void Method without parameters.");
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
					WriteLine("Error while loading the source file");
					WriteLine("----");
					WriteLine(e.ToString());
					CloseMessage();
					return -1;
				}

				var template = File.ReadAllText(templateData);
				using (var sourceFs = new FileStream(targetPath, FileMode.OpenOrCreate))
				{
					var document = Parser.ParseWithOptions(new ParserOptions(template, () => sourceFs, Encoding.UTF8, true)
					{
						Timeout = TimeSpan.FromMinutes(1),
						ValueResolver = resolver,
					});

					if (document.Errors.Any())
					{
						var sb = new StringBuilder();
						sb.AppendLine("Template Errors: ");
						foreach (var morestachioError in document.Errors)
						{
							morestachioError.Format(sb);
							sb.AppendLine();
							sb.AppendLine("----");
						}
						WriteLine(sb.ToString());
						CloseMessage();
						return -1;
					}

					document.Create(data);
					return 0;
				}
			}
			finally
			{
				BuildLog?.Dispose();
			}
		}
	}

	public enum SourceTypes
	{
		Json,
		Xml,
		NetFunction
	}
}
