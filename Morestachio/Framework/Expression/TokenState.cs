namespace Morestachio.Framework.Expression
{
	internal enum TokenState
	{
		None,
		StartOfExpression,
		Expression,
		ArgumentName,
		DecideArgumentType,
		ArgumentStart,
		EndOfExpression
	}
}