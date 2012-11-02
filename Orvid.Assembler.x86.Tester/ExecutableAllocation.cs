using System;

namespace Orvid.Assembler.x86.Testing
{
	public static unsafe class ExecutableAllocation
	{
		public static byte* AllocateExecutableMemory(IntPtr size)
		{
			switch (Environment.OSVersion.Platform)
			{
				case PlatformID.MacOSX:
				case PlatformID.Unix:
					return UnixExecutableAllocation.AllocateExecutableMemory(size);
				case PlatformID.Win32NT:
				case PlatformID.Win32Windows:
				case PlatformID.Win32S:
					return Win32ExecutableAllocation.AllocateExecutableMemory(size);
				default:
					throw new Exception("Unknown Platform!");
			}
		}
	}
}

