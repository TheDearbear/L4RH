using System;
using System.IO;

namespace L4RH.Compression;

/// <summary>
/// <para>Original File: <see href="https://github.com/MWisBest/OpenNFSTools/blob/master/LibNFS/Compression/JDLZ.cs">OpenNFSTools by MWisBest</see></para>
/// JLZ compression is used in various BlackBox NFS games, starting somewhere around Underground and continuing to World.
/// Like other formats used, it has a little 16 byte header, followed by the actual data.
/// 0x0 - 0x3 = 'JDLZ'
/// 0x4       = 0x02
/// 0x5       = 0x10
/// 0x6 - 0x7 = 0x0000
/// 0x8 - 0xB = (uncompressed data length; little-endian)
/// 0xC - 0xF = (compressed data length, including header; little-endian)
///
/// JLZ is very lightweight; it's read totally sequentially and is fairly simple,
/// however this obviously comes at a tradeoff of how effectively it compresses data.
/// </summary>
public static class JLZ
{
    public static bool IsJLZ(byte[] input)
        => input.Length >= LZ.HEADER_SIZE && input[0] == 'J' && input[1] == 'D' && input[2] == 'L' && input[3] == 'Z' && input[4] == 0x02;

    public static byte[] Decompress(byte[] input)
    {
        if (!IsJLZ(input))
            throw new InvalidDataException("Input header is not JLZ!");

        int flags1 = 1, flags2 = 1;
        int t, length;
        int inPos = LZ.HEADER_SIZE, outPos = 0;

        byte[] output = new byte[BitConverter.ToInt32(input, 8)];

        while (inPos < input.Length && outPos < output.Length)
        {
            if (flags1 == 1)
            {
                flags1 = input[inPos++] | 0x100;
            }
            if (flags2 == 1)
            {
                flags2 = input[inPos++] | 0x100;
            }

            if ((flags1 & 1) == 1)
            {
                if ((flags2 & 1) == 1) // 3 to 4098(?) iterations, backtracks 1 to 16(?) bytes
                {
                    // length max is 4098(?) (0x1002), assuming input[inPos] and input[inPos + 1] are both 0xFF
                    length = (input[inPos + 1] | (input[inPos] & 0xF0) << 4) + 3;
                    // t max is 16(?) (0x10), assuming input[inPos] is 0xFF
                    t = (input[inPos] & 0x0F) + 1;
                }
                else // 3(?) to 34(?) iterations, backtracks 17(?) to 2064(?) bytes
                {
                    // t max is 2064(?) (0x810), assuming input[inPos] and input[inPos + 1] are both 0xFF
                    t = (input[inPos + 1] | (input[inPos] & 0xE0) << 3) + 17;
                    // length max is 34(?) (0x22), assuming input[inPos] is 0xFF
                    length = (input[inPos] & 0x1F) + 3;
                }

                inPos += 2;

                for (int i = 0; i < length; ++i)
                {
                    output[outPos + i] = output[outPos + i - t];
                }

                outPos += length;
                flags2 >>= 1;
            }
            else
            {
                if (outPos < output.Length)
                {
                    output[outPos++] = input[inPos++];
                }
            }
            flags1 >>= 1;
        }
        return output;
    }

