using Andre.Formats;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core.Project;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Editors.TextEditor.Utils;
using StudioCore.Editors.TextureViewer.Enums;
using StudioCore.Interface;
using System.Collections.Generic;
using System.Linq;
using static StudioCore.Editors.TextureViewer.TextureFolderBank;

namespace StudioCore.Utilities;

/// <summary>
/// Alias presentation and display logic
/// </summary>
public static class AliasUtils
{
    public static void AliasTooltip(List<string> aliases, string title)
    {
        var lines = string.Join("\n- ", aliases);
        UIHelper.ShowHoverTooltip($"{title}\n- {lines}");
    }

    public static void DisplayTagAlias(string aliasName)
    {
        if (aliasName != "")
        {
            ImGui.SameLine();

            if (UI.Current.System_WrapAliasDisplay)
            {
                ImGui.PushTextWrapPos();
                ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, @$"[{aliasName}]");
                ImGui.PopTextWrapPos();
            }
            else
            {
                ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, @$"[{aliasName}]");
            }
        }
    }

    public static Dictionary<string, string> CharacterNameAliasCache = new Dictionary<string, string>();
    public static Dictionary<string, string> AssetNameAliasCache = new Dictionary<string, string>();
    public static Dictionary<string, string> PartNameAliasCache = new Dictionary<string, string>();
    public static Dictionary<string, string> MapPieceNameAliasCache = new Dictionary<string, string>();
    public static Dictionary<string, string> MapNameAliasCache = new Dictionary<string, string>();
    public static Dictionary<string, List<string>> MapTagAliasCache = new Dictionary<string, List<string>>();

    public static string GetMapNameAlias(string name)
    {
        if (MapNameAliasCache.ContainsKey(name))
        {
            return MapNameAliasCache[name];
        }
        else
        {

            foreach (var alias in Smithbox.BankHandler.MapAliases.Aliases.list)
            {
                if (name == alias.id)
                {
                    MapNameAliasCache.Add(name, alias.name);
                    return alias.name;
                }
            }

        }

        MapNameAliasCache.Add(name, "");
        return "";
    }

    public static List<string> GetMapTags(string name)
    {
        if (MapTagAliasCache.ContainsKey(name))
        {
            return MapTagAliasCache[name];
        }
        else
        {

            foreach (var alias in Smithbox.BankHandler.MapAliases.Aliases.list)
            {
                if (name == alias.id)
                {
                    MapTagAliasCache.Add(name, alias.tags);
                    return alias.tags;
                }
            }

        }

        MapTagAliasCache.Add(name, new List<string>());
        return new List<string>();
    }

    public static string GetCharacterAlias(string name)
    {
        if (CharacterNameAliasCache.ContainsKey(name))
        {
            return CharacterNameAliasCache[name];
        }
        else
        {
            foreach (var alias in Smithbox.BankHandler.CharacterAliases.Aliases.list)
            {
                if (name == alias.id)
                {
                    CharacterNameAliasCache.Add(name, alias.name);
                    return alias.name;
                }
            }
        }

        CharacterNameAliasCache.Add(name, "");
        return "";
    }
    public static string GetAssetAlias(string name)
    {
        if (AssetNameAliasCache.ContainsKey(name))
        {
            return AssetNameAliasCache[name];
        }
        else
        {
            foreach (var alias in Smithbox.BankHandler.AssetAliases.Aliases.list)
            {
                if (name == alias.id)
                {
                    AssetNameAliasCache.Add(name, alias.name);
                    return alias.name;
                }
            }
        }

        AssetNameAliasCache.Add(name, "");
        return "";
    }
    public static string GetPartAlias(string name)
    {
        if (PartNameAliasCache.ContainsKey(name))
        {
            return PartNameAliasCache[name];
        }
        else
        {
            foreach (var alias in Smithbox.BankHandler.PartAliases.Aliases.list)
            {
                if (name == alias.id)
                {
                    PartNameAliasCache.Add(name, alias.name);
                    return alias.name;
                }
            }
        }

        PartNameAliasCache.Add(name, "");
        return "";
    }
    public static string GetMapPieceAlias(string name)
    {
        if (MapPieceNameAliasCache.ContainsKey(name))
        {
            return MapPieceNameAliasCache[name];
        }
        else
        {
            foreach (var alias in Smithbox.BankHandler.MapPieceAliases.Aliases.list)
            {
                if (name == alias.id)
                {
                    MapPieceNameAliasCache.Add(name, alias.name);
                    return alias.name;
                }
            }
        }

        MapPieceNameAliasCache.Add(name, "");
        return "";
    }

    public static string GetTagListString(List<string> refTagList)
    {
        var tagListStr = "";

        if (refTagList.Count > 0)
        {
            var tagStr = refTagList[0];
            foreach (var entry in refTagList.Skip(1))
            {
                tagStr = $"{tagStr},{entry}";
            }
            tagListStr = tagStr;
        }
        else
        {
            tagListStr = "";
        }

        return tagListStr;
    }

    public static List<string> GetTagList(string tags)
    {
        var list = new List<string>();

        if (tags.Contains(","))
        {
            list = tags.Split(',').ToList();
        }
        else
        {
            list.Add(tags);
        }

        return list;
    }

    // Gparam Editor
    public static string GetGparamAliasName(string gparamName)
    {
        var mPrefix = gparamName;
        var sPrefix = gparamName;

        // Get the first 6 chars
        if (gparamName.Length >= 6)
        {
            mPrefix = gparamName.Substring(0, 6); // Map
            sPrefix = gparamName.Substring(0, 6).Replace("s", "m"); // Cutscene
        }

        if (Smithbox.BankHandler.GparamAliases.Aliases.list != null)
        {
            foreach (var entry in Smithbox.BankHandler.GparamAliases.Aliases.list)
            {
                // Check for normal entries, and for mXX_XX prefix or sXX_XX prefix
                if (entry.id == gparamName || entry.id == mPrefix || entry.id == sPrefix)
                {
                    return entry.name;
                }
            }
        }

        return "";
    }

    // Texture Viewer
    public static string GetTextureContainerAliasName(TextureViewInfo info)
    {
        var rawName = info.Name;
        var usedName = rawName;
        var aliasName = "";

        if (!CFG.Current.TextureViewer_FileList_ShowAliasName_Characters)
        {
            if (info.Category == TextureViewCategory.Character)
            {
                return aliasName;
            }
        }
        if (!CFG.Current.TextureViewer_FileList_ShowAliasName_Assets)
        {
            if (info.Category == TextureViewCategory.Asset || info.Category == TextureViewCategory.Object)
            {
                return aliasName;
            }
        }
        if (!CFG.Current.TextureViewer_FileList_ShowAliasName_Parts)
        {
            if (info.Category == TextureViewCategory.Part)
            {
                return aliasName;
            }
        }

        if (info.CachedName == null)
        {
            if (info.Category == TextureViewCategory.Character)
            {
                if (usedName.Contains("_h"))
                {
                    usedName = rawName.Replace("_h", "");
                }
                if (usedName.Contains("_l"))
                {
                    usedName = rawName.Replace("_l", "");
                }

                if (Smithbox.BankHandler.CharacterAliases.Aliases != null)
                {
                    aliasName = GetCharacterAlias(usedName);
                }
            }

            if (info.Category == TextureViewCategory.Asset || info.Category == TextureViewCategory.Object)
            {
                if (usedName.Contains("_l"))
                {
                    usedName = rawName.Replace("_l", "");
                }

                if (info.Category == TextureViewCategory.Asset)
                {
                    // Convert aet to aeg to match alias list)
                    usedName = usedName.Replace("aet", "aeg");
                }

                if (Smithbox.BankHandler.AssetAliases.Aliases != null)
                {
                    aliasName = GetAssetAlias(usedName);
                }
            }

            if (info.Category == TextureViewCategory.Part)
            {
                if (usedName.Contains("_l"))
                {
                    usedName = rawName.Replace("_l", "");
                }

                if (Smithbox.BankHandler.PartAliases.Aliases != null)
                {
                    aliasName = GetPartAlias(usedName);
                }
            }

            info.CachedName = aliasName;
        }
        else
        {
            aliasName = info.CachedName;
        }

        return aliasName;

    }

    public static void UpdateEntityAliasName(Entity e)
    {
        var aliasName = "";
        var modelName = "";

        if (e.IsPart())
        {
            modelName = e.GetPropertyValue<string>("ModelName");
            if (modelName == null)
            {
                return;
            }

            modelName = modelName.ToLower();
        }

        // Only grab the alias once, then refer to the cachedName within the entity
        if (e.CachedAliasName == null)
        {
            if (CFG.Current.MapEditor_MapObjectList_ShowCharacterNames && (e.IsPartEnemy() || e.IsPartDummyEnemy()))
            {
                if (Smithbox.BankHandler.CharacterAliases.Aliases.list != null)
                {
                    aliasName = GetCharacterAlias(modelName);
                    aliasName = $"{aliasName}";
                }
            }

            if (CFG.Current.MapEditor_MapObjectList_ShowAssetNames && (e.IsPartAsset() || e.IsPartDummyAsset()))
            {
                if (Smithbox.BankHandler.AssetAliases.Aliases.list != null)
                {
                    aliasName = GetAssetAlias(modelName);
                    aliasName = $"{aliasName}";
                }
            }

            if (CFG.Current.MapEditor_MapObjectList_ShowMapPieceNames && e.IsPartMapPiece())
            {
                if (Smithbox.BankHandler.MapPieceAliases.Aliases.list != null)
                {
                    aliasName = GetMapPieceAlias(modelName);
                    aliasName = $"{aliasName}";
                };
            }

            // Player/System Characters: peek in param/fmg for name
            if (CFG.Current.MapEditor_MapObjectList_ShowCharacterNames && (e.IsPartEnemy() || e.IsPartDummyEnemy()))
            {
                if (modelName == "c0000")
                {
                    aliasName = FindPlayerCharacterName(e, modelName);
                }

                if (modelName == "c0100" || modelName == "c0110" || modelName == "c0120" || modelName == "c1000")
                {
                    aliasName = FindSystemCharacterName(e, modelName);
                }
            }

            // Treasure: show itemlot row name
            if (CFG.Current.MapEditor_MapObjectList_ShowTreasureNames && e.IsEventTreasure())
            {
                aliasName = FindTreasureName(e);
            }

            e.CachedAliasName = aliasName;
        }
    }

    // Map Editor
    public static string GetEntityAliasName(Entity e)
    {
        var aliasName = "";
        var modelName = "";

        // Early returns if the show X vars are disabled
        if (!CFG.Current.MapEditor_MapObjectList_ShowCharacterNames && (e.IsPartEnemy() || e.IsPartDummyEnemy()))
            return aliasName;

        if (!CFG.Current.MapEditor_MapObjectList_ShowAssetNames && (e.IsPartAsset() || e.IsPartDummyAsset()))
            return aliasName;

        if (!CFG.Current.MapEditor_MapObjectList_ShowMapPieceNames && e.IsPartMapPiece())
            return aliasName;

        if (!CFG.Current.MapEditor_MapObjectList_ShowTreasureNames && e.IsEventTreasure())
            return aliasName;

        if (e.IsPart())
        {
            modelName = e.GetPropertyValue<string>("ModelName");
            if (modelName == null)
            {
                return "";
            }

            modelName = modelName.ToLower();
        }

        // Only grab the alias once, then refer to the cachedName within the entity
        if (e.CachedAliasName == null)
        {
            if (CFG.Current.MapEditor_MapObjectList_ShowCharacterNames && (e.IsPartEnemy() || e.IsPartDummyEnemy()))
            {
                if (Smithbox.BankHandler.CharacterAliases.Aliases.list != null)
                {
                    aliasName = GetCharacterAlias(modelName);
                    aliasName = $"{aliasName}";
                }
            }

            if (CFG.Current.MapEditor_MapObjectList_ShowAssetNames && (e.IsPartAsset() || e.IsPartDummyAsset()))
            {
                if (Smithbox.BankHandler.AssetAliases.Aliases.list != null)
                {
                    aliasName = GetAssetAlias(modelName);
                    aliasName = $"{aliasName}";
                }
            }

            if (CFG.Current.MapEditor_MapObjectList_ShowMapPieceNames && e.IsPartMapPiece())
            {
                if (Smithbox.BankHandler.MapPieceAliases.Aliases.list != null)
                {
                    aliasName = GetMapPieceAlias(modelName);
                    aliasName = $"{aliasName}";
                };
            }

            // Player/System Characters: peek in param/fmg for name
            if (CFG.Current.MapEditor_MapObjectList_ShowCharacterNames && (e.IsPartEnemy() || e.IsPartDummyEnemy()))
            {
                if (modelName == "c0000")
                {
                    aliasName = FindPlayerCharacterName(e, modelName);
                }

                if (modelName == "c0100" || modelName == "c0110" || modelName == "c0120" || modelName == "c1000")
                {
                    aliasName = FindSystemCharacterName(e, modelName);
                }
            }

            // Treasure: show itemlot row name
            if (CFG.Current.MapEditor_MapObjectList_ShowTreasureNames && e.IsEventTreasure())
            {
                aliasName = FindTreasureName(e);
            }

            e.CachedAliasName = aliasName;
        }
        else
        {
            aliasName = e.CachedAliasName;
        }

        return aliasName;
    }

    public static string FindPlayerCharacterName(Entity e, string modelName)
    {
        var aliasName = "";

        int npcId = e.GetPropertyValue<int>("NPCParamID");
        try
        {
            var param = ParamBank.PrimaryBank.GetParamFromName("NpcParam");
            if (param != null)
            {
                Param.Row row = param[npcId];

                if (row != null)
                {
                    bool nameSuccess = false;

                    // Try Name ID first
                    Param.Cell? cq = row["nameId"];
                    if (cq != null)
                    {
                        Param.Cell c = cq.Value;
                        var term = c.Value.ToParamEditorString();
                        var result = term;

                        if (TextBank.PrimaryBankLoaded)
                        {
                            var searchValue = int.Parse(term);
                            var textResult = TextFinder.GetTextResult("Title_Characters", searchValue);

                            if (textResult != null)
                            {
                                result = textResult.Entry.Text;
                                nameSuccess = true;
                            }
                        }

                        aliasName = $"{result}";
                    }

                    // Try Row Name instead if Name ID is not used
                    if (!nameSuccess)
                    {
                        aliasName = $"{row.Name}";
                    }
                }
            }
        }
        catch { }

        return aliasName;
    }

    public static string FindSystemCharacterName(Entity e, string modelName)
    {
        var aliasName = "";

        int npcId = e.GetPropertyValue<int>("NPCParamID");
        try
        {
            var param = ParamBank.PrimaryBank.GetParamFromName("NpcParam");
            if (param != null)
            {
                Param.Row row = param[npcId];

                aliasName = $"{row.Name}";
            }
        }
        catch { }

        return aliasName;
    }

    public static string FindTreasureName(Entity e)
    {
        var aliasName = "";

        int itemlotId = e.GetPropertyValue<int>("ItemLotID");

        if (Smithbox.ProjectType is ProjectType.DS3 or ProjectType.BB)
            itemlotId = e.GetPropertyValue<int>("ItemLot1");

        if (Smithbox.ProjectType is ProjectType.DES)
        {
            var treasureObject = (MSBD.Event.Treasure)e.WrappedObject;

            itemlotId = treasureObject.ItemLots[0];
        }

        if (Smithbox.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
        {
            var treasureObject = (MSB1.Event.Treasure)e.WrappedObject;

            itemlotId = treasureObject.ItemLots[0];
        }

        try
        {
            var paramName = "ItemLotParam";

            if (Smithbox.ProjectType == ProjectType.ER)
            {
                paramName = "ItemLotParam_map";
            }

            var param = ParamBank.PrimaryBank.GetParamFromName(paramName);
            if (param != null)
            {
                Param.Row row = param[itemlotId];

                if (row != null)
                {
                    aliasName = $"{row.Name}";
                }
            }
        }
        catch { }

        return aliasName;
    }
}
