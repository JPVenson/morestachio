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
		/// <summary>
		///		ctor
		/// </summary>
		public ByteCounterFactory(Func<ParserOptions, IByteCounterStream> output,
			Func<ParserOptions, IByteCounterStream> tempStream, 
			Func<ParserOptions, IByteCounterStream> getByteCounterStream)
		{
			Output = output ?? GetDefaultTempStream();
			TempStream = tempStream ?? GetDefaultTempStream();
			GetByteCounterStream = getByteCounterStream ?? GetDefaultByteCounter(GetDefaultStream);
		}
		
		/// <summary>
		///		ctor
		/// </summary>
		public ByteCounterFactory(Func<ParserOptions, IByteCounterStream> output,
			Func<ParserOptions, IByteCounterStream> tempStream) 
			: this(output, tempStream, GetDefaultByteCounter(GetDefaultStream))
		{
		}
		
		/// <summary>
		///		ctor
		/// </summary>
		public ByteCounterFactory(Func<ParserOptions, IByteCounterStream> output) : this(GetDefaultTempStream(), GetDefaultTempStream(), output)
		{
		}
		
		/// <summary>
		///		ctor
		/// </summary>
		public ByteCounterFactory(Func<Stream> output) : this(GetDefaultTempStream(), GetDefaultTempStream(), GetDefaultByteCounter(output ?? GetDefaultStream))
		{
		}
		
		/// <summary>
		///		ctor
		/// </summary>
		public ByteCounterFactory() : this(GetDefaultTempStream(), GetDefaultTempStream(), GetDefaultByteCounter(GetDefaultStream))
		{
		}

		private static Stream GetDefaultStream()
		{
			return new MemoryStream();
		}

		private static Func<ParserOptions, IByteCounterStream> GetDefaultTempStream()
		{
			return (ParserOptions options) => new ByteCounterTextWriter(new StringWriter(), options);
		}

		private static Func<ParserOptions, IByteCounterStream> GetDefaultByteCounter(Func<Stream> targetStream)
		{
			return (ParserOptions options) => new ByteCounterStream(targetStream(), MorestachioDocumentInfo.BufferSize, true, options);
		}

		/// <summary>
		///		Should return an stream that is the target of the template
		/// </summary>
		public Func<ParserOptions, IByteCounterStream> Output { get; }

		/// <summary>
		///		Should return an stream that is shot lived and is used for buffering data
		/// </summary>
		public Func<ParserOptions, IByteCounterStream> TempStream { get; }

		/// <summary>
		///		Should return an instance of <see cref="IByteCounterStream"/> that is used to write to <see cref="Output"/>
		/// </summary>
		public Func<ParserOptions, IByteCounterStream> GetByteCounterStream { get; }
	}
}