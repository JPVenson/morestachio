using System.Xml;
using Morestachio.Framework.Error;

namespace Morestachio.Parsing.ParserErrors;

/// <summary>
///		Defines an Error on a position within the Template.
/// </summary>
[Serializable]
public class InvalidPathSyntaxError : MorestachioErrorBase
{
	/// <inheritdoc />
	protected InvalidPathSyntaxError()
	{
		
	}

	/// <inheritdoc />
	protected InvalidPathSyntaxError(SerializationInfo info, StreamingContext c) 
		: base(info, c)
	{
		Token = info.GetString(nameof(Token));
	}
	
	/// <inheritdoc />
	public InvalidPathSyntaxError(CharacterLocationExtended location, string token, string helpText = null)
		: base(location, FormatHelpText(location, token, helpText))
	{
		Token = token;
	}

	private static string FormatHelpText(CharacterLocationExtended location,
										string token,
										string userHelpText)
	{
		var helpText =
			$"line:char '{location.Line}:{location.Character}' - The path '{token}' is not valid. Please see documentation for examples of valid paths.";
		if (userHelpText != null)
		{
			helpText += "\r\n" + userHelpText;
		}

		return helpText;
	}

	/// <summary>
	/// Gets the token.
	/// </summary>
	/// <value>
	/// The token.
	/// </value>
	public string Token { get; private set; }
	
	/// <inheritdoc />
	public override void ReadXml(XmlReader reader)
	{
		Token = reader.GetAttribute(nameof(Token));
		base.ReadXml(reader);
	}

	/// <inheritdoc />
	public override void WriteXml(XmlWriter writer)
	{
		writer.WriteAttributeString(nameof(Token), Token);
		base.WriteXml(writer);
	}

	/// <inheritdoc />
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		info.AddValue(nameof(Token), Token);
		base.GetObjectData(info, context);
	}

	/// <inheritdoc />
	public override bool Equals(IMorestachioError other)
	{
		return base.Equals(other) && (other is InvalidPathSyntaxError invalidPathSyntaxError) && invalidPathSyntaxError.Token.Equals(Token);
	}
}