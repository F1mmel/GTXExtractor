using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

public class GTXExtractor
{
    public static Dictionary<uint, string> Formats = new Dictionary<uint, string>
    {
        { 0x00000000, "GX2_SURFACE_FORMAT_INVALID" },
        { 0x0000001a, "GX2_SURFACE_FORMAT_TCS_R8_G8_B8_A8_UNORM" },
        { 0x0000041a, "GX2_SURFACE_FORMAT_TCS_R8_G8_B8_A8_SRGB" },
        { 0x00000019, "GX2_SURFACE_FORMAT_TCS_R10_G10_B10_A2_UNORM" },
        { 0x00000008, "GX2_SURFACE_FORMAT_TCS_R5_G6_B5_UNORM" },
        { 0x0000000a, "GX2_SURFACE_FORMAT_TC_R5_G5_B5_A1_UNORM" },
        { 0x0000000b, "GX2_SURFACE_FORMAT_TC_R4_G4_B4_A4_UNORM" },
        { 0x00000001, "GX2_SURFACE_FORMAT_TC_R8_UNORM" },
        { 0x00000007, "GX2_SURFACE_FORMAT_TC_R8_G8_UNORM" },
        { 0x00000002, "GX2_SURFACE_FORMAT_TC_R4_G4_UNORM" },
        { 0x00000031, "GX2_SURFACE_FORMAT_T_BC1_UNORM" },
        { 0x00000431, "GX2_SURFACE_FORMAT_T_BC1_SRGB" },
        { 0x00000032, "GX2_SURFACE_FORMAT_T_BC2_UNORM" },
        { 0x00000432, "GX2_SURFACE_FORMAT_T_BC2_SRGB" },
        { 0x00000033, "GX2_SURFACE_FORMAT_T_BC3_UNORM" },
        { 0x00000433, "GX2_SURFACE_FORMAT_T_BC3_SRGB" },
        { 0x00000034, "GX2_SURFACE_FORMAT_T_BC4_UNORM" },
        { 0x00000234, "GX2_SURFACE_FORMAT_T_BC4_SNORM" },
        { 0x00000035, "GX2_SURFACE_FORMAT_T_BC5_UNORM" },
        { 0x00000235, "GX2_SURFACE_FORMAT_T_BC5_SNORM" }
    };

    public static List<uint> BCnFormats = new List<uint>
    {
        0x31, 0x431, 0x32, 0x432, 0x33, 0x433, 0x34, 0x234, 0x35, 0x235
    };

    public class GFDData
    {
        public List<uint> Dim { get; set; } = new List<uint>();
        public List<uint> Width { get; set; } = new List<uint>();
        public List<uint> Height { get; set; } = new List<uint>();
        public List<uint> Depth { get; set; } = new List<uint>();
        public List<uint> NumMips { get; set; } = new List<uint>();
        public List<uint> Format { get; set; } = new List<uint>();
        public List<uint> AA { get; set; } = new List<uint>();
        public List<uint> Use { get; set; } = new List<uint>();
        public List<uint> ImageSize { get; set; } = new List<uint>();
        public List<ulong> ImagePtr { get; set; } = new List<ulong>();
        public List<uint> MipSize { get; set; } = new List<uint>();
        public List<ulong> MipPtr { get; set; } = new List<ulong>();
        public List<uint> TileMode { get; set; } = new List<uint>();
        public List<uint> Swizzle { get; set; } = new List<uint>();
        public List<uint> Alignment { get; set; } = new List<uint>();
        public List<uint> Pitch { get; set; } = new List<uint>();
        public List<List<uint>> CompSel { get; set; } = new List<List<uint>>();
        public List<uint> Bpp { get; set; } = new List<uint>();
        public List<uint> RealSize { get; set; } = new List<uint>();

        public List<uint> DataSize { get; set; } = new List<uint>();
        public List<byte[]> Data { get; set; } = new List<byte[]>();

        public List<List<uint>> MipOffsets { get; set; } = new List<List<uint>>();
        public Dictionary<int, byte[]> MipData { get; set; } = new Dictionary<int, byte[]>();

        public int NumImages { get; set; }
    }

    public class GFDHeader
    {
        public string Magic { get; set; }
        public uint Size { get; set; }
        public uint MajorVersion { get; set; }
        public uint MinorVersion { get; set; }
        public uint GpuVersion { get; set; }
        public uint AlignMode { get; set; }
        public uint Reserved1 { get; set; }
        public uint Reserved2 { get; set; }

        public void Data(byte[] data, int pos)
        {
            Magic = System.Text.Encoding.ASCII.GetString(data, pos, 4);
            Size = BigEndianToUInt32(data, pos + 4);
            MajorVersion = BigEndianToUInt32(data, pos + 8);
            MinorVersion = BigEndianToUInt32(data, pos + 12);
            GpuVersion = BigEndianToUInt32(data, pos + 16);
            AlignMode = BigEndianToUInt32(data, pos + 20);
            Reserved1 = BigEndianToUInt32(data, pos + 24);
            Reserved2 = BigEndianToUInt32(data, pos + 28);
        }
    }

