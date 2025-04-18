using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using DotNext.IO.MemoryMappedFiles;

namespace SoulsFormats
{
    /// <summary>
    /// A generic From file supporting transparent DCX reading and writing.
    /// </summary>
    public abstract class SoulsFile<TFormat> : ISoulsFile where TFormat : SoulsFile<TFormat>, new()
    {
        /// <summary>
        /// The type of DCX compression to be used when writing.
        /// </summary>
        public DCX.Type Compression { get; set; } = DCX.Type.None;

        /// <summary>
        /// Returns true if the data appears to be a file of this type.
        /// </summary>
        // This should really be a static method, but interfaces do not allow static inheritance; hence the dummy objects below.
        protected virtual bool Is(BinaryReaderEx br)
        {
            throw new NotImplementedException("Is is not implemented for this format.");
        }

        /// <summary>
        /// Returns true if the bytes appear to be a file of this type.
        /// </summary>
        public static bool Is(byte[] bytes)
        {
            if (bytes.Length == 0)
                return false;

            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            var dummy = new TFormat();
            return dummy.Is(SFUtil.GetDecompressedBR(br, out _));
        }

        /// <summary>
        /// Returns true if the file appears to be a file of this type.
        /// </summary>
        public static bool Is(string path)
        {
            using var file = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            using var accessor = file.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
            if (accessor.Size == 0)
                return false;

            BinaryReaderEx br = new BinaryReaderEx(false, accessor.Memory);
            var dummy = new TFormat();
            return dummy.Is(SFUtil.GetDecompressedBR(br, out _));
        }

        /// <summary>
        /// Loads file data from a BinaryReaderEx.
        /// </summary>
        protected virtual void Read(BinaryReaderEx br)
        {
            throw new NotImplementedException("Read is not implemented for this format.");
        }

        /// <summary>
        /// Loads file data from a BinaryReaderEx.
        /// </summary>
        protected virtual void Read(BinaryReaderEx br, string filename = "")
        {
            throw new NotImplementedException("Read is not implemented for this format.");
        }

        /// <summary>
        /// Loads a file from a byte array, automatically decompressing it if necessary.
        /// </summary>
        public static TFormat Read(Memory<byte> bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            TFormat file = new TFormat();
            br = SFUtil.GetDecompressedBR(br, out DCX.Type compression);
            file.Compression = compression;
            file.Read(br);
            return file;
        }

        /// <summary>
        /// Loads a file from a byte array, automatically decompressing it if necessary.
        /// </summary>
        public static TFormat Read(Memory<byte> bytes, string filename = "")
        {
            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            TFormat file = new TFormat();
            br = SFUtil.GetDecompressedBR(br, out DCX.Type compression);
            file.Compression = compression;
            file.Read(br, filename);
            return file;
        }

        /// <summary>
        /// Loads a file from the specified path, automatically decompressing it if necessary.
        /// </summary>
        public static TFormat Read(string path)
        {
            using var file = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            using var accessor = file.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
            BinaryReaderEx br = new BinaryReaderEx(false, accessor.Memory);
            TFormat ret = new TFormat();
            br = SFUtil.GetDecompressedBR(br, out DCX.Type compression);
            ret.Compression = compression;
            ret.Read(br);
            return ret;
        }


        /// <summary>
        /// Loads a file from the specified path, automatically decompressing it if necessary.
        /// </summary>
        public static TFormat Read(string path, string filename = "")
        {
            using var file = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            using var accessor = file.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
            BinaryReaderEx br = new BinaryReaderEx(false, accessor.Memory);
            TFormat ret = new TFormat();
            br = SFUtil.GetDecompressedBR(br, out DCX.Type compression);
            ret.Compression = compression;
            ret.Read(br, filename);
            return ret;
        }

