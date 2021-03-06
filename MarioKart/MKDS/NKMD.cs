﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LibEveryFileExplorer.Files;
using System.Drawing;
using System.IO;
using LibEveryFileExplorer.GameData;
using System.Windows.Forms;
using LibEveryFileExplorer.Collections;
using Tao.OpenGl;
using MarioKart.UI;
using LibEveryFileExplorer.GFX;
using LibEveryFileExplorer.Math;

namespace MarioKart.MKDS
{
	public class NKMD : FileFormat<NKMD.NKMDIdentifier>, IViewable
	{
		public NKMD(byte[] Data)
		{
			EndianBinaryReader er = new EndianBinaryReader(new MemoryStream(Data), Endianness.LittleEndian);
			try
			{
				Header = new NKMDHeader(er);
				foreach (var v in Header.SectionOffsets)
				{
					er.BaseStream.Position = Header.HeaderSize + v;
					String sig = er.ReadString(Encoding.ASCII, 4);
					er.BaseStream.Position -= 4;
					switch (sig)
					{
						case "OBJI": ObjectInformation = new OBJI(er); break;
						case "PATH": Path = new PATH(er); break;
						case "POIT": Point = new POIT(er); break;
						case "STAG": Stage = new STAG(er); break;
						case "KTPS": KartPointStart = new KTPS(er); break;
						case "KTPJ": KartPointJugem = new KTPJ(er, Header.Version); break;
						case "KTP2": KartPointSecond = new KTP2(er); break;
						case "KTPC": KartPointCannon = new KTPC(er); break;
						case "KTPM": KartPointMission = new KTPM(er); break;
						default:
							//throw new Exception("Unknown Section: " + sig);
							continue;
						//goto cont;
					}
				}
			cont:
				if (KartPointCannon == null) KartPointCannon = new KTPC();
				if (KartPointMission == null) KartPointMission = new KTPM();
			}
			catch (SignatureNotCorrectException e)
			{
				MessageBox.Show(e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			finally
			{
				er.Close();
			}
		}

		public Form GetDialog()
		{
			return new NKMDViewer(this);
		}

		public NKMDHeader Header;
		public class NKMDHeader
		{
			public NKMDHeader(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "NKMD") throw new SignatureNotCorrectException(Signature, "NKMD", er.BaseStream.Position - 4);
				Version = er.ReadUInt16();
				HeaderSize = er.ReadUInt16();
				SectionOffsets = er.ReadUInt32s((HeaderSize - 8) / 4);
			}
			public String Signature;
			public UInt16 Version;
			public UInt16 HeaderSize;
			public UInt32[] SectionOffsets;
		}

		public OBJI ObjectInformation;
		public class OBJI : GameDataSection<OBJI.OBJIEntry>
		{
			public OBJI() { Signature = "OBJI"; }
			public OBJI(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "OBJI") throw new SignatureNotCorrectException(Signature, "OBJI", er.BaseStream.Position - 4);
				NrEntries = er.ReadUInt32();
				for (int i = 0; i < NrEntries; i++) Entries.Add(new OBJIEntry(er));
			}

			public override String[] GetColumnNames()
			{
				return new String[] {
					"ID",
					"X", "Y", "Z",
					"X Angle", "Y Angle", "Z Angle",
					"X Scale", "Y Scale", "Z Scale",
					"Object ID",
					"Route ID",
					"Setting 1", "Setting 2", "Setting 3", "Setting 4", "Setting 5", "Setting 6", "Setting 7", "Setting 8",
					"TT Visible"
				};
			}

			public class OBJIEntry : GameDataSectionEntry
			{
				public OBJIEntry()
				{
					Position = new Vector3(0, 0, 0);
					Rotation = new Vector3(0, 0, 0);
					Scale = new Vector3(1, 1, 1);
					ObjectID = 0x0065;//itembox
					RouteID = -1;
					Settings = new ushort[8];
					TTVisible = true;
				}
				public OBJIEntry(EndianBinaryReader er)
				{
					Position = er.ReadVecFx32();
					Rotation = er.ReadVecFx32();
					Scale = er.ReadVecFx32();
					ObjectID = er.ReadUInt16();
					RouteID = er.ReadInt16();
					Settings = er.ReadUInt16s(8);
					TTVisible = er.ReadUInt32() == 1;
				}

				public override ListViewItem GetListViewItem()
				{
					ListViewItem m = new ListViewItem("");
					m.SubItems.Add(Position.X.ToString("#####0.############"));
					m.SubItems.Add(Position.Y.ToString("#####0.############"));
					m.SubItems.Add(Position.Z.ToString("#####0.############"));

					m.SubItems.Add(Rotation.X.ToString("#####0.############"));
					m.SubItems.Add(Rotation.Y.ToString("#####0.############"));
					m.SubItems.Add(Rotation.Z.ToString("#####0.############"));

					m.SubItems.Add(Scale.X.ToString("#####0.############"));
					m.SubItems.Add(Scale.Y.ToString("#####0.############"));
					m.SubItems.Add(Scale.Z.ToString("#####0.############"));

					//ObjectDb.Object ob = MKDS_Const.ObjectDatabase.GetObject(o.ObjectID);
					//if (ob != null) i.SubItems.Add(ob.ToString());
					/*else */
					m.SubItems.Add(GetHexReverse(ObjectID));
					m.SubItems.Add(RouteID.ToString());

					m.SubItems.Add(GetHexReverse(Settings[0]));
					m.SubItems.Add(GetHexReverse(Settings[1]));
					m.SubItems.Add(GetHexReverse(Settings[2]));
					m.SubItems.Add(GetHexReverse(Settings[3]));
					m.SubItems.Add(GetHexReverse(Settings[4]));
					m.SubItems.Add(GetHexReverse(Settings[5]));
					m.SubItems.Add(GetHexReverse(Settings[6]));
					m.SubItems.Add(GetHexReverse(Settings[7]));

					m.SubItems.Add(TTVisible.ToString());
					return m;
				}

				public Vector3 Position { get; set; }
				public Vector3 Rotation { get; set; }
				public Vector3 Scale { get; set; }
				public UInt16 ObjectID { get; set; }
				public Int16 RouteID { get; set; }
				public UInt16[] Settings { get; set; }//8
				public Boolean TTVisible { get; set; }
			}
		}