    public class GFDBlockHeader
    {
        public string Magic { get; set; }
        public uint Size { get; set; }
        public uint MajorVersion { get; set; }
        public uint MinorVersion { get; set; }
        public uint Type { get; set; }
        public uint DataSize { get; set; }
        public uint Id { get; set; }
        public uint TypeIdx { get; set; }

        public void Data(byte[] data, int pos)
        {
            Magic = System.Text.Encoding.ASCII.GetString(data, pos, 4);
            Size = BitConverter.ToUInt32(data, pos + 4);
            MajorVersion = BigEndianToUInt32(data, pos + 8);
            MinorVersion = BigEndianToUInt32(data, pos + 12);
            Type = BigEndianToUInt32(data, pos + 16);
            DataSize = BigEndianToUInt32(data, pos + 20);
            Id = BigEndianToUInt32(data, pos + 24);
            TypeIdx = BigEndianToUInt32(data, pos + 28);

            Console.WriteLine(Magic);
            Console.WriteLine(Size);
            Console.WriteLine(MajorVersion);
            Console.WriteLine(MinorVersion);
            Console.WriteLine(Type);
            Console.WriteLine(DataSize);
            Console.WriteLine(Id);
            Console.WriteLine(TypeIdx);
        }
    }

    public class GX2Surface
    {
        public uint Dim { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
        public uint Depth { get; set; }
        public uint NumMips { get; set; }
        public uint Format { get; set; }
        public uint AA { get; set; }
        public uint Use { get; set; }
        public uint ImageSize { get; set; }
        public ulong ImagePtr { get; set; }
        public uint MipSize { get; set; }
        public ulong MipPtr { get; set; }
        public uint TileMode { get; set; }
        public uint Swizzle { get; set; }
        public uint Alignment { get; set; }
        public uint Pitch { get; set; }

        public void Data(byte[] data, int pos)
        {
            if (data.Length < pos + 72) // 16 * 4 Bytes (64 Bytes) + 8 Bytes (2x ulong)
                throw new ArgumentException("Data array is too small.");

            Dim = BigEndianToUInt32(data, pos);
            Width = BigEndianToUInt32(data, pos + 4);
            Height = BigEndianToUInt32(data, pos + 8);
            Depth = BigEndianToUInt32(data, pos + 12);
            NumMips = BigEndianToUInt32(data, pos + 16);
            Format = BigEndianToUInt32(data, pos + 20);
            AA = BigEndianToUInt32(data, pos + 24);
            Use = BigEndianToUInt32(data, pos + 28);
            ImageSize = BigEndianToUInt32(data, pos + 32);
            ImagePtr = BigEndianToUInt32(data, pos + 36);
            MipSize = BigEndianToUInt32(data, pos + 40); // Überprüfe diesen Offset
            MipPtr = BigEndianToUInt32(data, pos + 44);
            TileMode = BigEndianToUInt32(data, pos + 48);
            Swizzle = BigEndianToUInt32(data, pos + 52);
            Alignment = BigEndianToUInt32(data, pos + 56);
            Pitch = BigEndianToUInt32(data, pos + 60);
        }

        public ulong BigEndianToUInt64(byte[] data, int pos)
        {
            return ((ulong)data[pos] << 56) | ((ulong)data[pos + 1] << 48) | ((ulong)data[pos + 2] << 40) |
                   ((ulong)data[pos + 3] << 32) | ((ulong)data[pos + 4] << 24) | ((ulong)data[pos + 5] << 16) |
                   ((ulong)data[pos + 6] << 8) | data[pos + 7];
        }

    }


    public static int DivRoundUp(int n, int d)
    {
        return (n + d - 1) / d;
    }

    public static string GetCompSel(GFDData data, int idx)
    {
        string compSel = "";
        foreach (var sel in data.CompSel[idx])
        {
            switch (sel)
            {
                case 0:
                    compSel += "0";
                    break;
                case 1:
                    compSel += "1";
                    break;
                case 4:
                    compSel += "r";
                    break;
                case 5:
                    compSel += "g";
                    break;
                case 6:
                    compSel += "b";
                    break;
                case 7:
                    compSel += "a";
                    break;
                default:
                    compSel += "x";
                    break;
            }
        }
        return compSel;
    }


    public static uint BigEndianToUInt32(byte[] data, int pos)
    {
        return (uint)((data[pos] << 24) | (data[pos + 1] << 16) | (data[pos + 2] << 8) | data[pos + 3]);
    }

