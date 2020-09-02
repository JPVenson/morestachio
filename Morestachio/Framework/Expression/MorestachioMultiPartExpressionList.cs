using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Morestachio.Framework.Expression
{
	/// <summary>
	///		Defines an expression that consists of multiple expressions
	/// </summary>
	[Serializable]
	public class MorestachioMultiPartExpressionList : MorestachioExpressionListBase
	{
		
		internal MorestachioMultiPartExpressionList()
		{
			
		}

		/// <inheritdoc />
		public MorestachioMultiPartExpressionList(CharacterLocation location) : base(location)
		{
		}
		
		/// <inheritdoc />
		public MorestachioMultiPartExpressionList(IList<IMorestachioExpression> expressions, CharacterLocation location) : base(expressions, location)
		{
		}
		
		/// <inheritdoc />
		protected MorestachioMultiPartExpressionList(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}