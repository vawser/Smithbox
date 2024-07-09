using Andre.Formats;
using ImGuiNET;
using StudioCore.Banks.AliasBank;
using StudioCore.Core;
using StudioCore.Editors.AssetBrowser;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.TextEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using static StudioCore.Editors.TextureViewer.TextureFolderBank;

namespace StudioCore.Interface;

/// <summary>
/// Alias presentation and display logic
/// </summary>
public static class AliasUtils
{
    public static void DisplayAlias(string aliasName)
    {
        if (aliasName != "")
        {
            ImGui.SameLine();
            if (CFG.Current.System_WrapAliasDisplay)
            {
                ImGui.PushTextWrapPos();
                ImGui.TextColored(CFG.Current.ImGui_AliasName_Text, @$"{aliasName}");
                ImGui.PopTextWrapPos();
            }
            else
            {
                ImGui.TextColored(CFG.Current.ImGui_AliasName_Text, @$"{aliasName}");
            }
        }
    }
    public static void DisplayTagAlias(string aliasName)
    {
        if (aliasName != "")
        {
            ImGui.SameLine();

            if (CFG.Current.System_WrapAliasDisplay)
            {
                ImGui.PushTextWrapPos();
                ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, @$"[{aliasName}]");
                ImGui.PopTextWrapPos();
            }
            else
            {
                ImGui.TextColored(CFG.Current.ImGui_Benefit_Text_Color, @$"[{aliasName}]");
            }
        }
    }

    public static string GetAliasFromCache(string name, List<AliasReference> referenceList)
    {
        foreach (var alias in referenceList)
        {
            if (name == alias.id)
            {
                return alias.name;
            }
        }

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

    public static List<String> GetTagList(string tags)
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

    // Asset Browser
    public static List<AliasReference> GetAliasReferenceList(AssetCategoryType category)
    {
        switch(category)
        {
            case AssetCategoryType.Character:
                return Smithbox.BankHandler.CharacterAliases.Aliases.list;
            case AssetCategoryType.Asset:
                return Smithbox.BankHandler.AssetAliases.Aliases.list;
            case AssetCategoryType.Part:
                return Smithbox.BankHandler.PartAliases.Aliases.list;
            case AssetCategoryType.MapPiece:
                return Smithbox.BankHandler.MapPieceAliases.Aliases.list;
        }

        return null;
    }

    public static string GetAssetBrowserAliasName(AssetCategoryType category, string rawName)
    {
        var aliasName = rawName;

        return aliasName;
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

                if(Smithbox.BankHandler.CharacterAliases.Aliases != null)
                {
                    aliasName = GetAliasFromCache(usedName, Smithbox.BankHandler.CharacterAliases.Aliases.list);
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
                    aliasName = GetAliasFromCache(usedName, Smithbox.BankHandler.AssetAliases.Aliases.list);
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
                    aliasName = GetAliasFromCache(usedName, Smithbox.BankHandler.PartAliases.Aliases.list);
                }
            }

            /*
            if (info.Category == TextureViewCategory.Particle)
            {
                aliasName = GetAliasFromCache(usedName, ParticleAliasBank.Bank.AliasNames.GetEntries("Particles"));
            }
            */

            info.CachedName = aliasName;
        }
        else
        {
            aliasName = info.CachedName;
        }

        return aliasName;

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
                if(Smithbox.BankHandler.CharacterAliases.Aliases.list != null)
                {
                    aliasName = GetAliasFromCache(modelName, Smithbox.BankHandler.CharacterAliases.Aliases.list);
                    aliasName = $"{aliasName}";
                }
            }

            if (CFG.Current.MapEditor_MapObjectList_ShowAssetNames && (e.IsPartAsset() || e.IsPartDummyAsset()))
            {
                if (Smithbox.BankHandler.AssetAliases.Aliases.list != null)
                {
                    aliasName = GetAliasFromCache(modelName, Smithbox.BankHandler.AssetAliases.Aliases.list);
                    aliasName = $"{aliasName}";
                }
            }

            if (CFG.Current.MapEditor_MapObjectList_ShowMapPieceNames && e.IsPartMapPiece())
            {
                if (Smithbox.BankHandler.MapPieceAliases.Aliases.list != null)
                {
                    aliasName = GetAliasFromCache(modelName, Smithbox.BankHandler.MapPieceAliases.Aliases.list);
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
                    bool nameSucces = false;

                    // Try Name ID first
                    Param.Cell? cq = row["nameId"];
                    if (cq != null)
                    {
                        Param.Cell c = cq.Value;
                        var term = c.Value.ToParamEditorString();
                        var result = term;

                        if (Smithbox.BankHandler.FMGBank.IsLoaded)
                        {
                            var matchingFmgInfo = Smithbox.BankHandler.FMGBank.FmgInfoBank.ToList().Find(x => x.Name.Contains("Character"));

                            if (matchingFmgInfo != null)
                            {
                                foreach (var entry in matchingFmgInfo.Fmg.Entries)
                                {
                                    if (entry.ID == int.Parse(term))
                                    {
                                        result = entry.Text;
                                        nameSucces = true;
                                        break;
                                    }
                                }
                            }
                        }

                        aliasName = $"{result}";
                    }

                    // Try Row Name instead if Name ID is not used
                    if (!nameSucces)
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

        if(Smithbox.ProjectType == ProjectType.DS3)
            itemlotId = e.GetPropertyValue<int>("ItemLot1");

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
