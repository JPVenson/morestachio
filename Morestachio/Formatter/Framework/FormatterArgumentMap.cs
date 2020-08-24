using System;
using JetBrains.Annotations;

namespace Morestachio.Formatter.Framework
{
	/// <summary>
	///		Maps the Index and Functions to obtain the specific value from and object
	/// </summary>
	public class FormatterArgumentMap
	{
		/// <summary>
		/// 
		/// </summary>
		public FormatterArgumentMap(int codeIndex, int? argumentIndex)
		{
			CodeParameterIndex = codeIndex;
			ParameterIndex = argumentIndex;
		}

		/// <summary>
		///		The index of the parameter of the cs function
		/// </summary>
		public int CodeParameterIndex { get; }

		/// <summary>
		///		The index of the parameter from template
		/// </summary>
		public int? ParameterIndex { get; }

		/// <summary>
		///		If set, the function to convert the value obtained from <see cref="ObtainValue"/>
		/// </summary>
		[CanBeNull]
		public Func<object, object> ConverterFunc { get; set; }

		/// <summary>
		///		The function to get the value from the list of parameters
		/// </summary>
		public ObtainValue ObtainValue { get; set; }
	}
}