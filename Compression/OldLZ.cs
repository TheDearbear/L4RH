using System;
using System.IO;

namespace L4RH.Compression;

/// <summary>
/// OldLZ compression can be used by BlackBox NFS games.
/// Like any other LZ format used in games, it has a little 16 byte header, followed by the actual data.
/// 0x0 - 0x3 = 'COMP'
/// 0x4       = 0x01
/// 0x5       = 0x10
/// 0x6 - 0x7 = 0x0000
/// 0x8 - 0xB = (uncompressed data length; little-endian)
/// 0xC - 0xF = (compressed data length, including header; little-endian)
/// </summary>
public static class OldLZ
{
    public static bool IsOldLZ(byte[] input)
        => input.Length >= LZ.HEADER_SIZE && input[0] == 'C' && input[1] == 'O' && input[2] == 'M' && input[3] == 'P' && input[4] == 0x01;

    /// <remarks>
    /// This should work... In theory
    /// </remarks>
    public static byte[] Decompress(byte[] input)
    {
        if (!IsOldLZ(input))
            throw new InvalidDataException("Input header is not OldLZ!");

        var span = new BinarySpan(input);
        var header = span.ReadStruct<LZ.Header>();

        if (header.Flags == 1)
            return span.Span.Slice(LZ.HEADER_SIZE, header.CompressedSize).ToArray();

        Span<byte> pbVar2;
        byte[] output = new byte[header.UncompressedSize];
        int outputPointer = 0;
        int flags = 1;

        while (span.Pointer < span.Length)
        {
            byte bVar1;

            if (flags == 1)
            {
                bVar1 = span.PeekByte();
                pbVar2 = span.Span[(span.Pointer + 1)..];
                span.Pointer += 2;
                flags = bVar1 | 0x10000 | pbVar2[0] << 8;
            }

            for (int i = span.Length > 0x20 ? 0 : 0xF; i != -1; i--, flags >>= 1)
            {
                bVar1 = span.PeekByte();

                if ((flags & 1) == 0)
                {
                    span.Pointer++;
                    output[outputPointer++] = bVar1;
                    continue;
                }

                pbVar2 = span.Span[(span.Pointer + 1)..];
                span.Pointer += 2;
                int uVar5 = bVar1 & 0xF;
                pbVar2 = output.AsSpan(outputPointer - ((bVar1 & 0xF0) << 4 | pbVar2[0]));
                output[outputPointer] = pbVar2[0];
                output[outputPointer + 1] = pbVar2[1];
                Span<byte> pbVar3 = pbVar2[3..];
                output[outputPointer + 2] = pbVar2[2];
                outputPointer += 3;

                while (uVar5 != 0)
                {
                    bVar1 = pbVar3[0];
                    uVar5--;
                    pbVar3 = pbVar3[1..];
                    output[outputPointer++] = bVar1;
                }
            }
        }

        return output;
    }

    public static byte[] Compress(byte[] input, bool packAsRaw = false)
    {
        if (packAsRaw)
        {
            byte[] output = RAW.Compress(input);
            output[0] = 0x43; // C
            output[1] = 0x4F; // O
            output[2] = 0x4D; // M
            output[3] = 0x50; // P

            // This byte checked only by OldLZ algorithm
            output[6] = 0x01; // Mark data as unpacked (plain)

            return output;
        }

        throw new NotImplementedException("Data encoding using OldLZ not supported, only packing raw data supported.");
    }
}
