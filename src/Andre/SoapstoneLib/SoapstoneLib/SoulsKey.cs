using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SoapstoneLib.Proto;

namespace SoapstoneLib
{
    /// <summary>
    /// A key for an entity in a FromSoft game.
    /// 
    /// The possible instances of this are currently sealed within this class. More can be added here,
    /// as well as custom per-editor types not directly representing game data.
    /// </summary>
    public interface SoulsKey
    {
        /// <summary>
        /// The game file associated with this entity.
        /// 
        /// In the case of a bnd file, this may be an individual file within the archive.
        /// </summary>
        FileKey File { get; }

        /// <summary>
        /// Overall key type directly representing a file, like an MSB or FMG file.
        /// </summary>
        public abstract class FileKey : SoulsKey
        {
            /// <summary>
            /// Self-reference to this file.
            /// </summary>
            public FileKey File => this;

            private protected abstract string InternalName { get; }
            
            // Try to reuse string instances when possible to minimize serialization overhead
            private string fileName;
            internal string FileName
            {
                get
                {
                    if (fileName == null)
                    {
                        fileName = InternalName;
                    }
                    return fileName;
                }
            }
        }

        /// <summary>
        /// Key for a param file representing a gameparam, such as SpEffectParam.
        /// </summary>
        public sealed class GameParamKey : FileKey
        {
            /// <summary>
            /// Key type which can be used to request and return this type in entity search RPCs.
            /// </summary>
            public static SoulsKeyType KeyType { get; } =
                SoulsKeyType.ForFile("param/GameParam/{0}.param", typeof(GameParamKey));

            /// <summary>
            /// Creates a key with the given fields.
            /// </summary>
            public GameParamKey(string Param)
            {
                this.Param = CheckValidPart(Param, nameof(Param));
            }

            /// <summary>
            /// The param name, such as SpEffectParam.
            /// </summary>
            public string Param { get; }

            private protected override string InternalName => $"param/GameParam/{Param}.param";

            /// <inheritdoc />
            public override string ToString() => $"GameParamKey[Param={Param}]";
            /// <inheritdoc />
            public override bool Equals(object obj) => obj is GameParamKey o && Param == o.Param;
            /// <inheritdoc />
            public override int GetHashCode() => Param.GetHashCode();
        }

        /// <summary>
        /// Key for an FMG file in a game, such as English NpcName.
        /// </summary>
        public sealed class FmgKey : FileKey
        {
            /// <summary>
            /// Key type which can be used to request and return this type in entity search RPCs.
            /// </summary>
            public static SoulsKeyType KeyType { get; } =
                SoulsKeyType.ForFile("msg/{0}/{1}.fmg", typeof(FmgKey));

            /// <summary>
            /// Creates a key with the given fields.
            /// </summary>
            public FmgKey(SoulsFmg.FmgLanguage Language, SoulsFmg.FmgType Type)
            {
                this.Language = Language;
                this.Type = Type;
            }

            /// <summary>
            /// The language of this FMG file.
            /// 
            /// Use SoulsFmg to convert this to a per-game folder name.
            /// </summary>
            public SoulsFmg.FmgLanguage Language { get; }

            /// <summary>
            /// The unique type of this FMG file within the game.
            /// 
            /// Use SoulsFmg to convet this to per-game data representing file locations.
            /// </summary>
            public SoulsFmg.FmgType Type { get; }

            private protected override string InternalName => $"msg/{Language}/{Type}.fmg";

            /// <inheritdoc />
            public override string ToString() => $"FmgKey[Language={Language},Type={Type}]";
            /// <inheritdoc />
            public override bool Equals(object obj) => obj is FmgKey o && Language == o.Language && Type == o.Type;
            /// <inheritdoc />
            public override int GetHashCode() => Language.GetHashCode() ^ Type.GetHashCode();
        }

        /// <summary>
        /// Key for an MSB file, such as m10_00_00_00
        /// </summary>
        public sealed class MsbKey : FileKey
        {
            /// <summary>
            /// Key type which can be used to request and return this type in entity search RPCs.
            /// </summary>
            public static SoulsKeyType KeyType { get; } =
                SoulsKeyType.ForFile("map/mapstudio/{0}.msb", typeof(MsbKey));

            /// <summary>
            /// Creates a key with the given fields.
            /// </summary>
            public MsbKey(string Map)
            {
                this.Map = CheckValidPart(Map, nameof(Map));
            }

