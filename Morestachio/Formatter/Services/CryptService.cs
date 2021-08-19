using Morestachio.Formatter.Predefined;

#pragma warning disable 1591

namespace Morestachio.Formatter.Services
{
	/// <summary>
	///		This class contains formatters for encrypting and decrypting data
	/// </summary>
	public class CryptService
	{
		public CryptService()
		{
			Aes = new AesCryptography();
		}

		public IMorestachioCryptographyService Aes { get; set; }
	}
}
