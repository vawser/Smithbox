using Andre.Formats;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextEditor.Utils;
using StudioCore.Editors.TextureViewer.Enums;
using StudioCore.Interface;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Utilities;

/// <summary>
/// Alias presentation and display logic
/// </summary>
public static class AliasUtils
{
    public static void AliasTooltip(List<string> aliases, string title)
    {
        var lines = string.Join("\n- ", aliases);
        UIHelper.Tooltip($"{title}\n- {lines}");
    }

    public static void DisplayTagAlias(string aliasName)
    {
        if (aliasName != "")
        {
            ImGui.SameLine();

            if (CFG.Current.System_WrapAliasDisplay)
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

    public static string GetMapNameAlias(ProjectEntry project, string name)
    {
        foreach (var alias in project.Aliases.MapNames)
        {
            if (name == alias.ID)
            {
                return alias.Name;
            }
        }

        return "";
    }

    public static List<string> GetMapTags(ProjectEntry project, string name)
    {
        foreach (var alias in project.Aliases.MapNames)
        {
            if (name == alias.ID)
            {
                return alias.Tags;
            }
        }

        return new List<string>();
    }

    public static string GetCharacterAlias(ProjectEntry project, string name)
    {
        foreach (var alias in project.Aliases.Characters)
        {
            if (name == alias.ID)
            {
                return alias.Name;
            }
        }

        return "";
    }
    public static string GetAssetAlias(ProjectEntry project, string name)
    {
        foreach (var alias in project.Aliases.Assets)
        {
            if (name == alias.ID)
            {
                return alias.Name;
            }
        }

        return "";
    }

    public static string GetPartAlias(ProjectEntry project, string name)
    {
        foreach (var alias in project.Aliases.Parts)
        {
            if (name == alias.ID)
            {
                return alias.Name;
            }
        }

        return "";
    }
    public static string GetMapPieceAlias(ProjectEntry project, string name)
    {
        foreach (var alias in project.Aliases.MapPieces)
        {
            if (name == alias.ID)
            {
                return alias.Name;
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
    public static string GetGparamAliasName(ProjectEntry project, string gparamName)
    {
        var mPrefix = gparamName;
        var sPrefix = gparamName;

        // Get the first 6 chars
        if (gparamName.Length >= 6)
        {
            mPrefix = gparamName.Substring(0, 6); // Map
            sPrefix = gparamName.Substring(0, 6).Replace("s", "m"); // Cutscene
        }

        foreach (var entry in project.Aliases.Gparams)
        {
            // Check for normal entries, and for mXX_XX prefix or sXX_XX prefix
            if (entry.ID == gparamName || entry.ID == mPrefix || entry.ID == sPrefix)
            {
                return entry.Name;
            }
        }

        return "";
    }

    // Texture Viewer
    public static string GetTextureContainerAliasName(ProjectEntry project, string filename, TextureViewCategory curCategory)
    {
        var rawName = filename;
        var usedName = rawName;
        var aliasName = "";

        if (!CFG.Current.TextureViewer_FileList_ShowAliasName_Characters)
        {
            if(curCategory == TextureViewCategory.Characters)
            {
                return aliasName;
            }
        }
        if (!CFG.Current.TextureViewer_FileList_ShowAliasName_Assets)
        {
            if (curCategory == TextureViewCategory.Assets || curCategory == TextureViewCategory.Objects)
            {
                return aliasName;
            }
        }
        if (!CFG.Current.TextureViewer_FileList_ShowAliasName_Parts)
        {
            if (curCategory == TextureViewCategory.Parts)
            {
                return aliasName;
            }
        }

        if (curCategory == TextureViewCategory.Characters)
        {
            if (usedName.Contains("_h"))
            {
                usedName = rawName.Replace("_h", "");
            }
            if (usedName.Contains("_l"))
            {
                usedName = rawName.Replace("_l", "");
            }

            aliasName = GetCharacterAlias(project, usedName);
        }

        if (curCategory == TextureViewCategory.Assets || curCategory == TextureViewCategory.Objects)
        {
            if (usedName.Contains("_l"))
            {
                usedName = rawName.Replace("_l", "");
            }

            if (curCategory == TextureViewCategory.Assets)
            {
                // Convert aet to aeg to match alias list)
                usedName = usedName.Replace("aet", "aeg");
            }

            aliasName = GetAssetAlias(project, usedName);
        }

        if (curCategory == TextureViewCategory.Parts)
        {
            if (usedName.Contains("_l"))
            {
                usedName = rawName.Replace("_l", "");
            }

            aliasName = GetPartAlias(project, usedName);
        }

        return aliasName;

    }

    public static void UpdateEntityAliasName(ProjectEntry project, Entity e)
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
                aliasName = GetCharacterAlias(project, modelName);
                aliasName = $"{aliasName}";
            }

            if (CFG.Current.MapEditor_MapObjectList_ShowAssetNames && (e.IsPartAsset() || e.IsPartDummyAsset()))
            {
                aliasName = GetAssetAlias(project, modelName);
                aliasName = $"{aliasName}";
            }

            if (CFG.Current.MapEditor_MapObjectList_ShowMapPieceNames && e.IsPartMapPiece())
            {
                aliasName = GetMapPieceAlias(project, modelName);
                aliasName = $"{aliasName}";
            }

            // Player/System Characters: peek in param/fmg for name
            if (CFG.Current.MapEditor_MapObjectList_ShowCharacterNames && (e.IsPartEnemy() || e.IsPartDummyEnemy()))
            {
                if (modelName == "c0000")
                {
                    aliasName = FindPlayerCharacterName(project, e, modelName);
                }

                if (modelName == "c0100" || modelName == "c0110" || modelName == "c0120" || modelName == "c1000")
                {
                    aliasName = FindSystemCharacterName(project, e, modelName);
                }
            }

            // Treasure: show itemlot row name
            if (CFG.Current.MapEditor_MapObjectList_ShowTreasureNames && e.IsEventTreasure())
            {
                aliasName = FindTreasureName(project, e);
            }

            e.CachedAliasName = aliasName;
        }
    }

    // Map Editor
    public static string GetEntityAliasName(ProjectEntry project, Entity e)
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
                aliasName = GetCharacterAlias(project, modelName);
                aliasName = $"{aliasName}";
            }

            if (CFG.Current.MapEditor_MapObjectList_ShowAssetNames && (e.IsPartAsset() || e.IsPartDummyAsset()))
            {
                aliasName = GetAssetAlias(project, modelName);
                aliasName = $"{aliasName}";
            }

            if (CFG.Current.MapEditor_MapObjectList_ShowMapPieceNames && e.IsPartMapPiece())
            {
                aliasName = GetMapPieceAlias(project, modelName);
                aliasName = $"{aliasName}";
            }

            // Player/System Characters: peek in param/fmg for name
            if (CFG.Current.MapEditor_MapObjectList_ShowCharacterNames && (e.IsPartEnemy() || e.IsPartDummyEnemy()))
            {
                if (modelName == "c0000")
                {
                    aliasName = FindPlayerCharacterName(project, e, modelName);
                }

                if (modelName == "c0100" || modelName == "c0110" || modelName == "c0120" || modelName == "c1000")
                {
                    aliasName = FindSystemCharacterName(project, e, modelName);
                }
            }

            // Treasure: show itemlot row name
            if (CFG.Current.MapEditor_MapObjectList_ShowTreasureNames && e.IsEventTreasure())
            {
                aliasName = FindTreasureName(project, e);
            }

            e.CachedAliasName = aliasName;
        }
        else
        {
            aliasName = e.CachedAliasName;
        }

