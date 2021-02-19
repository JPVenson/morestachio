using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Morestachio.Document;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Formatter.Framework.Converter;
using Morestachio.Helper;
using Morestachio.Helper.Logging;
using Morestachio.Util.Sealing;
#if ValueTask
using ObjectPromise = System.Threading.Tasks.ValueTask<object>;
#else
using ObjectPromise = System.Threading.Tasks.Task<object>;
#endif

namespace Morestachio.Formatter.Framework
{
	/// <summary>
	///     The Formatter service that can be used to interpret the Native C# formatter.
	///     To use this kind of formatter you must create a public static class where all formatting functions are located.
	///     Then create a public static function that accepts n arguments of the type you want to format. For Example:
	/// <example>
	///		If the formatter should be only used for int formatting and the argument will always be a string you have to create
	///     a function that has this header.
	///     It must not return a value.
	///     The functions must be annotated with the MorestachioFormatter attribute
	/// </example>
	///     
	/// </summary>
	public class MorestachioFormatterService : SealedBase, IMorestachioFormatterService
	{
		internal static readonly Regex ValidateFormatterNameRegEx =
			new Regex("^[a-zA-Z]{1}[a-zA-Z0-9_]*$", RegexOptions.Compiled);

		/// <summary>
		///     Initializes a new instance of the <see cref="MorestachioFormatterService" /> class.
		/// </summary>
		public MorestachioFormatterService(bool useCache = false)
		{
			Formatters = new Dictionary<string, IList<MorestachioFormatterModel>>();
			ValueConverter = new List<IFormatterValueConverter>
			{
				NumberConverter.Instance
			};
			DefaultConverter = GenericTypeConverter.Instance;
			ServiceCollectionAccess = new Dictionary<Type, object>
			{
				{typeof(IMorestachioFormatterService), this}
			};
			if (useCache)
			{
				Cache = new ConcurrentDictionary<FormatterCacheCompareKey, FormatterCache?>();
			}
		}

		/// <summary>
		///		Container for Services
		/// </summary>
		protected IDictionary<Type, object> ServiceCollectionAccess { get; }

		/// <summary>
		///     Gets the gloabl formatter that are used always for any formatting run.
		/// </summary>
		public IDictionary<string, IList<MorestachioFormatterModel>> Formatters { get; }

		/// <summary>
		///     The fallback Converter that should convert all known mscore lib types
		/// </summary>
		public IFormatterValueConverter DefaultConverter { get; set; }

		/// <summary>
		///     List of all Value Converters that can be used to convert formatter arguments
		/// </summary>
		public ICollection<IFormatterValueConverter> ValueConverter { get; }

		/// <inheritdoc />
		public bool AllParametersAllDefaultValue { get; set; }

		/// <inheritdoc />
		[Obsolete("The Formatter name must now always be in the exact casing as the given name", true)]
		public StringComparison FormatterNameCompareMode { get; set; } = StringComparison.Ordinal;

		/// <summary>
		/// 
		/// </summary>
		public FormatterServiceExceptionHandling ExceptionHandling { get; set; }

		/// <inheritdoc />
		public IReadOnlyDictionary<Type, object> ServiceCollection
		{
			get { return new ReadOnlyDictionary<Type, object>(ServiceCollectionAccess); }
		}

		/// <inheritdoc />
		public void AddService<T, TE>(TE service) where TE : T
		{
			ServiceCollectionAccess[typeof(TE)] = service;
		}

		/// <inheritdoc />
		public void AddService<T>(T service)
		{
			ServiceCollectionAccess[typeof(T)] = service;
		}

		/// <inheritdoc />
		public void AddService<T, TE>(Func<TE> serviceFactory) where TE : T
		{
			ServiceCollectionAccess[typeof(TE)] = serviceFactory;
		}

		/// <inheritdoc />
		public void AddService<T>(Func<T> serviceFactory)
		{
			ServiceCollectionAccess[typeof(T)] = serviceFactory;
		}

		/// <inheritdoc />
		public object GetService(Type serviceType)
		{
			var tryInvokeService = Framework.ServiceCollection.TryInvokeService(ServiceCollectionAccess,
				serviceType, out var service);
			return service;
		}

