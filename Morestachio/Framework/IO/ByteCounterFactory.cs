using System;
using System.IO;
using Morestachio.Framework;

namespace Morestachio
{
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

		public Func<Stream> Output { get; private set; }
		public Func<Stream> TempStream { get; private set; }
		public Func<ParserOptions, IByteCounterStream> GetByteCounterStream { get; private set; }
	}
}