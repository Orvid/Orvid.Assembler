using System;
using Orvid.DynamicLinker;

namespace Orvid.DynamicLinker.Testing
{
	class MainClass
	{
		private static byte[] TestData = new byte[]
		{
			(byte)'O', (byte)'R', (byte)'L', (byte)'C', // 4CC		-  4
			0x01, 0x00, 0x00, 0x00, // Version						-  8
			0x04, 0x00, 0x00, 0x00, // Pointer Size					- 12
			0x01, 0x00, 0x00, 0x00, // Entry Size					- 16
			0x00, 0x00, 0x00, 0x00, // Base Address					- 20
			0x01, // Entry Count									- 21
			0x00, // Entry - 1										- 22
			0x01, // Pointers Table Entry Count						- 23
			0x04, 0x01, // Pointers Table Entry - 1					- 25
			0x10, 0x00, 0x00, 0x00, // Actual File					- 29
			0x10, 0x00, 0x00, 0x00, // Actual File					- 33

		};

		public static unsafe void Main(string[] args)
		{
			fixed (byte* tData = TestData)
			{
				ORelocLinker linker = new ORelocLinker(tData, (uint)TestData.Length);
				linker.Rebase(0x100);
				//int i = 0;
			}
		}

		private static void PrintTestData()
		{

		}

	}
}