		/// <inheritdoc />
		public IEnumerable<MorestachioFormatterModel> Filter(Func<MorestachioFormatterModel, bool> filter)
		{
			return Formatters.Values.SelectMany(f => f.Where(filter));
		}

		/// <inheritdoc />
		public virtual MorestachioFormatterModel Add(MethodInfo method,
			MorestachioFormatterAttribute morestachioFormatterAttribute)
		{
			morestachioFormatterAttribute.ValidateFormatter(method);
			var arguments = morestachioFormatterAttribute.GetParameters(method);

			var morestachioFormatterModel = new MorestachioFormatterModel(morestachioFormatterAttribute.Name,
				morestachioFormatterAttribute.Description,
				arguments.FirstOrDefault(e => e.IsSourceObject)?.ParameterType ?? typeof(object),
				morestachioFormatterAttribute.OutputType ?? method.ReturnType,
				method.GetCustomAttributes<MorestachioFormatterInputAttribute>()
					.Select(e => new InputDescription(e.Description, e.OutputType, e.Example)).ToArray(),
				morestachioFormatterAttribute.ReturnHint,
				method,
				new MultiFormatterInfoCollection(arguments),
				!morestachioFormatterAttribute.IsSourceObjectAware,
				morestachioFormatterAttribute.LinkFunctionTarget);
			var name = morestachioFormatterAttribute.Name ?? "{NULL}";

			if (!Formatters.TryGetValue(name, out var formatters))
			{
				formatters = new List<MorestachioFormatterModel>();
				Formatters[name] = formatters;
			}

			formatters.Add(morestachioFormatterModel);
			return morestachioFormatterModel;
		}

		/// <summary>
		///		The cache for Formatter calls
		/// </summary>

		public IDictionary<FormatterCacheCompareKey, FormatterCache?> Cache { get; set; }

		/// <summary>
		///     Writes the specified log.
		/// </summary>
		[Conditional("LOG")]
		public void Log(ParserOptions parserOptions, Func<string> log)
		{
			parserOptions.Logger?.LogDebug(LoggingFormatter.FormatterServiceId, log());
		}
		/// <summary>
		///     Writes the specified log.
		/// </summary>
		[Conditional("LOG")]
		public void Log(ParserOptions parserOptions, Func<string> log, Func<IDictionary<string, object>> getValues)
		{
			parserOptions.Logger?.LogDebug(LoggingFormatter.FormatterServiceId, log(), getValues());
		}

		//public async ObjectPromise Execute(
		//	 FormatterArgumentType[] args,
		//	object sourceValue,
		//	 string name,
		//	ParserOptions options,
		//	ScopeData scope)
		//{
		//	var cacheItem = PrepareCallMostMatchingFormatter(sourceValue.GetType(), args, name, options, scope);
		//	if (cacheItem == null)
		//	{
		//		return FormatterFlow.Skip;
		//	}

		//	return await Execute(cacheItem.Value, sourceValue, args);
		//}

		/// <inheritdoc />
		public FormatterCache? PrepareCallMostMatchingFormatter(
			Type type,
			FormatterArgumentType[] arguments,
			string name,
			ParserOptions parserOptions,
			ScopeData scope)
		{
			var compareKey = new FormatterCacheCompareKey(type, arguments, name);
			if (Cache != null && Cache.TryGetValue(compareKey, out var cacheItem))
			{
				return cacheItem;
			}
			var hasFormatter = PrepareGetMatchingFormatterOn(type, arguments, parserOptions, name)
				.Where(e => e != null)
				.ToArray();

			var services = new ServiceCollection(ServiceCollectionAccess);
			services.AddService(parserOptions);
			services.AddService(scope.Profiler);
			services.AddService(scope);
			services.AddService(parserOptions.Logger);

			foreach (var morestachioFormatterModel in hasFormatter)
			{
				var tryCompose = PrepareComposeValues(morestachioFormatterModel,
					type,
					morestachioFormatterModel.Function,
					services,
					arguments, parserOptions);
				if (tryCompose != null)
				{
					var cache = new FormatterCache(morestachioFormatterModel, tryCompose);
					if (Cache != null)
					{
						Cache[compareKey] = cache;
					}

					return cache;
				}
			}

			if (Cache != null)
			{
				return Cache[compareKey] = null;
			}

			return null;
		}

