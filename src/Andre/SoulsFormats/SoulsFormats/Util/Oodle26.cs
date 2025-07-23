using System;
using System.Runtime.InteropServices;

namespace SoulsFormats
{
    internal class Oodle26 : IOodleCompressor
    {
        public unsafe byte[] Compress(Span<byte> source, Oodle.OodleLZ_Compressor compressor, Oodle.OodleLZ_CompressionLevel level)
        {
            IntPtr pOptions = OodleLZ_CompressOptions_GetDefault(compressor, level);
            Oodle.OodleLZ_CompressOptions options = Marshal.PtrToStructure<Oodle.OodleLZ_CompressOptions>(pOptions);
            // Required for the game to not crash
            options.seekChunkReset = true;
            // This is already the default but I am including it for authenticity to game code
            options.seekChunkLen = 0x40000;
            pOptions = Marshal.AllocHGlobal(Marshal.SizeOf<Oodle.OodleLZ_CompressOptions>());

            try
            {
                Marshal.StructureToPtr(options, pOptions, false);
                long compressedBufferSizeNeeded = OodleLZ_GetCompressedBufferSizeNeeded(source.Length);
                byte[] compBuf = new byte[compressedBufferSizeNeeded];
                fixed (byte* ptr = source)
                {
                    long compLen = OodleLZ_Compress(compressor, ptr, source.Length, compBuf, level, pOptions,
                        IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0);
                    Array.Resize(ref compBuf, (int)compLen);
                    return compBuf;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pOptions);
            }
        }

        public unsafe Memory<byte> Decompress(Span<byte> source, long uncompressedSize)
        {
            long decodeBufferSize = OodleLZ_GetDecodeBufferSize(uncompressedSize, true);
            byte[] rawBuf = new byte[decodeBufferSize];
            fixed (byte* ptr = source)
            {
                long rawLen = OodleLZ_Decompress(ptr, source.Length, rawBuf, uncompressedSize);
                return new Memory<byte>(rawBuf, 0, (int)rawLen);
            }
        }


        /// <param name="compressor"></param>
        /// <param name="rawBuf"></param>
        /// <param name="rawLen"></param>
        /// <param name="compBuf"></param>
        /// <param name="level"></param>
        /// <param name="pOptions">= NULL</param>
        /// <param name="dictionaryBase">= NULL</param>
        /// <param name="lrm">= NULL</param>
        /// <param name="scratchMem">= NULL</param>
        /// <param name="scratchSize">= 0</param>
        [DllImport("oo2core_6_win64.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern unsafe long OodleLZ_Compress(
            Oodle.OodleLZ_Compressor compressor,
            byte* rawBuf,
            long rawLen,
            [MarshalAs(UnmanagedType.LPArray)]
            byte[] compBuf,
            Oodle.OodleLZ_CompressionLevel level,
            IntPtr pOptions,
            IntPtr dictionaryBase,
            IntPtr lrm,
            IntPtr scratchMem,
            long scratchSize);

        private static unsafe long OodleLZ_Compress(Oodle.OodleLZ_Compressor compressor, byte* rawBuf, long rawLen, byte[] compBuf, Oodle.OodleLZ_CompressionLevel level)
            => OodleLZ_Compress(compressor, rawBuf, rawLen, compBuf, level,
                IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0);


        /// <param name="compressor">= OodleLZ_Compressor_Invalid</param>
        /// <param name="lzLevel">= OodleLZ_CompressionLevel_Normal</param>
        [DllImport("oo2core_6_win64.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern IntPtr OodleLZ_CompressOptions_GetDefault(
            Oodle.OodleLZ_Compressor compressor,
            Oodle.OodleLZ_CompressionLevel lzLevel);

        public static IntPtr OodleLZ_CompressOptions_GetDefault()
            => OodleLZ_CompressOptions_GetDefault(Oodle.OodleLZ_Compressor.OodleLZ_Compressor_Invalid, Oodle.OodleLZ_CompressionLevel.OodleLZ_CompressionLevel_Normal);


        /// <param name="compBuf"></param>
        /// <param name="compBufSize"></param>
        /// <param name="rawBuf"></param>
        /// <param name="rawLen"></param>
        /// <param name="fuzzSafe">= OodleLZ_FuzzSafe_Yes</param>
        /// <param name="checkCRC">= OodleLZ_CheckCRC_No</param>
        /// <param name="verbosity">= OodleLZ_Verbosity_None</param>
        /// <param name="decBufBase">= NULL</param>
        /// <param name="decBufSize">= 0</param>
        /// <param name="fpCallback">= NULL</param>
        /// <param name="callbackUserData">= NULL</param>
        /// <param name="decoderMemory">= NULL</param>
        /// <param name="decoderMemorySize">= 0</param>
        /// <param name="threadPhase">= OodleLZ_Decode_Unthreaded</param>
        [DllImport("oo2core_6_win64.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern unsafe long OodleLZ_Decompress(
            byte* compBuf,
            long compBufSize,
            [MarshalAs(UnmanagedType.LPArray)]
            byte[] rawBuf,
            long rawLen,
            Oodle.OodleLZ_FuzzSafe fuzzSafe,
            Oodle.OodleLZ_CheckCRC checkCRC,
            Oodle.OodleLZ_Verbosity verbosity,
            IntPtr decBufBase,
            long decBufSize,
            IntPtr fpCallback,
            IntPtr callbackUserData,
            IntPtr decoderMemory,
            long decoderMemorySize,
            Oodle.OodleLZ_Decode_ThreadPhase threadPhase);

        private static unsafe long OodleLZ_Decompress(byte* compBuf, long compBufSize, byte[] rawBuf, long rawLen)
            => OodleLZ_Decompress(compBuf, compBufSize, rawBuf, rawLen,
                Oodle.OodleLZ_FuzzSafe.OodleLZ_FuzzSafe_Yes, Oodle.OodleLZ_CheckCRC.OodleLZ_CheckCRC_No, Oodle.OodleLZ_Verbosity.OodleLZ_Verbosity_None,
                IntPtr.Zero, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, 0, Oodle.OodleLZ_Decode_ThreadPhase.OodleLZ_Decode_Unthreaded);


        [DllImport("oo2core_6_win64.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern long OodleLZ_GetCompressedBufferSizeNeeded(
               long rawSize);


        [DllImport("oo2core_6_win64.dll", CallingConvention = CallingConvention.StdCall)]
        private static extern long OodleLZ_GetDecodeBufferSize(
            long rawSize,
            [MarshalAs(UnmanagedType.Bool)]
            bool corruptionPossible);
    }
}
