using System;
using System.IO;

namespace Orvid.Assembler.xCore
{
	public sealed class xCoreStream : AssemblerStream
	{
		public xCoreStream(Stream parentStream) : base(parentStream, AssemblerEndianness.LittleEndian)
		{
		}
	}
}

