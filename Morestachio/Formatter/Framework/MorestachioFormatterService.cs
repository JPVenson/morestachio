using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using Morestachio.Attributes;

namespace Morestachio.Formatter.Framework
{
	/// <summary>
	///     The Formatter service that can be used to interpret the Native C# formatter.
	///     To use this kind of formatter you must create a public static class where all formatting functions are located.
	///     Then create a public static function that accepts n arguments of the type you want to format. For Example:
	///     If the formatter should be only used for int formatting and the argument will always be a string you have to create
	///     a function that has this header.
	///     It must not return a value.
	///     The function must have the MorestachioFormatter attribute
	/// </summary>
	public class MorestachioFormatterService
	{
		/// <summary>
		///     Initializes a new instance of the <see cref="MorestachioFormatterService" /> class.
		/// </summary>
		public MorestachioFormatterService()
		{
			GlobalFormatterModels = new List<MorestachioFormatterModel>();
		}

		/// <summary>
		///     Gets the gloabl formatter that are used always for any formatting run.
		/// </summary>
		public ICollection<MorestachioFormatterModel> GlobalFormatterModels { get; }

		/// <summary>
		///     Add all formatter into the given options object
		/// </summary>
		/// <param name="listOfFormatter">The list of formatter.</param>
		/// <param name="options">The options.</param>
		[PublicAPI]
		public void AddFormatterToMorestachio(IEnumerable<MorestachioFormatterModel> listOfFormatter,
			ParserOptions options)
		{
			foreach (var formatterGroup in listOfFormatter
				.GroupBy(e => e.InputType.Name)
				.OrderBy(e => e.Key == typeof(object).Name)
				.ToArray())
			{
				//if the object has a generic argument like lists, make it late bound
				FormatTemplateElement formatter;

				var type = formatterGroup.First().InputType;

				if (type.GetGenericArguments().Any())
				{
					formatter = options.Formatters.AddFormatter(type.GetGenericTypeDefinition(),
						new MorstachioFormatter((sourceObject, name, arguments) =>
							FormatConditonal(sourceObject, name, arguments, formatterGroup, options)));
				}
				else
				{
					formatter = options.Formatters.AddFormatter(type,
						new MorstachioFormatter((sourceObject, name, arguments) =>
							FormatConditonal(sourceObject, name, arguments, formatterGroup, options)));
				}


				formatter.MetaData
					.LastIsParams()
					.SetName("name", "Name");
			}
		}

		/// <summary>
		///     Add all formatter into the given options object
		/// </summary>
		/// <param name="options">The options.</param>
		[PublicAPI]
		public void AddFormatterToMorestachio(ParserOptions options)
		{
			AddFormatterToMorestachio(GlobalFormatterModels, options);
		}

		/// <summary>
		///		Checks if the provided MethodInfo can be called with the set of parameters
		///		Support for null -> class
		///		Support for optional
		///		Support for default value
		/// </summary>
		/// <param name="methodInfo"></param>
		/// <param name="parameter"></param>
		/// <returns></returns>
		public static object[] CanMethodCalledWith(MethodInfo methodInfo, object[] parameter)
		{
			var testParameterValueList = new List<object>();
			ParameterInfo[] parameters = methodInfo.GetParameters();
			for (var index = 0; index < parameters.Length; index++)
			{
				var parameterInfo = parameters[index];

				//check if the current parameter is optional
				if ((parameterInfo.IsOptional || parameterInfo.HasDefaultValue))
				{
					//its optional
					//if there is no value its fine
					if (parameter.Length <= index)
					{
						if (parameterInfo.HasDefaultValue)
						{
							testParameterValueList.Add(parameterInfo.DefaultValue);
						}
						else if (parameterInfo.IsOptional)
						{
							testParameterValueList.Add(Activator.CreateInstance(parameterInfo.ParameterType));
						}

						continue;
					}
				}
				else if (parameterInfo.GetCustomAttribute<RestParameterAttribute>() != null)
				{
					if (index + 1 < parameters.Length)
					{
						return null;
					}
					testParameterValueList.Add(parameter.Skip(index).ToArray());
				}
				else
				{
					//there must be a value for this parameter
					if (parameter.Length <= index)
					{
						return null;
					}
					//check the value to be used for this parameter
					var value = parameter[index];
					if (!parameterInfo.ParameterType.IsInstanceOfType(value))
					{
						return null;
					}
					testParameterValueList.Add(value);
				}
			}

			return testParameterValueList.ToArray();
		}