		/// <summary>
		///     Executes the specified formatter.
		/// </summary>
		public virtual async ObjectPromise Execute(
			FormatterCache formatter,
			object sourceValue,
			ParserOptions parserOptions,
			FormatterArgumentType[] args)
		{
			Log(parserOptions, () => $"Execute the formatter {formatter.Model.Name} with arguments", 
				() => args.ToDictionary(e => e.Name, e => (object)e));

			var mapedValues = new object[formatter.TestedTypes.Arguments.Count];
			var i = 0;
			foreach (var formatterArgumentMap in formatter.TestedTypes.Arguments)
			{
				var valueObtainValue = formatterArgumentMap.Value.ObtainValue(sourceValue, args);
				if (formatterArgumentMap.Value.ConverterFunc != null)
				{
					var convValue = formatterArgumentMap.Value.ConverterFunc(valueObtainValue);
					valueObtainValue = convValue ?? valueObtainValue;
				}
				mapedValues[i++] = valueObtainValue;
			}

			try
			{
				var functionTarget = formatter.Model.FunctionTarget;
				var method = formatter.TestedTypes.PrepareInvoke(mapedValues);

				if (formatter.Model.LinkFunctionTarget && !method.IsStatic &&
				    method.DeclaringType == sourceValue.GetType())
				{
					functionTarget = sourceValue;
				}

				var taskAlike = method.Invoke(functionTarget, mapedValues);
				return await taskAlike.UnpackFormatterTask();
			}
			catch (Exception e)
			{
				if (ExceptionHandling == FormatterServiceExceptionHandling.IgnoreSilently)
				{
					return null;
				}

				if (ExceptionHandling == FormatterServiceExceptionHandling.PrintExceptions)
				{
					return e.ToString();
				}

				throw;
			}
		}

		/// <summary>
		///		Gets all formatter that are invokable with on the given type and with the arguments.
		/// </summary>
		public virtual IEnumerable<MorestachioFormatterModel> PrepareGetMatchingFormatterOn(
			 Type typeToFormat,
			 FormatterArgumentType[] arguments,
			 ParserOptions parserOptions,
			 string name)
		{
			Log(parserOptions, () => $"Lookup formatter for type {typeToFormat} with name {name}", 
				() => arguments.ToDictionary(e => e.Name, e => (object)e));

			IList<MorestachioFormatterModel> formatters = null;
			if (!Formatters.TryGetValue(name ?? "{NULL}", out formatters))
			{
				Log(parserOptions, () => $"There are no formatters for the name {name}");
				return Enumerable.Empty<MorestachioFormatterModel>();
			}
			
			var filteredSourceList = new List<KeyValuePair<MorestachioFormatterModel, ulong>>();
			foreach (var formatTemplateElement in formatters)
			{
				Log(parserOptions, () => $"Test formatter input type: '{formatTemplateElement.InputType}' on formatter named '{formatTemplateElement.Function.Name}'");

				if (formatTemplateElement.InputType != typeToFormat
					&& !formatTemplateElement.InputType.GetTypeInfo().IsAssignableFrom(typeToFormat))
				{
					if (ValueConverter.All(e => !e.CanConvert(typeToFormat, formatTemplateElement.InputType)))
					{
						if (formatTemplateElement.MetaData.SourceObject != null &&
							formatTemplateElement.MetaData.SourceObject.FormatterValueConverterAttribute
							.Select(e => e.CreateInstance())
							.All(e => !e.CanConvert(typeToFormat, formatTemplateElement.InputType)))
						{
							if (!CheckGenericTypeForMatch(typeToFormat, formatTemplateElement))
							{
								var foundInterface = false;
								//foreach (var @interface in typeToFormat.GetInterfaces())
								//{
								//	if (CheckGenericTypeForMatch(@interface, formatTemplateElement))
								//	{
								//		foundInterface = true;
								//	}
								//}

								if (!foundInterface)
								{
									Log(parserOptions, () =>
										$"Exclude because formatter accepts '{formatTemplateElement.InputType}' is not assignable from '{typeToFormat}'");
									continue;
								}
							}
						}
					}
				}

				//count rest arguments
				//var mandatoryArguments = formatTemplateElement.MetaData
				//	.Where(e => !e.IsRestObject && !e.IsOptional && !e.IsSourceObject && !e.IsInjected).ToArray();
				if (formatTemplateElement.MetaData.MandetoryArguments.Count > arguments.Length)
				//if there are less arguments excluding rest then parameters
				{
					Log(parserOptions, () =>
						"Exclude because formatter has " +
						$"'{formatTemplateElement.MetaData.MandetoryArguments.Count}' " +
						"parameter and " +
						$"'{formatTemplateElement.MetaData.Count(e => e.IsRestObject)}' " +
						"rest parameter but needs less or equals" +
						$"'{arguments.Length}'.");
					continue;
				}

				ulong score = 1L;
				if (formatTemplateElement.Function.ReturnParameter == null ||
					formatTemplateElement.Function.ReturnParameter?.ParameterType == typeof(void))
				{
					score++;
				}

				score += (ulong)(arguments.Length - formatTemplateElement.MetaData.MandetoryArguments.Count);
				Log(parserOptions, () =>
					$"Take filter: '{formatTemplateElement.InputType} : {formatTemplateElement.Function}' Score {score}");
				filteredSourceList.Add(
					new KeyValuePair<MorestachioFormatterModel, ulong>(formatTemplateElement, score));
			}

			if (filteredSourceList.Count > 0)
			{
				var formatter = new List<MorestachioFormatterModel>();
				foreach (var formatTemplateElement in filteredSourceList.OrderBy(e => e.Value))
				{
					formatter.Add(formatTemplateElement.Key);
				}

				return formatter;
			}
			Log(parserOptions, () => "No formatter matches");
			return Enumerable.Empty<MorestachioFormatterModel>();
		}

