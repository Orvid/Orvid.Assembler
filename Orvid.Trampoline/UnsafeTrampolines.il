.assembly Orvid.Trampoline
{
}

.assembly extern mscorlib { }

.module Orvid.Trampoline
.namespace Orvid.Trampoline
{
	.class public auto ansi beforefieldinit sealed abstract UnsafeTrampolines
	{
		.method public hidebysig static uint32 Call(native int ptr, uint32 arg1, uint32 arg2) cil managed
		{
			.maxstack  2
			ldarg.2
			ldarg.1
			ldarg.0
			calli unmanaged cdecl uint32(uint32, uint32)
			ret
		}
	}	
}
