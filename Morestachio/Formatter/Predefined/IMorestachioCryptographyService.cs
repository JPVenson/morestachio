using Morestachio.Formatter.Framework.Attributes;
#pragma warning disable 1591

namespace Morestachio.Formatter.Predefined;

/// <summary>
///		Defines methods to be accessed by the {{$services.CryptService}}
/// </summary>
public interface IMorestachioCryptographyService
{
	/// <summary>
	///		The name of the Provider
	/// </summary>
	string Name { get; }
		

	[MorestachioFormatter("[MethodName]", "Encrypts the byte[] using the password byte[]. Hint: use the {{Encoding}} constant to convert a string to a byte[]")]
	byte[] Encrypt(byte[] value, byte[] password);
	[MorestachioFormatter("[MethodName]", "Decrypts the byte[] using the password byte[]. Hint: use the {{Encoding}} constant to convert a string to a byte[]")]
	byte[] Decrypt(byte[] value, byte[] password);
}