		private bool CheckGenericTypeForMatch(Type typeToFormat, MorestachioFormatterModel formatTemplateElement)
		{
			var typeToFormatGenerics = typeToFormat.GetTypeInfo().GetGenericArguments();

			//explicit check for array support
			if (typeToFormat.HasElementType)
			{
				var elementType = typeToFormat.GetElementType();
				typeToFormatGenerics = typeToFormatGenerics.Concat(new[]
				{
					elementType
				}).ToArray();
			}

			//the type check has maybe failed because of generic parameter. Check if both the formatter and the typ have generic arguments

			var formatterGenerics = formatTemplateElement.InputType.GetTypeInfo().GetGenericArguments();

			if (typeToFormatGenerics.Length <= 0 || formatterGenerics.Length <= 0 ||
				typeToFormatGenerics.Length != formatterGenerics.Length)
			{
				return false;
			}

			return true;
		}

		/// <summary>
		///		Internal
		/// </summary>
		/// <param name="givenType"></param>
		/// <param name="genericType"></param>
		/// <returns></returns>
		public static bool IsAssignableToGenericType(Type givenType, Type genericType)
		{
			var interfaceTypes = givenType.GetInterfaces();

			foreach (var it in interfaceTypes)
			{
				if (it.IsGenericType && it.GetGenericTypeDefinition() == genericType)
				{
					return true;
				}
			}

			if (givenType.IsGenericType && givenType.GetGenericTypeDefinition() == genericType)
			{
				return true;
			}

			var baseType = givenType.BaseType;
			if (baseType == null)
			{
				return false;
			}

			return IsAssignableToGenericType(baseType, genericType);
		}

