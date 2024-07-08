using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Prefabs;

public static class PrefabUtils
{
    public static string GetMapPieceAliasName(string modelName)
    {
        if (Smithbox.BankHandler.MapPieceAliases.Aliases == null)
            return modelName;

        if (Smithbox.BankHandler.MapPieceAliases.Aliases.list == null)
            return modelName;

        string fullname = modelName;

        foreach (var entry in Smithbox.BankHandler.MapPieceAliases.Aliases.list)
        {
            if (modelName == entry.id)
            {
                fullname = $"{modelName} <{entry.name}>";
            }
        }

        return fullname;
    }
    public static string GetCharacterAliasName(string modelName)
    {
        if (Smithbox.BankHandler.CharacterAliases.Aliases == null)
            return modelName;

        if (Smithbox.BankHandler.CharacterAliases.Aliases.list == null)
            return modelName;

        string fullname = modelName;

        foreach (var entry in Smithbox.BankHandler.CharacterAliases.Aliases.list)
        {
            if (modelName == entry.id)
            {
                fullname = $"{modelName} <{entry.name}>";
            }
        }

        return fullname;
    }
    public static string GetAssetAliasName(string modelName)
    {
        if (Smithbox.BankHandler.AssetAliases.Aliases == null)
            return modelName;

        if (Smithbox.BankHandler.AssetAliases.Aliases.list == null)
            return modelName;

        string fullname = modelName;

        foreach (var entry in Smithbox.BankHandler.AssetAliases.Aliases.list)
        {
            if (modelName == entry.id)
            {
                fullname = $"{modelName} <{entry.name}>";
            }
        }

        return fullname;
    }
}
