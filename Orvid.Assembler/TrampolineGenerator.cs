using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Orvid.Assembler
{
	public static class TrampolineGenerator
	{
		private struct ArgSet
		{
			public Type ReturnType;
			public Type Arg1;
			public Type Arg2;
			public Type Arg3;
			public Type Arg4;
			public Type Arg5;
			public Type Arg6;
			public Type Arg7;
			public Type Arg8;
			public Type Arg9;
			public Type Arg10;
			public Type Arg11;
			public Type Arg12;
			public Type Arg13;
			public Type Arg14;
			public Type Arg15;
		}

		private static List<DynamicMethod> DynMethods = new List<DynamicMethod>(32);
		private static Dictionary<ArgSet, object> GeneratedMethods = new Dictionary<ArgSet, object>(32);

		public static Func<IntPtr, TRet> GenerateMethodCall<TRet>(CallingConvention callConv)
		{
			ArgSet ar = new ArgSet()
			{
				ReturnType = typeof(TRet)
			};
			lock (GeneratedMethods)
			{
				if (!GeneratedMethods.ContainsKey(ar))
				{
					DynamicMethod m = new DynamicMethod("DynamicCall", typeof(TRet), new Type[] { typeof(IntPtr) });
					var gen = m.GetILGenerator();
					gen.Emit(OpCodes.Ldarg_0);
					gen.EmitCalli(OpCodes.Calli, callConv, typeof(TRet), new Type[] { });
					gen.Emit(OpCodes.Ret);
					m.DefineParameter(0, ParameterAttributes.None, "methodPtr");
					DynMethods.Add(m);
					GeneratedMethods.Add(ar, m.CreateDelegate(typeof(Func<IntPtr, TRet>)));
				}
				return (Func<IntPtr, TRet>)GeneratedMethods[ar];
			}
		}

		public static Func<IntPtr, T1, TRet> GenerateMethodCall<TRet, T1>(CallingConvention callConv)
		{
			ArgSet ar = new ArgSet()
			{
				ReturnType = typeof(TRet),
				Arg1 = typeof(T1)
			};
			lock (GeneratedMethods)
			{
				if (!GeneratedMethods.ContainsKey(ar))
				{
					DynamicMethod m = new DynamicMethod("DynamicCall", typeof(TRet), new Type[] { typeof(IntPtr), typeof(T1) });
					var gen = m.GetILGenerator();
					gen.Emit(OpCodes.Ldarg_1);
					gen.Emit(OpCodes.Ldarg_0);
					gen.EmitCalli(OpCodes.Calli, callConv, typeof(TRet), new Type[] { typeof(T1) });
					gen.Emit(OpCodes.Ret);
					m.DefineParameter(0, ParameterAttributes.None, "methodPtr");
					DynMethods.Add(m);
					GeneratedMethods.Add(ar, m.CreateDelegate(typeof(Func<IntPtr, T1, TRet>)));
				}
				return (Func<IntPtr, T1, TRet>)GeneratedMethods[ar];
			}
		}
		
		public static Func<IntPtr, T1, T2, TRet> GenerateMethodCall<TRet, T1, T2>(CallingConvention callConv)
		{
			ArgSet ar = new ArgSet()
			{
				ReturnType = typeof(TRet),
				Arg1 = typeof(T1),
				Arg2 = typeof(T2)
			};
			lock (GeneratedMethods)
			{
				if (!GeneratedMethods.ContainsKey(ar))
				{
					DynamicMethod m = new DynamicMethod("DynamicCall", typeof(TRet), new Type[] { typeof(IntPtr), typeof(T1), typeof(T2) });
					var gen = m.GetILGenerator();
					gen.Emit(OpCodes.Ldarg_2);
					gen.Emit(OpCodes.Ldarg_1);
					gen.Emit(OpCodes.Ldarg_0);
					gen.EmitCalli(OpCodes.Calli, callConv, typeof(TRet), new Type[] { typeof(T1), typeof(T2) });
					gen.Emit(OpCodes.Ret);
					m.DefineParameter(0, ParameterAttributes.None, "methodPtr");
					DynMethods.Add(m);
					GeneratedMethods.Add(ar, m.CreateDelegate(typeof(Func<IntPtr, T1, T2, TRet>)));
				}
				return (Func<IntPtr, T1, T2, TRet>)GeneratedMethods[ar];
			}
		}

	}
}

