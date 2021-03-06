﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibEveryFileExplorer.IO
{
	public class IOUtil
	{
		public static short ReadS16LE(byte[] Data, int Offset)
		{
			return (short)(Data[Offset] | (Data[Offset + 1] << 8));
		}

		public static short[] ReadS16sLE(byte[] Data, int Offset, int Count)
		{
			short[] res = new short[Count];
			for (int i = 0; i < Count; i++) res[i] = ReadS16LE(Data, Offset + i * 2);
			return res;
		}

		public static void WriteS16LE(byte[] Data, int Offset, short Value)
		{
			Data[Offset] = (byte)(Value & 0xFF);
			Data[Offset + 1] = (byte)((Value >> 8) & 0xFF);
		}

		public static void WriteS16sLE(byte[] Data, int Offset, short[] Values)
		{
			for (int i = 0; i < Values.Length; i++)
			{
				WriteS16LE(Data, Offset + i * 2, Values[i]);
			}
		}

		public static ushort ReadU16LE(byte[] Data, int Offset)
		{
			return (ushort)(Data[Offset] | (Data[Offset + 1] << 8));
		}

		public static void WriteU16LE(byte[] Data, int Offset, ushort Value)
		{
			Data[Offset] = (byte)(Value & 0xFF);
			Data[Offset + 1] = (byte)((Value >> 8) & 0xFF);
		}

		public static uint ReadU32LE(byte[] Data, int Offset)
		{
			return (uint)(Data[Offset] | (Data[Offset + 1] << 8) | (Data[Offset + 2] << 16) | (Data[Offset + 3] << 24));
		}

		public static void WriteU32LE(byte[] Data, int Offset, uint Value)
		{
			Data[Offset] = (byte)(Value & 0xFF);
			Data[Offset + 1] = (byte)((Value >> 8) & 0xFF);
			Data[Offset + 2] = (byte)((Value >> 16) & 0xFF);
			Data[Offset + 3] = (byte)((Value >> 24) & 0xFF);
		}

		public static ulong ReadU64LE(byte[] Data, int Offset)
		{
			return (ulong)Data[Offset] | ((ulong)Data[Offset + 1] << 8) | ((ulong)Data[Offset + 2] << 16) | ((ulong)Data[Offset + 3] << 24) | ((ulong)Data[Offset + 4] << 32) | ((ulong)Data[Offset + 5] << 40) | ((ulong)Data[Offset + 6] << 48) | ((ulong)Data[Offset + 7] << 56);
		}

		public static void WriteU64LE(byte[] Data, int Offset, ulong Value)
		{
			Data[Offset] = (byte)(Value & 0xFF);
			Data[Offset + 1] = (byte)((Value >> 8) & 0xFF);
			Data[Offset + 2] = (byte)((Value >> 16) & 0xFF);
			Data[Offset + 3] = (byte)((Value >> 24) & 0xFF);
			Data[Offset + 4] = (byte)((Value >> 32) & 0xFF);
			Data[Offset + 5] = (byte)((Value >> 40) & 0xFF);
			Data[Offset + 6] = (byte)((Value >> 48) & 0xFF);
			Data[Offset + 7] = (byte)((Value >> 56) & 0xFF);
		}

		public static void WriteSingleLE(byte[] Data, int Offset, float Value)
		{
			byte[] a = BitConverter.GetBytes(Value);
			Data[0 + Offset] = a[0];
			Data[1 + Offset] = a[1];
			Data[2 + Offset] = a[2];
			Data[3 + Offset] = a[3];
		}
	}
}
