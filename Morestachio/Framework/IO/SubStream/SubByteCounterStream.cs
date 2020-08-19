using System.IO;
using Morestachio.Document.Contracts;
using Morestachio.Framework.IO.SingleStream;

namespace Morestachio.Framework.IO.SubStream
{
	/// <summary>
	///		This stream can be used to buffer the contents of one or more <see cref="IDocumentItem.Render"/> calls without writing into the main stream but with taking Size into account
	/// </summary>
	public class SubByteCounterStream : ByteCounterStream, ISubByteCounterStream
	{
		private readonly IByteCounterStream _source;

		/// <summary>
		///		Creates a new SubByteCounterStream
		/// </summary>
		/// <param name="source"></param>
		/// <param name="options"></param>
		public SubByteCounterStream(
			IByteCounterStream source,
			ParserOptions options)
			: base(new MemoryStream(), 2024, false, options)
		{
			_source = source;
			BytesWritten = _source.BytesWritten;
			ReachedLimit = _source.ReachedLimit;
		}

		public string Read()
		{
			BaseWriter.Flush();
			return Options.Encoding.GetString((BaseWriter.BaseStream as MemoryStream).ToArray());
		}
	}
}