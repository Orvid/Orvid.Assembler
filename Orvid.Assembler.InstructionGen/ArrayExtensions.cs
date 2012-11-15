using System;
using System.Collections.Generic;

namespace Orvid.Assembler.InstructionGen
{
	public static class ArrayExtensions
	{
		public static T[] Slice<T>(this T[] sarr, int startIdx, int endIdx)
		{
			T[] darr = new T[endIdx - startIdx];
			Array.Copy(sarr, startIdx, darr, 0, endIdx - startIdx);
			return darr;
		}
	}
}

