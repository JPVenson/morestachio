using System.Security.Cryptography;
using System.Text;
using Morestachio.Formatter.Framework;
using Morestachio.Formatter.Framework.Attributes;

#pragma warning disable 1591

namespace Morestachio.Formatter.Services;

/// <summary>
///		This class contains formatters for hashing data
/// </summary>
public class HashService
{
	public HashService()
	{
	}

	private static byte[] HashWithUtf8(HashAlgorithm hashAlgorithm, string data)
	{
		return hashAlgorithm.ComputeHash(Encoding.UTF8.GetBytes(data));
	}

	public HashAlgorithm Md5
	{
		get { return MD5.Create(); }
	}

	public HashAlgorithm Sha1
	{
		get { return SHA1.Create(); }
	}

	public HashAlgorithm Sha256
	{
		get { return SHA256.Create(); }
	}

	public HashAlgorithm Sha384
	{
		get { return SHA384.Create(); }
	}

	public HashAlgorithm Sha512
	{
		get { return SHA512.Create(); }
	}

	[MorestachioFormatter("[MethodName]", "Hashes the argument with the set HashAlgorithm")]
	public byte[] HashWith(HashAlgorithm hashAlgorithm, byte[] data)
	{
		return hashAlgorithm.ComputeHash(data);
	}

	[MorestachioFormatter("[MethodName]", "Hashes the argument with the set HashAlgorithm and the set Encoding")]
	public byte[] HashWith(HashAlgorithm hashAlgorithm, string data, Encoding encoding)
	{
		return HashWith(hashAlgorithm, encoding.GetBytes(data));
	}

	[MorestachioFormatter("[MethodName]", "Hashes the argument with Md5")]
	public byte[] HashWithMd5(byte[] data)
	{
		return HashWith(MD5.Create(), data);
	}

	[MorestachioFormatter("[MethodName]", "Hashes the argument with Sha1")]
	public byte[] HashWithSha1(byte[] data)
	{
		return HashWith(SHA1.Create(), data);
	}

	[MorestachioFormatter("[MethodName]", "Hashes the argument with Sha256")]
	public byte[] HashWithSha256(byte[] data)
	{
		return HashWith(SHA256.Create(), data);
	}

	[MorestachioFormatter("[MethodName]", "Hashes the argument with Sha384")]
	public byte[] HashWithSha384(byte[] data)
	{
		return HashWith(SHA384.Create(), data);
	}

	[MorestachioFormatter("[MethodName]", "Hashes the argument with Sha512")]
	public byte[] HashWithSha512(byte[] data)
	{
		return HashWith(SHA384.Create(), data);
	}

	[MorestachioFormatter("[MethodName]", "Hashes the argument as UTF8 with Md5")]
	public byte[] HashWithMd5Utf8(string data)
	{
		return HashWithUtf8(MD5.Create(), data);
	}

	[MorestachioFormatter("[MethodName]", "Hashes the argument as UTF8 with Sha1")]
	public byte[] HashWithSha1Utf8(string data)
	{
		return HashWithUtf8(SHA1.Create(), data);
	}

	[MorestachioFormatter("[MethodName]", "Hashes the argument as UTF8 with Sha256")]
	public byte[] HashWithSha256Utf8(string data)
	{
		return HashWithUtf8(SHA256.Create(), data);
	}

	[MorestachioFormatter("[MethodName]", "Hashes the argument as UTF8 with Sha384")]
	public byte[] HashWithSha384Utf8(string data)
	{
		return HashWithUtf8(SHA384.Create(), data);
	}

	[MorestachioFormatter("[MethodName]", "Hashes the argument as UTF8 with Sha512")]
	public byte[] HashWithSha512Utf8(string data)
	{
		return HashWithUtf8(SHA384.Create(), data);
	}
}