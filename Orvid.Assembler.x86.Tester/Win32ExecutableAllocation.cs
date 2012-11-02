using System;
using System.Runtime.InteropServices;

namespace Orvid.Assembler.x86.Testing
{
	public static unsafe class Win32ExecutableAllocation
	{
		[Flags]
		private enum MemAllocationType : uint
		{
			Mem_Commit = 0x1000,
			Mem_Reserve = 0x2000,
			Mem_Reset = 0x80000,
			Mem_Top_Down = 0x100000,
			Mem_Write_Watch = 0x200000,
			Mem_Physical = 0x400000,
			Mem_Reset_Undo = 0x1000000,
			Mem_Large_Pages = 0x20000000,
		}

		private enum MemProtectionType : uint
		{
			Page_Execute_ReadWrite = 0x40,
		}
		[DllImport("kernel32.dll", CallingConvention = CallingConvention.Winapi)]
		private static extern void* VirtualAlloc(void* addressToAllocateAt, IntPtr allocationSize, MemAllocationType allocType, MemProtectionType protType);

		public static byte* AllocateExecutableMemory(IntPtr size)
		{
			void* mem = VirtualAlloc(null, size, MemAllocationType.Mem_Commit | MemAllocationType.Mem_Reserve, MemProtectionType.Page_Execute_ReadWrite);
			return (byte*)mem;
		}
	}
}

