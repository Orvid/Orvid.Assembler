using System;

namespace Orvid.Assembler.x86
{
	public sealed class Nop : x86Instruction
	{
		public Nop(x86Assembler parentAssembler) : base(parentAssembler) { }

		public override void Emit(x86Stream strm)
		{
			strm.WriteByte(0x90);
		}

		public override string ToString(x86AssemblySyntax syntax)
		{
			switch (syntax)
			{
				case x86AssemblySyntax.NASM:
					return "nop";
				case x86AssemblySyntax.GAS:
				default:
					throw new Exception("Not currently supported!");
			}
		}
	}
}

