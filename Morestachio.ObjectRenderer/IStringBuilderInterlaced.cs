using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace Morestachio.ObjectRenderer
{
	/// <summary>
	///		Provides Methods for Building a String with partial colored parts
	/// </summary>
	/// <typeparam name="TColor"></typeparam>
	public interface IStringBuilderInterlaced<TColor> : IEnumerable<string>, IEnumerable<StringBuilderInterlaced<TColor>.ITextWithColor> where TColor : class, new()
	{
		/// <summary>
		///		Appends the string to the buffer
		/// </summary>
		[StringFormatMethod("value")]
		IStringBuilderInterlaced<TColor> Append(string value, params object[] values);
		/// <summary>
		///		Appends the string to the buffer and assosiates a color with it
		/// </summary>
		IStringBuilderInterlaced<TColor> Append(string value, TColor color = null);
		/// <summary>
		///		Appends the string to the buffer and assosiates a color with it
		/// </summary>
		IStringBuilderInterlaced<TColor> Append(string value, TColor color = null, params object[] values);
		/// <summary>
		///		Appends the string with intents to the buffer
		/// </summary>
		IStringBuilderInterlaced<TColor> AppendInterlaced(string value, params object[] values);
		/// <summary>
		///		Appends the string with intents to the buffer and assosiates a color with it
		/// </summary>
		IStringBuilderInterlaced<TColor> AppendInterlaced(string value, TColor color = null);
		/// <summary>
		///		Appends the string with intents to the buffer and assosiates a color with it
		/// </summary>
		IStringBuilderInterlaced<TColor> AppendInterlaced(string value, TColor color = null, params object[] values);
		/// <summary>
		///		Appends the string with intents and a linebreak to the buffer
		/// </summary>
		IStringBuilderInterlaced<TColor> AppendInterlacedLine(string value, params object[] values);
		/// <summary>
		///		Appends the string with intents and a linebreak to the buffer and assosiates a color with it
		/// </summary>
		IStringBuilderInterlaced<TColor> AppendInterlacedLine(string value, TColor color = null);
		/// <summary>
		///		Appends the string with intents and a linebreak to the buffer and assosiates a color with it
		/// </summary>
		IStringBuilderInterlaced<TColor> AppendInterlacedLine(string value, TColor color = null, params object[] values);
		/// <summary>
		///		Adds a Linebreak
		/// </summary>
		IStringBuilderInterlaced<TColor> AppendLine();
		/// <summary>
		///		Appends the string and a linebreak to the buffer
		/// </summary>
		IStringBuilderInterlaced<TColor> AppendLine(string value, params object[] values);

		/// <summary>
		///		Appends the string and a linebreak to the buffer and assosiates a color with it
		/// </summary>
		IStringBuilderInterlaced<TColor> AppendLine(string value, TColor color = null);
		/// <summary>
		///		Appends the string and a linebreak to the buffer and assosiates a color with it
		/// </summary>
		IStringBuilderInterlaced<TColor> AppendLine(string value, TColor color = null, params object[] values);

		/// <summary>
		///		Sets the interal Color. this will cause all folloring text to be assosiated with that color
		/// </summary>
		/// <param name="color"></param>
		/// <returns></returns>
		IStringBuilderInterlaced<TColor> Color(TColor color);
		/// <summary>
		///		Decreases the current line Intent
		/// </summary>
		/// <returns></returns>
		IStringBuilderInterlaced<TColor> Down();
		/// <summary>
		///		Adds another <c>IStringBuilderInterlaced&lt;TColor&gt;</c>
		/// </summary>
		/// <param name="writer"></param>
		/// <returns></returns>
		IStringBuilderInterlaced<TColor> Insert(IStringBuilderInterlaced<TColor> writer);
		/// <summary>
		///		Adds another <c>IStringBuilderInterlaced&lt;TColor&gt;</c>
		/// </summary>
		/// <returns></returns>
		IStringBuilderInterlaced<TColor> Insert(Action<IStringBuilderInterlaced<TColor>> del);
		/// <summary>
		///		Clears the interal color
		/// </summary>
		/// <returns></returns>
		IStringBuilderInterlaced<TColor> RevertColor();

		/// <summary>
		///		Appends all strings
		/// </summary>
		/// <returns></returns>
		string ToString();
		/// <summary>
		///		Increses the current line Intent
		/// </summary>
		/// <returns></returns>
		IStringBuilderInterlaced<TColor> Up();

		/// <summary>
		///		Writes the Interal Buffer to a stream by using the ChangeColor method
		/// </summary>
		/// <param name="output"></param>
		/// <param name="changeColor"></param>
		/// <param name="changeColorBack"></param>
		void WriteToSteam(TextWriter output, Action<TColor> changeColor, Action changeColorBack);
	}
}