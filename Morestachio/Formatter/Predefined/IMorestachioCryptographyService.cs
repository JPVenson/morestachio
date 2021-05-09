using Morestachio.Formatter.Framework.Attributes;

namespace Morestachio.Formatter.Predefined
{
	public interface IMorestachioCryptographyService
	{
		string Name { get; }
		
		[MorestachioFormatter("[MethodName]", "Encrypts the byte[] using the password byte[]")]
		byte[] Encrypt(byte[] value, byte[] password);
		[MorestachioFormatter("[MethodName]", "Decrypts the byte[] using the password byte[]")]
		byte[] Decrypt(byte[] value, byte[] password);
	}
}