		private object FormatConditonal(object sourceObject, string name, object[] arguments,
			IEnumerable<MorestachioFormatterModel> formatterGroup, ParserOptions options)
		{
			if (name == null)
			{
				options.Formatters.Write(() => nameof(MorestachioFormatterService) + " | Name is null. Skip formatter");
				return FormatterMatcher.FormatterFlow.Skip;
			}

			if (sourceObject == null)
			{
				options.Formatters.Write(() =>
					nameof(MorestachioFormatterService) + " | Source Object is null. Skip formatter");
				return FormatterMatcher.FormatterFlow.Skip;
			}

			var directMatch = formatterGroup.Where(e => name.Equals(e.Name)).ToArray();
			arguments = new object[]{sourceObject}.Concat(arguments).ToArray();

			var type = sourceObject.GetType();
			var originalObject = sourceObject;
			if (!directMatch.Any())
			{
				options.Formatters.Write(() =>
				{
					var aggregate = formatterGroup.Any()
						? formatterGroup.Select(e => e.Name).Aggregate((e, f) => e + "," + f)
						: "";
					return
						$"{nameof(MorestachioFormatterService)} | No match Found for name: '{name}' Possible values for '{type}' are [{aggregate}]";
				});
				return FormatterMatcher.FormatterFlow.Skip;
			}

			foreach (var morestachioFormatterModel in directMatch)
			{
				originalObject = sourceObject;
				options.Formatters.Write(() =>
					$"{nameof(MorestachioFormatterService)} | Test {morestachioFormatterModel.Name}");

				var target = morestachioFormatterModel.Function;

				var localGen = morestachioFormatterModel.InputType.GetGenericArguments();
				var templateGen = type.GetGenericArguments();

				if (morestachioFormatterModel.InputType.ContainsGenericParameters)
				{
					if (localGen.Any() != templateGen.Any())
					{
						if (type.IsArray)
						{
							templateGen = new[] { type.GetElementType() };
						}
						else
						{
							options.Formatters.Write(() =>
								$"{nameof(MorestachioFormatterService)}| Generic type mismatch");
							continue;
						}
					}

					if (!morestachioFormatterModel.InputType.ContainsGenericParameters)
					{
						options.Formatters.Write(() =>
							$"{nameof(MorestachioFormatterService)}| Type has Generic but Method not");
						continue;
					}

					if (localGen.Length != templateGen.LongLength)
					{
						options.Formatters.Write(() =>
							$"{nameof(MorestachioFormatterService)}| Generic type count mismatch");
						continue;
					}

					target = target.MakeGenericMethod(templateGen);
				}
				else
				{
					if (!morestachioFormatterModel.InputType.IsInstanceOfType(originalObject))
					{
						options.Formatters
							.Write(() =>
								$"{nameof(MorestachioFormatterService)}| Generic Type mismatch. Expected '{morestachioFormatterModel?.InputType}' but got {originalObject?.GetType()}");
						continue;
					}
				}

				try
				{

					var canInvokeFormatter = CanMethodCalledWith(target, arguments);
					if (canInvokeFormatter == null)
					{
						options.Formatters.Write(
							() => $"{nameof(MorestachioFormatterService)} | Invalid usage of parameter");
						continue;
					}
					options.Formatters.Write(() => $"{nameof(MorestachioFormatterService)}| Execute");
					originalObject = target.Invoke(null, canInvokeFormatter);

					options.Formatters.Write(() =>
						$"{nameof(MorestachioFormatterService)}| Formatter created '{originalObject}'");
					return originalObject;
				}
				catch (Exception ex)
				{
					options.Formatters.Write(() =>
						$"{nameof(MorestachioFormatterService)}| calling of formatter has thrown a exception: '{ex}'");
					continue;
				}
			}

			return FormatterMatcher.FormatterFlow.Skip;
		}

		/// <summary>
		///     Adds all formatter that are decorated with the <see cref="MorestachioFormatterAttribute" />
		/// </summary>
		/// <param name="type">The type.</param>
		public void AddFromType(Type type)
		{
			foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public))
			{
				var hasFormatterAttr = method.GetCustomAttributes<MorestachioFormatterAttribute>();
				foreach (var morestachioFormatterAttribute in hasFormatterAttr)
				{
					if (morestachioFormatterAttribute == null)
					{
						continue;
					}

					var morestachioFormatterModel = new MorestachioFormatterModel(morestachioFormatterAttribute.Name,
						morestachioFormatterAttribute.Description,
						method.GetParameters().FirstOrDefault()?.ParameterType,
						morestachioFormatterAttribute.OutputType ?? method.ReturnType,
						method.GetCustomAttributes<MorestachioFormatterInputAttribute>()
							.Select(e => new InputDescription(e.Description, e.OutputType, e.Example)).ToArray(),
						morestachioFormatterAttribute.ReturnHint, method);
					GlobalFormatterModels.Add(morestachioFormatterModel);
				}
			}
		}
	}
}