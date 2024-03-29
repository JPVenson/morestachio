﻿using System.Collections.Concurrent;
using System.Reflection;
using System.Text.RegularExpressions;
using Morestachio.Document;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Formatter.Framework.Converter;
using Morestachio.Framework.Expression;
using Morestachio.Helper.Logging;
using Morestachio.Util.Sealing;

namespace Morestachio.Formatter.Framework;

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
	///		Initializes a new instance of the <see cref="MorestachioFormatterService" /> class with the <see cref="DefaultFormatterService"/> as its parent formatter.
	/// </summary>
	/// <param name="useCache">When enabled, all formatter building operations are cached. Not threadsafe!</param>
	public MorestachioFormatterService(bool useCache = false)
		: this(useCache, DefaultFormatterService.Default.Value)
	{
	}

	/// <summary>
	///     Initializes a new instance of the <see cref="MorestachioFormatterService" /> class.
	/// </summary>
	/// <param name="useCache">When enabled, all formatter building operations are cached. Not threadsafe!</param>
	/// <param name="parent">When set, will include all services from the parent as a fallback.</param>
	public MorestachioFormatterService(bool useCache = false, IMorestachioFormatterService parent = null)
	{
		Parent = parent;
		Formatters = new Dictionary<string, IList<MorestachioFormatterModel>>();
		Constants = new Dictionary<string, object>();

		ValueConverter = new List<IFormatterValueConverter>
		{
			NumberConverter.Instance
		};

		DefaultConverter = GenericTypeConverter.Instance;
		Services = new ServiceCollection();

		if (parent != null)
		{
			Constants = new AggregateDictionary<string, object>(Constants, parent.Constants);
			ValueConverter = new AggregateCollection<IFormatterValueConverter>(ValueConverter, parent.ValueConverter);
			Services = new ServiceCollection(parent.Services);
		}

		AddService(typeof(IMorestachioFormatterService), this);

		if (useCache)
		{
			Cache = new ConcurrentDictionary<FormatterCacheCompareKey, FormatterCache>();
		}
	}

	/// <summary>
	///		The parent formatter the should be included in lookup
	/// </summary>
	public IMorestachioFormatterService Parent { get; }

	/// <inheritdoc />
	public ServiceCollection Services { get; }

	/// <summary>
	///		Adds the service to the inner collection and registers all morestachio functions
	/// </summary>
	/// <param name="type"></param>
	/// <param name="instance"></param>
	public void AddService(Type type, object instance)
	{
		Services.AddService(type, instance);
		this.AddFromType(type);
	}

	/// <inheritdoc />
	public IDictionary<string, object> Constants { get; }

	/// <summary>
	///     Gets the global formatter that are used always for any formatting run.
	/// </summary>
	public IDictionary<string, IList<MorestachioFormatterModel>> Formatters { get; }

	private IFormatterValueConverter _defaultConverter;

	/// <summary>
	///     The fallback Converter that should convert all known mscore lib types
	/// </summary>
	public IFormatterValueConverter DefaultConverter
	{
		get { return _defaultConverter; }
		set
		{
			_defaultConverter = value ??
				throw new ArgumentNullException("value", "The value of the DefaultConverter may never be null.");
		}
	}

	/// <summary>
	///     List of all Value Converters that can be used to convert formatter arguments
	/// </summary>
	public ICollection<IFormatterValueConverter> ValueConverter { get; }

	/// <inheritdoc />
	public bool AllParametersAllDefaultValue { get; set; }

	/// <summary>
	///		Specifies how exceptions should be handled
	/// </summary>
	public FormatterServiceExceptionHandling ExceptionHandling { get; set; }

	/// <inheritdoc />
	public IEnumerable<MorestachioFormatterModel> Filter(Func<MorestachioFormatterModel, bool> filter)
	{
		var formatters = Formatters.Values.SelectMany(f => f.Where(filter));

		if (Parent != null)
		{
			formatters = formatters.Concat(Parent.Filter(filter));
		}

		return formatters;
	}

	/// <inheritdoc />
	public virtual MorestachioFormatterModel Add(MethodInfo method,
												IMorestachioFormatterDescriptor morestachioFormatterAttribute)
	{
		var name = morestachioFormatterAttribute.GetFormatterName(method);
		var morestachioFormatterModel = ModelFromMethodInfo(method, morestachioFormatterAttribute);

		name ??= "{NULL}";

		if (!Formatters.TryGetValue(name, out var formatters))
		{
			formatters = new List<MorestachioFormatterModel>();
			Formatters[name] = formatters;
		}

		formatters.Add(morestachioFormatterModel);
		return morestachioFormatterModel;
	}

	/// <summary>
	///		Creates a new <see cref="MorestachioFormatterModel"/> from a method and a <see cref="IMorestachioFormatterDescriptor"/>.
	/// </summary>
	/// <param name="methodInfo"></param>
	/// <param name="morestachioFormatterAttribute"></param>
	/// <returns></returns>
	public static MorestachioFormatterModel ModelFromMethodInfo(MethodInfo methodInfo,
																IMorestachioFormatterDescriptor
																	morestachioFormatterAttribute)
	{
		morestachioFormatterAttribute.ValidateFormatter(methodInfo);
		var arguments = morestachioFormatterAttribute.GetParameters(methodInfo);
		var name = morestachioFormatterAttribute.GetFormatterName(methodInfo);
		return new MorestachioFormatterModel(name,
			morestachioFormatterAttribute.Description,
			arguments.FirstOrDefault(e => e.IsSourceObject)?.ParameterType ?? typeof(object),
			morestachioFormatterAttribute.OutputType ?? methodInfo.ReturnType,
			methodInfo.GetCustomAttributes<MorestachioFormatterInputAttribute>()
				.Select(e => new InputDescription(e.Description, e.OutputType, e.Example)).ToArray(),
			morestachioFormatterAttribute.ReturnHint,
			methodInfo,
			new MultiFormatterInfoCollection(arguments),
			!morestachioFormatterAttribute.IsSourceObjectAware,
			morestachioFormatterAttribute.LinkFunctionTarget);
	}

	/// <summary>
	///		The cache for Formatter calls
	/// </summary>

	public IDictionary<FormatterCacheCompareKey, FormatterCache> Cache { get; set; }

	/// <inheritdoc />
	public FormatterCache PrepareCallMostMatchingFormatter(
		ref object value,
		Type type,
		FormatterArgumentType[] arguments,
		string name,
		ParserOptions parserOptions,
		ScopeData scope)
	{
		if (Cache != null && Cache.TryGetValue(new FormatterCacheCompareKey(type, arguments, name), out var cacheItem))
		{
			parserOptions.Logger?.LogDebug(LoggingFormatter.FormatterServiceId,
				$"Provide formatter {name} from cache.");
			return cacheItem;
		}

		if (value is IDynamicFormatterProvider dynamicProvider)
		{
			var dynamicFormatter = dynamicProvider.GetFormatter(this, type, arguments, name, parserOptions, scope);

			if (dynamicFormatter is not null)
			{
				return dynamicFormatter;
			}
		}

		var hasFormatter = PrepareGetMatchingFormatterOn(type, arguments, parserOptions, name)
			.Where(e => e != null)
			.ToArray();

		if (!hasFormatter.Any())
		{
			parserOptions.Logger?.LogDebug(LoggingFormatter.FormatterServiceId,
				$"Could not find logger {name} in {GetType()}. Ask Parent {Parent?.GetType()}");
			return Parent?.PrepareCallMostMatchingFormatter(
				ref value,
				type,
				arguments,
				name,
				parserOptions,
				scope);
		}

		var services = this.Services.CreateChild();
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
				arguments,
				parserOptions);

			if (tryCompose == null)
			{
				continue;
			}

			var cache = new FormatterCache(morestachioFormatterModel, tryCompose);

			if (Cache != null)
			{
				Cache[new FormatterCacheCompareKey(type, arguments, name)] = cache;
			}

			return cache;
		}

		parserOptions.Logger?.LogError(LoggingFormatter.FormatterServiceId,
			$"Could not find logger {name} in {GetType()}.");
		return null;
	}

	/// <summary>
	///     Executes the specified formatter.
	/// </summary>
	public virtual ObjectPromise Execute(
		FormatterCache formatter,
		object sourceValue,
		ParserOptions parserOptions,
		FormatterArgumentType[] args)
	{
		parserOptions.Logger?
			.LogTrace(LoggingFormatter.FormatterServiceId,
				$"Execute the formatter {formatter.Model.Name} with arguments. {args.Select(f => string.Join("\t", f.Index, f.Name, f.Type, f.Expression.AsStringExpression(), f.Value))}");

		if (formatter.Model.LinkFunctionTarget && sourceValue is null)
		{
			parserOptions.Logger?
				.LogError(LoggingFormatter.FormatterServiceId,
					$"Tried to execute formatter {formatter.Model.Name} but did not provide a reference to execute on.");
			return AsyncHelper.EmptyPromise<object>();
		}

		var mappedValues = formatter.ValueBuffer;

		var i = 0;

		foreach (var formatterArgumentMap in formatter.TestedTypes.Arguments)
		{
			var valueObtainValue = formatterArgumentMap.Value.ObtainValue(sourceValue, args);

			if (formatterArgumentMap.Value.ConverterFunc != null)
			{
				var convValue = formatterArgumentMap.Value.ConverterFunc(valueObtainValue);
				valueObtainValue = convValue ?? valueObtainValue;
			}

			mappedValues[i++] = valueObtainValue;
		}

		try
		{
			var functionTarget = formatter.Model.FunctionTarget;
			var callerWrapper = formatter.TestedTypes.PrepareInvoke(mappedValues);

			//if this is an instance method use the source type as the target for invoking the method
			if (formatter.Model.LinkFunctionTarget
				&& !callerWrapper.methodInfo.IsStatic
				&& callerWrapper.methodInfo.DeclaringType?.IsInstanceOfType(sourceValue) == true)
			{
				functionTarget = sourceValue;
			}

			return callerWrapper.method(functionTarget, mappedValues).UnpackFormatterTask();
		}
		catch (Exception e)
		{
			parserOptions.Logger?
				.LogError(LoggingFormatter.FormatterServiceId,
					$"Error while executing formatter {formatter.Model.Name}. Handle as {ExceptionHandling}. Exception {e}");

			if (ExceptionHandling == FormatterServiceExceptionHandling.IgnoreSilently)
			{
				return AsyncHelper.EmptyPromise<object>();
			}

			if (ExceptionHandling == FormatterServiceExceptionHandling.PrintExceptions)
			{
				return (e.ToString() as object).ToPromise();
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
		parserOptions.Logger?.LogTrace(LoggingFormatter.FormatterServiceId,
			$"Lookup formatter for type {typeToFormat} with name {name}. {arguments.Select(f => string.Join("\t", f.Index, f.Name, f.Type, f.Expression.AsStringExpression()))}");

		if (!Formatters.TryGetValue(name ?? "{NULL}", out var formatters))
		{
			parserOptions.Logger?.LogDebug(LoggingFormatter.FormatterServiceId,
				$"There are no formatters for the name {name}");
			return Enumerable.Empty<MorestachioFormatterModel>();
		}

		var filteredSourceList = new List<KeyValuePair<MorestachioFormatterModel, ulong>>();

		foreach (var formatTemplateElement in formatters)
		{
			parserOptions.Logger?.LogTrace(LoggingFormatter.FormatterServiceId,
				$"Test formatter input type: '{formatTemplateElement.InputType}' on formatter named '{formatTemplateElement.Function.Name}'");

			//this checks only for source type equality
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
						if (!CheckGenericTypeForMatch(typeToFormat, formatTemplateElement.InputType))
						{
							parserOptions.Logger?.LogTrace(LoggingFormatter.FormatterServiceId,
								$"Exclude because formatter accepts '{formatTemplateElement.InputType}' is not assignable from '{typeToFormat}'");
							continue;
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
				parserOptions.Logger?.LogTrace(LoggingFormatter.FormatterServiceId,
					$"Exclude because formatter has '{formatTemplateElement.MetaData.MandetoryArguments.Count}' parameter and '{formatTemplateElement.MetaData.Count(e => e.IsRestObject)}' rest parameter but needs less or equals'{arguments.Length}'.");
				continue;
			}

			ulong score = 1L;

			if (formatTemplateElement.Function.ReturnParameter == null ||
				formatTemplateElement.Function.ReturnParameter?.ParameterType == typeof(void))
			{
				score++;
			}

			score += (ulong)(arguments.Length - formatTemplateElement.MetaData.MandetoryArguments.Count);
			parserOptions.Logger?.LogTrace(LoggingFormatter.FormatterServiceId,
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

		parserOptions.Logger?.LogDebug(LoggingFormatter.FormatterServiceId, $"No formatter matches");
		return Enumerable.Empty<MorestachioFormatterModel>();
	}

	private bool CheckGenericTypeForMatch(Type inputType, Type targetType)
	{
		return IsAssignableToGenericType(inputType, targetType);
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

		if (genericType.ContainsGenericParameters)
		{
			genericType = genericType.GetGenericTypeDefinition();
		}

		foreach (var it in interfaceTypes)
		{
			if (it.IsGenericType)
			{
				var genericTypeDefinition = it.GetGenericTypeDefinition();

				if (genericTypeDefinition == genericType)
				{
					return true;
				}
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

	private MethodInfo MakeGenericMethod(MethodInfo method, Type[] givenTypes, ParserOptions parserOptions)
	{
		var generics = new List<Type>();
		var arguments = method.GetParameters().Zip(givenTypes, Tuple.Create).ToArray();
		var genericArguments = method.GetGenericArguments();

		foreach (var genericArgument in genericArguments)
		{
			var directlyFromParameter = arguments
				.Where(e => e.Item1.ParameterType.IsGenericParameter)
				.FirstOrDefault(e => e.Item1.ParameterType == genericArgument);

			if (directlyFromParameter != null)
			{
				//The parameter is a generic type
				generics.Add(directlyFromParameter.Item2);
				continue;
			}

			var nestedGeneric = arguments
				.Select(e => GetGenericTypeLookup(e.Item1.ParameterType, e.Item2, genericArgument)).FirstOrDefault();

			if (nestedGeneric != null)
			{
				generics.Add(nestedGeneric);
				continue;
			}

			var returnGeneric = GetGenericTypeLookup(method.ReturnType, method.ReturnType, genericArgument);

			if (method.ReturnType == genericArgument || returnGeneric != null)
			{
				parserOptions.Logger?.LogDebug(LoggingFormatter.FormatterServiceId,
					$"Cannot evaluate generic '{genericArgument}' of method '{method}'. Substitute with typeof(object).");
				generics.Add(typeof(object));
				continue;
			}

			return null;
		}

		return method.MakeGenericMethod(generics.ToArray());
	}

	private static Type GetGenericTypeLookup(Type declaredType, Type realType, Type genericArgument)
	{
		//examine the generic arguments and check that they both declare the same amount of generics
		var paramGenerics = declaredType.GetGenericArguments();
		var sourceGenerics = realType.GetGenericArguments();

		if (paramGenerics.Length != sourceGenerics.Length)
		{
			//in the special case that we need to assign a IEnumerable from an Array we have make this workaround
			var genericTypeDefinition = declaredType.GetGenericTypeDefinition();

			if (genericTypeDefinition == typeof(IEnumerable<>) && realType.IsArray &&
				paramGenerics[0] == genericArgument)
			{
				return realType.GetElementType();
			}

			return null;
		}

		//list all arguments of that parameter and try to substitute them
		//example
		//void Formatter<T>(IItem<T> item)
		//we always have an object where we can substitute for example an IItem<T> or an instance of Item where it inherts from Item<T>
		var argument = new Stack<Tuple<Type, Type>>();

		for (var index = 0; index < paramGenerics.Length; index++)
		{
			var paramGeneric = paramGenerics[index];
			var sourceGeneric = sourceGenerics[index];

			if (paramGeneric == genericArgument)
			{
				return sourceGeneric;
			}

			argument.Push(new Tuple<Type, Type>(paramGeneric, sourceGeneric));
		}

		while (argument.Any())
		{
			var arg = argument.Pop();

			if (arg.Item1 == genericArgument)
			{
				return arg.Item2;
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
					return innerSourceGeneric;
				}

				argument.Push(new Tuple<Type, Type>(innerParamGeneric, innerSourceGeneric));
			}
		}

		return null;
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
		parserOptions.Logger?.LogDebug(LoggingFormatter.FormatterServiceId,
			$"Compose values for object '{sourceType}' with formatter '{formatter.InputType}' targets '{formatter.Function.Name}'");

		var matched = new Dictionary<MultiFormatterInfo, FormatterArgumentMap>();

		for (var i = 0; i < formatter.MetaData.NonParamsArguments.Count; i++)
		{
			var parameter = formatter.MetaData.NonParamsArguments[i];
			parserOptions.Logger?.LogTrace(LoggingFormatter.FormatterServiceId,
				$"Match parameter '{parameter.ParameterType}' [{parameter.Name}]");

			//set ether the source object or the value from the given arguments

			FormatterArgumentType match;

			if (parameter.IsSourceObject)
			{
				parserOptions.Logger?.LogTrace(LoggingFormatter.FormatterServiceId, "Is Source object");

				matched[parameter] = new FormatterArgumentMap(i, null)
				{
					ObtainValue = (source, _) => source
				};
				match = new FormatterArgumentType(i, (string)null, sourceType);
			}
			else
			{
				if (parameter.IsInjected)
				{
					//match by index or name
					parserOptions.Logger?.LogTrace(LoggingFormatter.FormatterServiceId, "Get the injected service");

					if (services.TryGetService(parameter.ParameterType, out var service))
					{
						if (!parameter.ParameterType.IsInstanceOfType(service))
						{
							parserOptions.Logger?
								.LogError(LoggingFormatter.FormatterServiceId,
									$"Expected service of type '{parameter.ParameterType}' but got '{service}'");
							return default;
						}

						matched[parameter] = new FormatterArgumentMap(i, null)
						{
							ObtainValue = (_, _) => service
						};
						match = default;
					}
					else
					{
						parserOptions.Logger?
							.LogError(LoggingFormatter.FormatterServiceId,
								$"Requested service of type {parameter.ParameterType} is not present");
						return default;
					}
				}
				else
				{
					//match by index or name
					parserOptions.Logger?.LogTrace(LoggingFormatter.FormatterServiceId, "Try Match by Name");
					//match by name
					var index = 0;
					match = templateArguments.FirstOrDefault(e =>
					{
						index++;
						return !string.IsNullOrWhiteSpace(e.Name) && e.Name.Equals(parameter.Name);
					});

					if (!Equals(match, default(FormatterArgumentType))) //a match by named parameter
					{
						if (parameter.ParameterType == typeof(IMorestachioExpression))
						{
							matched[parameter] = new FormatterArgumentMap(i, index - 1)
							{
								ObtainValue = (_, args) => args[index - 1].Expression
							};
						}
						else
						{
							matched[parameter] = new FormatterArgumentMap(i, index - 1)
							{
								ObtainValue = (_, args) => args[index - 1].Value
							};
						}
					}
					else
					{
						parserOptions.Logger?.LogTrace(LoggingFormatter.FormatterServiceId, "Try Match by Index");
						//match by index
						index = 0;
						match = templateArguments.FirstOrDefault(_ => index++ == parameter.Index);

						if (!Equals(match, default(FormatterArgumentType))) //a match by index
						{
							if (parameter.ParameterType == typeof(IMorestachioExpression))
							{
								matched[parameter] = new FormatterArgumentMap(i, index - 1)
								{
									ObtainValue = (_, args) => args[index - 1].Expression
								};
							}
							else
							{
								matched[parameter] = new FormatterArgumentMap(i, index - 1)
								{
									ObtainValue = (_, args) => args[index - 1].Value
								};
							}
						}
						else
						{
							if (parameter.IsOptional) //no match but optional so set null
							{
								matched[parameter] = new FormatterArgumentMap(i, null)
								{
									ObtainValue = (_, _) => null
								};
							}
							else
							{
								parserOptions.Logger?.LogDebug(LoggingFormatter.FormatterServiceId,
									$"Skip: Could not match the parameter at index '{parameter.Index}' nether by name nor by index");
								return default;
							}
						}
					}
				}
			}

			//check for matching types
			if (!parameter.IsOptional && !Equals(match, default(FormatterArgumentType)) &&
				parameter.ParameterType != typeof(IMorestachioExpression))
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
			{
				var types = new Type[objects.Length];

				for (int i = 0; i < types.Length; i++)
				{
					types[i] = objects[i]?.GetType();
				}

				return MakeGenericMethod(method, types, parserOptions);
			};
		}
		else
		{
			methodCallback = (_) => method;
		}

		var hasRest = formatter.MetaData.ParamsArgument;

		if (hasRest == null)
		{
			return new PrepareFormatterComposingResult(methodCallback, matched);
		}


		parserOptions.Logger?.LogTrace(LoggingFormatter.FormatterServiceId,
			$"Match Rest argument '{hasRest.ParameterType}'");
		//return default;

		//{{"test", Buffer.X, "1"}}
		//only use the values that are not matched.
		if (typeof(FormatterParameterList) == hasRest.ParameterType)
		{
			var idxSource = new List<int>();

			for (var index = 0; index < templateArguments.Length; index++)
			{
				var argument = templateArguments[index];

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
				ObtainValue = (_, values) =>
				{
					var vals = new FormatterParameter[idxSource.Count];

					for (var index = 0; index < idxSource.Count; index++)
					{
						var i = idxSource[index];
						var formatterArgumentType = values[i];
						vals[index] = new FormatterParameter(formatterArgumentType.Name, formatterArgumentType.Value);
					}

					return new FormatterParameterList(vals);
				}
			};
		}
		else if (typeof(object[]).IsAssignableFrom(hasRest.ParameterType))
		{
			var idxSource = new List<int>();

			for (var index = 0; index < templateArguments.Length; index++)
			{
				var argument = templateArguments[index];

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
				ObtainValue = (_, values) =>
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
			parserOptions.Logger?.LogDebug(LoggingFormatter.FormatterServiceId,
				$"Skip: Match is Invalid because '{hasRest.ParameterType}' is no supported rest parameter. Only params object[] is supported as an rest parameter");
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
			//if (givenType == typeof(MorestachioTemplateExpression) && parameterParameterType.BaseType == typeof(MulticastDelegate))
			//{
			//	success = true;
			//	return o => (o as MorestachioTemplateExpression).As(parameterParameterType);
			//}

			//TODO check constraints of the generic type
			success = true;
			return null;
		}

		if (parameterParameterType.IsGenericParameter)
		{
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
						//do not use the parameters ParserOptions here as the Culture might have been changed.
						return (o as IConvertible).ToType(parameterParameterType,
							services.GetRequiredService<ParserOptions>().CultureInfo);
					}
					catch (Exception ex)
					{
						parserOptions.Logger?
							.LogError(LoggingFormatter.FormatterServiceId,
								$"Tried to convert {o} into {parameterParameterType} but was not successfully because {ex}");
						//this might just not work
						return null;
					}
				};
			}
			else
			{
				parserOptions.Logger?.LogTrace(LoggingFormatter.FormatterServiceId,
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