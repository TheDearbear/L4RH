using System;
using System.Diagnostics;

namespace L4RH.Model.Textures;

[DebuggerDisplay("TextureInfo '{Name}'")]
public class TextureInfo
{
    public string Name { get; set; } = string.Empty;
    public uint NameHash { get; set; }
    public uint ClassHash { get; set; }
    public uint? ParentImageHash { get; set; }

    public int PaletteOffset { get; set; }
    public int PaletteSize { get; set; }
    public byte[] Palette { get; set; } = Array.Empty<byte>();

    public int DataOffset { get; set; }
    public int DataSize { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();

    public int BaseImageSize { get; set; }

    public short Width { get; set; }
    public short Height { get; set; }
    public byte WidthShift { get; set; }
    public byte HeightShift { get; set; }

    public byte DataCompression { get; set; }
    public byte PaletteCompression { get; set; }

    public short PaletteCount { get; set; }
    public byte MipmapCount { get; set; }
    public byte TileableUV { get; set; }
    public byte BiasLevel { get; set; }
    public byte RenderOrder { get; set; }
    public byte ScrollType { get; set; }
    public bool FlagsUsed { get; set; }
    public bool ApplyAlphaSort { get; set; }
    public byte AlphaUsageType { get; set; }
    public byte AlphaBlendType { get; set; }
    public byte Flags { get; set; }
    public short ScrollTimestep { get; set; }
    public short ScrollSpeedS { get; set; }
    public short ScrollSpeedT { get; set; }
    public short OffsetS { get; set; }
    public short OffsetT { get; set; }
    public short ScaleS { get; set; }
    public short ScaleT { get; set; }
}
