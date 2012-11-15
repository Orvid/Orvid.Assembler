using System;

namespace Orvid.Assembler.x86
{
	public static class NamingHelper
	{
		public static string NameFPURegister(x86FPURegister reg)
		{
			switch (reg)
			{
				case x86FPURegister.ST0:
					return "ST(0)";
				case x86FPURegister.ST1:
					return "ST(1)";
				case x86FPURegister.ST2:
					return "ST(2)";
				case x86FPURegister.ST3:
					return "ST(3)";
				case x86FPURegister.ST4:
					return "ST(4)";
				case x86FPURegister.ST5:
					return "ST(5)";
				case x86FPURegister.ST6:
					return "ST(6)";
				case x86FPURegister.ST7:
					return "ST(7)";
				default:
					throw new Exception("Invalid FPU Register!");
			}
		}

		public static string NameSegment(x86Segment seg, bool upper = false)
		{
			switch (seg)
			{
				case x86Segment.CS:
					return upper ? "CS" : "cs";
				case x86Segment.DS:
					return upper ? "DS" : "ds";
				case x86Segment.ES:
					return upper ? "ES" : "es";
				case x86Segment.FS:
					return upper ? "FS" : "fs";
				case x86Segment.GS:
					return upper ? "GS" : "gs";
				case x86Segment.SS:
					return upper ? "SS" : "ss";
				default:
					throw new Exception("Invalid segment!");
			}
		}
		
		public static string NameControlRegister(x86ControlRegister reg)
		{
			switch (reg)
			{
				case x86ControlRegister.CR0:
					return "CR0";
				case x86ControlRegister.CR1:
					return "CR1";
				case x86ControlRegister.CR2:
					return "CR2";
				case x86ControlRegister.CR3:
					return "CR3";
				case x86ControlRegister.CR4:
					return "CR4";
				case x86ControlRegister.CR5:
					return "CR5";
				case x86ControlRegister.CR6:
					return "CR6";
				case x86ControlRegister.CR7:
					return "CR7";
				default:
					throw new Exception("Invalid Control Register!");
			}
		}
		
		public static string NameDebugRegister(x86DebugRegister reg)
		{
			switch (reg)
			{
				case x86DebugRegister.DR0:
					return "DR0";
				case x86DebugRegister.DR1:
					return "DR1";
				case x86DebugRegister.DR2:
					return "DR2";
				case x86DebugRegister.DR3:
					return "DR3";
				case x86DebugRegister.DR4:
					return "DR4";
				case x86DebugRegister.DR5:
					return "DR5";
				case x86DebugRegister.DR6:
					return "DR6";
				case x86DebugRegister.DR7:
					return "DR7";
				default:
					throw new Exception("Invalid Debug Register!");
			}
		}

		public static string NameRegister(x86Register reg, byte size)
		{
			if (reg == x86Register.None)
			{
				throw new Exception("This should never happen!");
			}
			switch (size)
			{
				case 1:
					switch(reg)
					{
						case x86Register.EAX:
							return "AL";
						case x86Register.EBP:
							return "CH";
						case x86Register.EBX:
							return "BL";
						case x86Register.ECX:
							return "CL";
						case x86Register.EDI:
							return "BH";
						case x86Register.EDX:
							return "DL";
						case x86Register.ESI:
							return "DH";
						case x86Register.ESP:
							return "AH";
						default:
							throw new Exception("Unknown register!");
					}
				case 2:
					switch(reg)
					{
						case x86Register.EAX:
							return "AX";
						case x86Register.EBP:
							return "BP";
						case x86Register.EBX:
							return "BX";
						case x86Register.ECX:
							return "CX";
						case x86Register.EDI:
							return "DI";
						case x86Register.EDX:
							return "DX";
						case x86Register.ESI:
							return "SI";
						case x86Register.ESP:
							return "SP";
						default:
							throw new Exception("Unknown register!");
					}
				case 4:
					return reg.ToString();
				default:
					throw new Exception("Unknown size!");
			}
		}

		public static string NameMembase(x86Register reg, int displacement)
		{
			int sVal = (int)displacement;
			if (displacement == 0)
			{
				return "[" + reg.ToString() + "]";
			}
			else if (x86Stream.InSByteRange(sVal))
			{
				return "[" + reg.ToString() + " " + (sVal < 0 ? "-" : "+") + " " + (sVal < 0 ? -sVal : sVal).ToString() + "]";
			}
			else
			{
				return "[" + reg.ToString() + " " + (sVal < 0 ? "-" : "+") + " " + (sVal < 0 ? -sVal : sVal).ToString() + "]";
			}
		}

		public static string NameDisplacement(x86Assembler a, x86Instruction baseInstr, int displacement)
		{
			return "[rel " + (displacement < 0 ? " - " : " + ") + "0x" + (displacement & int.MaxValue).ToString("X") + "]";
		}

		public static string NameMem(x86Assembler a, uint address, x86Segment segment)
		{
			// Assembler gets passed in here because we will eventually do symbol lookup.
			return "[" + NameSegment(segment) + ":0x" + address.ToString("X").PadLeft(8, '0') + "]";
		}

		public static string NameMemIndex(x86Assembler a, x86Register baseRegister, int displacement, x86Register indexRegister, x86IndexShift shift, x86Segment segment)
		{
			string ret = "";
			bool form2 = baseRegister == x86Register.None;
			if (form2)
			{
				ret += NameSegment(segment) + ":0x" + displacement.ToString("X").PadLeft(8, '0');
			}
			ret += "[" + NameRegister(indexRegister, 4);
			switch (shift)
			{
				case x86IndexShift.Mul1:
					break;
				case x86IndexShift.Mul2:
					ret += " * 2";
					break;
				case x86IndexShift.Mul4:
					ret += " * 4";
					break;
				case x86IndexShift.Mul8:
					ret += " * 8";
					break;
				default:
					throw new Exception("Invalid index shift!");
			}
			if (baseRegister != x86Register.None)
			{
				ret += " + " + NameRegister(baseRegister, 4);
			}
			if (displacement != 0 && !form2)
			{
				ret += " " + (displacement < 0 ? "-" : "+") + " " + (displacement < 0 ? -displacement : displacement).ToString();
			}
			return ret + "]";
		}
	}
}

