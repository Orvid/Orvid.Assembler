using System;

namespace Orvid.Assembler.xCore
{
	public static class NamingHelper
	{
		public static string NameRegister(xCoreRegister reg)
		{
			return reg.ToString();
		}

		public static string NameImm(byte imm)
		{
			return imm.ToString();
		}
	}
}

