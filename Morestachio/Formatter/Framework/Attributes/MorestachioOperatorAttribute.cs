using Morestachio.Framework.Expression;

namespace Morestachio.Formatter.Framework.Attributes
{
	/// <summary>
	///		Defines an function as an operator for one of the given <see cref="OperatorTypes"/>
	/// </summary>
	public class MorestachioOperatorAttribute : MorestachioFormatterAttribute
	{
		public MorestachioOperatorAttribute(OperatorTypes name, string description) 
			: base("op_" + name.ToString(), description)
		{
			OperatorType = name;
		}

		/// <summary>
		///		The Operator
		/// </summary>
		public OperatorTypes OperatorType { get; set; }
		
		public override bool ValidateFormatterName()
		{
			return MorestachioOperator.Operators.ContainsKey(OperatorType);
		}
	}
}