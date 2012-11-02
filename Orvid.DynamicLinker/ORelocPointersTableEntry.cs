using System;
using System.Runtime.InteropServices;

namespace Orvid.DynamicLinker
{
	[StructLayout(LayoutKind.Explicit, Size = 8, Pack = 1)]
    internal struct ORelocPointersTableEntry
    {
		[FieldOffset(0)]
        public uint StartOffset;
		[FieldOffset(4)]
        public uint PointerCount;
    }
}