		/// <summary>
		///     Internal use only
		/// </summary>
		/// <param name="methodInfo"></param>
		/// <param name="namedParameter"></param>
		/// <returns></returns>
		public static MethodInfo PrepareMakeGenericMethodInfoByValues(
			 MethodInfo methodInfo,
			 object[] namedParameter)
		{
			var generics = new List<Type>();
			foreach (var genericArgument in methodInfo.GetGenericArguments())
			{
				var found = false;
				for (var i = 0; i < methodInfo.GetParameters().Length; i++)
				{
					var parameterInfo = methodInfo.GetParameters()[i];
					var argument = new Stack<Tuple<Type, Type>>();
					var sourceValueType = namedParameter[i].GetType();

					//in case the parameter is an generic argument directly

					if (parameterInfo.ParameterType.IsGenericParameter &&
						parameterInfo.ParameterType == genericArgument)
					{
						found = true;
						generics.Add(sourceValueType);
						break;
					}

					//this is a hack for T[] to IEnumerable<T> support
					if (sourceValueType.IsArray &&
						IsAssignableToGenericType(parameterInfo.ParameterType, typeof(IEnumerable<>)) &&
						typeof(IEnumerable<>).MakeGenericType(sourceValueType.GetElementType())
							.IsAssignableFrom(sourceValueType))
					{
						found = true;
						//the source value is an array and the parameter is of type of IEnumerable<>
						generics.Add(sourceValueType.GetElementType());
						break;
					}

					//examine the generic arguments and check that they both declare the same amount of generics
					var paramGenerics = parameterInfo.ParameterType.GetGenericArguments();
					var sourceGenerics = sourceValueType.GetGenericArguments();
					if (paramGenerics.Length != sourceGenerics.Length)
					{
						return null;
					}

					for (var index = 0; index < paramGenerics.Length; index++)
					{
						var paramGeneric = paramGenerics[index];
						var sourceGeneric = sourceGenerics[index];

						if (paramGeneric == genericArgument)
						{
							found = true;
							generics.Add(sourceGeneric);
							argument.Clear();
							break;
						}

						argument.Push(new Tuple<Type, Type>(paramGeneric, sourceGeneric));
					}

					while (argument.Any())
					{
						var arg = argument.Pop();

						if (arg.Item1 == genericArgument)
						{
							found = true;
							generics.Add(arg.Item2);
							argument.Clear();
							break;
						}

						var innerParamGenerics = arg.Item1.GetGenericArguments();
						var innerSourceGenerics = arg.Item2.GetGenericArguments();
						if (paramGenerics.Length != sourceGenerics.Length)
						{
							return null;
						}

						for (var index = 0; index < innerParamGenerics.Length; index++)
						{
							var innerParamGeneric = innerParamGenerics[index];
							var innerSourceGeneric = innerSourceGenerics[index];

							if (innerParamGeneric == genericArgument)
							{
								found = true;
								generics.Add(innerSourceGeneric);
								argument.Clear();
								break;
							}

							argument.Push(new Tuple<Type, Type>(innerParamGeneric, innerSourceGeneric));
						}
					}

					if (found)
					{
						break;
					}
				}

				if (!found)
				{
					return null;
				}
			}

			try
			{
				return methodInfo.MakeGenericMethod(generics.ToArray());
			}
			catch
			{
				return null;
			}
		}


