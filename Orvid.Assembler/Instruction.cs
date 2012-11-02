using System;
using System.IO;

namespace Orvid.Assembler
{
	public abstract class Instruction
	{
		public Instruction()
		{
		}

		public abstract void Emit(Stream strm);
	}
}

