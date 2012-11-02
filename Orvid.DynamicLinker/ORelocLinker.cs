using System;
using System.IO;

namespace Orvid.DynamicLinker
{
	public class ORelocLinker
	{
#pragma warning disable 414
		private bool IsBigEndian;
#pragma warning restore 414
		private unsafe byte* ImageBase;
		private uint PointerSize;
		private uint originalBaseAddress;
		private uint OriginalBaseAddress
		{
			get { return originalBaseAddress;}
			set
			{
				originalBaseAddress = value;
				CurrentBaseAddress = value;
			}
		}
		private uint CurrentBaseAddress;
		private uint[] Entries;
		private ORelocPointersTableEntry[] PointersTableEntries;

		private ORelocLinker()
		{
			//BaseStream = new MemoryStream();
		}

		public unsafe ORelocLinker(byte* strm, uint length)
		{
			var memoryStream = new UnmanagedMemoryStream(strm, length);
			BinaryReader rdr = new BinaryReader(memoryStream);
			char[] hdr = rdr.ReadChars(4);
			if (hdr [0] == 'O' && hdr [1] == 'R' && hdr [2] == 'L' && hdr [3] == 'C')
			{
				uint version = rdr.ReadUInt32();
				if ((version & 0x10000000) != 0)
				{
					version &= 0x7FFFFFFF;
					this.IsBigEndian = true;
					throw new Exception("Big endian reloc tables aren't currently supported!");
				}
				else
				{
					this.PointerSize = rdr.ReadUInt32();
					uint entSize = rdr.ReadUInt32();
					if (PointerSize == 4)
					{
						uint entryCount = 0;
						this.OriginalBaseAddress = rdr.ReadUInt32();
						switch(entSize)
						{
							case 1:
								entryCount = rdr.ReadByte();
								break;
							case 2:
								entryCount = rdr.ReadUInt16();
								break;
							case 4:
								entryCount = rdr.ReadByte();
								break;
							default:
								throw new Exception("Unsupported entry size!");
						}
						this.Entries = new uint[entryCount];
						for (uint i = 0; i < entryCount; i++)
						{
							switch(entSize)
							{
								case 1:
                                    this.Entries[i] = rdr.ReadByte();
                                    break;
                                case 2:
									this.Entries[i] = rdr.ReadUInt16();
									break;
								case 4:
									this.Entries[i] = rdr.ReadUInt32();
									break;
							}
						}
						uint pointersTableEntryCount = 0;
						switch (entSize)
						{
							case 1:
								pointersTableEntryCount = rdr.ReadByte();
								break;
							case 2:
								pointersTableEntryCount = rdr.ReadUInt16();
								break;
							case 4:
								pointersTableEntryCount = rdr.ReadUInt32();
								break;
						}
						this.PointersTableEntries = new ORelocPointersTableEntry[pointersTableEntryCount];
						for (uint i = 0; i < pointersTableEntryCount; i++)
						{
							switch(entSize)
							{
								case 1:
									this.PointersTableEntries[i].StartOffset = rdr.ReadByte();
									this.PointersTableEntries[i].PointerCount = rdr.ReadByte();
									break;
								case 2:
									this.PointersTableEntries[i].StartOffset = rdr.ReadUInt16();
									this.PointersTableEntries[i].PointerCount = rdr.ReadUInt16();
									break;
								case 4:
									this.PointersTableEntries[i].StartOffset = rdr.ReadUInt32();
									this.PointersTableEntries[i].PointerCount = rdr.ReadUInt32();
									break;
							}
						}
					}
					else
					{
						throw new Exception("Pointer sizes other than 4-bytes aren't currently supported!");
					}
				}
			}
			else
			{
				throw new Exception("Invalid OReloc table!");
			}
			ImageBase = memoryStream.PositionPointer;
		}

		public unsafe void Rebase(uint newBaseAddress)
		{
			byte* localBaseStream = this.ImageBase;
			int addressDiff = (int)((long)newBaseAddress - (long)this.CurrentBaseAddress);
			fixed (uint* entries3 = this.Entries)
			{
				uint* entries = entries3;
				uint* lastEntry = &entries[this.Entries.Length];
				while (entries < lastEntry)
				{
					*(uint*)(localBaseStream + *entries) = (uint)(*(uint*)(localBaseStream + *entries) + addressDiff);
					entries++;
				}
			}
			fixed(ORelocPointersTableEntry* entries3 = this.PointersTableEntries)
			{
				ORelocPointersTableEntry* entries = entries3;
				ORelocPointersTableEntry* lastEntry = &entries[this.PointersTableEntries.Length];
				while(entries < lastEntry)
				{
					uint* table = (uint*)(localBaseStream + entries->StartOffset);
					uint* lastTableEntry = &table[entries->PointerCount];
					while (table < lastTableEntry)
					{
						*table = (uint)(*table + addressDiff);
						table++;
					}
					entries++;
				}
			}
			CurrentBaseAddress = newBaseAddress;
		}

	}
}
