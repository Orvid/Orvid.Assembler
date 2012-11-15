using System;
using System.IO;
using System.Collections.Generic;

namespace Orvid.Assembler.xCore
{
	public sealed class xCoreAssembler : Assembler
	{
		public List<xCoreInstruction> Instructions = new List<xCoreInstruction>();
		
		public override void Emit(Stream strm)
		{
			xCoreStream emissionStream = new xCoreStream(strm);
			for(int i = 0; i < Instructions.Count; i++)
			{
				Instructions[i].Emit(emissionStream);
			}
		}
	}
}

