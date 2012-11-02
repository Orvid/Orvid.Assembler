using System;
using System.IO;

namespace Orvid.Assembler
{
	public abstract class Assembler
	{
		public Assembler()
		{
		}

		public abstract void Emit(Stream strm);
	}
}

