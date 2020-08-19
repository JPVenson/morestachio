using System;
using System.IO;
using Morestachio.Framework.IO.SingleStream;

namespace Morestachio.Framework.IO
{
	/// <summary>
	///		This factory does contain all delegates for obtaining the IO output
	/// </summary>
	public class ByteCounterFactory
	{
		public ByteCounterFactory(Func<Stream> output, Func<Stream> tempStream, Func<ParserOptions, IByteCounterStream> getByteCounterStream)
		{
			Output = output ?? GetDefaultTempStream;
			TempStream = tempStream ?? GetDefaultTempStream;
			GetByteCounterStream = getByteCounterStream ?? GetDefaultByteCounter;
		}

		public ByteCounterFactory(Func<Stream> output, Func<Stream> tempStream) : this(output, tempStream, GetDefaultByteCounter)
		{
		}

		public ByteCounterFactory(Func<Stream> output) : this(output, GetDefaultTempStream, GetDefaultByteCounter)
		{
		}

		public ByteCounterFactory() : this(GetDefaultTempStream, GetDefaultTempStream, GetDefaultByteCounter)
		{
		}

		private static Stream GetDefaultTempStream()
		{
			return new MemoryStream();
		}

		private static IByteCounterStream GetDefaultByteCounter(ParserOptions options)
		{
			return new ByteCounterStream(options.StreamFactory.Output(), MorestachioDocumentInfo.BufferSize, true, options);
		}

		/// <summary>
		///		Should return an stream that is the target of the template
		/// </summary>
		public Func<Stream> Output { get; }

		/// <summary>
		///		Should return an stream that is shot lived and is used for buffering data
		/// </summary>
		public Func<Stream> TempStream { get; }

		/// <summary>
		///		Should return an instance of <see cref="IByteCounterStream"/> that is used to write to <see cref="Output"/>
		/// </summary>
		public Func<ParserOptions, IByteCounterStream> GetByteCounterStream { get; }
	}
}