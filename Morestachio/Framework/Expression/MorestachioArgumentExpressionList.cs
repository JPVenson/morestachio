using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Framework.Expression;

/// <summary>
///		Defines a list of arguments
/// </summary>
[Serializable]
public class MorestachioArgumentExpressionList : MorestachioExpressionListBase
{
		
	internal MorestachioArgumentExpressionList()
	{
			
	}
		
	/// <inheritdoc />
	public MorestachioArgumentExpressionList(TextRange location) : base(location)
	{
	}

	/// <inheritdoc />
	public MorestachioArgumentExpressionList(IList<IMorestachioExpression> expressions, TextRange location) : base(expressions, location)
	{
	}
		
	/// <inheritdoc />
	protected MorestachioArgumentExpressionList(SerializationInfo info, StreamingContext context) : base(info, context)
	{
	}
}