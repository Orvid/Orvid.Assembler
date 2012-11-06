using System;
using System.IO;

namespace Orvid.Assembler.x86
{
	public sealed class x86Stream : AssemblerStream
	{
		public x86Stream(Stream parentStream) : base(parentStream, AssemblerEndianness.LittleEndian)
		{
		}

		/// <summary>
		/// Writes a prefix to the stream.
		/// </summary>
		/// <param name='prefix'>The prefix to write.</param>
		public void WritePrefix(x86Prefix prefix)
		{
			WriteByte((byte)prefix);
		}

		/// <summary>
		/// Writes a ModR/M byte to the stream.
		/// </summary>
		/// <param name='mod'>The addressing mode to use.</param>
		/// <param name='opCode'>The register/opcode.</param>
		/// <param name='rm'>The r/m value.</param>
		public void WriteModRM(x86AddressingMode mod, byte opCode, byte rm)
		{
			WriteAddressByte((byte)mod, opCode, rm);
		}
		
		public void WriteRegister(byte o, x86Register r)
		{
			WriteModRM(x86AddressingMode.Register, o, (byte)r);
		}
		
		public void WriteRegister(byte o, x86DebugRegister r)
		{
			WriteModRM(x86AddressingMode.Register, o, (byte)r);
		}
		
		public void WriteRegister(byte o, x86ControlRegister r)
		{
			WriteModRM(x86AddressingMode.Register, o, (byte)r);
		}

		public void WriteRegister(byte o, x86Segment r)
		{
			WriteModRM(x86AddressingMode.Register, o, (byte)r);
		}

		private void WriteAddressByte(byte m, byte o, byte r)
		{
			WriteByte((byte)(((m & 0x03) << 6) | ((o & 0x07) << 3) | ((r & 0x07))));
		}

		public void WriteSegmentOverride(x86Segment selected, x86Segment defaultSeg)
		{
			if (selected != defaultSeg)
			{
				switch(selected)
				{
					case x86Segment.ES:
						WriteByte(0x26);
						break;
					case x86Segment.CS:
						WriteByte(0x2E);
						break;
					case x86Segment.SS:
						WriteByte(0x36);
						break;
					case x86Segment.DS:
						WriteByte(0x3E);
						break;
					case x86Segment.FS:
						WriteByte(0x64);
						break;
					case x86Segment.GS:
						WriteByte(0x65);
						break;
					default:
						throw new Exception("Unknown x86 Segment!");
				}
			}
		}

		public void WriteMem(byte r, uint address)
		{
			WriteAddressByte(0, r, (byte)x86Register.EBP);
			WriteImm32(address);
		}

		public void WriteMembase(byte r, x86Register baseReg, int displacement)
		{
			if (baseReg == x86Register.ESP)
			{
				if (displacement == 0)
				{
					WriteAddressByte(0, r, (byte)x86Register.ESP);
					WriteAddressByte(0, (byte)x86Register.ESP, (byte)x86Register.ESP);
				}
				else if (InSByteRange(displacement))
				{
					WriteAddressByte(1, r, (byte)x86Register.ESP);
					WriteAddressByte(0, (byte)x86Register.ESP, (byte)x86Register.ESP);
					WriteImm8((byte)displacement);
				}
				else
				{
					WriteAddressByte(2, r, (byte)x86Register.ESP);
					WriteAddressByte(0, (byte)x86Register.ESP, (byte)x86Register.ESP);
					WriteImm32((uint)displacement);
				}
			}
			else if (displacement == 0 && baseReg != x86Register.EBP)
			{
				WriteAddressByte(0, r, (byte)baseReg);
			}
			else if (InSByteRange(displacement))
			{
				WriteAddressByte(1, r, (byte)baseReg);
				WriteImm8((byte)displacement);
			}
			else
			{
				WriteAddressByte(2, r, (byte)baseReg);
				WriteImm32((uint)displacement);
			}
		}

		public void WriteMemIndex(byte r, x86Register baseReg, int displacement, x86Register indexReg, x86IndexShift iShift)
		{
			byte shift = (byte)iShift;
			switch (shift)
			{
				case 0:
				case 1:
				case 2:
				case 3:
					break;
				default:
					throw new Exception("Invalid shift!");
			}
			if (baseReg == x86Register.None)
			{
				WriteAddressByte(0, r, 4);
				WriteAddressByte(shift, (byte)indexReg, 5);
				WriteImm32((uint)displacement);
			}
			else if (displacement == 0 && baseReg != x86Register.EBP)
			{
				WriteAddressByte(0, r, 4);
				WriteAddressByte(shift, (byte)indexReg, (byte)baseReg);
			}
			else if (InSByteRange(displacement))
			{
				WriteAddressByte(1, r, 4);
				WriteAddressByte(shift, (byte)indexReg, (byte)baseReg);
				WriteImm8((byte)displacement);
			}
			else
			{
				WriteAddressByte(2, r, 4);
				WriteAddressByte(shift, (byte)indexReg, (byte)baseReg);
				WriteImm32((uint)displacement);
			}
		}
	}
}

