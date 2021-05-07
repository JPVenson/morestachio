namespace Morestachio.Formatter.Predefined
{
	public interface IMorestachioCryptographyService
	{
		string Name { get; }

		byte[] Encrypt(byte[] value, byte[] password);
		byte[] Decrypt(byte[] value, byte[] password);
	}
}