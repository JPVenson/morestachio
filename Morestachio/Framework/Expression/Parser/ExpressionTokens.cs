using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Morestachio.Framework.Expression.Framework;
using Morestachio.Parsing.ParserErrors;

namespace Morestachio.Framework.Expression.Parser;

/// <summary>
///		Contains a queue where all tokens of a expression is loaded
/// </summary>
public class ExpressionTokens
{
	/// <summary>
	///		Creates a new Token queue
	/// </summary>
	/// <param name="sourceExpression"></param>
	public ExpressionTokens(string sourceExpression)
	{
		SourceExpression = sourceExpression;
		_buffer = new IExpressionToken[_growSize];
	}
		
	private int _head;
	private int _size;
	private const int _growSize = 25;
	private IExpressionToken[] _buffer;

	internal void Reset()
	{
		_head = 0;
	}

	internal void Enqueue(IExpressionToken token)
	{
		if (_buffer.Length < _size + 1)
		{
			var nArray = new IExpressionToken[_size + _growSize];
			Array.Copy(_buffer, nArray, _buffer.Length);
			_buffer = nArray;
		}

		_buffer[_size] = token;
		_size++;
	}

	internal IExpressionToken Dequeue()
	{
		var item = _buffer[_head];
		_head++;
		return item;
	}

	internal IExpressionToken TryPeek()
	{
		if (_size - 1 < _head)
		{
			return null;
		}

		return _buffer[_head];
	}

	internal IExpressionToken Peek()
	{
		return _buffer[_head];
	}
		
	internal int Count
	{
		get
		{
			return _size - _head;
		}
	}

	/// <summary>
	///		Contains the original expression in its string form
	/// </summary>
	public string SourceExpression { get; }
		
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void Loop(
		Func<IExpressionToken, bool> condition,
		Action<IExpressionToken> action)
	{
		while (Count > 0 && condition(TryPeek()))
		{
			action(Dequeue());
		}
	}
		
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void PeekLoop(
		Func<IExpressionToken, bool> condition,
		Action<IExpressionToken> action)
	{
		IExpressionToken peek;
		IExpressionToken oldPeek = default;
		while (Count > 0
				&& condition(peek = Peek()))
		{
			if (Equals(oldPeek, peek))
			{
				throw new Exception();
			}
			action(peek);
			oldPeek = peek;
		}
	}
		
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void Loop(
		Func<IExpressionToken, bool> condition,
		Func<IExpressionToken, bool> action)
	{
		while (Count > 0 && condition(Peek()))
		{
			if (!action(Dequeue()))
			{
				break;
			}
		}
	}
		
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal void PeekLoop(
		Func<IExpressionToken, bool> condition,
		Func<IExpressionToken, bool> action)
	{
		IExpressionToken peek;
		IExpressionToken oldPeek = default;
		while (Count > 0
				&& condition(peek = Peek()))
		{
			if (Equals(oldPeek, peek))
			{
				throw new Exception();
			}

			if (!action(peek))
			{
				break;
			}
			oldPeek = peek;
		}
	}

	internal IExpressionToken TryDequeue(Action onError)
	{
		if (Count == 0)
		{
			onError();
			return null;
		}

		return Dequeue();
	}

	internal void SyntaxError(
		TokenzierContext context,
		TextRange location,
		string helpText)
	{
		context.Errors.Add(new InvalidPathSyntaxError(location, SourceExpression, helpText));
	}
}