﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Morestachio.Framework.Expression.Visitors;

/// <summary>
///     Allows building of strings in a interlaced and colored way
/// </summary>
public class StringBuilderInterlaced<TColor> : IStringBuilderInterlaced<TColor>, ICollection
	where TColor : class, new()
{
	private readonly List<ITextWithColor<TColor>> _source;
	private readonly string _interlacedText;
	private TColor _color;
	private int _interlacedLevel;

	/// <summary>
	///		Creates a new <see cref="StringBuilderInterlaced{TColor}"/> that uses the set color and the interlacedText after each line break
	/// </summary>
	public StringBuilderInterlaced(string interlacedText)
	{
		_interlacedText = interlacedText;
		_source = new List<ITextWithColor<TColor>>();
		SyncRoot = new object();
	}

	/// <summary>
	///     Sets the color for all Folloring Text parts
	/// </summary>
	/// <param name="color">The color.</param>
	/// <returns></returns>
	public virtual IStringBuilderInterlaced<TColor> Color(TColor color)
	{
		_color = color;
		return this;
	}

	/// <summary>
	///     Reverts the color.
	/// </summary>
	/// <returns></returns>
	public virtual IStringBuilderInterlaced<TColor> RevertColor()
	{
		_color = null;
		return this;
	}

	/// <summary>
	///     increases all folloring Text parts by 1
	/// </summary>
	/// <returns></returns>
	public virtual IStringBuilderInterlaced<TColor> Up()
	{
		_interlacedLevel++;
		return this;
	}

	/// <summary>
	///     decreases all folloring Text parts by 1
	/// </summary>
	/// <returns></returns>
	public virtual IStringBuilderInterlaced<TColor> Down()
	{
		if (_interlacedLevel > 0)
		{
			_interlacedLevel--;
		}

		return this;
	}

	/// <summary>
	///     Appends the line.
	/// </summary>
	/// <returns></returns>
	public virtual IStringBuilderInterlaced<TColor> AppendLine()
	{
		return Append(Environment.NewLine);
	}

	private void ApplyLevel()
	{
		var text = "";

		for (var i = 0; i < _interlacedLevel; i++)
		{
			text += _interlacedText;
		}

		Add(new ColoredString(text));
	}

	private void Add(ITextWithColor<TColor> text)
	{
		lock (SyncRoot)
		{
			_source.Add(text);
			Count += text.Text?.Length ?? 0;
		}
	}

	/// <summary>
	///     Appends the interlaced line.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="color">The color.</param>
	/// <returns></returns>
	public virtual IStringBuilderInterlaced<TColor> AppendInterlacedLine(string value, TColor color = null)
	{
		ApplyLevel();
		return AppendLine(value, color);
	}

	/// <summary>
	///     Appends the interlaced.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="color">The color.</param>
	/// <returns></returns>
	public virtual IStringBuilderInterlaced<TColor> AppendInterlaced(string value, TColor color = null)
	{
		ApplyLevel();
		return Append(value, color);
	}

	/// <summary>
	///     Appends the interlaced line.
	/// </summary>
	/// <returns></returns>
	public virtual IStringBuilderInterlaced<TColor> AppendInterlacedLine()
	{
		ApplyLevel();
		return AppendLine();
	}

	/// <summary>
	///     Appends the interlaced.
	/// </summary>
	/// <returns></returns>
	public virtual IStringBuilderInterlaced<TColor> AppendInterlaced()
	{
		ApplyLevel();
		return this;
	}

	/// <summary>
	///     Inserts the specified delete.
	/// </summary>
	/// <param name="del">The delete.</param>
	/// <returns></returns>
	public virtual IStringBuilderInterlaced<TColor> Insert(Action<IStringBuilderInterlaced<TColor>> del)
	{
		del(this);
		return this;
	}

	/// <summary>
	///     Inserts the specified writer.
	/// </summary>
	/// <param name="writer">The writer.</param>
	/// <returns></returns>
	public virtual IStringBuilderInterlaced<TColor> Insert(IStringBuilderInterlaced<TColor> writer)
	{
		foreach (var textWithColor in (IEnumerable<ITextWithColor<TColor>>)writer)
		{
			Add(textWithColor);
		}

		return this;
	}

	/// <summary>
	///     Appends the specified value.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="color">The color.</param>
	/// <returns></returns>
	public virtual IStringBuilderInterlaced<TColor> Append(string value, TColor color = null)
	{
		if (color == null)
		{
			color = _color;
		}

		//value = value.Replace("\r\n", "\r\n" + Enumerable.Repeat(_interlacedText, _interlacedLevel));

		Add(new ColoredString(value, color));
		return this;
	}

	/// <summary>
	///     Appends the line.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="color">The color.</param>
	/// <returns></returns>
	public virtual IStringBuilderInterlaced<TColor> AppendLine(string value, TColor color = null)
	{
		return Append(value + Environment.NewLine, color);
	}

	/// <summary>
	///     Appends the specified value.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="values">The values.</param>
	/// <returns></returns>
	public virtual IStringBuilderInterlaced<TColor> Append(string value, params object[] values)
	{
		return Append(string.Format(value, values));
	}

	/// <summary>
	///     Appends the line.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="values">The values.</param>
	/// <returns></returns>
	public virtual IStringBuilderInterlaced<TColor> AppendLine(string value, params object[] values)
	{
		return Append(string.Format(value, values) + Environment.NewLine);
	}

	/// <summary>
	///     Appends the interlaced line.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="values">The values.</param>
	/// <returns></returns>
	public virtual IStringBuilderInterlaced<TColor> AppendInterlacedLine(string value, params object[] values)
	{
		return AppendInterlacedLine(string.Format(value, values));
	}

	/// <summary>
	///     Appends the interlaced.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="values">The values.</param>
	/// <returns></returns>
	public virtual IStringBuilderInterlaced<TColor> AppendInterlaced(string value, params object[] values)
	{
		return AppendInterlaced(string.Format(value, values));
	}

	/// <summary>
	///     Appends the specified value.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="color">The color.</param>
	/// <param name="values">The values.</param>
	/// <returns></returns>
	public virtual IStringBuilderInterlaced<TColor> Append(string value, TColor color = null, params object[] values)
	{
		return Append(string.Format(value, values), color);
	}

	/// <summary>
	///     Appends the line.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="color">The color.</param>
	/// <param name="values">The values.</param>
	/// <returns></returns>
	public virtual IStringBuilderInterlaced<TColor> AppendLine(string value,
																TColor color = null,
																params object[] values)
	{
		Append(string.Format(value, values) + Environment.NewLine, color);
		return this;
	}

	/// <summary>
	///     Appends the interlaced line.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="color">The color.</param>
	/// <param name="values">The values.</param>
	/// <returns></returns>
	public virtual IStringBuilderInterlaced<TColor> AppendInterlacedLine(
		string value,
		TColor color = null,
		params object[] values)
	{
		return AppendInterlacedLine(string.Format(value, values), color);
	}

	/// <summary>
	///     Appends the interlaced.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <param name="color">The color.</param>
	/// <param name="values">The values.</param>
	/// <returns></returns>
	public virtual IStringBuilderInterlaced<TColor> AppendInterlaced(string value,
																	TColor color = null,
																	params object[] values)
	{
		return AppendInterlaced(string.Format(value, values), color);
	}

	/// <summary>
	///     Writes to steam.
	/// </summary>
	/// <param name="output">The output.</param>
	/// <param name="changeColor">Color of the change.</param>
	/// <param name="changeColorBack">The change color back.</param>
	public virtual void WriteToSteam(TextWriter output, Action<TColor> changeColor, Action changeColorBack)
	{
		TColor color = null;
		var sb = new StringBuilder();

		lock (SyncRoot)
		{
			foreach (var coloredString in _source)
			{
				var nColor = coloredString.Color;

				if (nColor != color && sb.Length > 0)
				{
					//write buffer to output
					if (color != null)
					{
						changeColor(color);
					}

					output.Write(sb.ToString());

					if (color != null)
					{
						changeColorBack();
					}

					sb.Clear();
				}

				sb.Append(coloredString);
				color = nColor;
			}
		}

		if (color != null)
		{
			changeColor(color);
		}

		output.Write(sb.ToString());

		if (color != null)
		{
			changeColorBack();
		}
	}

	/// <summary>
	///     Returns a <see cref="System.String" /> that represents all text parts without any color
	/// </summary>
	/// <returns>
	///     A <see cref="System.String" /> that represents this instance.
	/// </returns>
	public override string ToString()
	{
		if (_source.Count == 0)
		{
			return string.Empty;
		}

		return _source.Select(f => f.ToString()).Aggregate((e, f) => e + f).ToString();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return ((IEnumerable<string>)this).GetEnumerator();
	}

	IEnumerator<string> IEnumerable<string>.GetEnumerator()
	{
		return this._source.Select(f => f.Text).GetEnumerator();
	}

	IEnumerator<ITextWithColor<TColor>> IEnumerable<ITextWithColor<TColor>>.GetEnumerator()
	{
		return this._source.GetEnumerator();
	}

	/// <summary>
	/// throws NotImplementedException
	/// </summary>
	/// <param name="array"></param>
	/// <param name="index"></param>
	public void CopyTo(Array array, int index)
	{
		throw new NotImplementedException();
	}

	/// <summary>
	/// return the Count of all Text-String elements
	/// </summary>
	public int Count { get; private set; }

	/// <summary>
	/// Returns the internal String length
	/// </summary>
	public int Length { get; set; }

	/// <inheritdoc />
	public object SyncRoot { get; private set; }

	/// <inheritdoc />
	public bool IsSynchronized
	{
		get { return Monitor.IsEntered(SyncRoot); }
	}

	readonly struct ColoredString : ITextWithColor<TColor>
	{
		private readonly string _text;
		private readonly TColor _color;

		public ColoredString(string text, TColor color = null)
		{
			_color = color;
			_text = text;
		}

		public TColor GetColor()
		{
			return _color;
		}

		public override string ToString()
		{
			return _text;
		}

		public TColor Color
		{
			get { return _color; }
		}

		public string Text
		{
			get { return _text; }
		}
	}
}