		public PATH Path;
		public class PATH : GameDataSection<PATH.PATHEntry>
		{
			public PATH() { Signature = "PATH"; }
			public PATH(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "PATH") throw new SignatureNotCorrectException(Signature, "PATH", er.BaseStream.Position - 4);
				NrEntries = er.ReadUInt32();
				for (int i = 0; i < NrEntries; i++) Entries.Add(new PATHEntry(er));
			}

			public override String[] GetColumnNames()
			{
				return new String[] {
					"ID",
					"Index",
					"Loop",
					"Nr Points"
				};
			}

			public class PATHEntry : GameDataSectionEntry
			{
				public PATHEntry()
				{
					Index = 0;
					Loop = false;
					NrPoit = 0;
				}
				public PATHEntry(EndianBinaryReader er)
				{
					Index = er.ReadByte();
					Loop = er.ReadByte() == 1;
					NrPoit = er.ReadInt16();
				}

				public override ListViewItem GetListViewItem()
				{
					ListViewItem m = new ListViewItem("");
					m.SubItems.Add(Index.ToString());
					m.SubItems.Add(Loop.ToString());
					m.SubItems.Add(NrPoit.ToString());
					return m;
				}

				public Byte Index { get; set; }
				public Boolean Loop { get; set; }
				public Int16 NrPoit { get; set; }
			}
		}

		public POIT Point;
		public class POIT : GameDataSection<POIT.POITEntry>
		{
			public POIT() { Signature = "POIT"; }
			public POIT(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "POIT") throw new SignatureNotCorrectException(Signature, "POIT", er.BaseStream.Position - 4);
				NrEntries = er.ReadUInt32();
				for (int i = 0; i < NrEntries; i++) Entries.Add(new POITEntry(er));
			}

			public override String[] GetColumnNames()
			{
				return new String[] {
					"ID",
					"X", "Y", "Z",
					"Index",
					"Duration",
					"?"
				};
			}

			public class POITEntry : GameDataSectionEntry
			{
				public POITEntry()
				{
					Position = new Vector3(0, 0, 0);
					Index = 0;
					Duration = 0;
					Unknown = 0;
				}
				public POITEntry(EndianBinaryReader er)
				{
					Position = er.ReadVecFx32();
					Index = er.ReadInt16();
					Duration = er.ReadInt16();
					Unknown = er.ReadUInt32();
				}

				public override ListViewItem GetListViewItem()
				{
					ListViewItem m = new ListViewItem("");
					m.SubItems.Add(Position.X.ToString("#####0.############"));
					m.SubItems.Add(Position.Y.ToString("#####0.############"));
					m.SubItems.Add(Position.Z.ToString("#####0.############"));

					m.SubItems.Add(Index.ToString());
					m.SubItems.Add(Duration.ToString());
					m.SubItems.Add(GetHexReverse(Unknown));
					return m;
				}

