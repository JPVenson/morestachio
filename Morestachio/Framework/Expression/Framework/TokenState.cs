namespace Morestachio.Framework.Expression.Framework
{
	internal enum TokenState
	{
		Expression,
		DecideArgumentType,
		ArgumentStart,
		Operator,
		Bracket
	}
}