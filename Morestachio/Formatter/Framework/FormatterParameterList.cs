using System;
using System.Collections.Generic;
using System.Text;

namespace Morestachio.Formatter.Framework
{
	/// <summary>
	///		Represents all unmatched parameters of an Formatter
	/// </summary>
	public class FormatterParameterList
	{
		/// <summary>
		///		Creates a new Parameter instance
		/// </summary>
		/// <param name="parameters"></param>
		public FormatterParameterList(FormatterParameter[] parameters)
		{
			Parameters = parameters;
		}

		/// <summary>
		///		All Parameters for a Formatter
		/// </summary>
		public FormatterParameter[] Parameters { get; }
	}

	/// <summary>
	///		A single parameter from template for a Formatter
	/// </summary>
	public class FormatterParameter
	{
		public FormatterParameter(string parameterName, object value)
		{
			ParameterName = parameterName;
			Value = value;
		}

		/// <summary>
		///		The Name of the Parameter
		/// </summary>
		public string ParameterName { get; }

		/// <summary>
		///		The value of the Parameter
		/// </summary>
		public object Value { get; }
	}
}
