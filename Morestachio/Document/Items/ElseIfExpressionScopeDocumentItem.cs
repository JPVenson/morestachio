﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Morestachio.Document.Visitor;
using Morestachio.Framework;
using Morestachio.Framework.Expression;
using Morestachio.Framework.Tokenizing;

namespace Morestachio.Document.Items
{
	/// <summary>
	///		Defines an else Expression. This expression MUST come ether directly or only separated by <see cref="ContentDocumentItem"/> after an <see cref="IfExpressionScopeDocumentItem"/> or an <see cref="InvertedExpressionScopeDocumentItem"/>
	/// </summary>
	[Serializable]
	public class ElseIfExpressionScopeDocumentItem : IfExpressionScopeDocumentItem, ISupportCustomCompilation
	{
		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal ElseIfExpressionScopeDocumentItem()
		{

		}

		/// <summary>
		///		Used for XML Serialization
		/// </summary>
		internal ElseIfExpressionScopeDocumentItem(CharacterLocation location,
			IMorestachioExpression expression,
			IEnumerable<ITokenOption> tagCreationOptions) : base(location, expression, tagCreationOptions, false)
		{

		}

		/// <inheritdoc />

		protected ElseIfExpressionScopeDocumentItem(SerializationInfo info, StreamingContext c) : base(info, c)
		{
		}

		/// <inheritdoc />
		public override void Accept(IDocumentItemVisitor visitor)
		{
			visitor.Visit(this);
		}
	}
}