            /// <summary>
            /// The map name, such as m10_00_00_00.
            /// </summary>
            public string Map { get; }

            private protected override string InternalName => $"map/mapstudio/{Map}.msb";

            /// <inheritdoc />
            public override string ToString() => $"MsbKey[Map={Map}]";
            /// <inheritdoc />
            public override bool Equals(object obj) => obj is MsbKey o && Map == o.Map;
            /// <inheritdoc />
            public override int GetHashCode() => Map.GetHashCode();
        }

        /// <summary>
        /// Key of an unknown format in this library version.
        /// 
        /// Other key types can be added in the future, as well as debug information about unknown
        /// keys, but for now this requires both client and server to know about the same keys.
        /// </summary>
        public sealed class UnknownKey : FileKey
        {
            internal UnknownKey() { }

            private protected override string InternalName => throw new InvalidOperationException();

            /// <inheritdoc />
            public override string ToString() => "UnknownKey";
        }

        /// <summary>
        /// Overall key type representing a uniquely identifiable entity within a file.
        /// </summary>
        public abstract class ObjectKey : SoulsKey
        {
            /// <summary>
            /// The file key for this object.
            /// </summary>
            public abstract FileKey File { get; }

            /// <summary>
            /// The namespace for the object's id, if the id is not completely unique.
            /// </summary>
            public KeyNamespace Namespace { get; protected set;  }

            /// <summary>
            /// An index which may be used to disambiguate object ids, if multiple with the same id are allowed.
            /// </summary>
            public int Index { get; protected set; }

            internal abstract object InternalID { get; }
        }

        /// <summary>
        /// Key for a row in a game param, like row 7000 in SpEffectParam.
        /// </summary>
        public sealed class GameParamRowKey : ObjectKey
        {
            /// <summary>
            /// Key type which can be used to request and return this type in entity search RPCs.
            /// </summary>
            public static SoulsKeyType KeyType { get; } =
                SoulsKeyType.ForEntry(GameParamKey.KeyType, typeof(GameParamRowKey));

            private readonly GameParamKey file;

            /// <summary>
            /// Creates a key with the given fields.
            /// </summary>
            public GameParamRowKey(GameParamKey file, int ID, int Index = 0)
            {
                this.file = file;
                this.ID = ID;
                this.Index = Index;
            }

            /// <summary>
            /// The param file.
            /// </summary>
            public override GameParamKey File => file;

            /// <summary>
            /// The row ID.
            /// </summary>
            public int ID { get; }

            internal override object InternalID => ID;

            /// <inheritdoc />
            public override string ToString() => $"GameParamRowKey[Param={File.Param},ID={ID}]";
            /// <inheritdoc />
            public override bool Equals(object obj) => obj is GameParamRowKey o && File.Equals(o.File) && ID == o.ID && Index == o.Index;
            /// <inheritdoc />
            public override int GetHashCode() => File.GetHashCode() ^ ID.GetHashCode() ^ Index.GetHashCode();
        }

        /// <summary>
        /// Key for an entry in an FMG file, like entry 9000 in Japanese PlaceName.
        /// </summary>
        public sealed class FmgEntryKey : ObjectKey
        {
            /// <summary>
            /// Key type which can be used to request and return this type in entity search RPCs.
            /// </summary>
            public static SoulsKeyType KeyType { get; } =
                SoulsKeyType.ForEntry(FmgKey.KeyType, typeof(FmgEntryKey));

            private readonly FmgKey file;

            /// <summary>
            /// Creates a key with the given fields.
            /// </summary>
            public FmgEntryKey(FmgKey file, int ID, int Index = 0)
            {
                this.file = file;
                this.ID = ID;
                this.Index = Index;
            }

            /// <summary>
            /// The FMG file.
            /// </summary>
            public override FmgKey File => file;

            /// <summary>
            /// The entry ID.
            /// </summary>
            public int ID { get; }

            internal override object InternalID => ID;

            /// <inheritdoc />
            public override string ToString() => $"FmgEntryKey[Language={File.Language},Type={File.Type},ID={ID}]";
            /// <inheritdoc />
            public override bool Equals(object obj) => obj is FmgEntryKey o && File.Equals(o.File) && Index == o.Index;
            /// <inheritdoc />
            public override int GetHashCode() => File.GetHashCode() ^ Index.GetHashCode();
        }

