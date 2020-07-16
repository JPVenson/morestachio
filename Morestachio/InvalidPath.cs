using System;
using JetBrains.Annotations;
using Morestachio.Framework;
using Morestachio.Framework.Expression;

namespace Morestachio
{
	/// <summary>
	///		Delegate for the Event Handler in <see cref="ParserOptions.UnresolvedPath"/>
	/// </summary>
	public delegate void InvalidPath(InvalidPathEventArgs args);

	/// <summary>
	///		The Event arguments for an Invalid path event
	/// </summary>
	public class InvalidPathEventArgs : EventArgs
	{
		public InvalidPathEventArgs(ContextObject sender, IMorestachioExpression expression, string pathPart, Type type)
		{
			Sender = sender;
			Expression = expression;
			PathPart = pathPart;
			Type = type;
		}

		/// <summary>
		///		The context object that is invoking the event
		/// </summary>
		public ContextObject Sender { get; private set; }

		/// <summary>
		///		The expression that could not be resolved
		/// </summary>
		public IMorestachioExpression Expression { get; private set; }

		/// <summary>
		///		The part of the expression that could not be resolved
		/// </summary>
		public string PathPart { get; private set; }

		/// <summary>
		///		The type that the part of the Path cannot be resolved
		/// </summary>
		public Type Type { get; private set; }
	}
}