    public static void ExtractDDS(byte[] fileBytes, string outputFolder)
    {
        GFDHeader gfdHeader = new GFDHeader();
        gfdHeader.Data(fileBytes, 0);

        int pos = 32;
        GFDData gfdData = new GFDData();

        while (pos < fileBytes.Length)
        {
            GFDBlockHeader blockHeader = new GFDBlockHeader();
            blockHeader.Data(fileBytes, pos);

            Console.WriteLine(pos.ToString() + " :: " + fileBytes.Length + " :: " + blockHeader.Size);

            if (blockHeader.Magic == "BLK{")
            {
                GX2Surface surface = new GX2Surface();
                surface.Data(fileBytes, pos + 32);

                gfdData.Dim.Add(surface.Dim);
                gfdData.Width.Add(surface.Width);
                gfdData.Height.Add(surface.Height);
                gfdData.Depth.Add(surface.Depth);
                gfdData.NumMips.Add(surface.NumMips);
                gfdData.Format.Add(surface.Format);
                gfdData.AA.Add(surface.AA);
                gfdData.Use.Add(surface.Use);
                gfdData.ImageSize.Add(surface.ImageSize);
                gfdData.ImagePtr.Add(surface.ImagePtr);
                gfdData.MipSize.Add(surface.MipSize);
                gfdData.MipPtr.Add(surface.MipPtr);
                gfdData.TileMode.Add(surface.TileMode);
                gfdData.Swizzle.Add(surface.Swizzle);
                gfdData.Alignment.Add(surface.Alignment);
                gfdData.Pitch.Add(surface.Pitch);

                List<uint> compSel = new List<uint>();
                for (int i = 0; i < 4; i++)
                {
                    compSel.Add(BigEndianToUInt32(fileBytes, pos + 72 + i * 4));
                }
                gfdData.CompSel.Add(compSel);

                /*Console.WriteLine("Dim: " + surface.Dim);
                Console.WriteLine("Width: " + surface.Width);
                Console.WriteLine("Height: " + surface.Height);
                Console.WriteLine("Depth: " + surface.Depth);
                Console.WriteLine("NumMips: " + surface.NumMips);
                Console.WriteLine("Format: " + surface.Format);
                Console.WriteLine("AA: " + surface.AA);
                Console.WriteLine("Use: " + surface.Use);
                Console.WriteLine("ImageSize: " + surface.ImageSize);
                Console.WriteLine("ImagePtr: " + surface.ImagePtr);
                Console.WriteLine("MipSize: " + surface.MipSize);
                Console.WriteLine("MipPtr: " + surface.MipPtr);
                Console.WriteLine("TileMode: " + surface.TileMode);
                Console.WriteLine("Swizzle: " + surface.Swizzle);
                Console.WriteLine("Alignment: " + surface.Alignment);
                Console.WriteLine("Pitch: " + surface.Pitch);
                Console.WriteLine("DataSize: " + blockHeader.DataSize);*/
                gfdData.Bpp.Add((uint)(blockHeader.DataSize / (surface.Width * surface.Height * surface.Depth)));
                gfdData.RealSize.Add(blockHeader.DataSize);

                gfdData.DataSize.Add(blockHeader.DataSize);
                byte[] data = new byte[blockHeader.DataSize];
                Array.Copy(fileBytes, (int)surface.ImagePtr, data, 0, blockHeader.DataSize);
                gfdData.Data.Add(data);

                List<uint> mipOffsets = new List<uint>();
                for (int i = 0; i < surface.NumMips; i++)
                {
                    mipOffsets.Add(BigEndianToUInt32(fileBytes, (int)(surface.MipPtr) + (i * 4)));
                }
                gfdData.MipOffsets.Add(mipOffsets);

                for (int i = 0; i < surface.NumMips; i++)
                {
                    //byte[] mipData = new byte[mipOffsets[(int)surface.NumMips - 1]];
                    //Array.Copy(fileBytes, (int)(surface.MipPtr + mipOffsets[i]), mipData, 0, mipData.Length);
                    //gfdData.MipData.Add(i, mipData);
                }
            }

            pos += (int)blockHeader.Size;
        }

        gfdData.NumImages = gfdData.Dim.Count;

        for (int i = 0; i < gfdData.NumImages; i++)
        {
            string formatName = Formats[gfdData.Format[i]];
            string compSel = GetCompSel(gfdData, i);
            string ddsFileName = $"{outputFolder}/Image_{i}.dds";

            Console.WriteLine(ddsFileName);

            using (BinaryWriter writer = new BinaryWriter(File.Open(ddsFileName, FileMode.Create)))
            {
                writer.Write(0x20534444); // 'DDS '
                writer.Write(0x7C); // size
                writer.Write(0x1 | 0x2 | 0x4 | 0x1000); // flags
                writer.Write((int)gfdData.Height[i]);
                writer.Write((int)gfdData.Width[i]);
                writer.Write((int)gfdData.ImageSize[i]);
                writer.Write(0); // depth
                writer.Write((int)gfdData.NumMips[i]);

                for (int j = 0; j < 11; j++)
                {
                    writer.Write(0); // reserved
                }

                writer.Write(0x20); // size
                writer.Write(0x4); // flags
                writer.Write(0); // fourCC
                writer.Write((int)gfdData.Bpp[i]);
                writer.Write(0x41 | 0x20000 | 0x40000); // flags
                writer.Write(0x1000); // caps
                writer.Write(0); // caps2
                writer.Write(0); // caps3
                writer.Write(0); // caps4
                writer.Write(0); // reserved

                writer.Write(gfdData.Data[i]);
            }
        }
    }
}
