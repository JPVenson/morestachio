using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Morestachio.Framework.Expression
{
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
		public MorestachioArgumentExpressionList(CharacterLocation location) : base(location)
		{
		}

		/// <inheritdoc />
		public MorestachioArgumentExpressionList(IList<IMorestachioExpression> expressions, CharacterLocation location) : base(expressions, location)
		{
		}
		
		/// <inheritdoc />
		protected MorestachioArgumentExpressionList(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}
	}
}