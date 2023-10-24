using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace L4RH;

public static class BinaryExtensions
{

    #region Binary Reader

    public static string ReadString(this BinaryReader reader, int length)
    {
        byte[] buffer = new byte[length];
        reader.Read(buffer, 0, length);

        return Encoding.ASCII.GetString(buffer).Trim('\0');
    }

    public static byte[] ReadArray(this BinaryReader reader, int length)
    {
        byte[] buffer = new byte[length];
        reader.Read(buffer);

        return buffer;
    }

    public static byte[] ReadArray(this BinaryReader reader, uint length)
    {
        byte[] buffer = new byte[length];
        reader.Read(buffer);

        return buffer;
    }

    public static void SkipPadding(this BinaryReader reader)
    {
        if (reader.ReadUInt32() != 0)
        {
            reader.BaseStream.Position -= 4;
            return;
        }

        uint length = reader.ReadUInt32();
        reader.BaseStream.Position += length;
    }

    public static TStruct ReadStruct<TStruct>(this BinaryReader reader) where TStruct : struct
    {
        GCHandle handle = GCHandle.Alloc(reader.ReadArray(Marshal.SizeOf<TStruct>()), GCHandleType.Pinned);
        TStruct structure = Marshal.PtrToStructure<TStruct>(handle.AddrOfPinnedObject());
        handle.Free();

        return structure;
    }

    public static int AlignPosition(this BinaryReader reader)
    {
        int skipped = 0;
        long start = reader.BaseStream.Position;
        while (reader.BaseStream.Position + 4 <= reader.BaseStream.Length && reader.ReadUInt32() == 0x11111111) skipped += 4;
        if (reader.BaseStream.Position != reader.BaseStream.Length && reader.BaseStream.Position != start) reader.BaseStream.Position -= 4;
        return skipped;
    }

    #endregion

    #region Binary Writer

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

    #endregion
}