				public Vector3 Position { get; set; }
				public Int16 Index { get; set; }
				public Int16 Duration { get; set; }
				public UInt32 Unknown { get; set; }
			}
		}

		public STAG Stage;
		public class STAG
		{
			public STAG(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "STAG") throw new SignatureNotCorrectException(Signature, "STAG", er.BaseStream.Position - 4);
				Unknown1 = er.ReadUInt16();
				NrLaps = er.ReadInt16();
				Unknown2 = er.ReadByte();
				FogEnabled = er.ReadByte() == 1;
				FogTableGenMode = er.ReadByte();
				FogSlope = er.ReadByte();
				UnknownData1 = er.ReadBytes(0x8);
				FogDensity = er.ReadFx32();
				FogColor = Color.FromArgb((int)GFXUtil.XBGR1555ToArgb(er.ReadUInt16()));
				FogAlpha = er.ReadUInt16();
				KclColor1 = Color.FromArgb((int)GFXUtil.XBGR1555ToArgb(er.ReadUInt16()));
				KclColor2 = Color.FromArgb((int)GFXUtil.XBGR1555ToArgb(er.ReadUInt16()));
				KclColor3 = Color.FromArgb((int)GFXUtil.XBGR1555ToArgb(er.ReadUInt16()));
				KclColor4 = Color.FromArgb((int)GFXUtil.XBGR1555ToArgb(er.ReadUInt16()));
				UnknownData2 = er.ReadBytes(0x8);
			}

			public String Signature;
			public UInt16 Unknown1;
			public Int16 NrLaps;
			public Byte Unknown2;
			public Boolean FogEnabled;
			public Byte FogTableGenMode;
			public Byte FogSlope;
			public byte[] UnknownData1;
			public Single FogDensity;
			public Color FogColor;
			public UInt16 FogAlpha;
			public Color KclColor1;
			public Color KclColor2;
			public Color KclColor3;
			public Color KclColor4;
			public byte[] UnknownData2;
		}

		public KTPS KartPointStart;
		public class KTPS : GameDataSection<KTPS.KTPSEntry>
		{
			public KTPS() { Signature = "KTPS"; }
			public KTPS(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "KTPS") throw new SignatureNotCorrectException(Signature, "KTPS", er.BaseStream.Position - 4);
				NrEntries = er.ReadUInt32();
				for (int i = 0; i < NrEntries; i++) Entries.Add(new KTPSEntry(er));
			}

			public override String[] GetColumnNames()
			{
				return new String[] {
					"ID",
					"X", "Y", "Z",
					"X Angle", "Y Angle", "Z Angle",
					"?",
					"Index"
				};
			}

			public class KTPSEntry : GameDataSectionEntry
			{
				public KTPSEntry()
				{
					Position = new Vector3(0, 0, 0);
					Rotation = new Vector3(0, 0, 0);
					Unknown = 0xFFFF;
					Index = -1;
				}
				public KTPSEntry(EndianBinaryReader er)
				{
					Position = er.ReadVecFx32();
					Rotation = er.ReadVecFx32();
					Unknown = er.ReadUInt16();
					Index = er.ReadInt16();
				}

				public override ListViewItem GetListViewItem()
				{
					ListViewItem m = new ListViewItem("");
					m.SubItems.Add(Position.X.ToString("#####0.############"));
					m.SubItems.Add(Position.Y.ToString("#####0.############"));
					m.SubItems.Add(Position.Z.ToString("#####0.############"));

					m.SubItems.Add(Rotation.X.ToString("#####0.############"));
					m.SubItems.Add(Rotation.Y.ToString("#####0.############"));
					m.SubItems.Add(Rotation.Z.ToString("#####0.############"));

					m.SubItems.Add(GetHexReverse(Unknown));
					m.SubItems.Add(Index.ToString());
					return m;
				}

				public Vector3 Position { get; set; }
				public Vector3 Rotation { get; set; }
				public UInt16 Unknown { get; set; }
				public Int16 Index { get; set; }
			}
		}

		public KTPJ KartPointJugem;
		public class KTPJ : GameDataSection<KTPJ.KTPJEntry>
		{
			public KTPJ() { Signature = "KTPJ"; }
			public KTPJ(EndianBinaryReader er, UInt16 Version)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "KTPJ") throw new SignatureNotCorrectException(Signature, "KTPJ", er.BaseStream.Position - 4);
				NrEntries = er.ReadUInt32();
				for (int i = 0; i < NrEntries; i++) Entries.Add(new KTPJEntry(er, Version));
			}

			public override String[] GetColumnNames()
			{
				return new String[] {
					"ID",
					"X", "Y", "Z",
					"X Angle", "Y Angle", "Z Angle",
					"EPOI Id",
					"IPOI Id",
					"Index"
				};
			}

			public class KTPJEntry : GameDataSectionEntry
			{
				private UInt16 Version = 37;
				public KTPJEntry()
				{
					Position = new Vector3(0, 0, 0);
					Rotation = new Vector3(0, 0, 0);
					EnemyPositionID = 0;
					ItemPositionID = 0;
					Index = 0;
				}
				public KTPJEntry(EndianBinaryReader er, UInt16 Version)
				{
					this.Version = Version;
					Position = er.ReadVecFx32();
					Rotation = er.ReadVecFx32();
					if (Version <= 34)
					{
						float yangle = (float)Math.Atan2(Rotation.X, Rotation.Z);
						Rotation = new Vector3(0, MathUtil.RadToDeg(yangle), 0);
					}
					EnemyPositionID = er.ReadInt16();
					ItemPositionID = er.ReadInt16();
					if (Version >= 34) Index = er.ReadInt32();
				}

				public override ListViewItem GetListViewItem()
				{
					ListViewItem m = new ListViewItem("");
					m.SubItems.Add(Position.X.ToString("#####0.############"));
					m.SubItems.Add(Position.Y.ToString("#####0.############"));
					m.SubItems.Add(Position.Z.ToString("#####0.############"));

					m.SubItems.Add(Rotation.X.ToString("#####0.############"));
					m.SubItems.Add(Rotation.Y.ToString("#####0.############"));
					m.SubItems.Add(Rotation.Z.ToString("#####0.############"));

					m.SubItems.Add(EnemyPositionID.ToString());
					m.SubItems.Add(ItemPositionID.ToString());
					m.SubItems.Add(Index.ToString());
					return m;
				}

				public Vector3 Position { get; set; }
				public Vector3 Rotation { get; set; }
				public Int16 EnemyPositionID { get; set; }
				public Int16 ItemPositionID { get; set; }
				public Int32 Index { get; set; }
			}
		}

		public KTP2 KartPointSecond;
		public class KTP2 : GameDataSection<KTP2.KTP2Entry>
		{
			public KTP2() { Signature = "KTP2"; }
			public KTP2(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "KTP2") throw new SignatureNotCorrectException(Signature, "KTP2", er.BaseStream.Position - 4);
				NrEntries = er.ReadUInt32();
				for (int i = 0; i < NrEntries; i++) Entries.Add(new KTP2Entry(er));
			}

			public override String[] GetColumnNames()
			{
				return new String[] {
					"ID",
					"X", "Y", "Z",
					"X Angle", "Y Angle", "Z Angle",
					"?",
					"Index"
				};
			}

			public class KTP2Entry : GameDataSectionEntry
			{
				public KTP2Entry()
				{
					Position = new Vector3(0, 0, 0);
					Rotation = new Vector3(0, 0, 0);
					Unknown = 0xFFFF;
					Index = -1;
				}
				public KTP2Entry(EndianBinaryReader er)
				{
					Position = er.ReadVecFx32();
					Rotation = er.ReadVecFx32();
					Unknown = er.ReadUInt16();
					Index = er.ReadInt16();
				}

				public override ListViewItem GetListViewItem()
				{
					ListViewItem m = new ListViewItem("");
					m.SubItems.Add(Position.X.ToString("#####0.############"));
					m.SubItems.Add(Position.Y.ToString("#####0.############"));
					m.SubItems.Add(Position.Z.ToString("#####0.############"));

					m.SubItems.Add(Rotation.X.ToString("#####0.############"));
					m.SubItems.Add(Rotation.Y.ToString("#####0.############"));
					m.SubItems.Add(Rotation.Z.ToString("#####0.############"));

					m.SubItems.Add(GetHexReverse(Unknown));
					m.SubItems.Add(Index.ToString());
					return m;
				}

				public Vector3 Position { get; set; }
				public Vector3 Rotation { get; set; }
				public UInt16 Unknown { get; set; }
				public Int16 Index { get; set; }
			}
		}

		public KTPC KartPointCannon;
		public class KTPC : GameDataSection<KTPC.KTPCEntry>
		{
			public KTPC() { Signature = "KTPC"; }
			public KTPC(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "KTPC") throw new SignatureNotCorrectException(Signature, "KTPC", er.BaseStream.Position - 4);
				NrEntries = er.ReadUInt32();
				for (int i = 0; i < NrEntries; i++) Entries.Add(new KTPCEntry(er));
			}

			public override String[] GetColumnNames()
			{
				return new String[] {
					"ID",
					"X", "Y", "Z",
					"X Angle", "Y Angle", "Z Angle",
					"Next MEPO",
					"Index"
				};
			}

			public class KTPCEntry : GameDataSectionEntry
			{
				public KTPCEntry()
				{
					Position = new Vector3(0, 0, 0);
					Rotation = new Vector3(0, 0, 0);
					NextMEPO = -1;
					Index = -1;
				}
				public KTPCEntry(EndianBinaryReader er)
				{
					Position = er.ReadVecFx32();
					Rotation = er.ReadVecFx32();
					NextMEPO = er.ReadInt16();
					Index = er.ReadInt16();
				}

				public override ListViewItem GetListViewItem()
				{
					ListViewItem m = new ListViewItem("");
					m.SubItems.Add(Position.X.ToString("#####0.############"));
					m.SubItems.Add(Position.Y.ToString("#####0.############"));
					m.SubItems.Add(Position.Z.ToString("#####0.############"));

					m.SubItems.Add(Rotation.X.ToString("#####0.############"));
					m.SubItems.Add(Rotation.Y.ToString("#####0.############"));
					m.SubItems.Add(Rotation.Z.ToString("#####0.############"));

					m.SubItems.Add(NextMEPO.ToString());
					m.SubItems.Add(Index.ToString());
					return m;
				}

				public Vector3 Position { get; set; }
				public Vector3 Rotation { get; set; }
				public Int16 NextMEPO { get; set; }
				public Int16 Index { get; set; }
			}
		}

		public KTPM KartPointMission;
		public class KTPM : GameDataSection<KTPM.KTPMEntry>
		{
			public KTPM() { Signature = "KTPM"; }
			public KTPM(EndianBinaryReader er)
			{
				Signature = er.ReadString(Encoding.ASCII, 4);
				if (Signature != "KTPM") throw new SignatureNotCorrectException(Signature, "KTPM", er.BaseStream.Position - 4);
				NrEntries = er.ReadUInt32();
				for (int i = 0; i < NrEntries; i++) Entries.Add(new KTPMEntry(er));
			}

			public override String[] GetColumnNames()
			{
				return new String[] {
					"ID",
					"X", "Y", "Z",
					"X Angle", "Y Angle", "Z Angle",
					"?",
					"Index"
				};
			}

			public class KTPMEntry : GameDataSectionEntry
			{
				public KTPMEntry()
				{
					Position = new Vector3(0, 0, 0);
					Rotation = new Vector3(0, 0, 0);
					Unknown = 0xFFFF;
					Index = 1;
				}
				public KTPMEntry(EndianBinaryReader er)
				{
					Position = er.ReadVecFx32();
					Rotation = er.ReadVecFx32();
					Unknown = er.ReadUInt16();
					Index = er.ReadInt16();
				}

				public override ListViewItem GetListViewItem()
				{
					ListViewItem m = new ListViewItem("");
					m.SubItems.Add(Position.X.ToString("#####0.############"));
					m.SubItems.Add(Position.Y.ToString("#####0.############"));
					m.SubItems.Add(Position.Z.ToString("#####0.############"));

					m.SubItems.Add(Rotation.X.ToString("#####0.############"));
					m.SubItems.Add(Rotation.Y.ToString("#####0.############"));
					m.SubItems.Add(Rotation.Z.ToString("#####0.############"));

					m.SubItems.Add(GetHexReverse(Unknown));
					m.SubItems.Add(Index.ToString());
					return m;
				}

				public Vector3 Position { get; set; }
				public Vector3 Rotation { get; set; }
				public UInt16 Unknown { get; set; }
				public Int16 Index { get; set; }
			}
		}

		public class NKMDIdentifier : FileFormatIdentifier
		{
			public override string GetCategory()
			{
				return "Mario Kart DS";
			}

			public override string GetFileDescription()
			{
				return "Nitro Kart Map Data (NKM)";
			}

			public override string GetFileFilter()
			{
				return "Nitro Kart Map Data (*.nkm)|*.nkm";
			}

			public override Bitmap GetIcon()
			{
				return Resource.Cone;
			}

			public override FormatMatch IsFormat(EFEFile File)
			{
				if (File.Data.Length > 4 && File.Data[0] == 'N' && File.Data[1] == 'K' && File.Data[2] == 'M' && File.Data[3] == 'D') return FormatMatch.Content;
				return FormatMatch.No;
			}
		}
	}
}
