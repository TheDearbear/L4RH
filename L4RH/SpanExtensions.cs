using System.Runtime.InteropServices;
using System.Text;
using System.Buffers.Binary;

namespace L4RH;

public static class ByteSpanExtensions
{
    #region Span<byte> Reader

    /*public static byte ReadByte(this Span<byte> span, ref int pointer)
        => span[pointer++];

    public static short ReadInt16(this Span<byte> span, ref int pointer)
        => BinaryPrimitives.ReadInt16LittleEndian(span.ReadArray(ref pointer, 2));

    public static int ReadInt32(this Span<byte> span, ref int pointer)
        => BinaryPrimitives.ReadInt32LittleEndian(span.ReadArray(ref pointer, 4));

    public static long ReadInt64(this Span<byte> span, ref int pointer)
        => BinaryPrimitives.ReadInt64LittleEndian(span.ReadArray(ref pointer, 8));

    public static ushort ReadUInt16(this Span<byte> span, ref int pointer)
        => BinaryPrimitives.ReadUInt16LittleEndian(span.ReadArray(ref pointer, 2));

    public static uint ReadUInt32(this Span<byte> span, ref int pointer)
        => BinaryPrimitives.ReadUInt32LittleEndian(span.ReadArray(ref pointer, 4));

    public static ulong ReadUInt64(this Span<byte> span, ref int pointer)
        => BinaryPrimitives.ReadUInt64LittleEndian(span.ReadArray(ref pointer, 8));

    public static float ReadSingle(this Span<byte> span, ref int pointer)
        => BinaryPrimitives.ReadSingleLittleEndian(span.ReadArray(ref pointer, 4));

    public static double ReadDouble(this Span<byte> span, ref int pointer)
        => BinaryPrimitives.ReadDoubleLittleEndian(span.ReadArray(ref pointer, 8));

    public static string ReadString(this Span<byte> span, ref int pointer, int length)
        => Encoding.ASCII.GetString(span.ReadArray(ref pointer, length)).Trim('\0');

    public static Span<byte> ReadArray(this Span<byte> span, ref int pointer, int length)
    {
        Span<byte> bytes = span.Slice(pointer, length);
        pointer += length;

        return bytes;
    }

    public static Span<byte> ReadArray(this Span<byte> span, ref int pointer, uint length)
    {
        int signed = unchecked((int)length);

        Span<byte> bytes = span.Slice(pointer, signed);
        pointer += signed;

        return bytes;
    }

    public static void SkipPadding(this Span<byte> span, ref int pointer)
    {
        if (span.ReadUInt32(ref pointer) != 0)
        {
            pointer -= 4;
            return;
        }

        int length = span.ReadInt32(ref pointer);
        pointer += length;
    }

    public static TStruct ReadStruct<TStruct>(this Span<byte> span, ref int pointer) where TStruct : struct
    {
        GCHandle handle = GCHandle.Alloc(span.ReadArray(ref pointer, Marshal.SizeOf<TStruct>()).ToArray(), GCHandleType.Pinned);
        TStruct structure = Marshal.PtrToStructure<TStruct>(handle.AddrOfPinnedObject());
        handle.Free();

        return structure;
    }

    public static int AlignPosition(this Span<byte> span, ref int pointer)
    {
        int skipped = 0;
        long start = pointer;
        while (pointer + 4 <= span.Length && span.ReadUInt32(ref pointer) == 0x11111111) skipped += 4;
        if (pointer != span.Length && pointer != start) pointer -= 4;
        return skipped;
    }*/

    #endregion

    #region Binary Writer
    /*
    public static int WriteString(this BinaryWriter writer, string str, int? length = null)
    {
        int dataLength = length is not null ? length.Value : str.Length + 1;

        byte[] buffer = new byte[dataLength];
        Encoding.ASCII.GetBytes(str, 0, str.Length).CopyTo(buffer, 0);

        writer.Write(buffer, 0, dataLength);

        return dataLength;
    }

    public static void AddPadding(this BinaryWriter writer, int align)
    {
        if (writer.BaseStream.Position % align == 0)
            return;

        // Chunk Id
        writer.Write(0x00000000);
        // Chunk Length
        writer.Write(0x00000000);

        if (writer.BaseStream.Position % align == 0)
            return;

        int needBytes = align - (int)(writer.BaseStream.Position % align);

        writer.BaseStream.Position -= 4;
        writer.Write(needBytes);

        for (int i = 0; i < needBytes; i++)
            writer.Write((byte)0);
    }

    public static void WriteStruct<TStruct>(this BinaryWriter writer, TStruct data) where TStruct : struct
    {
        int size = Marshal.SizeOf<TStruct>();
        byte[] array = new byte[size];
        IntPtr ptr = IntPtr.Zero;

        try
        {
            ptr = Marshal.AllocHGlobal(size);
            Marshal.StructureToPtr(data, ptr, true);
            Marshal.Copy(ptr, array, 0, size);
        }
        finally
        {
            Marshal.FreeHGlobal(ptr);
        }

        writer.Write(array);
    }

    public static int AlignPosition(this BinaryWriter writer, int align)
    {
        long start = writer.BaseStream.Position;
        while (writer.BaseStream.Position % align != 0)
            writer.Write((byte)0x11);

        return (int)(writer.BaseStream.Position - start);
    }
    */
    #endregion
}