    /// <summary>
    /// This lovely JLZ compressor was written by the user "zombie28" of the encode.ru forums.
    /// Its compression ratios are within a few percent of the NFS games' JLZ compressor. Awesome!!
    /// </summary>
    /// <param name="input">bytes to compress with JLZ</param>
    /// <param name="hashSize">speed/ratio tunable; use powers of 2. results vary per file.</param>
    /// <param name="maxSearchDepth">speed/ratio tunable. results vary per file.</param>
    /// <returns>JLZ-compressed bytes, w/ 16 byte header</returns>
    public static byte[] Compress(byte[] input, int hashSize = 0x2000, int maxSearchDepth = 16)
    {
        const int MinMatchLength = 3;

        int inputBytes = input.Length;
        byte[] output = new byte[inputBytes + (inputBytes + 7) / 8 + LZ.HEADER_SIZE + 1];
        int[] hashPos = new int[hashSize];
        int[] hashChain = new int[inputBytes];

        int outPos = 0;
        int inPos = 0;
        byte flags1bit = 1;
        byte flags2bit = 1;
        byte flags1 = 0;
        byte flags2 = 0;

        output[outPos++] = 0x4A; // 'J'
        output[outPos++] = 0x44; // 'D'
        output[outPos++] = 0x4C; // 'L'
        output[outPos++] = 0x5A; // 'Z'
        output[outPos++] = 0x02;
        output[outPos++] = 0x10;
        output[outPos++] = 0x00;
        output[outPos++] = 0x00;
        output[outPos++] = (byte)inputBytes;
        output[outPos++] = (byte)(inputBytes >> 8);
        output[outPos++] = (byte)(inputBytes >> 16);
        output[outPos++] = (byte)(inputBytes >> 24);
        outPos += 4;

        int flags1Pos = outPos++;
        int flags2Pos = outPos++;

        flags1bit <<= 1;
        output[outPos++] = input[inPos++];
        inputBytes--;

        while (inputBytes > 0)
        {
            int bestMatchLength = MinMatchLength - 1;
            int bestMatchDist = 0;

            if (inputBytes >= MinMatchLength)
            {
                int hash = -0x1A1 * (input[inPos] ^ (input[inPos + 1] ^ input[inPos + 2] << 4) << 4) & hashSize - 1;
                int matchPos = hashPos[hash];
                hashPos[hash] = inPos;
                hashChain[inPos] = matchPos;
                int prevMatchPos = inPos;

                for (int i = 0; i < maxSearchDepth; i++)
                {
                    int matchDist = inPos - matchPos;

                    if (matchDist > 2064 || matchPos >= prevMatchPos)
                    {
                        break;
                    }

                    int matchLengthLimit = matchDist <= 16 ? 4098 : 34;
                    int maxMatchLength = inputBytes;

                    if (maxMatchLength > matchLengthLimit)
                    {
                        maxMatchLength = matchLengthLimit;
                    }
                    if (bestMatchLength >= maxMatchLength)
                    {
                        break;
                    }

                    int matchLength = 0;
                    while (matchLength < maxMatchLength && input[inPos + matchLength] == input[matchPos + matchLength])
                    {
                        matchLength++;
                    }

                    if (matchLength > bestMatchLength)
                    {
                        bestMatchLength = matchLength;
                        bestMatchDist = matchDist;
                    }

                    prevMatchPos = matchPos;
                    matchPos = hashChain[matchPos];
                }
            }

            if (bestMatchLength >= MinMatchLength)
            {
                flags1 |= flags1bit;
                inPos += bestMatchLength;
                inputBytes -= bestMatchLength;
                bestMatchLength -= MinMatchLength;

                if (bestMatchDist < 17)
                {
                    flags2 |= flags2bit;
                    output[outPos++] = (byte)(bestMatchDist - 1 | bestMatchLength >> 4 & 0xF0);
                    output[outPos++] = (byte)bestMatchLength;
                }
                else
                {
                    bestMatchDist -= 17;
                    output[outPos++] = (byte)(bestMatchLength | bestMatchDist >> 3 & 0xE0);
                    output[outPos++] = (byte)bestMatchDist;
                }

                flags2bit <<= 1;
            }
            else
            {
                output[outPos++] = input[inPos++];
                inputBytes--;
            }

            flags1bit <<= 1;

            if (flags1bit == 0)
            {
                output[flags1Pos] = flags1;
                flags1 = 0;
                flags1Pos = outPos++;
                flags1bit = 1;
            }

            if (flags2bit == 0)
            {
                output[flags2Pos] = flags2;
                flags2 = 0;
                flags2Pos = outPos++;
                flags2bit = 1;
            }
        }

        if (flags2bit > 1)
        {
            output[flags2Pos] = flags2;
        }
        else if (flags2Pos == outPos - 1)
        {
            outPos = flags2Pos;
        }

        if (flags1bit > 1)
        {
            output[flags1Pos] = flags1;
        }
        else if (flags1Pos == outPos - 1)
        {
            outPos = flags1Pos;
        }

        output[12] = (byte)outPos;
        output[13] = (byte)(outPos >> 8);
        output[14] = (byte)(outPos >> 16);
        output[15] = (byte)(outPos >> 24);

        Array.Resize(ref output, outPos);
        return output;
    }
}