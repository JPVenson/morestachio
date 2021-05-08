using System;
using System.Collections.Generic;
using System.Text;

namespace Morestachio.Formatter.Constants
{
	/// <summary>
	///		Grants access to the Encodings supported
	/// </summary>
	public class EncodingConstant
	{
		private EncodingConstant()
		{
			
		}

		private static EncodingConstant _instance;
		/// <summary>
		///		The Singleton Instance
		/// </summary>
		public static EncodingConstant Instance
		{
			get
			{
				return _instance ?? (_instance = new EncodingConstant());
			}
		}
		
		/// <summary>Gets an encoding for the ASCII (7-bit) character set.</summary>
		/// <returns>An  encoding for the ASCII (7-bit) character set.</returns>
		public Encoding ASCII
		{
			get { return Encoding.ASCII; }
		}
		
		/// <summary>Gets an encoding for the UTF-16 format that uses the big endian byte order.</summary>
		/// <returns>An encoding object for the UTF-16 format that uses the big endian byte order.</returns>
		public Encoding BigEndianUnicode
		{
			get { return Encoding.BigEndianUnicode; }
		}
		
		/// <summary>Gets an encoding for the operating system's current ANSI code page.</summary>
		/// <returns>An encoding for the operating system's current ANSI code page.</returns>
		public Encoding Default
		{
			get { return Encoding.Default; }
		}
		
		/// <summary>Gets an encoding for the UTF-16 format using the little endian byte order.</summary>
		/// <returns>An encoding for the UTF-16 format using the little endian byte order.</returns>
		public Encoding Unicode
		{
			get { return Encoding.Unicode; }
		}
		
		/// <summary>Gets an encoding for the UTF-32 format using the little endian byte order.</summary>
		/// <returns>An  encoding object for the UTF-32 format using the little endian byte order.</returns>
		public Encoding UTF32
		{
			get { return Encoding.UTF32; }
		}
		
		/// <summary>Gets an encoding for the UTF-7 format.</summary>
		/// <returns>An encoding for the UTF-7 format.</returns>
		public Encoding UTF7
		{
			get { return Encoding.UTF7; }
		}
		
		/// <summary>Gets an encoding for the UTF-8 format.</summary>
		/// <returns>An encoding for the UTF-8 format.</returns>
		public Encoding UTF8
		{
			get { return Encoding.UTF8; }
		}
	}
}
