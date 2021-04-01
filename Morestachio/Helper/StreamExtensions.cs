using System;
using System.IO;
using System.Text;
using Morestachio.Framework.IO;
using Morestachio.Framework.IO.SingleStream;

namespace Morestachio.Helper
{
	/// <summary>
	///     Helper class for Steam operations
	/// </summary>
	public static class StreamExtensions
	{
		/// <summary>
		///     Reads all content from the Stream and returns it as a String
		/// </summary>
		/// <param name="source"></param>
		/// <param name="disposeOriginal"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static string Stringify(this Stream source, bool disposeOriginal, Encoding encoding)
		{
			try
			{
				source.Seek(0, SeekOrigin.Begin);
				if (source is MemoryStream stream)
				{
					return encoding.GetString(stream.ToArray());
				}

				using (var ms = new StreamReader(source, encoding))
				{
					return ms.ReadToEnd();
				}
			}
			finally
			{
				if (disposeOriginal)
				{
					source.Dispose();
				}
			}
		}

		/// <summary>
		///     Reads all content from the Stream and returns it as a String
		/// </summary>
		/// <param name="source"></param>
		/// <param name="disposeOriginal"></param>
		/// <param name="encoding"></param>
		/// <returns></returns>
		public static string Stringify(this IByteCounterStream source, bool disposeOriginal, Encoding encoding)
		{
			if (source is ByteCounterStream bcs)
			{
				return bcs.Stream.Stringify(disposeOriginal, encoding);
			}

			if (source is ByteCounterTextWriter bcsw)
			{
				try
				{
					return bcsw.Writer.ToString();
				}
				finally
				{
					if (disposeOriginal)
					{
						source.Dispose();
					}
				}
			}

			if (source is ByteCounterStringBuilder bcsb)
			{
				try
				{
					return bcsb.StringBuilder.ToString();
				}
				finally
				{
					if (disposeOriginal)
					{
						source.Dispose();
					}
				}
			}

			throw new NotImplementedException();
		}
	}
}