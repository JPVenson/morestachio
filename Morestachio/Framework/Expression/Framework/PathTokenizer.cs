using System;
using System.Collections.Generic;
using Morestachio.ParserErrors;

namespace Morestachio.Framework.Expression.Framework
{
	/// <summary>
	///		Defines a part that can be traversed by the <see cref="ContextObject"/>
	/// </summary>
	public readonly struct PathPart
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="pathType"></param>
		/// <param name="value"></param>
		public PathPart(PathType pathType, string value)
		{
			PathType = pathType;
			Value = value;
		}

		/// <summary>
		///		The type of path to traverse
		/// </summary>
		public PathType PathType { get; }

		/// <summary>
		///		In case of DataPath or Number the value
		/// </summary>
		public string Value { get; }
	}

	/// <summary>
	///		Defines the types the <see cref="ContextObject"/> can traverse
	/// </summary>
	public enum PathType
	{
		/// <summary>
		///		Traverse the data structure down by the value of <see cref="PathPart.Value"/>
		/// </summary>
		DataPath,

		/// <summary>
		///		Traverse the structure up to the upper most object
		/// </summary>
		RootSelector,

		/// <summary>
		///		Traverse the structure up one level
		/// </summary>
		ParentSelector,

		/// <summary>
		///		Do nothing and get yourself
		/// </summary>
		SelfAssignment,

		/// <summary>
		///		Enumerate the current objects structure and output a list
		/// </summary>
		ObjectSelector,

		///// <summary>
		/////		Create a new Number based on the value of <see cref="PathPart.Value"/>
		///// </summary>
		//Number,

		/// <summary>
		///		Defines a null value
		/// </summary>
		Null,

		/// <summary>
		///		Defines an boolean that can ether be true or false
		/// </summary>
		Boolean
	}

