﻿using System;
using System.Text;

namespace Morestachio.Parsing.ParserErrors;

/// <summary>
///		Defines a Error while parsing a Template
/// </summary>
public interface IMorestachioError
{
	/// <summary>
	///		The location within the Template where the error occured
	/// </summary>
	CharacterLocationExtended Location { get; }

	/// <summary>
	/// Gets the exception.
	/// </summary>
	/// <returns></returns>
	Exception GetException();

	/// <summary>
	///		Gets a string that describes the Error
	/// </summary>
	string HelpText { get; }

	/// <summary>
	///		Formats the contents of the error into an <see cref="StringBuilder"/>
	/// </summary>
	/// <param name="sb"></param>
	void Format(StringBuilder sb);
}