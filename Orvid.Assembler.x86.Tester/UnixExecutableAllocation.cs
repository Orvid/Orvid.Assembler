using System;
using System.Runtime.InteropServices;

namespace Orvid.Assembler.x86.Testing
{
	public static unsafe class UnixExecutableAllocation
	{
		[Flags]
		private enum MemProtectionType : uint
		{
			Prot_Read = 0x1,
			Prot_Write = 0x2,
			Prot_Execute = 0x4,
		}

		[DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
		private static extern int posix_memalign(IntPtr* allocatedAddress, IntPtr alignment, IntPtr size);
		
		[DllImport("libc", CallingConvention = CallingConvention.Cdecl)]
		private static extern int mprotect(void* addr, IntPtr length, MemProtectionType protection);

		public static byte* AllocateExecutableMemory(IntPtr size)
		{
			IntPtr mem = (IntPtr)0;
			posix_memalign(&mem, (IntPtr)4, size);
			mprotect((void*)mem, size, MemProtectionType.Prot_Read | MemProtectionType.Prot_Write | MemProtectionType.Prot_Execute);
			return (byte*)mem;
		}
	}
}

