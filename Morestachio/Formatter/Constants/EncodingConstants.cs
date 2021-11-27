using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Morestachio.Formatter.Framework.Attributes;
using NetEncoding = System.Text.Encoding;

namespace Morestachio.Formatter.Constants
{
	/// <summary>
	///		Grants access to the Encodings supported
	/// </summary>
	public static class EncodingConstant
	{
		/// <summary>Gets an encoding for the ASCII (7-bit) character set.</summary>
		/// <returns>An  encoding for the ASCII (7-bit) character set.</returns>
		[Description("Gets an encoding for the ASCII (7-bit) character set.")]
		public static Encoding ASCII
		{
			get { return new Encoding(NetEncoding.ASCII); }
		}
		
		/// <summary>Gets an encoding for the UTF-16 format that uses the big endian byte order.</summary>
		/// <returns>An encoding object for the UTF-16 format that uses the big endian byte order.</returns>
		[Description("Gets an encoding for the UTF-16 format that uses the big endian byte order.")]
		public static Encoding BigEndianUnicode
		{
			get { return new Encoding(NetEncoding.BigEndianUnicode); }
		}
		
		/// <summary>Gets an encoding for the operating system's current ANSI code page.</summary>
		/// <returns>An encoding for the operating system's current ANSI code page.</returns>
		[Description("Gets an encoding for the operating system's current ANSI code page.")]
		public static Encoding Default
		{
			get { return new Encoding(NetEncoding.Default); }
		}
		
		/// <summary>Gets an encoding for the UTF-16 format using the little endian byte order.</summary>
		/// <returns>An encoding for the UTF-16 format using the little endian byte order.</returns>
		[Description("Gets an encoding for the UTF-16 format using the little endian byte order.")]
		public static Encoding Unicode
		{
			get { return new Encoding(NetEncoding.Unicode); }
		}
		
		/// <summary>Gets an encoding for the UTF-32 format using the little endian byte order.</summary>
		/// <returns>An  encoding object for the UTF-32 format using the little endian byte order.</returns>
		[Description("Gets an encoding for the UTF-32 format using the little endian byte order.")]
		public static Encoding UTF32
		{
			get { return new Encoding(NetEncoding.UTF32); }
		}
		
		/// <summary>Gets an encoding for the UTF-7 format.</summary>
		/// <returns>An encoding for the UTF-7 format.</returns>
		[Description("Gets an encoding for the UTF-7 format.")]
		public static Encoding UTF7
		{
			get { return new Encoding(NetEncoding.UTF7); }
		}
		
		/// <summary>Gets an encoding for the UTF-8 format.</summary>
		/// <returns>An encoding for the UTF-8 format.</returns>
		[Description("Gets an encoding for the UTF-8 format.")]
		public static Encoding UTF8
		{
			get { return new Encoding(NetEncoding.UTF8); }
		}

		/// <summary>Returns the encoding associated with the specified code page identifier.</summary>
		/// <param name="codePage">The code page identifier of the preferred encoding. Possible values are listed in the Code Page column of the table that appears in the <see cref="T:System.Text.Encoding"></see> class topic.   -or-   0 (zero), to use the default encoding.</param>
		/// <returns>The encoding that is associated with the specified code page.</returns>
		[MorestachioFormatter("[MethodName]", "Returns the encoding associated with the specified code page identifier")]
		public static Encoding GetEncoding(int codePage)
		{
			return new Encoding(NetEncoding.GetEncoding(codePage));
		}

		/// <summary>Returns the encoding associated with the specified code page name.</summary>
		/// <param name="name">The code page name of the preferred encoding. Any value returned by the <see cref="P:System.Text.Encoding.WebName"></see> property is valid. Possible values are listed in the Name column of the table that appears in the <see cref="T:System.Text.Encoding"></see> class topic.</param>
		/// <returns>The encoding  associated with the specified code page.</returns>
		[MorestachioFormatter("[MethodName]", "Returns the encoding associated with the specified code page name.")]
		public static Encoding GetEncoding(string name)
		{
			return new Encoding(NetEncoding.GetEncoding(name));
		}
	}
	
	/// <summary>
	///		Wraps an System.Text.Encoding for access from Morestachio
	/// </summary>
	public class Encoding : NetEncoding
	{
		private readonly NetEncoding _encoding;

		/// <summary>
		///		Creates a new Wrapper
		/// </summary>
		/// <param name="encoding"></param>
		public Encoding(NetEncoding encoding)
		{
			_encoding = encoding;
		}

		[MorestachioFormatter("[MethodName]", "When overridden in a derived class, calculates the number of bytes produced by encoding the characters in the specified string.")]
		public override int GetByteCount(string s)
		{
			return base.GetByteCount(s);
		}

		[MorestachioFormatter("[MethodName]", "When overridden in a derived class, encodes all the characters in the specified string into a sequence of bytes.")]
		public override byte[] GetBytes(string s)
		{
			return base.GetBytes(s);
		}

		[MorestachioFormatter("[MethodName]", "When overridden in a derived class, decodes all the bytes in the specified byte array into a string.")]
		public override string GetString(byte[] bytes)
		{
			return base.GetString(bytes);
		}

		public override int GetByteCount(char[] chars, int index, int count)
		{
			return _encoding.GetByteCount(chars, index, count);
		}

		public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
		{
			return _encoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex);
		}

		public override int GetCharCount(byte[] bytes, int index, int count)
		{
			return _encoding.GetCharCount(bytes, index, count);
		}

		public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
		{
			return _encoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);
		}

		public override int GetMaxByteCount(int charCount)
		{
			return _encoding.GetMaxByteCount(charCount);
		}

		public override int GetMaxCharCount(int byteCount)
		{
			return _encoding.GetMaxCharCount(byteCount);
		}
	}
}
