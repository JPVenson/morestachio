//using System.Diagnostics;
//using JetBrains.Annotations;

//namespace Morestachio.Formatter
//{
//	/// <summary>
//	///		An Argument for a Formatter
//	/// </summary>
//	[DebuggerDisplay("[{Name ?? 'Unnamed'}] {Argument}")]
//	public class FormatterToken
//	{
//		/// <summary>
//		/// Initializes a new instance of the <see cref="FormatterToken"/> class.
//		/// </summary>
//		/// <param name="name">The name.</param>
//		/// <param name="argument">The argument.</param>
//		public FormatterToken(string name, FormatExpression argument)
//		{
//			Name = name;
//			Argument = argument ?? new FormatExpression();
//		}

//		/// <summary>
//		///		Ether the Name of the Argument or Null
//		/// </summary>
//		[CanBeNull]
//		public string Name { get; set; }

//		/// <summary>
//		///		The value of the Argument
//		/// </summary>
//		[NotNull]
//		public FormatExpression Argument { get; set; }
//	}
//}