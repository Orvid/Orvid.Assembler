using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;

namespace Orvid.TrueType
{
	[StructLayout(LayoutKind.Explicit)]
	/// <summary>
	/// Represents a fixed point number 
	/// with 6 bits of fraction.
	/// </summary>
	public struct F26Dot6 : IEquatable<F26Dot6>
	{
		public static readonly F26Dot6 Zero = 0;
		public static readonly F26Dot6 One = 1;
		public static readonly F26Dot6 NegativeOne = -1;
		[FieldOffset(0)]
		internal int value;
		
		private const double A64th = 1.0d / 64.0d;
		
		public F26Dot6(uint val) { value = FromDouble(val).value; }
		public F26Dot6(int val) { value = FromDouble(val).value; }
		public F26Dot6(byte val) { value = FromDouble(val).value; }
		public F26Dot6(ushort val) { value = FromDouble(val).value; }
		public F26Dot6(short val) { value = FromDouble(val).value; }
		
		public static double ToDouble(F26Dot6 val)
		{
			return (val.value * A64th);
		}
		
		public static F26Dot6 FromDouble(double d)
		{
			return F26Dot6.FromLiteral((int)(d / A64th));
		}
		
		private const double A8192nd = 1.0d / 8192d;
		public static F26Dot6 FromF2Dot14(int val)
		{
			return FromDouble((double)val * A8192nd);
		}
		
		public static int AsLiteralF2Dot14(F26Dot6 val)
		{
			return (int)(((int)F26Dot6.ToDouble(val)) * 8192d);
		}
		
		
		public static int AsLiteral(F26Dot6 val)
		{
			return val.value;
		}
		
		public static F26Dot6 FromLiteral(int v)
		{
			F26Dot6 val = new F26Dot6();
			val.value = v;
			return val;
		}
		
		public override string ToString()
		{
			if (value < 0)
			{
				return ((int)(this.value / 64)).ToString() + "." + (-this.value % 64).ToString() + "/64";
			}
			return ((int)(this.value / 64)).ToString() + "." + (this.value % 64).ToString() + "/64";
		}
		
		/// <summary>
		/// Computes (a * b) / c with maximum
		/// accuracy and rounding.
		/// </summary>
		public static unsafe F26Dot6 MulDiv(F26Dot6 pA, F26Dot6 pB, F26Dot6 pC)
		{
			return F26Dot6.FromLiteral(MulDiv(*(int*)&pA, *(int*)&pB, *(int*)&pC));
		}
		
		/// <summary>
		/// Computes (a * b) / c with maximum
		/// accuracy and rounding.
		/// </summary>
		public static int MulDiv(int a, int b, int c)
		{
			if (c == 0)
			{
				throw new DivideByZeroException("Cannot divide by zero!");
			}
			int s;
			int d;
			
			// s will be -1 if the result
			// is negative.
			s = (int)((a ^ b ^ c) & ((uint)int.MaxValue + 1));
			// These next 3 statements 
			// can actually be optimized
			// to be no branches at all,
			// and actually become only
			// 3 instructions each on x86.
			if (a < 0) { a = -a; }
			if (b < 0) { b = -b; }
			if (c < 0) { c = -c; }
			d = (int)(((long)a * b + (c >> 1)) / c);
			return (int)((s != 0) ? -d : d);
		}
		
		/// <summary>
		/// Computes (a * b) / c with maximum
		/// accuracy and no rounding.
		/// </summary>
		public static unsafe F26Dot6 MulDiv_NoRound(F26Dot6 pA, F26Dot6 pB, F26Dot6 pC)
		{
			return F26Dot6.FromLiteral(MulDiv_NoRound(*(int*)&pA, *(int*)&pB, *(int*)&pC));
		}
		
		/// <summary>
		/// Computes (a * b) / c with maximum
		/// accuracy and no rounding.
		/// </summary>
		public static int MulDiv_NoRound(int a, int b, int c)
		{
			if (c == 0)
			{
				throw new DivideByZeroException("Cannot divide by zero!");
			}
			int s;
			int d;
			
			// s will be -1 if the result
			// is negative.
			s = (int)((a ^ b ^ c) & ((uint)int.MaxValue + 1));
			// These next 3 statements 
			// can actually be optimized
			// to be no branches at all,
			// and actually become only
			// 3 instructions each on x86.
			if (a < 0) { a = -a; }
			if (b < 0) { b = -b; }
			if (c < 0) { c = -c; }
			d = (int)(((long)a * b) / c);
			return (int)((s != 0) ? -d : d);
		}
		
		#region Mathmatical Operators
		
		public static unsafe F26Dot6 Div_NoRound(F26Dot6 pA, F26Dot6 pB)
		{
			int a = *(int*)&pA;
			int b = *(int*)&pB;
			return F26Dot6.FromLiteral((a << 6) / b);
		}
		
		public static unsafe F26Dot6 operator /(F26Dot6 pA, F26Dot6 pB)
		{
			int a = *(int*)&pA;
			int b = *(int*)&pB;
			if (a == 0)
				return F26Dot6.Zero;
			if (b == 0)
				throw new DivideByZeroException("Cannot divide by zero!");
			int s;
			int d;
			s = (int)((a ^ b) & ((uint)int.MaxValue + 1));
			if (a < 0) { a = -a; }
			if (b < 0) { b = -b; }
			d = (int)((((long)a << 6) + (b >> 1)) / b);
			return F26Dot6.FromLiteral((int)((s != 0) ? -d : d));
		}
		
