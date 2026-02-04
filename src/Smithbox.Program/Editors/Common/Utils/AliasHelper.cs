using Andre.Formats;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.TextEditor;
using StudioCore.Editors.TextureViewer;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.Common;

/// <summary>
/// Alias presentation and display logic
/// </summary>
public static class AliasHelper
{
    public static string GetAliasName(List<AliasEntry> aliases, string id)
    {
        var alias = aliases.FirstOrDefault(e => e.ID == id);
        return alias?.Name ?? string.Empty;
    }

    public static string GetAliasName(this ProjectEntry project, ProjectAliasType aliasType, string id)
    {
        if (!project.Handler.ProjectData.Aliases.TryGetValue(aliasType, out var aliases)) return string.Empty;
        var alias = aliases.FirstOrDefault(e => e.ID == id);
        return alias?.Name ?? string.Empty;
    }

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

            if (CFG.Current.Interface_Alias_Wordwrap_General)
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
        project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.MapNames, out var aliases);
        return aliases?.FirstOrDefault(e => e.ID == name)?.Name ?? string.Empty;
    }

    public static List<string> GetMapTags(ProjectEntry project, string name)
    {
        project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.MapNames, out var aliases);

        var alias = aliases?.FirstOrDefault(e => e.ID == name);
        if (alias != null)
            return alias.Tags;

        return new List<string>();
    }

    public static string GetCharacterAlias(ProjectEntry project, string name)
    {
        project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.Characters, out var aliases);
        return aliases?.FirstOrDefault(e => e.ID == name)?.Name ?? string.Empty;
    }
    public static string GetAssetAlias(ProjectEntry project, string name)
    {
        project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.Assets, out var aliases);
        return aliases?.FirstOrDefault(e => e.ID == name)?.Name ?? string.Empty;
    }

    public static string GetPartAlias(ProjectEntry project, string name)
    {
        project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.Parts, out var aliases);
        return aliases?.FirstOrDefault(e => e.ID == name)?.Name ?? string.Empty;
    }
    public static string GetMapPieceAlias(ProjectEntry project, string name)
    {
        project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.MapPieces, out var aliases);
        return aliases?.FirstOrDefault(e => e.ID == name)?.Name ?? string.Empty;
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
        if (!project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.Gparams, out var aliases)) return string.Empty;

        var mPrefix = gparamName;
        var sPrefix = gparamName;

        // Get the first 6 chars
        if (gparamName.Length >= 6)
        {
            mPrefix = gparamName.Substring(0, 6); // Map
            sPrefix = gparamName.Substring(0, 6).Replace("s", "m"); // Cutscene
        }

        foreach (var entry in aliases)
        {
            // Check for normal entries, and for mXX_XX prefix or sXX_XX prefix
            if (entry.ID == gparamName || entry.ID == mPrefix || entry.ID == sPrefix)
            {
                return entry.Name;
            }
        }

        return string.Empty;
    }

    // Texture Viewer
    public static string GetTextureContainerAliasName(ProjectEntry project, string filename, TextureViewCategory curCategory)
    {
        var rawName = filename;
        var usedName = rawName;
        var aliasName = "";

        if (!CFG.Current.TextureViewer_File_List_Display_Character_Aliases)
        {
            if(curCategory == TextureViewCategory.Characters)
            {
                return aliasName;
            }
        }
        if (!CFG.Current.TextureViewer_File_List_Display_Asset_Aliases)
        {
            if (curCategory == TextureViewCategory.Assets || curCategory == TextureViewCategory.Objects)
            {
                return aliasName;
            }
        }
        if (!CFG.Current.TextureViewer_File_List_Display_Part_Aliases)
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
            if (CFG.Current.MapEditor_Map_Contents_Display_Character_Aliases && (e.IsPartEnemy() || e.IsPartDummyEnemy()))
            {
                aliasName = GetCharacterAlias(project, modelName);
                aliasName = $"{aliasName}";
            }

            if (CFG.Current.MapEditor_Map_Contents_Display_Asset_Aliases && (e.IsPartAsset() || e.IsPartDummyAsset()))
            {
                aliasName = GetAssetAlias(project, modelName);
                aliasName = $"{aliasName}";
            }

            if (CFG.Current.MapEditor_Map_Contents_Display_Map_Piece_Aliases && e.IsPartMapPiece())
            {
                aliasName = GetMapPieceAlias(project, modelName);
                aliasName = $"{aliasName}";
            }

            // Player/System Characters: peek in param/fmg for name
            if (CFG.Current.MapEditor_Map_Contents_Display_Character_Aliases && (e.IsPartEnemy() || e.IsPartDummyEnemy()))
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
            if (CFG.Current.MapEditor_Map_Contents_Display_Treasure_Aliases && e.IsEventTreasure())
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
        if (!CFG.Current.MapEditor_Map_Contents_Display_Character_Aliases && (e.IsPartEnemy() || e.IsPartDummyEnemy()))
            return aliasName;

        if (!CFG.Current.MapEditor_Map_Contents_Display_Asset_Aliases && (e.IsPartAsset() || e.IsPartDummyAsset()))
            return aliasName;

        if (!CFG.Current.MapEditor_Map_Contents_Display_Map_Piece_Aliases && e.IsPartMapPiece())
            return aliasName;

        if (!CFG.Current.MapEditor_Map_Contents_Display_Treasure_Aliases && e.IsEventTreasure())
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
            if (CFG.Current.MapEditor_Map_Contents_Display_Character_Aliases && (e.IsPartEnemy() || e.IsPartDummyEnemy()))
            {
                aliasName = GetCharacterAlias(project, modelName);
                aliasName = $"{aliasName}";
            }

            if (CFG.Current.MapEditor_Map_Contents_Display_Asset_Aliases && (e.IsPartAsset() || e.IsPartDummyAsset()))
            {
                aliasName = GetAssetAlias(project, modelName);
                aliasName = $"{aliasName}";
            }

            if (CFG.Current.MapEditor_Map_Contents_Display_Map_Piece_Aliases && e.IsPartMapPiece())
            {
                aliasName = GetMapPieceAlias(project, modelName);
                aliasName = $"{aliasName}";
            }

            // Player/System Characters: peek in param/fmg for name
            if (CFG.Current.MapEditor_Map_Contents_Display_Character_Aliases && (e.IsPartEnemy() || e.IsPartDummyEnemy()))
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
            if (CFG.Current.MapEditor_Map_Contents_Display_Treasure_Aliases && e.IsEventTreasure())
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
        if (project.Handler.ParamEditor == null)
            return "";

        if (project.Handler.TextEditor == null)
            return "";

        var activeView = project.Handler.TextEditor.ViewHandler.ActiveView;

        if (activeView == null)
            return "";

        var aliasName = "";

        int npcId = e.GetPropertyValue<int>("NPCParamID");
        try
        {
            var param = project.Handler.ParamData.PrimaryBank.GetParamFromName("NpcParam");
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
                        var textResult = TextFinder.GetTextResult(activeView, "Title_Characters", searchValue);

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
        if (project.Handler.ParamEditor == null)
            return "";

        var aliasName = "";

        int npcId = e.GetPropertyValue<int>("NPCParamID");
        try
        {
            var param = project.Handler.ParamData.PrimaryBank.GetParamFromName("NpcParam");
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
        if (project.Handler.ParamEditor == null)
            return "";

        var aliasName = "";

        int itemlotId = e.GetPropertyValue<int>("ItemLotID");

        if (project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB)
            itemlotId = e.GetPropertyValue<int>("ItemLot1");

        if (project.Descriptor.ProjectType is ProjectType.DES)
        {
            var treasureObject = (MSBD.Event.Treasure)e.WrappedObject;

            itemlotId = treasureObject.ItemLots[0];
        }

        if (project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
        {
            var treasureObject = (MSB1.Event.Treasure)e.WrappedObject;

            itemlotId = treasureObject.ItemLots[0];
        }

        try
        {
            var paramName = "ItemLotParam";

            if (project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
            {
                paramName = "ItemLotParam_map";
            }

            var param = project.Handler.ParamData.PrimaryBank.GetParamFromName(paramName);
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
