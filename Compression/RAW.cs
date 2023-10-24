using System;
using System.IO;
using System.Linq;

namespace L4RH.Compression;

public static class RAW
{
    public static bool IsRAW(byte[] input)
        => input.Length >= LZ.HEADER_SIZE && input[0] == 'R' && input[1] == 'A' && input[2] == 'W' && input[3] == 'W' && input[4] == 0x01;

    public static byte[] Decompress(byte[] input)
    {
        if (!IsRAW(input))
            throw new InvalidDataException("Input header is not RAW!");

        int length = BitConverter.ToInt32(input, 8);

        return input.Skip(LZ.HEADER_SIZE).Take(length).ToArray();
    }

    public static byte[] Compress(byte[] input)
    {
        byte[] output = new byte[LZ.HEADER_SIZE + input.Length];

        output[0] = 0x52; // R
        output[1] = 0x41; // A
        output[2] = 0x57; // W
        output[3] = 0x57; // W
        output[4] = 0x01;
        output[5] = 0x10;
        output[6] = 0x00;
        output[7] = 0x00;

        byte[] length = BitConverter.GetBytes(input.Length);
        length.CopyTo(output, 8);
        length.CopyTo(output, 12);

        input.CopyTo(output, LZ.HEADER_SIZE);

        return output;
    }
}
