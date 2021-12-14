using System;
using System.Collections.Generic;
using System.Text;
using Morestachio.Framework.Tokenizing;
using Morestachio.Parsing.ParserErrors;
using Morestachio.Util;

namespace Morestachio.Framework.Expression.Framework;
#pragma warning disable 1591

/// <summary>
///		This path is internal and should not be used in your code
/// </summary>
internal class PathTokenizer
{
	public PathTokenizer()
	{
		PathParts = new PathPartsCollection();
		_currentPart = StringBuilderCache.Acquire();
	}

	private PathPartsCollection PathParts { get; set; }
	internal class PathPartsCollection
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

	private StringBuilder _currentPart;
	public bool LastCharWasDelimiter { get; set; }

	private bool PartEquals(string text, int offset = 0)
	{
		if (_currentPart.Length != text.Length)
		{
			return false;
		}

		for (int i = offset; i < text.Length; i++)
		{
			if (_currentPart[i] != text[i])
			{
				return false;
			}
		}

		return true;
	}

	private bool PartEquals(char text, int offset = 0)
	{
		if (_currentPart.Length != 1)
		{
			return false;
		}

		return _currentPart[offset] == text;
	}

	private bool PartStartsWith(char text, int offset = 0)
	{
		if (_currentPart.Length < 1)
		{
			return false;
		}

		return _currentPart[offset] == text;
	}

	public bool Add(char c, TokenzierContext context, int index, out Func<IMorestachioError> errProducer)
	{
		if (!Tokenizer.IsExpressionPathChar(c))
		{
			var text = _currentPart.ToString();
			errProducer = () => new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
				.AddWindow(new CharacterSnippedLocation(1, index, text)), text);
			return false;
		}

