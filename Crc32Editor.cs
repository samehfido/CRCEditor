using System;
using System.IO;
using System.Linq;

namespace com.Google.Inc.Crc32Editor
{

    public class Crc32Editor
    {
        private static long[] crc32Table = new long[256];

        static Crc32Editor()
        {
            long j;
            for (int i = 0; i < 256; i++)
            {
                long j2 = (long)i;
                for (int i2 = 0; i2 < 8; i2++)
                {
                    if ((j2 & ((long)1)) == ((long)1))
                    {
                        j = 3988292384L ^ (j2 >> 1);
                    }
                    else
                    {
                        j = j2 >> 1;
                    }
                    j2 = j;
                }
                crc32Table[i] = j2;
            }

        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="FileName"></param>
        /// <param name="CRCHash"></param>
        /// <returns></returns>
        public static bool editFile(string FileName, string CRCHash)
        {
            try
            {
                long longValue = Convert.ToInt64(CRCHash, 16);
                long CurrentHash = GetFileCrc32(FileName);
                byte[] bytesToWrite = new byte[4];
                sbyte[] aa = getAddBytes(CurrentHash, longValue);
                for (int i = 0; i < aa.Length; i++)
                {
                    if (aa[i] > UInt32.MinValue)
                        bytesToWrite[i] = (byte)Convert.ToUInt32(aa[i]);
                    else
                        bytesToWrite[i] = (byte)0;
                }
                FileStream fileOutputStream = new FileStream(FileName, FileMode.Append);
                fileOutputStream.Write(bytesToWrite, 0, bytesToWrite.Length);
                fileOutputStream.Close();
                if (GetFileCrc32(FileName) != longValue)
                {
                    return editFile(FileName, CRCHash);
                }
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public static string GetFileCrc32String(string str) => GetFileCrc32(str).ToString("X");
        public static long GetFileCrc32(string str)
        {
            try
            {
                FileStream fileInputStream = new FileStream(str, FileMode.Open, FileAccess.Read);
                byte[] bArr = new byte[fileInputStream.Length];
                fileInputStream.Read(bArr, 0, bArr.Length);
                fileInputStream.Close();
                // var hash = new Crc32().Get(bArr);
                //CRC_Hasher.Crc32.Compute(bArr);
                return calculateCRC(bArr);
            }
            catch (Exception)
            {
                return (long)-1;
            }
        }

        private static uint calculateCRC(byte[] Array)
        {
            const uint POLYNOMIAL = 0xEDB88320;
            uint result = 0xFFFFFFFF;
            uint Crc32;
            uint[] table_CRC32 = new uint[256];
            unchecked
            {
                for (int i = 0; i < 256; i++)
                {
                    Crc32 = (uint)i;
                    for (int j = 8; j > 0; j--)
                    {
                        if ((Crc32 & 1) == 1)
                        {
                            Crc32 = (Crc32 >> 1) ^ POLYNOMIAL;
                        }
                        else
                        {
                            Crc32 >>= 1;
                        }
                    }
                    table_CRC32[i] = Crc32;
                }
                for (int i = 0; i < Array.Length; i++)
                {
                    result = ((result) >> 8) ^ table_CRC32[(Array[i]) ^ ((result) & 0x000000FF)];
                }
                System.Array.Clear(Array, 0, Array.Length);
            }
            return ~result;
        }

        private static sbyte[] getAddBytes(long j, long j2)
        {
            sbyte[] bArr = new sbyte[4];
            int[] iArr = new int[4];
            long[] jArr = new long[4];
            long[] jArr2 = new long[4];
            jArr[3] = j2 ^ 4294967295L;
            iArr[3] = getIndex(jArr[3] >> 24);
            jArr[2] = jArr[3] ^ crc32Table[iArr[3]];
            iArr[2] = getIndex(jArr[2] >> 16);
            jArr[1] = jArr[2] ^ (crc32Table[iArr[2]] >> 8);
            iArr[1] = getIndex(jArr[1] >> 8);
            jArr[0] = jArr[1] ^ (crc32Table[iArr[1]] >> 16);
            iArr[0] = getIndex(jArr[0]);
            jArr2[0] = j ^ 4294967295L;
            bArr[0] = getByte(jArr2[0], iArr[0]);
            jArr2[1] = getXor(jArr2[0], bArr[0]);
            bArr[1] = getByte(jArr2[1], iArr[1]);
            jArr2[2] = getXor(jArr2[1], bArr[1]);
            bArr[2] = getByte(jArr2[2], iArr[2]);
            jArr2[3] = getXor(jArr2[2], bArr[2]);
            bArr[3] = getByte(jArr2[3], iArr[3]);
            return bArr;
        }

        private static long getXor(long j, sbyte b)
        {
            return crc32Table[(int)((j ^ ((long)b)) & ((long)255))] ^ (j >> 8);
        }

        private static int getIndex(long j)
        {
            for (int i = 0; i < crc32Table.Length; i++)
            {
                if ((crc32Table[i] >> 24) == j)
                {
                    return i;
                }
            }
            return 0;
        }

        private static sbyte getByte(long j, int i)
        {
            int i2 = -128;
            while (true)
            {
                sbyte b = (sbyte)i2;
                if (b >= sbyte.MaxValue)
                {
                    return (sbyte)0;
                }
                if (i == ((int)((j ^ ((long)b)) & ((long)255))))
                {
                    return b;
                }
                i2 = b + 1;
            }
        }


    }

}