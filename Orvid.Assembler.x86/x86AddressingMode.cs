using System;

namespace Orvid.Assembler.x86
{
	public enum x86AddressingMode : byte
	{
		Membase0 = 0x00,
		Membase1 = 0x01,
		//Membase2 = 0x02,
		Membase4 = 0x02,
		Register = 0x03,
	}
}