		if (PathParts.IsNullValue)
		{
			var text = _currentPart.ToString();
			errProducer = () => new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
					.AddWindow(new CharacterSnippedLocation(1, index, text)),
				text,
				"Nothing can follow on a null");
			return false;
		}

		if (c == '/')
		{
			if (PartEquals(".."))
			{
				if (PathParts.Any && !PathParts.HasParentSelector)
				{
					var text = _currentPart.ToString();
					errProducer = () => new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
							.AddWindow(new CharacterSnippedLocation(1, index, text)),
						text,
						"An Parent selector '..\\' can only follow on another parent selector like and never on an root or an data selector");

					return false;
				}
				errProducer = null;
				PathParts.Add(null, PathType.ParentSelector);
				_currentPart.Clear();
				return true;
			}
			var errText = _currentPart.ToString();
			errProducer = () => new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
					.AddWindow(new CharacterSnippedLocation(1, index, errText)),
				errText,
				"Unexpected '/'. Expected ether the start of an expression or an './'");
			return false;
		}

		if (c == '~')
		{
			if (_currentPart.Length > 0 || PathParts.Any)
			{
				var text = _currentPart.ToString();
				errProducer = () => new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
						.AddWindow(new CharacterSnippedLocation(1, index, text)),
					text,
					"An root selector '~' must be at the start of an expression");

				return false;
			}
				
			errProducer = null;
			PathParts.Add(null, PathType.RootSelector);
			_currentPart.Clear();
			return true;
		}

		//otherwise an ?? null cor operator would be tokenized as two times a object selector
		if (c == '?')
		{
			errProducer = null;
			if (!LastCharWasDelimiter)
			{
				return false;
			}
			PathParts.Add(null, PathType.ObjectSelector);
			_currentPart.Clear();
			return true;
		}

		if (c != '.' && PartEquals('.') && !PathParts.Any)
		{
			//in this case somebody wrote .data
			//so ignore the dot
			_currentPart.Clear();
		}
		LastCharWasDelimiter = c == '.';

		if (_currentPart.Length > 0 && !PartEquals('.') && c == '.')
		{
			if (!ComputeCurrentPart(context, index, out errProducer))
			{
				return false;
			}
				
			_currentPart.Clear();
		}
		else
		{
			_currentPart.Append(c);
		}
			
		errProducer = null;
		return true;
	}

	private bool ComputeCurrentPart(TokenzierContext context, int index, out Func<IMorestachioError> errProducer)
	{
		errProducer = null;
		var checkPathPart = CheckPathPart();
		if (checkPathPart != -1)
		{
			var text = _currentPart.ToString();
			errProducer = () => (
				new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
						.AddWindow(new CharacterSnippedLocation(1, checkPathPart, text)),
					text));

			return false;
		}

		if (PartEquals("null"))
		{
			if (PathParts.Any)
			{
				var text = _currentPart.ToString();
				errProducer = () => (
					new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
							.AddWindow(new CharacterSnippedLocation(1, index, text)),
						text,
						"An null must be at the start of an expression"));

				return false;
			}
			PathParts.Add(null, PathType.Null);
			return true;
		}

		if (PartEquals("this"))
		{
			if (PathParts.Any)
			{
				var text = _currentPart.ToString();
				errProducer = () => (
					new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
							.AddWindow(new CharacterSnippedLocation(1, index, text)),
						text,
						"An 'this' must be at the start of an expression"));

				return false;
			}
			PathParts.Add(null, PathType.ThisPath);
			return true;
		}

		if (PartEquals('.'))
		{
			if (PathParts.Any)
			{
				var text = _currentPart.ToString();
				errProducer = () => (
					new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
							.AddWindow(new CharacterSnippedLocation(1, index, text)),
						text,
						"An '.' must be at the start of an expression"));

				return false;
			}
			PathParts.Add(null, PathType.SelfAssignment);
			return true;
		}

		if (PartEquals("true") || PartEquals("false"))
		{
			if (PathParts.Any)
			{
				var text = _currentPart.ToString();
				errProducer = () => (
					new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
							.AddWindow(new CharacterSnippedLocation(1, index, text)),
						text,
						"An boolean must be at the start of an expression"));

				return false;
			}

			PathParts.Add(_currentPart.ToString(), PathType.Boolean);
			return true;
		}
		if (PartEquals("../"))
		{
			PathParts.Add(null, PathType.ParentSelector);
			return true;
		}
		if (PartEquals('~'))
		{
			PathParts.Add(null, PathType.RootSelector);
			return true;
		}
		if (PartEquals('?'))
		{
			PathParts.Add(null, PathType.ObjectSelector);
			return true;
		}
		PathParts.Add(_currentPart.ToString(), PathType.DataPath);
		return true;
	}

	private int CheckPathPart()
	{
		var offset = 0;
		if (PartStartsWith('$') && !PartEquals('$'))
		{
			offset = 1;
		}

		if (PartStartsWith('.', offset))
		{
			if (PartEquals("../", offset))
			{
				return -1;
			}
			if (PartEquals('.', offset))
			{
				return -1;
			}

			return 0;
		}
		if (PartEquals('~', offset))
		{
			return -1;
		}
		if (PartEquals('?', offset))
		{
			return -1;
		}

		for (int i = offset; i < _currentPart.Length; i++)
		{
			if (!Tokenizer.IsExpressionDataPathChar(_currentPart[i]))
			{
				return i;
			}
		}

		return -1;
	}

	public string GetFormatterName(TokenzierContext context, int index)
	{
		var last = CompileCurrent(context, index);

		if (last == null)
		{
			if (_currentPart.Length == 0)
			{
				last = new KeyValuePair<string, PathType>(string.Empty, PathType.DataPath);
			}
			else
			{
				return null;
			}
		}

		if (last.Value.Value != PathType.DataPath)
		{
			PathParts.Add(last.Value.Key, last.Value.Value);
			return string.Empty;
		}
		return last.Value.Key;
	}

	public IList<KeyValuePair<string, PathType>> Compile(TokenzierContext context, int index)
	{
		StringBuilderCache.Release(_currentPart);
		return PathParts.GetList();
	}

	public IList<KeyValuePair<string, PathType>> CompileListWithCurrent(TokenzierContext context, int index)
	{
		var last = CompileCurrent(context, index);
		StringBuilderCache.Release(_currentPart);
		if (last == null)
		{
			return PathParts.GetList();
		}

		if (!(PathParts.Many && (last.Value.Value == PathType.SelfAssignment || last.Value.Value == PathType.ThisPath)))
		{
			PathParts.Add(last.Value.Key, last.Value.Value);
		}
		return PathParts.GetList();
	}

	public KeyValuePair<string, PathType>? CompileCurrent(TokenzierContext context, int index)
	{
		if (PartEquals('.'))
		{
			PathParts.Add(null, PathType.SelfAssignment);
		}
		else if (PartEquals("../"))
		{
			PathParts.Add(null, PathType.ParentSelector);
		}
		else if (PartEquals('~'))
		{
			PathParts.Add(null, PathType.RootSelector);
		}
		else if (PartEquals('?'))
		{
			PathParts.Add(null, PathType.ObjectSelector);
		}
		else if (_currentPart.Length > 0 /*.Trim() != string.Empty*/)
		{
			if (!ComputeCurrentPart(context, index, out var errProducer))
			{
				var text = _currentPart.ToString();
				errProducer();
				context.Errors.Add(new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
						.AddWindow(new CharacterSnippedLocation(1, index, text)),
					text,
					"Invalid character"));
				return default;
			}
		}
		else
		{
			return default;
		}

		if (PathParts.Last != null)
		{
			var pathPartsLast = PathParts.Last.Value;
			PathParts.RemoveLast();
			_currentPart.Clear();
			return pathPartsLast;
		}
			
		var errText = _currentPart.ToString();
		context.Errors.Add(
			new InvalidPathSyntaxError(context.CurrentLocation.Offset(index)
					.AddWindow(new CharacterSnippedLocation(1, index, errText)),
				errText,
				"Invalid character"));
		return null;
	}
}