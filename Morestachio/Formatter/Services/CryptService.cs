using Morestachio.Formatter.Predefined;

#pragma warning disable 1591

namespace Morestachio.Formatter.Services
{
	/// <summary>
	///		This class contains formatters for encrypting and decrypting data
	/// </summary>
	public class CryptService
	{
		private CryptService()
		{
			Aes = new AesCryptography();
		}

		private static CryptService _instance;
		public static CryptService Instance
		{
			get { return _instance ?? (_instance = new CryptService()); }
		}

		public IMorestachioCryptographyService Aes { get; set; }
	}
}
