using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Andre.Formats.Util
{
    /// <summary>
    /// Provides a safe interface to native (Rust) code to decrypt RSA-encrypted data.
    /// Used because the previous C#-native implementation takes about twice as long as the current native implementation,
    /// and data2 took around 13 seconds to decrypt with C# on my system.
    /// </summary>
    internal partial class NativeRsa
    {
        private const string Dll = "bhd5_decrypt_rust";

        private enum Bhderr : int
        {
            BHD_OK = 0,
            BHD_ERR_INVALID_ARG = 1,
            BHD_ERR_KEY_PARSE = 2,
            BHD_ERR_UTF8 = 3,
            BHD_ERR_OUTPUT_TOO_SMALL = 4,
            BHD_ERR_INTERNAL = 5,
        }
        
        [LibraryImport(Dll)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static unsafe partial int get_decrypted_size(
            byte* key_pem,
            nuint key_pem_len,
            nuint encrypted_len,
            out nuint out_decrypted_len,
            out byte* err_msg
        );

        [LibraryImport(Dll)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static unsafe partial int decrypt(
            byte* key_pem,
            nuint key_pem_len,
            byte* data,
            nuint data_len,
            byte* output,
            nuint output_cap,
            nuint num_threads,
            out nuint out_written,
            out byte* err_msg
        );

        [LibraryImport(Dll)]
        [UnmanagedCallConv(CallConvs = [typeof(System.Runtime.CompilerServices.CallConvCdecl)])]
        private static unsafe partial void free_error(byte* err_msg);

        private static unsafe void CheckError(int errorCode, byte* err)
        {
            if (errorCode == (int)Bhderr.BHD_OK)
                return;

            string msg = $"Native error code {(Bhderr)errorCode}";
            if (err != null)
            {
                int len = 0;
                while (err[len] != 0) len++;
                msg = Encoding.UTF8.GetString(err, len);
                free_error(err);
            }

            throw new InvalidOperationException(msg);
        }

        private static byte[] GetKeyBytes(string key)
            => Encoding.UTF8.GetBytes(key);
        
        /// <summary>
        /// Determines how many bytes a piece of data will be when decrypted with a given key
        /// </summary>
        public static int GetDecryptedSize(int encryptedSize, string key)
        {
            unsafe
            {
                var keyBytes = GetKeyBytes(key);

                fixed (byte* keyPtr = keyBytes)
                {
                    int code = get_decrypted_size(
                        keyPtr,
                        (nuint)keyBytes.Length,
                        (nuint)encryptedSize,
                        out nuint outSize,
                        out byte* err
                    );

                    CheckError(code, err);
                    return checked((int)outSize);
                }
            }
        }

        /// <summary>
        /// Decrypts RSA-encrypted data, given a user-provided number for how large the output will be
        /// </summary>
        public static byte[] Decrypt(Memory<byte> encrypted, string key, int decryptedSize, int numThreads)
        {
            if (numThreads <= 0)
                throw new ArgumentOutOfRangeException(nameof(numThreads));

            var keyBytes = GetKeyBytes(key);
            var ans = new byte[decryptedSize];
            unsafe
            {
                fixed (byte* keyPtr = keyBytes)
                fixed (byte* outPtr = ans)
                {
                    using var pin = encrypted.Pin();

                    int code = decrypt(
                        keyPtr,
                        (nuint)keyBytes.Length,
                        (byte*)pin.Pointer,
                        (nuint)encrypted.Length,
                        outPtr,
                        (nuint)ans.Length,
                        (nuint)numThreads,
                        out nuint written,
                        out byte* err
                    );

                    CheckError(code, err);

                    if (written != (nuint)ans.Length)
                    {
                        Array.Resize(ref ans, (int)written);
                    }
                }
            }

            return ans;
        }

        /// <summary>
        /// Decrypts RSA-encrypted data, automatically determining how large the output will be
        /// </summary>
        public static byte[] Decrypt(Memory<byte> encrypted, string key, int numThreads)
        {
            int decryptedSize = GetDecryptedSize(encrypted.Length, key);
            return Decrypt(encrypted, key, decryptedSize, numThreads);
        }
    }
}