        /// <summary>
        /// Loads a file from a byte array while ignoring compression.
        /// </summary>
        public static TFormat ReadIgnoreCompression(Memory<byte> bytes)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, bytes);
            TFormat file = new TFormat();
            file.Read(br);
            return file;
        }

        /// <summary>
        /// Loads a file from the specified path while ignoring compression.
        /// </summary>
        public static TFormat ReadIgnoreCompression(string path)
        {
            using var file = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            using var accessor = file.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
            BinaryReaderEx br = new BinaryReaderEx(false, accessor.Memory);
            TFormat ret = new TFormat();
            ret.Read(br);
            return ret;
        }

        private static bool IsRead(BinaryReaderEx br, out TFormat file)
        {
            var test = new TFormat();
            br = SFUtil.GetDecompressedBR(br, out DCX.Type compression);
            if (test.Is(br))
            {
                br.Position = 0;
                test.Compression = compression;
                test.Read(br);
                file = test;
                return true;
            }
            else
            {
                file = null;
                return false;
            }
        }

        /// <summary>
        /// Returns whether the bytes appear to be a file of this type and reads it if so.
        /// </summary>
        public static bool IsRead(byte[] bytes, out TFormat file)
        {
            var br = new BinaryReaderEx(false, bytes);
            return IsRead(br, out file);
        }

        /// <summary>
        /// Returns whether the file appears to be a file of this type and reads it if so.
        /// </summary>
        public static bool IsRead(string path, out TFormat ret)
        {
            using var file = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            using var accessor = file.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
            var br = new BinaryReaderEx(false, accessor.Memory);
            return IsRead(br, out ret);
        }

        /// <summary>
        /// Checks the object for any fatal problems; Write will throw the returned exception on failure.
        /// </summary>
        public virtual bool Validate(out Exception ex)
        {
            ex = null;
            return true;
        }

        /// <summary>
        /// Returns whether the object is not null, otherwise setting ex to a NullReferenceException with the given message.
        /// </summary>
        protected static bool ValidateNull(object obj, string message, out Exception ex)
        {
            if (obj == null)
            {
                ex = new NullReferenceException(message);
                return false;
            }
            else
            {
                ex = null;
                return true;
            }
        }

        /// <summary>
        /// Returns whether the index is in range, otherwise setting ex to an IndexOutOfRangeException with the given message.
        /// </summary>
        protected static bool ValidateIndex(long count, long index, string message, out Exception ex)
        {
            if (index < 0 || index >= count)
            {
                ex = new IndexOutOfRangeException(message);
                return false;
            }
            else
            {
                ex = null;
                return true;
            }
        }

        /// <summary>
        /// Writes file data to a BinaryWriterEx.
        /// </summary>
        protected virtual void Write(BinaryWriterEx bw)
        {
            throw new NotImplementedException("Write is not implemented for this format.");
        }

        /// <summary>
        /// Writes file data to a BinaryWriterEx, compressing it afterwards if specified.
        /// </summary>
        private void Write(BinaryWriterEx bw, DCX.Type compression)
        {
            if (compression == DCX.Type.None)
            {
                Write(bw);
            }
            else
            {
                BinaryWriterEx bwUncompressed = new BinaryWriterEx(false);
                Write(bwUncompressed);
                byte[] uncompressed = bwUncompressed.FinishBytes();
                DCX.Compress(uncompressed, bw, compression);
            }
        }

        /// <summary>
        /// Writes the file to an array of bytes, automatically compressing it if necessary.
        /// </summary>
        public byte[] Write()
        {
            return Write(Compression);
        }

        /// <summary>
        /// Writes the file to an array of bytes, compressing it as specified.
        /// </summary>
        public byte[] Write(DCX.Type compression)
        {
            if (!Validate(out Exception ex))
                throw ex;

            BinaryWriterEx bw = new BinaryWriterEx(false);
            Write(bw, compression);
            return bw.FinishBytes();
        }

        /// <summary>
        /// Writes the file to the specified path, automatically compressing it if necessary.
        /// </summary>
        public void Write(string path)
        {
            Write(path, Compression);
        }

        /// <summary>
        /// Writes the file to the specified path, compressing it as specified.
        /// </summary>
        public void Write(string path, DCX.Type compression)
        {
            if (!Validate(out Exception ex))
                throw ex;

            Directory.CreateDirectory(Path.GetDirectoryName(path));
            using (FileStream stream = File.Create(path))
            {
                BinaryWriterEx bw = new BinaryWriterEx(false, stream);
                Write(bw, compression);
                bw.Finish();
            }
        }
    }
}
