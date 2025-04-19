using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

// Credit to GoogleBen (https://github.com/googleben/Smithbox/tree/VFS)
namespace Andre.Core.Util
{
    /// <summary>
    /// Provides a safe interface to native (Rust) code to decrypt RSA-encrypted data.
    /// Used because the previous C#-native implementation takes about twice as long as the current native implementation,
    /// and data2 took around 13 seconds to decrypt with C# on my system.
    /// </summary>
    internal class NativeRsa
    {
        [DllImport("bhd5_decrypt_rust.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int get_decrypted_size(int encryptedLen, [In] byte[] key, out byte* err);

        [DllImport("bhd5_decrypt_rust.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe int decrypt([In] byte* data, int dataLen, byte[] output, int outputSize, [In] byte[] key, int numThreads, out byte* err);

        [DllImport("bhd5_decrypt_rust.dll", CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern unsafe void free_error(byte* err);

        private static unsafe void CheckError(byte* err)
        {
            if (err != null)
            {
                int len = 0;
                while (*(err + len) != 0) len++;
                var str = Encoding.UTF8.GetString(err, len);
                free_error(err);
                throw new Exception(str);
            }
        }

        /// <summary>
        /// Determines how many bytes a piece of data will be when decrypted with a given key
        /// </summary>
        public static int GetDecryptedSize(int encryptedSize, string key)
        {
            unsafe
            {
                int ans = get_decrypted_size(encryptedSize, Encoding.UTF8.GetBytes(key), out var err);
                CheckError(err);
                return ans;
            }
        }

        /// <summary>
        /// Decrypted RSA-encrypted data, given a user-provided number for how large the output will be
        /// </summary>
        public static byte[] Decrypt(Memory<byte> encrypted, string key, int decryptedSize, int numThreads)
        {
            if (numThreads < 1) throw new Exception("Cannot use 0 threads");
            var ans = new byte[decryptedSize];
            unsafe
            {
                byte* err;
                int realDecSize;
                using (var pinned = encrypted.Pin())
                {
                    realDecSize = decrypt((byte*)pinned.Pointer, encrypted.Length, ans, decryptedSize,
                        Encoding.UTF8.GetBytes(key), numThreads, out err);
                }

                CheckError(err);
                if (realDecSize != decryptedSize) ans = ans[..realDecSize];
            }
            return ans;
        }

        /// <summary>
        /// Decrypts RSA-encrypted data, automatically determining how large the output will be
        /// </summary>
        public static byte[] Decrypt(Memory<byte> encrypted, string key, int numThreads)
        {
            unsafe
            {
                var decryptedSize = GetDecryptedSize(encrypted.Length, key);
                return Decrypt(encrypted, key, decryptedSize, numThreads);
            }
        }
    }
}