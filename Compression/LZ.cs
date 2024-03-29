﻿using System;

namespace L4RH.Compression;

public static class LZ
{
    public const uint LZHEADER_SIZE = 16;

    public enum Id : uint
    {
        /// <summary>
        /// <see cref="JLZ"/> alternative for better compression ratios in some cases
        /// <para>FourCC: HUFF</para>
        /// </summary>
        HUFF = 0x46465548,

        /// <summary>
        /// The only algorithm that unused by encoder
        /// <para>FourCC: COMP</para>
        /// </summary>
        /// <remarks>Only decoding supported</remarks>
        OLDLZ = 0x504D4F43,

        /// <summary>
        /// Contains plain data prefixed by <see cref="Header"/>.
        /// <see cref="Header.CompressedSize"/> and <see cref="Header.UncompressedSize"/> will be same.
        /// <para>FourCC: RAWW</para>
        /// </summary>
        /// <remarks>Used only if all other algorithms increased data size.</remarks>
        RAW = 0x57574152,

        /// <summary>
        /// The most used algorithm by encoder
        /// <para>FourCC: JDLZ</para>
        /// </summary>
        JLZ = 0x5A4C444A
    }

    public struct Header
    {
        public Id Id;

        public byte Version;
        public byte HeaderSize;
        public ushort Flags;

        public uint UncompressedSize;
        public uint CompressedSize;
    }

    public static byte[] Decompress(byte[] input)
    {
        if (JLZ.IsJLZ(input))          return JLZ.Decompress(input);
#if false // Currently not supported due to lack of implementation
        else if (HUFF.IsHUFF(input))   return HUFF.Decompress(input);
#endif
        else if (RAW.IsRAW(input))     return RAW.Decompress(input);
        else if (OldLZ.IsOldLZ(input)) return OldLZ.Decompress(input);
        else throw new ArgumentException("Invalid data passed or this compression algorithm is not supported!", nameof(input));
    }

    public static byte[] Compress(byte[] input, Id? preferred = null)
    {
        if (preferred == null)
        {
            var jlzData = JLZ.Decompress(input);
            var huffData = HUFF.Decompress(input);
            
            if (huffData.Length <= jlzData.Length)
            {
                if (input.Length + LZHEADER_SIZE < huffData.Length)
                    return RAW.Compress(input);

                return huffData;
            }

            return jlzData;
        }

        if (preferred == Id.JLZ)
            return JLZ.Compress(input);

        if (preferred == Id.HUFF)
            return HUFF.Compress(input);

        if (preferred == Id.OLDLZ)
            return OldLZ.Compress(input);

        return RAW.Compress(input);
    }
}
