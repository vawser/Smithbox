using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SoulsFormats;

public class Oodle
{
    public class NoOodleFoundException : Exception
    {
        public NoOodleFoundException(string message) : base(message) { }
    }

    static bool Oodle6Exists = false;
    static bool Oodle8Exists = false;

    private static bool CanUseOodle6()
    {
        if (Oodle6Exists)
        {
            return true;
        }
        if (Path.Exists($@"{Directory.GetCurrentDirectory()}\oo2core_6_win64.dll"))
        {
            Oodle6Exists = true;
            return true;
        }
        return false;
    }

    private static bool CanUseOodle8()
    {
        if (Oodle8Exists)
        {
            return true;
        }
        if (Path.Exists($@"{Directory.GetCurrentDirectory()}\oo2core_8_win64.dll"))
        {
            Oodle8Exists = true;
            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns oodle class to use for compression and decompression.
    /// </summary>
    /// <param name="compressionLevel">Used to determine preferred oodle. If not applicable, any available oodle will be used.</param>
    public static IOodleCompressor GetOodleCompressor(int compressionLevel = -1)
    {
        if (compressionLevel != -1)
        {
            // Try to get preferred oodle using compressionLevel.
            if (compressionLevel == 9)
            {
                if (CanUseOodle8())
                    return new Oodle28();
                if (CanUseOodle6())
                    return new Oodle26();
            }
            else if (compressionLevel == 6)
            {
                if (CanUseOodle6())
                    return new Oodle26();
                if (CanUseOodle8())
                    return new Oodle28();
            }
        }
        else
        {
            if (CanUseOodle6())
                return new Oodle26();
            if (CanUseOodle8())
                return new Oodle28();
        }

        throw new NoOodleFoundException($"Could not find a supported version of oo2core. "
            + $"Please copy oo2core_6_win64.dll or oo2core_8_win64.dll into the program directory");
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OodleLZ_CompressOptions
    {
        public uint verbosity;
        public int minMatchLen;
        [MarshalAs(UnmanagedType.Bool)]
        public bool seekChunkReset;
        public int seekChunkLen;
        public Oodle.OodleLZ_Profile profile;
        public int dictionarySize;
        public int spaceSpeedTradeoffBytes;
        public int maxHuffmansPerChunk;
        [MarshalAs(UnmanagedType.Bool)]
        public bool sendQuantumCRCs;
        public int maxLocalDictionarySize;
        public int makeLongRangeMatcher;
        public int matchTableSizeLog2;
    }

    public enum OodleLZ_CompressionLevel : int
    {
        OodleLZ_CompressionLevel_None = 0,
        OodleLZ_CompressionLevel_SuperFast = 1,
        OodleLZ_CompressionLevel_VeryFast = 2,
        OodleLZ_CompressionLevel_Fast = 3,
        OodleLZ_CompressionLevel_Normal = 4,

        OodleLZ_CompressionLevel_Optimal1 = 5,
        OodleLZ_CompressionLevel_Optimal2 = 6,
        OodleLZ_CompressionLevel_Optimal3 = 7,
        OodleLZ_CompressionLevel_Optimal4 = 8,
        OodleLZ_CompressionLevel_Optimal5 = 9,

        OodleLZ_CompressionLevel_HyperFast1 = -1,
        OodleLZ_CompressionLevel_HyperFast2 = -2,
        OodleLZ_CompressionLevel_HyperFast3 = -3,
        OodleLZ_CompressionLevel_HyperFast4 = -4,

        OodleLZ_CompressionLevel_HyperFast = OodleLZ_CompressionLevel_HyperFast1,
        OodleLZ_CompressionLevel_Optimal = OodleLZ_CompressionLevel_Optimal2,
        OodleLZ_CompressionLevel_Max = OodleLZ_CompressionLevel_Optimal5,
        OodleLZ_CompressionLevel_Min = OodleLZ_CompressionLevel_HyperFast4,

        OodleLZ_CompressionLevel_Force32 = 0x40000000,
        OodleLZ_CompressionLevel_Invalid = OodleLZ_CompressionLevel_Force32
    }

    public enum OodleLZ_Compressor : int
    {
        OodleLZ_Compressor_Invalid = -1,
        OodleLZ_Compressor_None = 3,

        OodleLZ_Compressor_Kraken = 8,
        OodleLZ_Compressor_Leviathan = 13,
        OodleLZ_Compressor_Mermaid = 9,
        OodleLZ_Compressor_Selkie = 11,
        OodleLZ_Compressor_Hydra = 12,

        OodleLZ_Compressor_BitKnit = 10,
        OodleLZ_Compressor_LZB16 = 4,
        OodleLZ_Compressor_LZNA = 7,
        OodleLZ_Compressor_LZH = 0,
        OodleLZ_Compressor_LZHLW = 1,
        OodleLZ_Compressor_LZNIB = 2,
        OodleLZ_Compressor_LZBLW = 5,
        OodleLZ_Compressor_LZA = 6,

        OodleLZ_Compressor_Count = 14,
        OodleLZ_Compressor_Force32 = 0x40000000
    }

    public enum OodleLZ_CheckCRC : int
    {
        OodleLZ_CheckCRC_No = 0,
        OodleLZ_CheckCRC_Yes = 1,
        OodleLZ_CheckCRC_Force32 = 0x40000000
    }

    public enum OodleLZ_Decode_ThreadPhase : int
    {
        OodleLZ_Decode_ThreadPhase1 = 1,
        OodleLZ_Decode_ThreadPhase2 = 2,
        OodleLZ_Decode_ThreadPhaseAll = 3,
        OodleLZ_Decode_Unthreaded = OodleLZ_Decode_ThreadPhaseAll
    }

    public enum OodleLZ_FuzzSafe : int
    {
        OodleLZ_FuzzSafe_No = 0,
        OodleLZ_FuzzSafe_Yes = 1
    }

    public enum OodleLZ_Profile : int
    {
        OodleLZ_Profile_Main = 0,
        OodleLZ_Profile_Reduced = 1,
        OodleLZ_Profile_Force32 = 0x40000000
    }

    public enum OodleLZ_Verbosity : int
    {
        OodleLZ_Verbosity_None = 0,
        OodleLZ_Verbosity_Minimal = 1,
        OodleLZ_Verbosity_Some = 2,
        OodleLZ_Verbosity_Lots = 3,
        OodleLZ_Verbosity_Force32 = 0x40000000
    }
}
