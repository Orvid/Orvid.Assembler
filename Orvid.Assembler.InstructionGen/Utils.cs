using System;
using Orvid.CodeDom;

namespace Orvid.Assembler.InstructionGen
{
	public static class Utils
	{

		public static byte GetHighestBitIndexSet(uint val)
		{
			// This will get expanded out by even
			// a half-decent loop unroller, meaning
			// this form is just as fast while being
			// much more maintainable.
			for (byte i = 31; i >= 0; i--)
			{
				if ((val & (1 << i)) != 0)
					return i;
			}
			return 0;
		}

		public static CodeExpression WrapInCast(CodeExpression srcExpr, byte sizeInBits, bool forceCastOnLiteral = false)
		{
			CodeTypeReference tp = null;
			if (sizeInBits <= 8)
				tp = StaticTypeReferences.Byte;
			else if (sizeInBits <= 16)
				tp = StaticTypeReferences.UShort;
			else if (sizeInBits <= 32)
				tp = StaticTypeReferences.UInt;
			else if (sizeInBits <= 64)
				tp = StaticTypeReferences.ULong;
			else
				throw new Exception("Geeze, that's giant!");
			
			if (srcExpr is CodeCastExpression)
			{
				if (((CodeCastExpression)srcExpr).TargetType != tp)
					((CodeCastExpression)srcExpr).TargetType = tp;
				return srcExpr;
			}
			// This is only valid because of the 
			// context in which this method is called.
			// (aka. this call is used only to widen
			// values, it is never used to make them
			// smaller)
			else if (!forceCastOnLiteral && srcExpr is CodePrimitiveExpression)
			{
				return srcExpr;
			}
			
			return new CodeCastExpression(tp, srcExpr);
		}

		public static byte RoundUpToDataSize(byte bitLen)
		{
			if (bitLen <= 8)
				return 8;
			else if (bitLen <= 16)
				return 16;
			else if (bitLen <= 32)
				return 32;
			else if (bitLen <= 64)
				return 64;
			else
				throw new Exception("Alas, the bit length was too big :(");
		}

		public static unsafe uint ParseBinary(string strArg)
		{
			uint val = 0;
			fixed (char* str2 = strArg)
			{
				char* str = str2;
				char* strEnd = &str2[strArg.Length];
				while (str < strEnd)
				{
					val <<= 1;
					if (*str == '1')
						val |= 1;
					str++;
				}
			}
			return val;
		}

		public static byte SingleDigitParse(char digit)
		{
			switch (digit)
			{
				case '0':
					return 0;
				case '1':
					return 1;
				case '2':
					return 2;
				case '3':
					return 3;
				case '4':
					return 4;
				case '5':
					return 5;
				case '6':
					return 6;
				case '7':
					return 7;
				case '8':
					return 8;
				case '9':
					return 9;
				default:
					throw new Exception("Unknown digit '" + digit + "'!");
			}
		}

	}
}

