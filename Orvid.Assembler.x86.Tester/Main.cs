#define UseStackFrame
using System;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using Orvid.TrueType;
using Orvid.Assembler.x86;
using Reg = Orvid.Assembler.x86.x86Register;
using x86 = Orvid.Assembler.x86;

namespace Orvid.Assembler.x86.Testing
{
	class MainClass
	{
		public static unsafe void Main(string[] args)
		{
			x86Assembler a = new x86Assembler();

#if UseStackFrame
			const x86Register argBaseRegister = Reg.EBP;
			const int arg1Offset = 8;
			const int arg2Offset = 12;
			new x86.Push(a, Reg.EBP, 4);
			new x86.Mov (a, Reg.EBP, Reg.ESP, 4);
#else
			const x86Register argBaseRegister = Reg.ESP;
			const int arg1Offset = 4;
			const int arg2Offset = 8;
#endif

			#region DivFix
			new x86.Mov(a, Reg.EAX, argBaseRegister, arg1Offset,  4);
			new x86.Mov(a, Reg.EDX, argBaseRegister, arg2Offset,  4);
			


			#endregion

			#region MulFix
			//new x86.Mov(a, Reg.EAX, argBaseRegister, arg1Offset,  4);
			//new x86.Mov(a, Reg.EDX, argBaseRegister, arg2Offset,  4);

			//new x86.IMul(a, Reg.EDX, 4);
			//new x86.Mov (a, Reg.ECX, Reg.EDX, 4);
			//new x86.SAR (a, Reg.ECX, 31u, 4);
			//new x86.Add (a, Reg.ECX, 0x20, 4);
			//new x86.Add (a, Reg.EAX, Reg.ECX, 4);
			//new x86.Adc (a, Reg.EDX, 0u, 4);
			//new x86.SHRD(a, Reg.EAX, Reg.EDX, 6, 4);
			//new x86.Add (a, Reg.EAX, Reg.EDX, 4);
			#endregion

			#region CE
			//new x86.Mov(a, Reg.EBX, Reg.EBP, 8, 4);
			//new x86.Mov(a, Reg.ECX, Reg.EBP, 12, 4);
			//new x86.XOr(a, Reg.EAX, Reg.EAX, 4);
			//new x86.Cmp(a, Reg.ECX, Reg.EBX, 4);
			//new x86.SetE(a, Reg.EAX);
			#endregion

#if UseStackFrame
			new x86.Leave(a);
#endif
			new x86.Ret(a);


			StreamWriter rtr = new StreamWriter("assemblerOut.txt", false);
			for (int i = 0; i < a.Instructions.Count; i++)
			{
				rtr.WriteLine(a.Instructions[i].ToString());
			}

			if (File.Exists("assemblerOut.bin"))
				File.Delete("assemblerOut.bin");
			FileStream strm = new FileStream("assemblerOut.bin", FileMode.OpenOrCreate, FileAccess.ReadWrite);
			a.Emit(strm);
			strm.Flush();
			strm.Position = 0;
			byte[] barr = new byte[strm.Length];
			strm.Read(barr, 0, (int)strm.Length);
			IntPtr alloced = (IntPtr)ExecutableAllocation.AllocateExecutableMemory((IntPtr)barr.Length);
			Marshal.Copy(barr, 0, alloced, barr.Length);
			strm.Close();

			var del = TrampolineGenerator.GenerateMethodCall<uint, F26Dot6, F26Dot6>(CallingConvention.Cdecl);

			const uint ValA = 64;
			const uint ValB = 32;
			const uint IterCount = 1000000;
			F26Dot6 valFA = F26Dot6.FromLiteral((int)ValA);
			F26Dot6 valFB = F26Dot6.FromLiteral((int)ValB);
			F26Dot6 val3 = 0;
			uint val4 = 0;

			Stopwatch s = new Stopwatch();
			s.Start();
			for (uint i = 0; i < IterCount; i++)
			{
				// Yikes... Delegates really are slow...
				// 770MS for 1 million calls via delegates
				// vs. 
				// 20MS for 1 million calls via a static indirect call (17MS of that is pure interop)
				//val4 = del(alloced, valFA, valFB);
				val4 = Orvid.Trampoline.UnsafeTrampolines.Call(alloced, ValA, ValB);
			}
			s.Stop();
			Console.WriteLine("ASM: Took a total of " + s.ElapsedMilliseconds.ToString() + "MS to make " + IterCount.ToString() + " calls (" + ((double)s.ElapsedMilliseconds / (double)IterCount) + "MS / call) ");
			//val4++;

			s.Restart();
			s.Start();
			for (uint i = 0; i < IterCount; i++)
			{
				// 5MS for 100k calls.
				val3 = valFA / valFB;
			}
			s.Stop();
			Console.WriteLine("Managed: Took a total of " + s.ElapsedMilliseconds.ToString() + "MS to make " + IterCount.ToString() + " calls (" + ((double)s.ElapsedMilliseconds / (double)IterCount) + "MS / call)");
			F26Dot6 valB = F26Dot6.Div_NoRound(valFA, valFB);

			uint val2 = del(alloced, valFA, valFB);
			F26Dot6 val = F26Dot6.FromLiteral((int)val2);
			//rtr.WriteLine("Value returned: " + val.ToString());
			Console.WriteLine("Value2 Returned: " + val.ToString());
			Console.WriteLine("Value2  (as an int) Returned: " + val2.ToString());
			Console.WriteLine("Value4 Returned: " + F26Dot6.FromLiteral((int)val4).ToString());
			Console.WriteLine("Value4 (as an int) Returned: " + val4.ToString());
			Console.WriteLine("Value3 Returned: " + val3.ToString());
			Console.WriteLine("Value3 (as an int) Returned: " + val3.value.ToString());
			Console.WriteLine("ValueB Returned: " + valB.ToString());
			Console.WriteLine("ValueB (as an int) Returned: " + valB.value.ToString());


			
			rtr.Close();
		}
		
	}
}
