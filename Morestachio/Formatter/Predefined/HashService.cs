using System.Security.Cryptography;
using System.Text;
using Morestachio.Formatter.Framework;

#pragma warning disable 1591

namespace Morestachio.Formatter.Predefined
{
	/// <summary>
	///		This class contains formatters for hashing data
	/// </summary>
	public class HashService
	{
		private static HashService _instance;
		public static HashService Instance
		{
			get { return _instance ?? (_instance = new HashService()); }
		}

		public HashService()
		{
		}

		private static byte[] HashWith(HashAlgorithm hashAlgorithm, byte[] data)
		{
			return hashAlgorithm.ComputeHash(data);
		}

		private static byte[] HashWithUtf8(HashAlgorithm hashAlgorithm, string data)
		{
			return hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(data));
		}

		[MorestachioGlobalFormatter("[MethodName]", "Hashes the argument with Md5")]
		public byte[] HashWithMd5(byte[] data)
		{
			return HashWith(MD5.Create(), data);
		}
		
		[MorestachioGlobalFormatter("[MethodName]", "Hashes the argument with Sha1")]
		public byte[] HashWithSha1(byte[] data)
		{
			return HashWith(SHA1.Create(), data);
		}
		
		[MorestachioGlobalFormatter("[MethodName]", "Hashes the argument with Sha256")]
		public byte[] HashWithSha256(byte[] data)
		{
			return HashWith(SHA256.Create(), data);
		}
		
		[MorestachioGlobalFormatter("[MethodName]", "Hashes the argument with Sha384")]
		public byte[] HashWithSha384(byte[] data)
		{
			return HashWith(SHA384.Create(), data);
		}
		
		[MorestachioGlobalFormatter("[MethodName]", "Hashes the argument with Sha512")]
		public byte[] HashWithSha512(byte[] data)
		{
			return HashWith(SHA384.Create(), data);
		}
		
		[MorestachioGlobalFormatter("[MethodName]", "Hashes the argument as UTF8 with Md5")]
		public byte[] HashWithMd5Utf8(string data)
		{
			return HashWithUtf8(MD5.Create(), data);
		}

		[MorestachioGlobalFormatter("[MethodName]", "Hashes the argument as UTF8 with Sha1")]
		public byte[] HashWithSha1Utf8(string data)
		{
			return HashWithUtf8(SHA1.Create(), data);
		}
		
		[MorestachioGlobalFormatter("[MethodName]", "Hashes the argument as UTF8 with Sha256")]
		public byte[] HashWithSha256Utf8(string data)
		{
			return HashWithUtf8(SHA256.Create(), data);
		}
		
		[MorestachioGlobalFormatter("[MethodName]", "Hashes the argument as UTF8 with Sha384")]
		public byte[] HashWithSha384Utf8(string data)
		{
			return HashWithUtf8(SHA384.Create(), data);
		}
		
		[MorestachioGlobalFormatter("[MethodName]", "Hashes the argument as UTF8 with Sha512")]
		public byte[] HashWithSha512Utf8(string data)
		{
			return HashWithUtf8(SHA384.Create(), data);
		}
	}
}