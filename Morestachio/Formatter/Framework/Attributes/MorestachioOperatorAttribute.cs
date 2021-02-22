using System;
using System.Reflection;
using Morestachio.Framework.Expression;

namespace Morestachio.Formatter.Framework.Attributes
{
	/// <summary>
	///		Defines an function as an operator for one of the given <see cref="OperatorTypes"/>
	/// </summary>
	public class MorestachioOperatorAttribute : MorestachioFormatterAttribute
	{
		/// <inheritdoc />
		public MorestachioOperatorAttribute(OperatorTypes name, string description) 
			: base("op_" + name.ToString(), description)
		{
			OperatorType = name;
		}

		/// <summary>
		///		The Operator
		/// </summary>
		public OperatorTypes OperatorType { get; set; }
		
		/// <inheritdoc />
		public override bool ValidateFormatterName()
		{
			return MorestachioOperator.Operators.ContainsKey(OperatorType);
		}
		
		/// <inheritdoc />
		public override void ValidateFormatter(MethodInfo method)
		{
			base.ValidateFormatter(method);
			var multiFormatterInfos = base.GetParameters(method);
			if (multiFormatterInfos.Length < 1 || multiFormatterInfos.Length > 2)
			{
				throw new InvalidOperationException(
					$"The formatter '{Name}' is invalid. An operators must at least have one and at most two arguments");
			}
		}
	}
}