using System;

namespace L4RH.Compression;

/// <summary>
/// HUFF compression is used in BlackBox NFS games.
/// Like any other LZ format used in games, it has a little 16 byte header, followed by the actual data.
/// 0x0 - 0x3 = 'HUFF'
/// 0x4       = 0x01
/// 0x5       = 0x10
/// 0x6 - 0x7 = 0x0000
/// 0x8 - 0xB = (uncompressed data length; little-endian)
/// 0xC - 0xF = (compressed data length, including header; little-endian)
/// </summary>
public static class HUFF
{
    public const int ZERO = 0;

    public static bool IsHUFF(byte[] input)
        => input.Length >= LZ.HEADER_SIZE && input[0] == 'H' && input[1] == 'U' && input[2] == 'F' && input[3] == 'F' && input[4] == 0x01;

    public static byte[] Decompress(byte[] input)
    {
        Decompress(input, out var output);
        return output ?? Array.Empty<byte>();
    }

    /// <remarks>
    /// Not working lol (Crashes at some point)
    /// </remarks>
    public static uint Decompress(byte[] input, out byte[]? output)
    {
        if (!IsHUFF(input))
        {
            output = null;
            return 0;
        }

        BinarySpan span = new(input);
        LZ.Header header = span.ReadStruct<LZ.Header>();
        output = new byte[header.UncompressedSize];

        int iVar11 = -0x10;
        uint uVar15 = 0;
        uint local_3fc = 0;
        int local_3c8 = 0;
        BinarySpan local_404 = span.Slice(2);

        if (ZERO != 0)
        {
            iVar11 = -ZERO - 0x10;
            local_404 = span;
        }

        if (iVar11 < 0)
        {
            local_3fc = (uint)(span.PeekByte() << 8 + span.PeekByte(1));
            uVar15 = local_3fc << (-(char)iVar11 & 0x1f);
            iVar11 += 0x10;
        }

        uint local_3cc = uVar15 >> 0x10;
        uint uVar16 = uVar15 << 0x10;
        int iVar12 = iVar11 - 0x10;
        if (iVar12 < 0)
        {
            local_3fc = (uint)((local_3fc << 8) + (local_404.PeekByte() << 8) + local_404.PeekByte(1));
            local_404.Pointer += 2;
            uVar16 = local_3fc << (-(char)iVar12 & 0x1f);
            iVar12 = iVar11;
        }

        int iVar13;
        if (unchecked((int)uVar15) < 0)
        {
            if ((local_3cc & 0x100) != 0)
            {
                iVar11 = 0;
                iVar13 = iVar12 - 0x10;
                if (iVar13 < 0)
                {
                    local_3fc = (uint)((local_3fc << 8) + (local_404.PeekByte() << 8) + local_404.PeekByte(1));
                    local_404.Pointer += 2;
                    iVar11 = unchecked((int)(local_3fc << (-(char)iVar13 & 0x1f)));
                    iVar13 = iVar12;
                }

                uVar16 = unchecked((uint)iVar11) << 0x10;
                iVar12 = iVar13 - 0x10;
                if (iVar12 < 0)
                {
                    uVar16 = local_3fc << (-(char)iVar12 & 0x1f);
                    iVar12 = iVar13;
                }
            }

            uVar15 = uVar16 >> 0x10;
            uVar16 <<= 0x10;
            iVar12 -= 0x10;
        }
        else
        {
            if ((local_3cc & 0x100) != 0)
            {
                iVar13 = unchecked((int)(uVar16 << 8));
                iVar11 = iVar12 - 8;
                if (iVar11 < 0)
                {
                    local_3fc = (uint)((local_3fc << 8) + (local_404.PeekByte() << 8) + local_404.PeekByte(1));
                    local_404.Pointer += 2;
                    iVar13 = unchecked((int)(local_3fc << (-(char)iVar11 & 0x1f)));
                    iVar11 = iVar12 + 8;
                }
                
                uVar16 = unchecked((uint)iVar13) << 0x10;
                iVar12 = iVar11 - 0x10;
                if (iVar12 < 0)
                {
                    local_3fc = (uint)((local_3fc << 8) + (local_404.PeekByte() << 8) + local_404.PeekByte(1));
                    local_404.Pointer += 2;
                    uVar16 = local_3fc << (-(char)iVar11 & 0x1f);
                    iVar12 = iVar11;
                }
            }

            uVar15 = uVar16 >> 0x18;
            uVar16 <<= 8;
            iVar12 -= 8;
        }

        local_3cc &= 0xFFFFFEFF;

        if (iVar12 < 0)
        {
            local_3fc = (uint)((local_3fc << 8) + (local_404.PeekByte() << 8) + local_404.PeekByte(1));
            local_404.Pointer += 2;
            uVar16 = local_3fc << (-(char)iVar12 & 0x1f);
            iVar12 += 0x10;
        }

        uint uVar17 = uVar16 << 0x10;
        iVar11 = iVar12 - 0x10;
        if (iVar11 < 0)
        {
            local_3fc = (uint)((local_3fc << 8) + (local_404.PeekByte() << 8) + local_404.PeekByte(1));
            local_404.Pointer += 2;
            uVar17 = local_3fc << (-(char)iVar11 & 0x1f);
            iVar11 = iVar12;
        }

        uVar16 = (uVar16 >> 0x10) | (uVar15 << 0x10);
        uVar15 = uVar17 << 8;
        iVar12 = iVar11 - 8;
        if (iVar12 < 0)
        {
            local_3fc = (uint)((local_3fc << 8) + (local_404.PeekByte() << 8) + local_404.PeekByte(1));
            local_404.Pointer += 2;
            uVar15 = local_3fc << (-(char)iVar12 & 0x1f);
            iVar12 = iVar11 + 8;
        }

        iVar13 = 0;
        int local_3ec = 0;
        int local_3f0 = 1;
        int iVar2;
        int iVar14;
        byte bVar1;
        uint uVar4;
        uint uVar10 = 0;
        int[] aiStack_240 = new int[16];
        uint[] auStack_3c0 = new uint[15];
        uint[] auStack_384 = new uint[10];
        uint[] local_340 = new uint[64];
        iVar11 = local_3f0;

        do
        {
            local_3f0 = iVar11;
            iVar2 = local_3f0;
            aiStack_240[local_3f0] = iVar13 * 2 - local_3ec;

            if (unchecked((int)uVar15) < 0)
            {
                uVar10 = uVar15 >> 0x1d;
                uVar15 <<= 3;
                iVar14 = iVar12 - 3;
                if (iVar14 < 0)
                {
                    local_3fc = (uint)((local_3fc << 8) + (local_404.PeekByte() << 8) + local_404.PeekByte(1));
                    local_404.Pointer += 2;
                    uVar15 = local_3fc << (-(char)iVar14 & 0x1f);
                    iVar14 = iVar12 + 0xd;
                }

                uVar10 -= 4;
            }
            else
            {
                iVar11 = 2;
                if ((uVar15 & 0xFFFF0000) == 0)
                {
                    do
                    {
                        iVar11++;
                        uVar10 = unchecked((uint)-(uVar15 >> 0x1f));
                        uVar15 <<= 1;
                        iVar14 = iVar12 - 1;
                        if (iVar14 < 0)
                        {
                            local_3fc = (uint)((local_3fc << 8) + (local_404.PeekByte() << 8) + local_404.PeekByte(1));
                            local_404.Pointer += 2;
                            uVar15 = local_3fc << (-(char)iVar14 & 0x1f);
                            iVar14 = iVar12 + 0xf;
                        }

                        iVar12 = iVar14;
                    } while (uVar10 == 0);
                }
                else
                {
                    do
                    {
                        uVar4 = uVar15;
                        uVar15 = uVar4 << 1;
                        iVar11++;
                    } while (unchecked((int)uVar15) > -1);

                    iVar14 = iVar12 + (1 - iVar11);
                    uVar15 = uVar4 << 2;

                    if (ZERO != 0)
                    {
                        uVar10 = uVar15 >> (0x20 - ZERO & 0x1f);
                        uVar15 <<= ZERO & 0x1f;
                        iVar14 -= ZERO;
                    }

                    if (iVar14 < 0)
                    {
                        local_3fc = (uint)((local_3fc << 8) + (local_404.PeekByte() << 8) + local_404.PeekByte(1));
                        local_404.Pointer += 2;
                        uVar15 = local_3fc << (-(char)iVar14 & 0x1f);
                        iVar14 += 0x10;
                    }
                }

                bVar1 = (byte)iVar11;
                if (iVar11 < 0x11)
                {
                    if (iVar11 != 0)
                    {
                        uVar10 = uVar15 >> (0x20 - bVar1 & 0x1f);
                        uVar15 <<= bVar1 & 0x1f;
                        iVar14 -= iVar11;
                    }

                    if (iVar14 < 0)
                    {
                        local_3fc = (uint)((local_3fc << 8) + (local_404.PeekByte() << 8) + local_404.PeekByte(1));
                        local_404.Pointer += 2;
                        uVar15 = local_3fc << (-(char)iVar14 & 0x1f);
                        iVar14 += 0x10;
                    }
                }
                else
                {
                    iVar12 = iVar14;
                    if (iVar11 != 0x10)
                    {
                        uVar10 = uVar15 >> (0x30 - bVar1 & 0x1f);
                        uVar15 <<= bVar1 - 0x10 & 0x1f;
                        iVar12 = iVar14 + (0x10 - iVar11);
                    }

                    if (iVar12 < 0)
                    {
                        local_3fc = (uint)((local_3fc << 8) + (local_404.PeekByte() << 8) + local_404.PeekByte(1));
                        local_404.Pointer += 2;
                        uVar15 = local_3fc << (-(char)iVar12 & 0x1f);
                        iVar12 += 0x10;
                    }

                    uVar4 = uVar15 >> 0x10;
                    uVar15 <<= 0x10;
                    iVar14 = iVar12 - 0x10;
                    if (iVar14 < 0)
                    {
                        local_3fc = (uint)((local_3fc << 8) + (local_404.PeekByte() << 8) + local_404.PeekByte(1));
                        local_404.Pointer += 2;
                        uVar15 = local_3fc << (-(char)iVar14 & 0x1f);
                        iVar14 = iVar12;
                    }

                    uVar10 = uVar10 << 0x10 | uVar4;
                }

                uVar10 = uVar10 - 4 + unchecked((uint)(1 << (bVar1 & 0x1f)));
            }

            auStack_3c0[local_3f0] = uVar10;
            iVar13 = iVar13 * 2 + unchecked((int)uVar10);
            local_3ec += unchecked((int)uVar10);
            uVar4 = uVar10 == 0 ? 0u : unchecked((uint)iVar13) << unchecked((int)(0x10u - local_3f0 & 0x1f)) & 0xFFFF;
            auStack_384[local_3f0 + 1] = uVar4;
            iVar11 = local_3f0 + 1;
            iVar12 = iVar14;
        } while (uVar10 == 0 || uVar4 != 0);

        auStack_384[iVar11] = uint.MaxValue;
        uint[] puVar5 = local_340;
        iVar11 = 0x10;
        for (int i = 0; iVar11 > 0; i++, iVar11--)
        {
            puVar5[i] = puVar5[i + 1] = puVar5[i + 2] = puVar5[i + 3] = 0;
        }

        int local_3e0 = 0;
        byte[] local_100 = new byte[256];
        iVar11 = iVar14;

        if (local_3ec > 0)
        {
            do
            {
                uVar10 = 0;
                if (unchecked((int)uVar15) < 0)
                {
                    uVar10 = uVar15 >> 0x1d;
                    uVar15 <<= 3;
                    iVar14 = iVar11 - 3;
                    if (iVar14 < 0)
                    {
                        local_3fc = (uint)((local_3fc << 8) + (local_404.PeekByte() << 8) + local_404.PeekByte(1));
                        local_404.Pointer += 2;
                        uVar15 = local_3fc << (-(char)iVar14 & 0x1f);
                        iVar14 = iVar11 + 0xd;
                    }

                    iVar11 = unchecked((int)uVar10) - 4;
                }
                else
                {
                    iVar12 = 2;
                    if ((uVar15 & 0xFFFF0000) == 0)
                    {
                        do
                        {
                            iVar12++;
                            uVar10 = unchecked((uint)-(unchecked((int)uVar15) >> 0x1f));
                            uVar15 <<= 1;
                            iVar14 = iVar11 - 1;
                            if (iVar14 < 0)
                            {
                                local_3fc = (uint)((local_3fc << 8) + (local_404.PeekByte() << 8) + local_404.PeekByte(1));
                                local_404.Pointer += 2;
                                uVar15 = local_3fc << (-(char)iVar14 & 0x1f);
                                iVar14 = iVar11 + 0xf;
                            }

                            iVar11 = iVar14;
                        } while (uVar10 == 0);
                    }
                    else
                    {
                        do
                        {
                            uVar4 = uVar15;
                            uVar15 = uVar4 << 1;
                            iVar12++;
                        } while (unchecked((int)uVar15) > -1);

                        iVar14 = iVar11 + (1 - iVar12);
                        uVar15 = uVar4 << 2;

                        if (ZERO != 0)
                        {
                            uVar10 = uVar15 >> (0x20 - ZERO & 0x1f);
                            uVar15 <<= ZERO & 0x1f;
                            iVar14 -= ZERO;
                        }

                        if (iVar14 < 0)
                        {
                            local_3fc = (uint)((local_3fc << 8) + (local_404.PeekByte() << 8) + local_404.PeekByte(1));
                            local_404.Pointer += 2;
                            uVar15 = local_3fc << (-(char)iVar14 & 0x1f);
                            iVar14 += 0x10;
                        }
                    }

                    bVar1 = (byte)iVar12;
                    if (iVar12 < 0x11)
                    {
                        if (iVar12 != 0)
                        {
                            uVar10 = uVar15 >> (0x20 - bVar1 & 0x1f);
                            uVar15 <<= bVar1 & 0x1f;
                            iVar14 -= iVar12;
                        }

                        if (iVar14 < 0)
                        {
                            local_3fc = (uint)((local_3fc << 8) + (local_404.PeekByte() << 8) + local_404.PeekByte(1));
                            local_404.Pointer += 2;
                            uVar15 = local_3fc << (-(char)iVar14 & 0x1f);
                            iVar14 += 0x10;
                        }
                    }
                    else
                    {
                        iVar11 = iVar14;
                        if (iVar12 != 0x10)
                        {
                            uVar10 = uVar15 >> (0x30 - bVar1 & 0x1f);
                            uVar15 <<= bVar1 - 0x10 & 0x1f;
                            iVar11 = iVar14 + (0x10 - iVar12);
                        }

                        if (iVar11 < 0)
                        {
                            local_3fc = (uint)((local_3fc << 8) + (local_404.PeekByte() << 8) + local_404.PeekByte(1));
                            local_404.Pointer += 2;
                            uVar15 = local_3fc << (-(char)iVar11 & 0x1f);
                            iVar11 += 0x10;
                        }

                        uVar4 = uVar15 >> 0x10;
                        uVar15 <<= 0x10;
                        iVar14 = iVar11 - 0x10;
                        if (iVar14 < 0)
                        {
                            local_3fc = (uint)((local_3fc << 8) + (local_404.PeekByte() << 8) + local_404.PeekByte(1));
                            local_404.Pointer += 2;
                            uVar15 = local_3fc << (-(char)iVar14 & 0x1f);
                            iVar14 = iVar11;
                        }

                        uVar10 = uVar10 << 0x10 | uVar4;
                    }

                    iVar11 = unchecked((int)uVar10) - 4 + 1 << (bVar1 & 0x1f);
                }

                iVar11++;

                byte[] local_340_asBytes = new byte[local_340.Length * sizeof(uint)];
                Buffer.BlockCopy(local_340, 0, local_340_asBytes, 0, local_340_asBytes.Length);
                int local_3fd = 0;
                do
                {
                    local_3fd++;
                    if (local_340_asBytes[local_3fd] == 0)
                        iVar11--;
                } while (iVar11 != 0);

                local_340_asBytes[local_3fd] = 1;
                Buffer.BlockCopy(local_340_asBytes, 0, local_340, 0, local_340_asBytes.Length);

                local_100[local_3e0] = (byte)local_3fd;
                local_3e0++;
                iVar11 = iVar14;
            } while (local_3e0 < local_3ec);
        }

        uint[] local_200 = new uint[64];
        puVar5 = local_200;
        iVar11 = 0x10;
        for (int i = 0; iVar11 > 0; i++, iVar11--)
        {
            puVar5[i] = puVar5[i + 1] = puVar5[i + 2] = puVar5[i + 3] = 0x40404040;
        }

        uint[] local_3dc = local_340;
        uint[] puStack_3e0 = local_200;
        local_3ec = 1;
        byte local_3d4 = (byte)(uVar17 >> 0x18);
        byte bVar7;

        if (local_3f0 > 0)
        {
            int local_3e8 = 7;
            uint[] pbStack_3f0 = local_200;

            do
            {
                if (local_3e8 < 0) break;

                uVar10 = 1u << (byte)(local_3e8 & 0x1f);
                uint local_3d8;

                if (auStack_3c0[local_3ec] != 0)
                {
                    local_3d8 = auStack_3c0[local_3ec];
                    do
                    {
                        byte uVar3 = (byte)local_3ec;
                        bVar7 = (byte)pbStack_3f0[0];
                        pbStack_3f0 = pbStack_3f0[1..];

                        if (bVar7 == uVar17 >> 0x18)
                        {
                            local_3c8 = local_3ec;
                            uVar3 = 0x60;
                        }

                        if (uVar10 > 0)
                        {
                            puVar5 = puStack_3e0;
                            byte[] puVar5_asBytes = new byte[puVar5.Length * sizeof(uint)];
                            Buffer.BlockCopy(puVar5, 0, puVar5_asBytes, 0, puVar5_asBytes.Length);
                            BinarySpan puVar5_asSpan = new(puVar5_asBytes);

                            for (int i = 0; i < uVar10; i++)
                            {
                                puVar5_asSpan.WriteByte(uVar3);
                            }

                            for (int i = 0; i < (uVar10 & 3); i++)
                            {
                                puVar5_asSpan.WriteByte(uVar3);
                            }

                            Buffer.BlockCopy(puVar5_asSpan.Span.ToArray(), 0, puStack_3e0, 0, puVar5_asSpan.Length);

                            puVar5 = local_3dc;
                            puVar5_asBytes = new byte[puVar5.Length * sizeof(uint)];
                            Buffer.BlockCopy(puVar5, 0, puVar5_asBytes, 0, puVar5_asBytes.Length);
                            puVar5_asSpan = new(puVar5_asBytes);

                            for (int i = 0; i < uVar10; i++)
                            {
                                puVar5_asSpan.WriteByte(bVar7);
                            }

                            for (int i = 0; i < (uVar10 & 3); i++)
                            {
                                puVar5_asSpan.WriteByte(bVar7);
                            }

                            Buffer.BlockCopy(puVar5_asSpan.Span.ToArray(), 0, local_3dc, 0, puVar5_asSpan.Length);
                        }

                        local_3d8--;
                    } while (local_3d8 != 0);
                }

                local_3ec++;
                local_3e8--;
            } while (local_3ec <= iVar2);
        }

        return 0;
    }

    public static byte[] Compress(byte[] input)
    {
        throw new NotImplementedException();
    }
}
