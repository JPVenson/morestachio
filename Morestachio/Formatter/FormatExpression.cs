namespace Morestachio.Formatter
{
	public class FormatExpression
	{
		public FormatExpression()
		{
		}

		public string OrigialString { get; set; }
		internal IFormatterArgumentType ParsedArguments { get; set; }
	}
}