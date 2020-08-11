using System;
using System.IO;

namespace Morestachio.Framework
{
	public interface IByteCounterStreamPart : IDisposable
	{
		ByteCounterInfo Info { get; set; }

		ByteCounterStreamPartType State { get; }

		/// <summary>
		///		Writes the Content into the underlying Stream when the limit is not exceeded
		/// </summary>
		/// <param name="content"></param>
		void Write(string content);
		Stream BaseStream();
	}
}