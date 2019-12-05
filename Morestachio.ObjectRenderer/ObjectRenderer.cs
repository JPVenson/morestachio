using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Morestachio.ObjectRenderer
{
	/// <summary>
	///		Can be used to Render a Tree of object for a Documentation
	/// </summary>
	public class ObjectRenderer
	{
		public ObjectRenderer()
		{
			
		}

		///  <summary>
		/// 	Renders the Type with the given options to the renderer Strategy.
		///		This Renderer will render one type at a time and when encountering a property with a complex type will enqueue this type
		///  </summary>
		///  <param name="type">The type that should be rendered</param>
		///  <param name="instance">can be null. If set the correct type in the object will be used. Supply an object if you are using interfaces or deviated classes as they cannot be determinated otherwise</param>
		///  <returns></returns>
		public async Task RenderDetailedTypes(Type type, 
			ParserOptions option, 
			IObjectRendererStrategy rendererStrategy,
			object instance = null)
		{
			var typeStack = new Stack<Tuple<Type, object>>();
			var knownTypes = new List<Type>();

			typeStack.Push(Tuple.Create(type, instance));

			await rendererStrategy.BeginRenderingAsync();
			while (typeStack.Any())
			{
				var typeTuple = typeStack.Pop();

				var realType = typeTuple.Item2?.GetType() ?? typeTuple.Item1;

				knownTypes.Add(realType);

				//check if this type has an element type
				if (realType.HasElementType)
				{
					var elementType = realType.GetElementType();
					if (!knownTypes.Contains(elementType))
					{
						typeStack.Push(Tuple.Create(elementType, (object)null));
					}
				}

				//get all generic arguments
				//we only care for closed generic type arguments
				foreach (var info in realType.GenericTypeArguments)
				{
					if (!knownTypes.Contains(info))
					{
						typeStack.Push(Tuple.Create(info, (object)null));
					}
				}

				var filterForType = option.Formatters.Filter(e => realType.IsAssignableFrom(e.InputType)).ToArray();
				await rendererStrategy.BeginRenderTypeAsync(realType, filterForType);

				//write all public properties
				var propertyInfos = realType.GetProperties(BindingFlags.Public)
					.Where(e => e.CanRead)
					.ToArray();
				await rendererStrategy.BeginRenderPropertiesAsync(propertyInfos);
				foreach (var propertyInfo in propertyInfos)
				{

					// check if this is an object
					if (propertyInfo.PropertyType.Assembly == typeof(string).Assembly)
					{
						continue;
					}

					object propValue = null;

					if (typeTuple.Item2 != null)
					{
						propValue = propertyInfo.GetValue(typeTuple.Item2);
					}

					var propertyType = propValue?.GetType() ?? propertyInfo.PropertyType;

					if (!knownTypes.Contains(propertyType))
					{
						typeStack.Push(Tuple.Create(propertyType, (object)null));
					}

					await rendererStrategy.BeginRenderPropertyAsync(propertyInfo, propertyType);
					await rendererStrategy.EndRenderPropertyAsync(propertyInfo, propertyType);
				}
				await rendererStrategy.EndRenderPropertiesAsync(propertyInfos);
				await rendererStrategy.EndRenderTypeAsync(realType, filterForType);
			}
			await rendererStrategy.EndRenderingAsync();
		}
	}
}
