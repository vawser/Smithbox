using System;
using SoapstoneLib.Proto.Internal;

namespace SoapstoneLib
{
    /// <summary>
    /// Representation for a SoulsKey type which can be sent over RPC, including to or from servers not written in C#.
    /// </summary>
    public class SoulsKeyType
    {
        internal string File { get; }
        internal PrimaryKeyCategory Category { get; set; }
        internal Type Type { get; set; }

        internal static SoulsKeyType ForFile(string File, Type Type) =>
            new SoulsKeyType(File, PrimaryKeyCategory.File, Type);
        internal static SoulsKeyType ForEntry(SoulsKeyType FileType, Type Type) =>
            new SoulsKeyType(FileType.File, PrimaryKeyCategory.Entry, Type);

        internal SoulsKeyType(string File, PrimaryKeyCategory Category, Type Type)
        {
            this.File = File;
            this.Category = Category;
            this.Type = Type;
        }

        /// <summary>
        /// Determines if a given C# type is compatible with this requested result type.
        /// 
        /// In V1, only class types are used to determine result compatibility. This may change.
        /// </summary>
        public bool Matches(Type resultType)
        {
            return Type.IsAssignableFrom(resultType);
        }

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is SoulsKeyType o && o.File == File && o.Category == Category;
        /// <inheritdoc />
        public override int GetHashCode() => File.GetHashCode() ^ Category.GetHashCode();
    }
}