        return aliasName;
    }

    public static string FindPlayerCharacterName(ProjectEntry project, Entity e, string modelName)
    {
        if (project.ParamEditor == null)
            return "";

        if (project.TextEditor == null)
            return "";

        var aliasName = "";

        int npcId = e.GetPropertyValue<int>("NPCParamID");
        try
        {
            var param = project.ParamData.PrimaryBank.GetParamFromName("NpcParam");
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

                        var searchValue = int.Parse(term);
                        var textResult = TextFinder.GetTextResult(project.TextEditor, "Title_Characters", searchValue);

                        if (textResult != null)
                        {
                            result = textResult.Entry.Text;
                            nameSuccess = true;
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

    public static string FindSystemCharacterName(ProjectEntry project, Entity e, string modelName)
    {
        if (project.ParamEditor == null)
            return "";

        var aliasName = "";

        int npcId = e.GetPropertyValue<int>("NPCParamID");
        try
        {
            var param = project.ParamData.PrimaryBank.GetParamFromName("NpcParam");
            if (param != null)
            {
                Param.Row row = param[npcId];

                aliasName = $"{row.Name}";
            }
        }
        catch { }

        return aliasName;
    }

    public static string FindTreasureName(ProjectEntry project, Entity e)
    {
        if (project.ParamEditor == null)
            return "";

        var aliasName = "";

        int itemlotId = e.GetPropertyValue<int>("ItemLotID");

        if (project.ProjectType is ProjectType.DS3 or ProjectType.BB)
            itemlotId = e.GetPropertyValue<int>("ItemLot1");

        if (project.ProjectType is ProjectType.DES)
        {
            var treasureObject = (MSBD.Event.Treasure)e.WrappedObject;

            itemlotId = treasureObject.ItemLots[0];
        }

        if (project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
        {
            var treasureObject = (MSB1.Event.Treasure)e.WrappedObject;

            itemlotId = treasureObject.ItemLots[0];
        }

        try
        {
            var paramName = "ItemLotParam";

            if (project.ProjectType == ProjectType.ER)
            {
                paramName = "ItemLotParam_map";
            }

            var param = project.ParamData.PrimaryBank.GetParamFromName(paramName);
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
