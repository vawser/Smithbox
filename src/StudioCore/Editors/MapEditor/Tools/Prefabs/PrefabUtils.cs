using SoulsFormats;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor.Tools.Prefabs;

public static class PrefabUtils
{
    public static string GetMapPieceAliasName(string modelName)
    {
        string fullname = modelName;

        if (Smithbox.BankHandler.MapPieceAliases.Aliases != null)
        {
            foreach (var entry in Smithbox.BankHandler.MapPieceAliases.Aliases.list)
            {
                if (modelName == entry.id)
                {
                    fullname = $"{modelName} <{entry.name}>";
                }
            }
        }

        return fullname;
    }
    public static string GetCharacterAliasName(string modelName)
    {
        string fullname = modelName;

        if (Smithbox.BankHandler.CharacterAliases.Aliases != null)
        {
            foreach (var entry in Smithbox.BankHandler.CharacterAliases.Aliases.list)
            {
                if (modelName == entry.id)
                {
                    fullname = $"{modelName} <{entry.name}>";
                }
            }
        }

        return fullname;
    }
    public static string GetAssetAliasName(string modelName)
    {
        string fullname = modelName;

        if (Smithbox.BankHandler.AssetAliases.Aliases != null)
        {
            foreach (var entry in Smithbox.BankHandler.AssetAliases.Aliases.list)
            {
                if (modelName == entry.id)
                {
                    fullname = $"{modelName} <{entry.name}>";
                }
            }
        }

        return fullname;
    }

    public static IEnumerable<IMsbEntry> GetMapMsbEntries(IMsb map)
    {
        return new IEnumerable<IMsbEntry>[] {
                map.Parts.GetEntries(),
                map.Events.GetEntries(),
                map.Regions.GetEntries(),
            }
            .SelectMany(e => e);
    }
}
