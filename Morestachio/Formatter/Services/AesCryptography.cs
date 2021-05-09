using System.IO;
using System.Security.Cryptography;
using Morestachio.Formatter.Framework.Attributes;
using Morestachio.Formatter.Predefined;

namespace Morestachio.Formatter.Services
{
	/// <summary>
	///		Uses the AES Cryptography
	/// </summary>
	public class AesCryptography : IMorestachioCryptographyService
	{
		/// <summary>
		/// 
		/// </summary>
		public AesCryptography()
		{
			Name = "AES";
		}

		/// <inheritdoc />
		public string Name { get; }
		
		/// <inheritdoc />
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
		
		/// <inheritdoc />
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