#pragma warning disable 1591

	/// <summary>
	///		This path is internal and should not be used in your code
	/// </summary>
	internal class PathTokenizer
	{
		public PathTokenizer()
		{
			PathParts = new PathPartsCollection();
			CurrentPart = "";
		}

		private PathPartsCollection PathParts { get; set; }

		private class PathPartsCollection
		{
			public PathPartsCollection()
			{
				_parts = new List<KeyValuePair<string, PathType>>();
			}
			private IList<KeyValuePair<string, PathType>> _parts;

			public KeyValuePair<string, PathType>? Last
			{
				get { return _last; }
			}

			private bool _any;
			private bool _hasParentSelector;
			private bool _isNullValue;
			private KeyValuePair<string, PathType>? _last;
			private bool _many;

			public bool Many
			{
				get { return _many; }
			}

			public bool IsNullValue
			{
				get { return _isNullValue; }
			}

			public bool HasParentSelector
			{
				get { return _hasParentSelector; }
			}

			public bool Any
			{
				get { return _any; }
			}

			public void Add(string value, PathType part)
			{
				if (!_any)
				{
					_hasParentSelector = part == PathType.ParentSelector;
					_isNullValue = part == PathType.Null;
				}
				else
				{
					_many = true;
				}
				_any = true;
				if (_last != null)
				{
					_parts.Add(_last.Value);
				}
				_last = new KeyValuePair<string, PathType>(value, part);
			}

			public void RemoveLast()
			{
				_last = null;
			}

			public IList<KeyValuePair<string, PathType>> GetList()
			{
				if (_last != null)
				{
					_parts.Add(_last.Value);
				}

				return _parts;
			}
		}

		public string CurrentPart { get; set; }
		public bool LastCharWasDelimiter { get; set; }
		//public bool CurrentPartIsNumber { get; set; }

		public bool Add(char c, TokenzierContext context, int index, out Func<IMorestachioError> errProducer)
		{
			errProducer = null;
			if (!Tokenizer.IsExpressionPathChar(c))
			{
				errProducer = () => new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
						.AddWindow(new CharacterSnippedLocation(1, index, CurrentPart)),
					CurrentPart);
				return false;
			}

			if (PathParts.IsNullValue)
			{
				errProducer = () => new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
						.AddWindow(new CharacterSnippedLocation(1, index, CurrentPart)),
					CurrentPart,
					"Nothing can follow on a null");
				return false;
			}

			LastCharWasDelimiter = c == '.';

			if (c == '/')
			{
				if (CurrentPart == "..")
				{
					if (PathParts.Any && !PathParts.HasParentSelector)
					{
						errProducer = () => new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
								.AddWindow(new CharacterSnippedLocation(1, index, CurrentPart)),
							CurrentPart,
							"An Parent selector '..\\' can only follow on another parent selector like and never on an root or an data selector");

						return false;
					}
					PathParts.Add(null, PathType.ParentSelector);
					CurrentPart = string.Empty;
					return true;
				}
				errProducer = () => new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
						.AddWindow(new CharacterSnippedLocation(1, index, CurrentPart)),
					CurrentPart,
					"Unexpected '/'. Expected ether the start of an expression or an './'");
				return false;
			}

			if (c == '~')
			{
				if (CurrentPart != string.Empty || PathParts.Any)
				{
					errProducer = () => new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
							.AddWindow(new CharacterSnippedLocation(1, index, CurrentPart)),
						CurrentPart,
						"An root selector '~' must be at the start of an expression");

					return false;
				}

				PathParts.Add(null, PathType.RootSelector);
				CurrentPart = string.Empty;
				return true;
			}

			if (c == '?')
			{
				PathParts.Add(null, PathType.ObjectSelector);
				CurrentPart = string.Empty;
				return true;
			}

			if (c != '.' && CurrentPart == "." && !PathParts.Any)
			{
				//in this case somebody wrote .data
				//so ignore the dot
				CurrentPart = string.Empty;
			}

			if (CurrentPart != string.Empty && CurrentPart != "." && c == '.')
			{
				if (!ComputeCurrentPart(context, index, out errProducer))
				{
					return false;
				}

				CurrentPart = "";
			}
			else
			{
				CurrentPart += c;
			}

			return true;
		}

		private bool ComputeCurrentPart(TokenzierContext context, int index, out Func<IMorestachioError> errProducer)
		{
			errProducer = null;
			var checkPathPart = CheckPathPart();
			if (checkPathPart != -1)
			{
				errProducer = () => (
					new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
							.AddWindow(new CharacterSnippedLocation(1, checkPathPart, CurrentPart)),
						CurrentPart));

				return false;
			}

			if (CurrentPart == "null")
			{
				if (PathParts.Any)
				{
					errProducer = () => (
						new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
								.AddWindow(new CharacterSnippedLocation(1, index, CurrentPart)),
							CurrentPart,
							"An null must be at the start of an expression"));

					return false;
				}
				PathParts.Add(null, PathType.Null);
				return true;
			}

			if (CurrentPart == "true" || CurrentPart == "false")
			{
				if (PathParts.Any)
				{
					errProducer = () => (
						new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
								.AddWindow(new CharacterSnippedLocation(1, index, CurrentPart)),
							CurrentPart,
							"An boolean must be at the start of an expression"));

					return false;
				}

				PathParts.Add(CurrentPart, PathType.Boolean);
				return true;
			}
			if (CurrentPart == "../")
			{
				PathParts.Add(null, PathType.ParentSelector);
				return true;
			}
			if (CurrentPart == "~")
			{
				PathParts.Add(null, PathType.RootSelector);
				return true;
			}
			if (CurrentPart == "?")
			{
				PathParts.Add(null, PathType.ObjectSelector);
				return true;
			}
			PathParts.Add(CurrentPart, PathType.DataPath);
			return true;
		}

		private int CheckPathPart()
		{
			var part = CurrentPart;
			if (part.StartsWith("$") && part != "$")
			{
				part = part.Substring(1, part.Length - 1);
			}

			if (part.StartsWith("."))
			{
				if (part.Equals("../"))
				{
					return -1;
				}
				if (part.Equals("."))
				{
					return -1;
				}

				return 0;
			}
			if (part.Equals("~"))
			{
				return -1;
			}
			if (part.Equals("?"))
			{
				return -1;
			}

			for (int i = 0; i < part.Length; i++)
			{
				if (!Tokenizer.IsExpressionDataPathChar(part[i]))
				{
					return i;
				}
			}

			return -1;
		}

		public string GetFormatterName(TokenzierContext context, int index, out bool found, out Func<IMorestachioError> errProducer)
		{
			var last = CompileCurrent(context, index, out errProducer);

			if (last == null)
			{
				if (CurrentPart == string.Empty)
				{
					last = new KeyValuePair<string, PathType>(string.Empty, PathType.DataPath);
				}
				else
				{
					found = false;
					return null;
				}
			}

			found = true;

			if (last.Value.Value != PathType.DataPath)
			{
				PathParts.Add(last.Value.Key, last.Value.Value);
				return string.Empty;
			}
			return last.Value.Key;
		}

		public IList<KeyValuePair<string, PathType>> Compile(TokenzierContext context, int index)
		{
			return PathParts.GetList();
		}

		public IList<KeyValuePair<string, PathType>> CompileListWithCurrent(TokenzierContext context, int index, out Func<IMorestachioError> errProducer)
		{
			var last = CompileCurrent(context, index, out errProducer);
			if (last == null)
			{
				return PathParts.GetList();
			}

			if (!(PathParts.Many && last.Value.Value == PathType.SelfAssignment))
			{
				PathParts.Add(last.Value.Key, last.Value.Value);
			}
			return PathParts.GetList();
		}

		public KeyValuePair<string, PathType>? CompileCurrent(TokenzierContext context, int index, out Func<IMorestachioError> errProducer)
		{
			if (CurrentPart == ".")
			{
				PathParts.Add(null, PathType.SelfAssignment);
			}
			else if (CurrentPart == "../")
			{
				PathParts.Add(null, PathType.ParentSelector);
			}
			else if (CurrentPart == "~")
			{
				PathParts.Add(null, PathType.RootSelector);
			}
			else if (CurrentPart == "?")
			{
				PathParts.Add(null, PathType.ObjectSelector);
			}
			else if (CurrentPart.Trim() != string.Empty)
			{
				if (!ComputeCurrentPart(context, index, out errProducer))
				{
					errProducer = () => (
						new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
								.AddWindow(new CharacterSnippedLocation(1, index, CurrentPart)),
							CurrentPart,
							"Invalid character"));
					return default;
				}
			}
			else
			{
				errProducer = null;
				return default;
			}

			if (PathParts.Last != null)
			{
				var pathPartsLast = PathParts.Last.Value;
				PathParts.RemoveLast();
				CurrentPart = "";
				errProducer = null;
				return pathPartsLast;
			}
			
			errProducer = () => (
				new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
						.AddWindow(new CharacterSnippedLocation(1, index, CurrentPart)),
					CurrentPart,
					"Invalid character"));
			return null;
		}
	}
}