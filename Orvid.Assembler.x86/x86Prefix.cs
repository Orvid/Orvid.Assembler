using System;

namespace Orvid.Assembler.x86
{
	public enum x86Prefix : byte
	{
		/// <summary>
		/// Only valid on the following op-codes:
		/// ADC, Add, And,
		/// BTC, BTR, BTS, 
		/// Dec, Inc, Neg, 
		/// Not, Or,  SBB, 
		/// Sub, XAdd, XChg,
		/// XOr, CmpXChg, 
		/// CmpXChg8B, CmpXChg16B
		/// </summary>
		Lock = 0xF0,
		/// <summary>
		/// Only valid on string and I/O instructions.
		/// </summary>
		RepNE = 0xF2,
		/// <summary>
		/// Only valid on string and I/O instructions.
		/// </summary>
		RepNZ = 0xF2,
		/// <summary>
		/// Only valid on string and I/O instructions.
		/// </summary>
		Rep = 0xF3,
		/// <summary>
		/// Only valid on string and I/O instructions.
		/// </summary>
		RepE = 0xF3,
		/// <summary>
		/// Only valid on string and I/O instructions.
		/// </summary>
		RepZ = 0xF3,
		/// <summary>
		/// Use of this prefix on a Jcc instruction is reserved.
		/// </summary>
		SegmentOverride_CS = 0x2E,
		/// <summary>
		/// Use of this prefix on a Jcc instruction is reserved.
		/// </summary>
		SegmentOverride_SS = 0x36,
		/// <summary>
		/// Use of this prefix on a Jcc instruction is reserved.
		/// </summary>
		SegmentOverride_DS = 0x3E,
		/// <summary>
		/// Use of this prefix on a Jcc instruction is reserved.
		/// </summary>
		SegmentOverride_ES = 0x26,
		/// <summary>
		/// Use of this prefix on a Jcc instruction is reserved.
		/// </summary>
		SegmentOverride_FS = 0x64,
		/// <summary>
		/// Use of this prefix on a Jcc instruction is reserved.
		/// </summary>
		SegmentOverride_GS = 0x65,
		/// <summary>
		/// Only valid on a Jcc instruction.
		/// </summary>
		BranchHint_Taken = 0x2E,
		/// <summary>
		/// Only valid on a Jcc instruction.
		/// </summary>
		BranchHint_NotTaken = 0x3E,
		OperandSizeOverride = 0x66,
		AddressSizeOverride = 0x67,
	}
}

