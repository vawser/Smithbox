using System.Collections.Generic;
using System.Linq;
using SoapstoneLib.Proto;

namespace SoapstoneLib
{
    /// <summary>
    /// Utility class for identifying all FMGs across all games.
    /// 
    /// No in-game key consistently works across all games, so Soapstone uses a custom FmgType enum
    /// which maps to other key data per-game.
    /// </summary>
    public static partial class SoulsFmg
    {
        /// <summary>
        /// High-level grouping for a FMG. This can be expanded to represent other groupings.
        /// </summary>
        public enum FmgCategory
        {
            /// <summary>
            /// FMG not contained within any msgbnd.
            /// </summary>
            None,
            /// <summary>
            /// FMG contained within an item msgbnd.
            /// </summary>
            Item,
            /// <summary>
            /// FMG contained within a menu msgbnd
            /// </summary>
            Menu,
        }

        /// <summary>
        /// Produces all FmgTypes for a given game and binder ID.
        /// 
        /// When this returns true, the list will contain at least one element. In all
        /// currently supported FromSoft games, the list contains exactly one element.
        /// </summary>
        public static bool TryGetFmgBinderType(FromSoftGame game, int binderID, out List<FmgType> types)
        {
            if (TryGetFmgGameInfo(game, out FmgGameInfo gameInfo)
                && gameInfo.ByBinderID.TryGetValue(binderID, out types))
            {
                // Defensive copy
                types = types.ToList();
                return true;
            }
            types = null;
            return false;
        }

        /// <summary>
        /// Produces all FmgTypes for a given game and vanilla FMG name, without the ".fmg" suffix.
        /// 
        /// When this returns true, the list will contain at least one element. In all
        /// currently supported FromSoft games, at most two types may be returned.
        /// </summary>
        public static bool TryGetFmgNameType(FromSoftGame game, string fmgName, out List<FmgType> types)
        {
            if (TryGetFmgGameInfo(game, out FmgGameInfo gameInfo)
                && gameInfo.ByFmgName.TryGetValue(fmgName, out types))
            {
                // Defensive copy
                types = types.ToList();
                return true;
            }
            types = null;
            return false;
        }

        /// <summary>
        /// Produces FmgKeyInfo metadata for a given game and FmgType.
        /// </summary>
        public static bool TryGetFmgInfo(FromSoftGame game, FmgType type, out FmgKeyInfo info)
        {
            if (TryGetFmgGameInfo(game, out FmgGameInfo gameInfo)
                && gameInfo.ByType.TryGetValue(type, out info))
            {
                return true;
            }
            info = null;
            return false;
        }

        /// <summary>
        /// Produces FmgKeyInfo metadata for a given game and key.
        /// </summary>
        public static bool TryGetFmgInfo(FromSoftGame game, SoulsKey.FmgKey key, out FmgKeyInfo info)
        {
            return TryGetFmgInfo(game, key.Type, out info);
        }

        /// <summary>
        /// Produces a language folder name for a given game and FmgLanguage.
        /// 
        /// This is "japanese" for Demon's Souls Japanese, which does not have a dedicated folder.
        /// </summary>
        public static bool TryGetFmgLanguage(FromSoftGame game, FmgLanguage langEnum, out string lang)
        {
            if (TryGetFmgGameInfo(game, out FmgGameInfo gameInfo)
                && gameInfo.FromLanguageEnum.TryGetValue(langEnum, out lang))
            {
                return true;
            }
            lang = null;
            return false;
        }

        /// <summary>
        /// Produces a language folder name for a given game and key.
        /// </summary>
        public static bool TryGetFmgLanguage(FromSoftGame game, SoulsKey.FmgKey key, out string lang)
        {
            return TryGetFmgLanguage(game, key.Language, out lang);
        }

        /// <summary>
        /// Produces an FmgLanguage for a given game and language folder name.
        /// </summary>
        public static bool TryGetFmgLanguageEnum(FromSoftGame game, string lang, out FmgLanguage langEnum)
        {
            if (TryGetFmgGameInfo(game, out FmgGameInfo gameInfo)
                && gameInfo.ToLanguageEnum.TryGetValue(lang.ToLowerInvariant(), out langEnum))
            {
                return true;
            }
            langEnum = default;
            return false;
        }

        /// <summary>
        /// Produces all FMG file keys for a given base type.
        /// 
        /// This is used in cases where patch FMG files for DLC. The keys
        /// are returned in decreasing order of bnd id, or decreasing order
        /// of priority.
        /// </summary>
        public static bool TryGetBaseFmgKeys(
            FromSoftGame game,
            FmgType baseType,
            FmgLanguage lang,
            out List<SoulsKey.FmgKey> keys)
        {
            if (!TryGetFmgGameInfo(game, out FmgGameInfo gameInfo)
                || !gameInfo.ByType.ContainsKey(baseType)
                || !gameInfo.FromLanguageEnum.ContainsKey(lang))
            {
                keys = null;
                return false;
            }
            keys = new();
            if (gameInfo.Overrides.TryGetValue(baseType, out List<FmgType> overrides))
            {
                foreach (FmgType altType in overrides)
                {
                    keys.Add(new SoulsKey.FmgKey(lang, altType));
                }
            }
            keys.Add(new SoulsKey.FmgKey(lang, baseType));
            return true;
        }

        /// <summary>
        /// Game-specific info for a known FMG file.
        /// </summary>
        public class FmgKeyInfo
        {
            internal FmgKeyInfo(FromSoftGame Game, FmgCategory Category, FmgType Type, FmgType BaseType, string FmgName, int BinderID)
            {
                this.Game = Game;
                this.Category = Category;
                this.Type = Type;
                this.BaseType = BaseType;
                this.FmgName = FmgName;
                this.BinderID = BinderID;
            }

            /// <summary>
            /// The game where this FMG can be found.
            /// </summary>
            public FromSoftGame Game { get; }

            /// <summary>
            /// Which FMG grouping this FMG belongs to, including overall binder file.
            /// </summary>
            public FmgCategory Category { get; }

            /// <summary>
            /// The type designation for this FMG, uniquely identifying it within the game.
            /// </summary>
            public FmgType Type { get; }

            /// <summary>
            /// The base type designation. For DLC or patch files, this is the main file. Otherwise, this matches Type.
            /// </summary>
            public FmgType BaseType { get; }

            /// <summary>
            /// The vanilla name of the FMG file, not including the ".fmg" file extension.
            /// </summary>
            public string FmgName { get; }

            /// <summary>
            /// The binder ID for this FMG, or -1 if it's not in a msgbnd.
            /// </summary>
            public int BinderID { get; }
        }

        internal class FmgGameInfo
        {
            public Dictionary<FmgType, FmgKeyInfo> ByType { get; set; }
            public Dictionary<FmgType, List<FmgType>> Overrides { get; set; }
            public Dictionary<string, List<FmgType>> ByFmgName { get; set; }
            public Dictionary<int, List<FmgType>> ByBinderID { get; set; }
            public Dictionary<string, FmgLanguage> ToLanguageEnum { get; set; }
            public Dictionary<FmgLanguage, string> FromLanguageEnum { get; set; }
        }
    }
}
