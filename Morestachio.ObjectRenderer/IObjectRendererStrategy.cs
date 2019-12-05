using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Morestachio.Formatter.Framework;

namespace Morestachio.ObjectRenderer
{
	public interface IObjectRendererStrategy
	{
		/// <summary>
		///		Starts a new Rendering process
		/// </summary>
		/// <returns></returns>
		Task BeginRenderingAsync();

		/// <summary>
		///		Indicates the start of a new Type that should be rendered
		/// </summary>
		/// <param name="type"></param>
		/// <param name="formatter"></param>
		/// <returns></returns>
		Task BeginRenderTypeAsync(Type type, IEnumerable<MorestachioFormatterModel> formatter);

		/// <summary>
		///		Indicates the start of a new Set of properties are started to be rendered. The list of properties can be empty
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		Task BeginRenderPropertiesAsync(PropertyInfo[] property);
		
		/// <summary>
		///		Indicates the start of a new Property to be rendered
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		Task BeginRenderPropertyAsync(PropertyInfo property, Type extractedType);

		/// <summary>
		///		Indicates that the property was rendered successfully
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		Task EndRenderPropertyAsync(PropertyInfo property, Type extractedType);

		/// <summary>
		///		Indicates that all properties are rendered
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		Task EndRenderPropertiesAsync(PropertyInfo[] property);

		/// <summary>
		///		Indicates that the type was rendered
		/// </summary>
		/// <param name="type"></param>
		/// <param name="formatter"></param>
		/// <returns></returns>
		Task EndRenderTypeAsync(Type type, IEnumerable<MorestachioFormatterModel> formatter);

		/// <summary>
		///		Indicates that all types are rendered. Can be used to flush an underlying stream.
		/// </summary>
		/// <returns></returns>
		Task EndRenderingAsync();
	}
}