        /// <summary>
        /// Key for a named entity in an MSB.
        /// 
        /// This uses the SoulsFormats convention of disambiguating names within a namespace,
        /// so that named references can be unambiguous.
        /// </summary>
        public sealed class MsbEntryKey : ObjectKey
        {
            /// <summary>
            /// Key type which can be used to request and return this type in entity search RPCs.
            /// </summary>
            public static SoulsKeyType KeyType { get; } =
                SoulsKeyType.ForEntry(MsbKey.KeyType, typeof(MsbEntryKey));

            /// <summary>
            /// All supported namespaces for MSB entry names.
            /// </summary>
            public static readonly IReadOnlyList<KeyNamespace> Namespaces = new List<KeyNamespace>
            {
                KeyNamespace.MapEvent,
                KeyNamespace.MapRegion,
                KeyNamespace.MapPart,
            }.AsReadOnly();

            private readonly MsbKey file;

            /// <summary>
            /// Creates a key with the given fields.
            /// 
            /// The entity namespace must belong to the Namespaces list.
            /// </summary>
            public MsbEntryKey(MsbKey file, KeyNamespace Namespace, string Name)
            {
                if (!Namespaces.Contains(Namespace))
                {
                    throw new ArgumentException($"Invalid map entity namespace {Namespace} (must be one of {string.Join(", ", Namespaces)})");
                }
                this.file = file;
                this.Namespace = Namespace;
                this.Name = Name;
            }

            /// <summary>
            /// The MSB file this entity appears in.
            /// </summary>
            public override MsbKey File => file;

            /// <summary>
            /// The unique name (within the namespace) of this entity.
            /// </summary>
            public string Name { get; }

            internal override object InternalID => Name;

            /// <inheritdoc />
            public override string ToString() => $"MsbEntryKey[Map={File.Map},Namespace={Namespace},Name={Name}]";
            /// <inheritdoc />
            public override bool Equals(object obj) =>
                obj is MsbEntryKey o && File.Equals(o.File) && Namespace == o.Namespace && Name == o.Name;
            /// <inheritdoc />
            public override int GetHashCode() => File.GetHashCode() ^ Namespace.GetHashCode() ^ Name.GetHashCode();
        }

        internal static readonly HashSet<SoulsKeyType> KeyTypes = new HashSet<SoulsKeyType>
        {
            GameParamKey.KeyType,
            FmgKey.KeyType,
            MsbKey.KeyType,
            GameParamRowKey.KeyType,
            FmgEntryKey.KeyType,
            MsbEntryKey.KeyType,
        };

        private static bool ExtractExtension(string file, string ext, out string name)
        {
            if (file.EndsWith(ext))
            {
                name = file.Substring(0, file.Length - ext.Length);
                return true;
            }
            name = null;
            return false;
        }

        internal static FileKey ParseFileKey(string str)
        {
            string[] parts = str.Split('/');
            if (parts.Length == 3 && parts[0] == "param" && parts[1] == "GameParam" && ExtractExtension(parts[2], ".param", out string param))
            {
                return new GameParamKey(param);
            }
            else if (parts.Length == 3 && parts[0] == "msg" && ExtractExtension(parts[2], ".fmg", out string fmg)
                && Enum.TryParse(parts[1], out SoulsFmg.FmgLanguage lang) && Enum.TryParse(fmg, out SoulsFmg.FmgType type))
            {
                return new FmgKey(lang, type);
            }
            else if (parts.Length == 3 && parts[0] == "map" && parts[1] == "mapstudio" && ExtractExtension(parts[2], ".msb", out string msb))
            {
                return new MsbKey(msb);
            }
            // At the moment, ignore unknown keys.
            // If needed, we can do partial parsing of the filename, for compatibility in future versions,
            // but we'd strongly prefer if clients upgrade to use new key types.
            return new UnknownKey();
        }

        // Standard invalid characters, and path separators.
        // Also disallow whitespace for the time being (can revisit this later).
        // https://stackoverflow.com/questions/1976007/what-characters-are-forbidden-in-windows-and-linux-directory-names
        private static readonly Regex invalidFileCharacters = new Regex(@"[\p{C}<>:\\/|?*""\s]", RegexOptions.Compiled);
        private static string CheckValidPart(string part, string name)
        {
            if (part == null)
            {
                throw new ArgumentNullException(name);
            }
            if (invalidFileCharacters.IsMatch(part))
            {
                throw new ArgumentException($"{name} \"{part}\" contains invalid characters for a file path");
            }
            return part;
        }
    }
}