		public static F26Dot6 operator /(F26Dot6 a, int b)
		{
			if (a.value == 0)
				return new F26Dot6(0);
			if (b == 0)
				throw new DivideByZeroException("Cannot divide by zero!");
			return a / FromLiteral(b << 6);
		}
		
		public static unsafe F26Dot6 Mul_NoRound(F26Dot6 pA, F26Dot6 pB)
		{
			int a = *(int*)&pA;
			int b = *(int*)&pB;
			return F26Dot6.FromLiteral((a * b) >> 6);
		}
		
		public static unsafe F26Dot6 operator *(F26Dot6 pA, F26Dot6 pB)
		{
			int a = *(int*)&pA;
			int b = *(int*)&pB;
			if (a == 0 || b == 0)
				return F26Dot6.Zero;
			int s;
			int d;
			s = (int)((a ^ b) & ((uint)int.MaxValue + 1));
			if (a < 0) { a = -a; }
			if (b < 0) { b = -b; }
			d = (int)(((long)a * b + 32L) >> 6);
			return F26Dot6.FromLiteral((int)((s != 0) ? -d : d));
		}
		
		public static F26Dot6 operator +(F26Dot6 a, F26Dot6 b)
		{
			return F26Dot6.FromLiteral((int)(a.value + b.value));
		}
		
		public static F26Dot6 operator -(F26Dot6 a, F26Dot6 b)
		{
			return F26Dot6.FromLiteral((int)(a.value - b.value));
		}
		
		public static F26Dot6 operator -(int a, F26Dot6 b)
		{
			return F26Dot6.FromLiteral((int)((a << 6) - b.value));
		}
		
		public static F26Dot6 operator -(F26Dot6 a)
		{
			return F26Dot6.FromLiteral(-a.value);
		}
		
		public static F26Dot6 operator ^(F26Dot6 a, F26Dot6 b)
		{
			return F26Dot6.FromLiteral(a.value ^ b.value);
		}
		
		public static F26Dot6 operator &(F26Dot6 a, F26Dot6 b)
		{
			return F26Dot6.FromLiteral(a.value & b.value);
		}
		
		public static F26Dot6 operator <<(F26Dot6 a, int b)
		{
			return F26Dot6.FromLiteral(a.value << b);
		}
		
		public static F26Dot6 operator >>(F26Dot6 a, int b)
		{
			return F26Dot6.FromLiteral(a.value >> b);
		}
		
		public static bool operator <(F26Dot6 a, F26Dot6 b)
		{
			return a.value < b.value;
		}
		
		public static bool operator <=(F26Dot6 a, F26Dot6 b)
		{
			return a.value <= b.value;
		}
		
		public static bool operator >(F26Dot6 a, F26Dot6 b)
		{
			return a.value > b.value;
		}
		
		public static bool operator >=(F26Dot6 a, F26Dot6 b)
		{
			return a.value >= b.value;
		}
		
		public static bool operator ==(F26Dot6 a, int b)
		{
			return a.value == b << 6;
		}
		
		public static bool operator !=(F26Dot6 a, int b)
		{
			return a.value != b << 6;
		}
		
		public static bool operator ==(F26Dot6 a, F26Dot6 b)
		{
			return a.value == b.value;
		}
		
		public static bool operator !=(F26Dot6 a, F26Dot6 b)
		{
			return a.value != b.value;
		}
		
		
		public static F26Dot6 Abs(F26Dot6 val)
		{
			if (val.value < 0)
			{
				return F26Dot6.FromLiteral(-val.value);
			}
			return F26Dot6.FromLiteral(val.value);
		}
		
		public static F26Dot6 Ceiling(F26Dot6 val)
		{
			return F26Dot6.FromDouble(Math.Ceiling(ToDouble(val)));
		}
		
		public static F26Dot6 Floor(F26Dot6 val)
		{
			return F26Dot6.FromDouble(Math.Floor(ToDouble(val)));
		}
		
		public static F26Dot6 Round(F26Dot6 val)
		{
			return F26Dot6.FromDouble(Math.Round(ToDouble(val)));
		}
#endregion
		
		#region Explicit Conversion Operators
		public static unsafe explicit operator int(F26Dot6 val)
		{
			return *(int*)&val;
		}
#endregion
		
		#region Implicit Conversion Operators
		public static implicit operator F26Dot6(byte val) { return new F26Dot6(val); }
		public static implicit operator F26Dot6(sbyte val) { return new F26Dot6(val); }
		public static implicit operator F26Dot6(ushort val) { return new F26Dot6(val); }
		public static implicit operator F26Dot6(short val) { return new F26Dot6(val); }
		public static implicit operator F26Dot6(uint val) { return new F26Dot6(val); }
		public static implicit operator F26Dot6(int val) { return new F26Dot6(val); }
		public static implicit operator F26Dot6(float val) { return FromDouble((double)val); }
		public static implicit operator F26Dot6(double val) { return FromDouble(val); }
#endregion
		
		#region IEquatable<F26Dot6> Members
		bool IEquatable<F26Dot6>.Equals(F26Dot6 other)
		{
			return (other.value == this.value);
		}
#endregion
		
		#region Other Members
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
		public override bool Equals(object obj)
		{
			if (obj is F26Dot6)
				return value == ((F26Dot6)obj).value;
			else
				return false;
		}
#endregion
		
	}
}
