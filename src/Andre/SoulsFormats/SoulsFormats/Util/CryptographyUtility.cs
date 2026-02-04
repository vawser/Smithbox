using System;
using System.IO;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Security;
using System.Threading;
using System.Threading.Tasks;

namespace SoulsFormats.Util
{
    /// <summary>
    /// A cryptographic class for implementing aes-ctr enc/decryption
    /// </summary>
    public static class CryptographyUtility
    {
        public static byte[] DecryptAesEcb(Stream inputStream, byte[] key)
        {
            var cipher = CreateAesEcbCipher(key);
            return DecryptAes(inputStream, cipher, inputStream.Length);
        }

        public static byte[] DecryptAesCbc(Stream inputStream, byte[] key, byte[] iv)
        {
            AesEngine engine = new AesEngine();
            KeyParameter keyParameter = new KeyParameter(key);
            ICipherParameters parameters = new ParametersWithIV(keyParameter, iv);

            BufferedBlockCipher cipher = new BufferedBlockCipher(new CbcBlockCipher(engine));
            cipher.Init(false, parameters);
            return DecryptAes(inputStream, cipher, inputStream.Length);
        }

        public static byte[] DecryptAesCtr(Stream inputStream, byte[] key, byte[] iv)
        {
            AesEngine engine = new AesEngine();
            KeyParameter keyParameter = new KeyParameter(key);
            ICipherParameters parameters = new ParametersWithIV(keyParameter, iv);

            BufferedBlockCipher cipher = new BufferedBlockCipher(new SicBlockCipher(engine));
            cipher.Init(false, parameters);
            return DecryptAes(inputStream, cipher, inputStream.Length);
        }

        public static byte[] EncryptAesCtr(byte[] input, byte[] key, byte[] iv)
        {
            IBufferedCipher cipher = CipherUtilities.GetCipher("AES/CTR/NoPadding");
            cipher.Init(true, new ParametersWithIV(ParameterUtilities.CreateKeyParameter("AES", key), iv));
            return cipher.DoFinal(input);
        }

        private static BufferedBlockCipher CreateAesEcbCipher(byte[] key)
        {
            AesEngine engine = new AesEngine();
            KeyParameter parameter = new KeyParameter(key);
            BufferedBlockCipher cipher = new BufferedBlockCipher(engine);
            cipher.Init(false, parameter);
            return cipher;
        }

        private static byte[] DecryptAes(Stream inputStream, BufferedBlockCipher cipher, long length)
        {
            int blockSize = cipher.GetBlockSize();
            int inputLength = (int)length;
            int paddedLength = inputLength;
            if (paddedLength % blockSize > 0)
            {
                paddedLength += blockSize - paddedLength % blockSize;
            }

            byte[] input = new byte[paddedLength];
            byte[] output = new byte[cipher.GetOutputSize(paddedLength)];

            inputStream.Read(input, 0, inputLength);
            int len = cipher.ProcessBytes(input, 0, input.Length, output, 0);
            cipher.DoFinal(output, len);
            return output;
        }

        /// <summary>
        ///     Decrypts a file with a provided decryption key.
        /// </summary>
        /// <param name="filePath">An encrypted file</param>
        /// <param name="key">The RSA key in PEM format</param>
        /// <exception cref="ArgumentNullException">When the argument filePath is null</exception>
        /// <exception cref="ArgumentNullException">When the argument keyPath is null</exception>
        /// <returns>A memory stream with the decrypted file</returns>
        public static MemoryStream DecryptRsa(string filePath, string key)
        {
            ArgumentNullException.ThrowIfNull(filePath);
            ArgumentNullException.ThrowIfNull(key);

            AsymmetricKeyParameter keyParameter = GetKeyOrDefault(key);
            RsaEngine engine = new RsaEngine();
            engine.Init(false, keyParameter);

            int inputBlockSize = engine.GetInputBlockSize();
            int outputBlockSize = engine.GetOutputBlockSize();

            // Zero buffer so we can pad with zeros without allocating each time
            byte[] zeroPad = new byte[Math.Min(outputBlockSize, 4096)];

            using var inputStream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 1 << 20,                 // 1 MiB
                options: FileOptions.SequentialScan);

