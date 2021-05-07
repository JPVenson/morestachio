using System.IO;
using System.Security.Cryptography;
using Morestachio.Formatter.Framework.Attributes;

namespace Morestachio.Formatter.Predefined
{
	public class AesCryptography : IMorestachioCryptographyService
	{
		public AesCryptography()
		{
			Name = "AES";
		}

		public string Name { get; }
		[MorestachioFormatter("[MethodName]", "Encrypts the byte[] using the password byte[]")]
		public byte[] Encrypt(byte[] value, byte[] password)
		{
			byte[] iv = new byte[16];
			using (Aes aes = Aes.Create())
			{
				aes.Key = password;
				aes.IV = iv;

				var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

				using (MemoryStream memoryStream = new MemoryStream())
				{
					using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
					{
						cryptoStream.Write(value, 0, value.Length);
						return memoryStream.ToArray();
					}
				}
			}

		}

		[MorestachioFormatter("[MethodName]", "Decrypts the byte[] using the password byte[]")]
		public byte[] Decrypt(byte[] value, byte[] password)
		{
			byte[] iv = new byte[16];
			using (Aes aes = Aes.Create())
			{
				aes.Key = password;
				aes.IV = iv;

				var encryptor = aes.CreateDecryptor(aes.Key, aes.IV);

				using (MemoryStream memoryStream = new MemoryStream())
				{
					using (CryptoStream cryptoStream = new CryptoStream((Stream)memoryStream, encryptor, CryptoStreamMode.Write))
					{
						cryptoStream.Write(value, 0, value.Length);
						return memoryStream.ToArray();
					}
				}
			}
		}
	}
}