using System;
using System.Collections.Generic;
using Orvid.CodeDom;

namespace Orvid.Assembler.InstructionGen
{
	public static class FieldTypeRegistry
	{
		public struct FieldEntry
		{
			public readonly int ID;
			public readonly string Name;
			public readonly string Prefix;
			public readonly CodeTypeReference CodeType;

			internal FieldEntry(int id, string name, string prefix)
			{
				this.ID = id;
				this.Name = name;
				this.Prefix = prefix;
				this.CodeType = new CodeTypeReference(name);
			}
		}

		private static int mMaxFieldID = 0;
		private static FieldEntry[] mFields = new FieldEntry[InitialFieldsSize];
		public static FieldEntry[] Fields
		{
			get { return mFields; }
		}
		
		public static readonly FieldEntry SByte =   new FieldEntry(0 , typeof(sbyte).ToString(),  "sbyte_");
		public static readonly FieldEntry Byte =    new FieldEntry(1 , typeof(byte).ToString(),   "byte_");
		public static readonly FieldEntry Short =   new FieldEntry(2 , typeof(short).ToString(),  "short_");
		public static readonly FieldEntry UShort =  new FieldEntry(3 , typeof(ushort).ToString(), "ushort_");
		public static readonly FieldEntry Int =     new FieldEntry(4 , typeof(int).ToString(),    "int_");
		public static readonly FieldEntry UInt =    new FieldEntry(5 , typeof(uint).ToString(),   "uint_");
		public static readonly FieldEntry Long =    new FieldEntry(6 , typeof(long).ToString(),   "long_");
		public static readonly FieldEntry ULong =   new FieldEntry(7 , typeof(ulong).ToString(),  "ulong_");
		public static readonly FieldEntry Float =   new FieldEntry(8 , typeof(float).ToString(),  "float_");
		public static readonly FieldEntry Double =  new FieldEntry(9 , typeof(double).ToString(), "double_");
		public static readonly FieldEntry String =  new FieldEntry(10, typeof(string).ToString(), "string_");
		public static readonly FieldEntry Char =    new FieldEntry(11, typeof(char).ToString(),   "char_");
		public static readonly FieldEntry Invalid = new FieldEntry(12, typeof(void).ToString(),   "error_");

		public static FieldEntry Segment;

		static FieldTypeRegistry()
		{
			Initialize();
		}
		
		// The initial size here should always be a power of 2
		// so that it is fast to allocate.
		private const int InitialFieldsSize = 16;
		private static void Initialize()
		{
			// If you add to this list make sure it still fits in the initial
			// size of mFields. This list currently has 13 entries.
			// The last value in the list should NOT increment mMaxFieldID.
			mFields[mMaxFieldID++] = FieldTypeRegistry.SByte;
			mFields[mMaxFieldID++] = FieldTypeRegistry.Byte;
			mFields[mMaxFieldID++] = FieldTypeRegistry.Short;
			mFields[mMaxFieldID++] = FieldTypeRegistry.UShort;
			mFields[mMaxFieldID++] = FieldTypeRegistry.Int;
			mFields[mMaxFieldID++] = FieldTypeRegistry.UInt;
			mFields[mMaxFieldID++] = FieldTypeRegistry.Long;
			mFields[mMaxFieldID++] = FieldTypeRegistry.ULong;
			mFields[mMaxFieldID++] = FieldTypeRegistry.Float;
			mFields[mMaxFieldID++] = FieldTypeRegistry.Double;
			mFields[mMaxFieldID++] = FieldTypeRegistry.String;
			mFields[mMaxFieldID++] = FieldTypeRegistry.Char;
			mFields[mMaxFieldID] =   FieldTypeRegistry.Invalid;
			// Decimal is deleberately left out of this list.
		}

		public static void Reset()
		{
			mFields = new FieldEntry[InitialFieldsSize];
			mMaxFieldID = 0;
			Initialize();
		}

		/// <summary>
		/// The maximum field ID used.
		/// This is needed because the mFields
		/// array doubles in size every time it
		/// fills.
		/// </summary>
		public static int MaxFieldID
		{
			get { return mMaxFieldID; }
		}

		// SPEED: The speed of this can be improved
		//        by using a dictionary for the lookup
		//        that is keyed on the name, and holds
		//        the FieldID.
		public static int GetType(string name)
		{
			switch (name.ToLower())
			{
				case "byte":
					return Byte.ID;
				case "sbyte":
					return SByte.ID;
				case "ushort":
					return UShort.ID;
				case "short":
					return Short.ID;
				case "uint":
					return UInt.ID;
				case "int":
					return Int.ID;
				case "ulong":
					return ULong.ID;
				case "long":
					return Long.ID;
				case "float":
					return Float.ID;
				case "double":
					return Double.ID;
				case "string":
					return String.ID;
				case "char":
					return Char.ID;
				case "invalid":
					return Invalid.ID;
				default:
					for (int i = 0; i <= mMaxFieldID; i++)
					{
						if (mFields[i].Name == name)
							return mFields[i].ID;
					}
					throw new Exception("Unknown type '" + name + "'!");
			}
		}

		public static int RegisterType(string name, string prefix)
		{
			// FieldID is an index.
			if (mMaxFieldID + 2 > mFields.Length)
			{
				Array.Resize(ref mFields, mFields.Length << 1);
			}
			mMaxFieldID++;
			mFields[mMaxFieldID] = new FieldEntry(mMaxFieldID, name, prefix);
			return mMaxFieldID;
		}



	}
}

