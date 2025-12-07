using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Program.Debugging;


public static class BytePerfectHelper
{
    public static byte[] Md5(ReadOnlySpan<byte> data) => MD5.HashData(data);

    public static bool Md5Equal(ReadOnlySpan<byte> a, ReadOnlySpan<byte> b)
        => CryptographicOperations.FixedTimeEquals(MD5.HashData(a), MD5.HashData(b));

    public static string Md5Hex(ReadOnlySpan<byte> data) => Convert.ToHexString(MD5.HashData(data));

}