		/// <summary>
		///     Composes the values into a Dictionary for each formatter. If returns null, the formatter will not be called.
		/// </summary>
		public virtual PrepareFormatterComposingResult PrepareComposeValues(MorestachioFormatterModel formatter,
			Type sourceType,
			MethodInfo method,
			ServiceCollection services,
			FormatterArgumentType[] templateArguments,
			ParserOptions parserOptions)
		{
			Log(parserOptions, () =>
				$"Compose values for object '{sourceType}' with formatter '{formatter.InputType}' targets '{formatter.Function.Name}'");

			var matched = new Dictionary<MultiFormatterInfo, FormatterArgumentMap>();

			for (var i = 0; i < formatter.MetaData.NonParamsArguments.Count; i++)
			{
				var parameter = formatter.MetaData.NonParamsArguments[i];
				Log(parserOptions, () => $"Match parameter '{parameter.ParameterType}' [{parameter.Name}]");

				//set ether the source object or the value from the given arguments

				FormatterArgumentType match;
				if (parameter.IsSourceObject)
				{
					Log(parserOptions, () => "Is Source object");

					matched[parameter] = new FormatterArgumentMap(i, null)
					{
						ObtainValue = (source, args) => source
					};
					match = new FormatterArgumentType(i, (string)null, sourceType);
				}
				else
				{
					if (parameter.IsInjected)
					{
						//match by index or name
						Log(parserOptions, () => "Get the injected service");
						if (services.TryGetService(parameter.ParameterType, out var service))
						{
							if (!parameter.ParameterType.IsInstanceOfType(service))
							{
								Log(parserOptions, () => $"Expected service of type '{parameter.ParameterType}' but got '{service}'");
								return default;
							}
							matched[parameter] = new FormatterArgumentMap(i, null)
							{
								ObtainValue = (source, args) => service
							};
							match = default;
						}
						else
						{
							Log(parserOptions, () => $"Requested service of type {parameter.ParameterType} is not present");
							return default;
						}
					}
					else
					{
						//match by index or name
						Log(parserOptions, () => "Try Match by Name");
						//match by name
						var index = 0;
						match = templateArguments.FirstOrDefault(e =>
						{
							index++;
							return !string.IsNullOrWhiteSpace(e.Name) && e.Name.Equals(parameter.Name);
						});
						if (!Equals(match, default(FormatterArgumentType)))
						{
							matched[parameter] = new FormatterArgumentMap(i, index - 1)
							{
								ObtainValue = (source, args) => args[index - 1].Value
							};
						}
						else
						{
							Log(parserOptions, () => "Try Match by Index");
							//match by index
							index = 0;
							match = templateArguments.FirstOrDefault(g => index++ == parameter.Index);
							if (!Equals(match, default(FormatterArgumentType)))
							{
								matched[parameter] = new FormatterArgumentMap(i, index - 1)
								{
									ObtainValue = (source, args) => args[index - 1].Value
								};
							}
							else
							{
								if (parameter.IsOptional)
								{
									matched[parameter] = new FormatterArgumentMap(i, null)
									{
										ObtainValue = (source, args) => null
									};
								}
								else
								{
									Log(parserOptions, () =>
										 $"Skip: Could not match the parameter at index '{parameter.Index}' nether by name nor by index");
									return default;
								}
							}
						}
					}
				}

				//check for matching types
				if (!parameter.IsOptional && !Equals(match, default(FormatterArgumentType)))
				{
					var converterFunction =
						PrepareComposeArgumentValue(parameter, i, services, match.Type, out var success, parserOptions);
					if (!success)
					{
						return default;
					}

					matched[parameter].ConverterFunc = converterFunction;
				}

				if (parameter.IsOptional || parameter.IsSourceObject)
				{
					continue; //value and source object are optional so we do not to check for its existence 
				}
				//here we could inject a null check
			}

			Func<object[], MethodInfo> methodCallback;

			if (method.ContainsGenericParameters)
			{
				methodCallback = objects =>
					PrepareMakeGenericMethodInfoByValues(method, objects);
			}
			else
			{
				methodCallback = (arguments) => method;
			}

			var hasRest = formatter.MetaData.ParamsArgument;
			if (hasRest == null)
			{
				return new PrepareFormatterComposingResult(methodCallback, matched);
			}


			Log(parserOptions, () => $"Match Rest argument '{hasRest.ParameterType}'");
			//return default;

			//{{"test", Buffer.X, "1"}}
			//only use the values that are not matched.
			if (typeof(Tuple<string, object>[]) == hasRest.ParameterType)
			{
				var idxSource = new List<int>();
				foreach (var argument in templateArguments)
				{
					if (matched
						.Where(e => e.Value.ParameterIndex.HasValue)
						.All(f => f.Value.ParameterIndex != argument.Index))
					{
						idxSource.Add(argument.Index);
					}
				}

				//keep the name value pairs
				matched[hasRest] = new FormatterArgumentMap(formatter.MetaData.NonParamsArguments.Count, -1)
				{
					ObtainValue = (source, values) =>
					{
						var vals = new object[idxSource.Count];
						for (var index = 0; index < idxSource.Count; index++)
						{
							var i = idxSource[index];
							var formatterArgumentType = values[i];
							vals[index] = Tuple.Create(formatterArgumentType.Name, formatterArgumentType.Value);
						}

						return vals;
					}
				};
			}
			else if (typeof(object[]).IsAssignableFrom(hasRest.ParameterType))
			{
				var idxSource = new List<int>();
				foreach (var argument in templateArguments)
				{
					if (matched
						.Where(e => e.Value.ParameterIndex.HasValue)
						.All(f => f.Value.ParameterIndex != argument.Index))
					{
						idxSource.Add(argument.Index);
					}
				}

				//keep the name value pairs
				matched[hasRest] = new FormatterArgumentMap(formatter.MetaData.NonParamsArguments.Count, null)
				{
					ObtainValue = (source, values) =>
					{
						var vals = new object[idxSource.Count];
						for (var index = 0; index < idxSource.Count; index++)
						{
							vals[index] = values[idxSource[index]].Value;
						}

						return vals;
					}
				};
			}
			else
			{
				Log(parserOptions, () => $"Skip: Match is Invalid because '{hasRest.ParameterType}' is no supported rest parameter. Only params object[] is supported as an rest parameter");
				//unknown type in params argument cannot call
				return default;
			}
			return new PrepareFormatterComposingResult(methodCallback, matched);
		}

