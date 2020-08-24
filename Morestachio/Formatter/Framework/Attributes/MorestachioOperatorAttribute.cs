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
	}
}