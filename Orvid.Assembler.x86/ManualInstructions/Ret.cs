using System;

namespace Orvid.Assembler.x86
{
	public sealed class Ret : x86Instruction
	{
		private x86Segment Segment;
		private ushort PopSize;

		public Ret(x86Assembler parentAssembler, ushort popSize = 0, x86Segment seg = x86Segment.CS) : base(parentAssembler)
		{
			PopSize = popSize;
			Segment = seg;
		}

		public override void Emit(x86Stream strm)
		{
			if (PopSize == 0)
			{
				// No pop
				if (Segment == x86Segment.CS)
				{
					// Near return
					strm.WriteByte(0xC3);
				}
				else
				{
					// Far return
					strm.WriteByte(0xCB);
				}
			}
			else
			{
				// pop
				if (Segment == x86Segment.CS)
				{
					// Near return
					strm.WriteByte(0xC2);
					strm.WriteImm16(PopSize);
				}
				else
				{
					// Far return
					strm.WriteByte(0xCA);
					strm.WriteImm16(PopSize);
				}
			}
		}

		public override string ToString(x86AssemblySyntax syntax)
		{
			switch (syntax)
			{
				case x86AssemblySyntax.NASM:
					if (PopSize == 0)
					{
						if (Segment == x86Segment.CS)
						{
							return "retn";
						}
						else
						{
							return "retf";
						}
					}
					else
					{
						if (Segment == x86Segment.CS)
						{
							return "retn " + PopSize.ToString();
						}
						else
						{
							return "retf " + PopSize.ToString();
						}
					}
				case x86AssemblySyntax.GAS:
				default:
					throw new Exception("Not currently supported!");
			}
		}
	}
}

