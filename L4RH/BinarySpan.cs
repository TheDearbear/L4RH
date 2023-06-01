using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;

namespace L4RH;

public ref struct BinarySpan
{
    public int Length => Span.Length;

    public Span<byte> Span;
    public int Pointer;

    public BinarySpan(Span<byte> span)
        => Span = span;

    public byte ReadByte()
        => Span[Pointer++];

    public short ReadInt16()
        => BinaryPrimitives.ReadInt16LittleEndian(ReadArray(2));

    public int ReadInt32()
        => BinaryPrimitives.ReadInt32LittleEndian(ReadArray(4));

    public long ReadInt64()
        => BinaryPrimitives.ReadInt64LittleEndian(ReadArray(8));

    public ushort ReadUInt16()
        => BinaryPrimitives.ReadUInt16LittleEndian(ReadArray(2));

    public uint ReadUInt32()
        => BinaryPrimitives.ReadUInt32LittleEndian(ReadArray(4));

    public ulong ReadUInt64()
        => BinaryPrimitives.ReadUInt64LittleEndian(ReadArray(8));

    public float ReadSingle()
        => BinaryPrimitives.ReadSingleLittleEndian(ReadArray(4));

    public double ReadDouble()
        => BinaryPrimitives.ReadDoubleLittleEndian(ReadArray(8));

    public string ReadString(int length)
        => Encoding.ASCII.GetString(ReadArray(length)).Trim('\0');

    public Span<byte> ReadArray(int length)
    {
        Span<byte> bytes = Span.Slice(Pointer, length);
        Pointer += length;

        return bytes;
    }

    public Span<byte> ReadArray(uint length)
    {
        int signed = unchecked((int)length);

        Span<byte> bytes = Span.Slice(Pointer, signed);
        Pointer += signed;

        return bytes;
    }

    public void SkipPadding()
    {
        if (ReadUInt32() != 0)
        {
            Pointer -= 4;
            return;
        }

        int length = ReadInt32();
        Pointer += length;
    }

    public TStruct ReadStruct<TStruct>() where TStruct : struct
    {
        GCHandle handle = GCHandle.Alloc(ReadArray(Marshal.SizeOf<TStruct>()).ToArray(), GCHandleType.Pinned);
        TStruct structure = Marshal.PtrToStructure<TStruct>(handle.AddrOfPinnedObject());
        handle.Free();

        return structure;
    }

    public int AlignPosition()
    {
        int skipped = 0;
        long start = Pointer;
        while (Pointer + 4 <= Span.Length && ReadUInt32() == 0x11111111) skipped += 4;
        if (Pointer != Span.Length && Pointer != start) Pointer -= 4;
        return skipped;
    }
}
