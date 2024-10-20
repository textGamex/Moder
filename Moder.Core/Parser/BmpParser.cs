using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Xml.Linq;
using Moder.Core.Services.Config;
using ParadoxPower.CSharp;
using ParadoxPower.Process;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Gaming.Input;

namespace Moder.Core.Parser;

public class BmpParser
{
    private ushort BmpType = 0x4d42;
    private ushort MapBitcount = 24;
    public string FilePath { get; }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;
    public ushort Type;
    public uint Size;
    public ushort Reserved1;
    public ushort Reserved2;
    public uint Offbits;
    public uint Bitsize;
    public uint Wide;
    public uint Height;
    public ushort Planes;
    public ushort Bitcount;
    public uint Compression;
    public uint SizeImage;
    public uint XPelsPerMeter;
    public uint YPelsPerMeter;
    public uint ClrUsed;
    public uint ClrImportant;
    public byte[,,] Colors = new byte[5000, 5000, 3];//R,G,B
    public BmpParser(string filePath)
    {
        FilePath = File.Exists(filePath) ? filePath : throw new FileNotFoundException($"找不到文件: {filePath}", filePath);
        var fileName = Path.GetFileName(filePath);
        FileStream bmpStream = new FileStream(filePath,FileMode.Open);
        BinaryReader bmpBinaryReader = new BinaryReader(bmpStream);
        Type=bmpBinaryReader.ReadUInt16();
        Size = bmpBinaryReader.ReadUInt32();
        Reserved1 = bmpBinaryReader.ReadUInt16();
        Reserved2 = bmpBinaryReader.ReadUInt16();
        Offbits = bmpBinaryReader.ReadUInt32();
        Bitsize = bmpBinaryReader.ReadUInt32();
        Wide = bmpBinaryReader.ReadUInt32();
        Height = bmpBinaryReader.ReadUInt32();
        Planes = bmpBinaryReader.ReadUInt16();
        Bitcount = bmpBinaryReader.ReadUInt16();
        Compression = bmpBinaryReader.ReadUInt32();
        SizeImage = bmpBinaryReader.ReadUInt32();
        XPelsPerMeter = bmpBinaryReader.ReadUInt32();
        YPelsPerMeter = bmpBinaryReader.ReadUInt32();
        ClrUsed = bmpBinaryReader.ReadUInt32();
        ClrImportant = bmpBinaryReader.ReadUInt32();
        if (Type != BmpType) { throw new Exception("Type Error"); }
        if (Reserved1!=0) { throw new Exception("Reserved1 Error"); }
        if (Reserved2 != 0) { throw new Exception("Reserved2 Error"); }
        if (Planes != 1) { throw new Exception("Planes Error"); }
        if (Compression != 0) { throw new Exception("Compression Error"); }
        if (Wide > 4000 || Height > 4000) { throw new Exception("File too large"); }
        if (Bitcount!=MapBitcount) { throw new Exception("没有使用24位保存"); }
        for (uint i = 0; i < Height; i++) 
        for (uint j = 0; j < Wide; j++){
                byte[] ThisPixel=new byte[3];
                bmpBinaryReader.Read(ThisPixel, 0, 3);
                Colors[i,j,0] = ThisPixel[0];
                Colors[i,j,1] = ThisPixel[1];
                Colors[i,j,2] = ThisPixel[2];
            }
        bmpStream.Dispose();
        bmpBinaryReader.Dispose();
    }
    /*
    public void WriteTo(string OutputPath)
    {
        if(!File.Exists(OutputPath))throw new FileNotFoundException($"找不到文件: {OutputPath}", OutputPath);
        var outputName = Path.GetFileName(OutputPath);
        FileStream BmpStream = File.Open(OutputPath, FileMode.Create);
        BinaryWriter BmpBinaryWriter = new BinaryWriter(BmpStream);
        return;
    }
    //Not fixed
    */
}
