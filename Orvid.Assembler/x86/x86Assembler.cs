using System;
using System.IO;
using System.Collections.Generic;

namespace Orvid.Assembler.x86
{
	public sealed class x86Assembler : Assembler
	{
		public List<x86Instruction> Instructions = new List<x86Instruction>();

		public override void Emit(Stream strm)
		{
			x86Stream emissionStream = new x86Stream(strm);
			for(int i = 0; i < Instructions.Count; i++)
			{
				Instructions[i].Emit(emissionStream);
			}
		}
	}
}

