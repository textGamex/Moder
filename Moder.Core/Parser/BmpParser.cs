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
    public string FilePath { get; }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;
    public ushort BmpType;
    public uint BmpSize;
    public ushort BmpReserved1;
    public ushort BmpReserved2;
    public uint BmpOffbits;
    public uint BmpBitsize;
    public uint BmpWide;
    public uint BmpHeight;
    public ushort BmpPlanes;
    public ushort BmpBitcount;
    public uint BmpCompression;
    public uint BmpSizeImage;
    public uint BmpXPelsPerMeter;
    public uint BmpYPelsPerMeter;
    public uint BmpClrUsed;
    public uint BmpClrImportant;
    public byte[,,] BmpColors = new byte[5000, 5000, 3];//R,G,B
    public BmpParser(string filePath)
    {
        FilePath = File.Exists(filePath) ? filePath : throw new FileNotFoundException($"找不到文件: {filePath}", filePath);
        var fileName = Path.GetFileName(filePath);
        FileStream BmpStream = new FileStream(filePath,FileMode.Open);
        BinaryReader BmpBinaryReader = new BinaryReader(BmpStream);
        BmpType=BmpBinaryReader.ReadUInt16();
        BmpSize = BmpBinaryReader.ReadUInt32();
        BmpReserved1 = BmpBinaryReader.ReadUInt16();
        BmpReserved2 = BmpBinaryReader.ReadUInt16();
        BmpOffbits = BmpBinaryReader.ReadUInt32();
        BmpBitsize = BmpBinaryReader.ReadUInt32();
        BmpWide = BmpBinaryReader.ReadUInt32();
        BmpHeight = BmpBinaryReader.ReadUInt32();
        BmpPlanes = BmpBinaryReader.ReadUInt16();
        BmpBitcount = BmpBinaryReader.ReadUInt16();
        BmpCompression = BmpBinaryReader.ReadUInt32();
        BmpSizeImage = BmpBinaryReader.ReadUInt32();
        BmpXPelsPerMeter = BmpBinaryReader.ReadUInt32();
        BmpYPelsPerMeter = BmpBinaryReader.ReadUInt32();
        BmpClrUsed = BmpBinaryReader.ReadUInt32();
        BmpClrImportant = BmpBinaryReader.ReadUInt32();
        if (BmpType != 0x4d42) { throw new Exception("BmpType Error"); }
        if (BmpReserved1!=0) { throw new Exception("BmpReserved1 Error"); }
        if (BmpReserved2 != 0) { throw new Exception("BmpReserved2 Error"); }
        if (BmpPlanes != 1) { throw new Exception("BmpPlanes Error"); }
        if (BmpCompression != 0) { throw new Exception("BmpCompression Error"); }
        if (BmpWide > 4000 || BmpHeight > 4000) { throw new Exception("Bmp too large"); }
        if (BmpBitcount!=24) { throw new Exception("Bmp没有使用24位保存"); }
        for (uint i = 0; i < BmpHeight; i++) 
        for (uint j = 0; j < BmpWide; j++){
                byte[] ThisPixel=new byte[3];
                BmpBinaryReader.Read(ThisPixel, 0, 3);
                BmpColors[i,j,0] = ThisPixel[0];
                BmpColors[i,j,1] = ThisPixel[1];
                BmpColors[i,j,2] = ThisPixel[2];
            }
        return;
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
