using System;
using System.Collections.Generic;
using System.Text;

#pragma warning disable 1591

namespace Morestachio.Formatter.Predefined
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

		public AesCryptography Aes { get; set; }
	}
}
