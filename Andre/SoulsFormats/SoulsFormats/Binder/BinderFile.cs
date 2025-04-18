using System;
using static SoulsFormats.Binder;

namespace SoulsFormats
{
    /// <summary>
    /// A generic file in a BND3, BND4, BXF3, or BXF4 container.
    /// </summary>
    public class BinderFile
    {
        /// <summary>
        /// Flags indicating compression, and possibly other things.
        /// </summary>
        public FileFlags Flags { get; set; }

        /// <summary>
        /// ID of the file, or -1 for none.
        /// </summary>
        public int ID { get; set; }

        /// <summary>
        /// Name of the file, or null for none.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Raw file data.
        /// </summary>
        public Memory<byte> Bytes { get; set; }

        /// <summary>
        /// If compressed, which type of compression to use.
        /// </summary>
        public DCX.Type CompressionType { get; set; }

        /// <summary>
        /// Creates a new file with 0 bytes and no ID or name.
        /// </summary>
        public BinderFile() : this(FileFlags.Flag1, -1, null, new byte[0]) { }

        /// <summary>
        /// Creates a new file with no ID or name.
        /// </summary>
        public BinderFile(FileFlags flags, byte[] bytes) : this(flags, -1, null, bytes) { }

        /// <summary>
        /// Creates a new file with no name.
        /// </summary>
        public BinderFile(FileFlags flags, int id, byte[] bytes) : this(flags, id, null, bytes) { }

        /// <summary>
        /// Creates a new file with no ID.
        /// </summary>
        public BinderFile(FileFlags flags, string name, byte[] bytes) : this(flags, -1, name, bytes) { }

        /// <summary>
        /// Creates a new file.
        /// </summary>
        public BinderFile(FileFlags flags, int id, string name, Memory<byte> bytes)
        {
            Flags = flags;
            ID = id;
            Name = name;
            Bytes = bytes;
            CompressionType = DCX.Type.Zlib;
        }

        /// <summary>
        /// Returns the file flags, ID, name, and byte length as a string.
        /// </summary>
        public override string ToString()
        {
            return $"Flags: 0x{(byte)Flags:X2} | ID: {ID} | Name: {Name} | Length: {Bytes.Length}";
        }
    }
}