		/// <summary>
		///     Should compose the givenValue and/or transform it in any way that it can match the
		///     <para>parameter</para>
		/// </summary>
		/// <returns></returns>
		protected virtual Func<object, object> PrepareComposeArgumentValue(MultiFormatterInfo parameter,
			int argumentIndex,
			ServiceCollection services,
			Type givenType,
			out bool success,
			ParserOptions parserOptions)
		{
			var parameterParameterType = parameter.ParameterType;
			if (parameterParameterType.IsConstructedGenericType)
			{
				//TODO check constraints of the generic type
				success = true;
				return null;
			}

			if (givenType == null)
			{
				success = AllParametersAllDefaultValue;
				return null;
			}

			if (givenType == typeof(Number) && parameterParameterType != typeof(Number))
			{
				if (NumberConverter.Instance.CanConvert(typeof(Number), parameterParameterType))
				{
					success = true;
					return o => NumberConverter.Instance.Convert(o, parameterParameterType);
				}
			}
			else if (givenType != typeof(Number) && parameterParameterType == typeof(Number))
			{
				if (NumberConverter.Instance.CanConvert(givenType, parameterParameterType))
				{
					success = true;
					return o => NumberConverter.Instance.Convert(o, parameterParameterType);
				}
			}

			if (!parameterParameterType.GetTypeInfo().IsAssignableFrom(givenType))
			{
				if (parameterParameterType.IsEnum &&
					(givenType == typeof(string)))
				{
					success = true;
					if (givenType == typeof(string))
					{
						return o => Enum.Parse(parameterParameterType, o.ToString(), true);
					}
				}

				var typeConverter =
					ValueConverter.FirstOrDefault(e => e.CanConvert(givenType, parameterParameterType));
				typeConverter = typeConverter ??
								(DefaultConverter.CanConvert(givenType, parameterParameterType)
									? DefaultConverter
									: null);
				var perParameterConverter = parameter.FormatterValueConverterAttribute
					.Select(f => f.CreateInstance())
					.FirstOrDefault(e => e.CanConvert(givenType, parameterParameterType));

				if (perParameterConverter != null)
				{
					success = true;
					return o => perParameterConverter.Convert(o, parameterParameterType);
				}
				else if (typeConverter != null)
				{
					success = true;
					return o => typeConverter.Convert(o, parameterParameterType);
				}
				else if (givenType == typeof(IConvertible) &&
						 typeof(IConvertible).IsAssignableFrom(parameterParameterType))
				{

					success = true;
					return o =>
					{
						try
						{
							return (o as IConvertible).ToType(parameterParameterType,
								services.GetRequiredService<ParserOptions>().CultureInfo);
						}
						catch
						{
							//this might just not work
							return null;
						}
					};
				}
				else
				{
					Log(parserOptions, () =>
						 $"Skip: Match is Invalid because type at {argumentIndex} of '{parameterParameterType.Name}' was not expected. Abort.");
					//The type in the template and the type defined in the formatter do not match. Abort	

					success = false;
					return null;
				}
			}

			success = true;
			return null;
		}
	}
}