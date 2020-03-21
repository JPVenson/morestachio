namespace Morestachio.Framework.Expression
{
	internal enum TokenState
	{
		None,
		Expression,
		ArgumentName,
		DecideArgumentType,
		ArgumentStart,
		EndOfExpression
	}
}