using System;
using System.IO;
using xCore = Orvid.Assembler.xCore;
using Reg = Orvid.Assembler.xCore.xCoreRegister;

namespace Orvid.Assembler.xCore.Tester
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			xCoreAssembler a = new xCoreAssembler();

			new xCore.Add(a, Reg.R0, Reg.R2, Reg.R4);
			
			StreamWriter rtr = new StreamWriter("assemblerOut.txt", false);
			for (int i = 0; i < a.Instructions.Count; i++)
			{
				rtr.WriteLine(a.Instructions[i].ToString());
			}
			rtr.Flush();
			rtr.Close();
			
			if (File.Exists("assemblerOut.bin"))
				File.Delete("assemblerOut.bin");
			FileStream strm = new FileStream("assemblerOut.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite);
			a.Emit(strm);
			strm.Flush();
			strm.Close();
		}
	}
}