            long inputLength = inputStream.Length;
            long blockCount = (inputLength + inputBlockSize - 1) / inputBlockSize;
            long estimatedOutputLength = blockCount * outputBlockSize;

            var outputStream = new MemoryStream((int)estimatedOutputLength);

            byte[] inputBlock = new byte[inputBlockSize];

            while (true)
            {
                int totalRead = 0;
                while (totalRead < inputBlockSize)
                {
                    int read = inputStream.Read(inputBlock, totalRead, inputBlockSize - totalRead);
                    if (read == 0) break;
                    totalRead += read;
                }

                if (totalRead == 0)
                    break;

                byte[] outputBlock = engine.ProcessBlock(inputBlock, 0, totalRead);

                int requiredPadding = outputBlockSize - outputBlock.Length;
                while (requiredPadding > 0)
                {
                    int chunk = Math.Min(requiredPadding, zeroPad.Length);
                    outputStream.Write(zeroPad, 0, chunk);
                    requiredPadding -= chunk;
                }

                outputStream.Write(outputBlock, 0, outputBlock.Length);
            }

            outputStream.Seek(0, SeekOrigin.Begin);
            return outputStream;
        }
        
        /// <summary>
        ///     Decrypts a file with a provided decryption key using multiple threads.
        /// </summary>
        /// <param name="filePath">An encrypted file</param>
        /// <param name="key">The RSA key in PEM format</param>
        /// <param name="maxThreadCount">The maximum number of threads to use</param>
        /// <exception cref="ArgumentNullException">When the argument filePath is null</exception>
        /// <exception cref="ArgumentNullException">When the argument keyPath is null</exception>
        /// <returns>A memory stream with the decrypted file</returns>
        public static MemoryStream DecryptRsaParallel(string filePath, string key, int maxThreadCount)
        {
            ArgumentNullException.ThrowIfNull(filePath);
            ArgumentNullException.ThrowIfNull(key);

            AsymmetricKeyParameter keyParameter = GetKeyOrDefault(key);

            // Use a temporary engine just to get block sizes (cheap).
            var sizingEngine = new RsaEngine();
            sizingEngine.Init(false, keyParameter);
            int inputBlockSize = sizingEngine.GetInputBlockSize();
            int outputBlockSize = sizingEngine.GetOutputBlockSize();

            byte[] encrypted = File.ReadAllBytes(filePath);

            int blockCount = (encrypted.Length + inputBlockSize - 1) / inputBlockSize;

            long totalOutputLength = (long)blockCount * outputBlockSize;

            byte[] decrypted = new byte[(int)totalOutputLength];

            // Pretty sure RsaEngine is not thread-safe, so each thread gets its own
            var engineLocal = new ThreadLocal<RsaEngine>(() =>
            {
                var e = new RsaEngine();
                e.Init(false, keyParameter);
                return e;
            });

            var options = new ParallelOptions { MaxDegreeOfParallelism = maxThreadCount };

            Parallel.For(0, blockCount, options, i =>
            {
                int inOffset = i * inputBlockSize;
                int inLen = Math.Min(inputBlockSize, encrypted.Length - inOffset);

                byte[] outBlock = engineLocal.Value!.ProcessBlock(encrypted, inOffset, inLen);

                int outOffset = i * outputBlockSize;
                int pad = outputBlockSize - outBlock.Length;

                // decrypted[] is already zero-initialized, so padding is “free”.
                Buffer.BlockCopy(outBlock, 0, decrypted, outOffset + pad, outBlock.Length);
            });

            engineLocal.Dispose();

            //TODO: Maybe switch off MemoryStream?
            return new MemoryStream(decrypted, writable: false);
        }


        public static AsymmetricKeyParameter GetKeyOrDefault(string key)
        {
            try
            {
                PemReader pemReader = new PemReader(new StringReader(key));
                return (AsymmetricKeyParameter)pemReader.ReadObject();
            }
            catch
            {
                return null;
            }
        }
    }
}
