using System;
using System.Collections.Generic;
using System.Linq;
using Morestachio.ParserErrors;

namespace Morestachio.Framework.Expression
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

		/// <summary>
		///		Create a new Number based on the value of <see cref="PathPart.Value"/>
		/// </summary>
		Number,

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
			PathParts = new List<KeyValuePair<string, PathType>>();
			CurrentPart = "";
		}

		private IList<KeyValuePair<string, PathType>> PathParts { get; set; }

		public string CurrentPart { get; set; }
		public bool LastCharWasDelimiter { get; set; }
		public bool CurrentPartIsNumber { get; set; }

		public bool Add(char c, TokenzierContext context, int index)
		{
			if (!Tokenizer.IsExpressionChar(c))
			{
				context.Errors.Add(
					new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
							.AddWindow(new CharacterSnippedLocation(1, index, CurrentPart)),
						CurrentPart));
				return false;
			}

			if (PathParts.Any(f => f.Value == PathType.Null))
			{
				context.Errors.Add(
					new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
							.AddWindow(new CharacterSnippedLocation(1, index, CurrentPart)),
						CurrentPart,
						"Nothing can follow on a null"));
			}

			LastCharWasDelimiter = c == '.';

			if (c == '/')
			{
				if (CurrentPart == "..")
				{
					if (PathParts.Any() && PathParts.Any(e => e.Value != PathType.ParentSelector))
					{
						context.Errors.Add(
							new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
									.AddWindow(new CharacterSnippedLocation(1, index, CurrentPart)),
								CurrentPart,
								"An Parent selector '..\\' can only follow on another parent selector like and never on an root or an data selector"));

						return false;
					}
					PathParts.Add(new KeyValuePair<string, PathType>(null, PathType.ParentSelector));
					CurrentPart = "";
					return true;
				}
				context.Errors.Add(
					new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
							.AddWindow(new CharacterSnippedLocation(1, index, CurrentPart)),
						CurrentPart,
						"Unexpected '/'. Expected ether the start of an expression or an './'"));
				return false;
			}

			if (c == '~')
			{
				if (CurrentPart != string.Empty || PathParts.Any())
				{
					context.Errors.Add(
						new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
								.AddWindow(new CharacterSnippedLocation(1, index, CurrentPart)),
							CurrentPart,
							"An root selector '~' must be at the start of an expression"));

					return false;
				}

				PathParts.Add(new KeyValuePair<string, PathType>(null, PathType.RootSelector));
				CurrentPart = "";
				return true;
			}

			if (c == '?')
			{
				PathParts.Add(new KeyValuePair<string, PathType>(null, PathType.ObjectSelector));
				CurrentPart = "";
				return true;
			}

			if (c != '.' && CurrentPart == "." && PathParts.Count == 0)
			{
				//in this case somebody wrote .data
				//so ignore the dot
				CurrentPart = "";
			}

			if (CurrentPart != "" && CurrentPart != "." && c == '.')
			{
				if (CurrentPartIsNumber)
				{
					if (CurrentPart.Contains("."))
					{
						PathParts.Add(new KeyValuePair<string, PathType>(CurrentPart, PathType.Number));
						CurrentPart = "";
					}
					else
					{
						CurrentPart += c;
					}

					return true;
				}

				if (!ComputeCurrentPart(context, index))
				{
					return false;
				}

				CurrentPart = "";
			}
			else
			{
				if (CurrentPart == string.Empty && char.IsDigit(c))
				{
					CurrentPartIsNumber = true;
					if (PathParts.Any())
					{
						context.Errors.Add(
							new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
									.AddWindow(new CharacterSnippedLocation(1, index, CurrentPart)),
								CurrentPart,
								"A number expression must be at the start of the expression and cannot follow on anything else"));

						return false;
					}
				}

				CurrentPart += c;
			}

			return true;
		}

		private bool ComputeCurrentPart(TokenzierContext context, int index)
		{
			var checkPathPart = CheckPathPart();
			if (checkPathPart != -1)
			{
				context.Errors.Add(
					new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
							.AddWindow(new CharacterSnippedLocation(1, checkPathPart, CurrentPart)),
						CurrentPart));

				return false;
			}

			if (CurrentPart == "null")
			{
				if (PathParts.Any())
				{
					context.Errors.Add(
						new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
								.AddWindow(new CharacterSnippedLocation(1, index, CurrentPart)),
							CurrentPart,
							"An null must be at the start of an expression"));

					return false;
				}

				PathParts.Add(new KeyValuePair<string, PathType>(null, PathType.Null));
			}
			else if (CurrentPart == "true" || CurrentPart == "false")
			{
				if (PathParts.Any())
				{
					context.Errors.Add(
						new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
								.AddWindow(new CharacterSnippedLocation(1, index, CurrentPart)),
							CurrentPart,
							"An boolean must be at the start of an expression"));

					return false;
				}

				PathParts.Add(new KeyValuePair<string, PathType>(CurrentPart, PathType.Boolean));
			}
			else
			{
				PathParts.Add(new KeyValuePair<string, PathType>(CurrentPart, PathType.DataPath));
			}

			return true;
		}

		private int CheckPathPart()
		{
			var part = CurrentPart;
			if (part.StartsWith("$") && part != "$")
			{
				part = part.Substring(1, part.Length - 1);
			}

			var match = Tokenizer.NegativeCharRegex.Match(part);
			if (match.Success)
			{
				return match.Index;
			}

			return -1;
		}

		public string GetFormatterName()
		{
			string lastPartIsFormatterName;//PathParts.LastOrDefault().Key;
			if (LastCharWasDelimiter)
			{
				lastPartIsFormatterName = CurrentPart;
			}
			else
			{
				lastPartIsFormatterName = PathParts.LastOrDefault().Key;
				PathParts.RemoveAt(PathParts.Count - 1);
			}

			CurrentPart = "";
			return lastPartIsFormatterName;
		}

		public IList<KeyValuePair<string, PathType>> Compile(TokenzierContext context, int index)
		{
			if (CurrentPart == ".")
			{
				PathParts.Add(new KeyValuePair<string, PathType>(null, PathType.SelfAssignment));
			}
			else if (CurrentPart == "../")
			{
				PathParts.Add(new KeyValuePair<string, PathType>(null, PathType.ParentSelector));
			}
			else if (CurrentPart == "~")
			{
				PathParts.Add(new KeyValuePair<string, PathType>(null, PathType.RootSelector));
			}
			else if (CurrentPart == "?")
			{
				PathParts.Add(new KeyValuePair<string, PathType>(null, PathType.ObjectSelector));
			}
			else if (CurrentPart.Trim() != "")
			{
				if (CurrentPartIsNumber)
				{
					PathParts.Add(new KeyValuePair<string, PathType>(CurrentPart, PathType.Number));
				}
				else
				{
					if (!ComputeCurrentPart(context, index))
					{
						return new KeyValuePair<string, PathType>[0];
					}
				}
			}

			if (PathParts.Count > 1 && PathParts.Last().Value == PathType.SelfAssignment)
			{
				PathParts.Remove(PathParts.Last());
			}

			//if (!PathParts.Any())
			//{
			//	hasError = 0;
			//	return PathParts;
			//}

			CurrentPart = "";
			return PathParts;
		}
	}
}