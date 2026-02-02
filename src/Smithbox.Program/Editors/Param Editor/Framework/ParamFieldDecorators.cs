using Andre.Formats;
using Hexa.NET.ImGui;
using Hexa.NET.ImPlot;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.TextEditor;
using StudioCore.Interface;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.ParamEditor;
public class ParamFieldDecorators
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;
    public ParamView ParentView;

    public ParamFieldDecorators(ParamEditorScreen editor, ProjectEntry project, ParamView curView)
    {
        Editor = editor;
        Project = project;
        ParentView = curView;
    }

    public void HandleLabels(FieldMetaContext metaContext, Param.Row row)
    {
        if (metaContext.HasAnyDisplayedElements())
        {
            ImGui.BeginGroup();

            // Param reference label
            if (metaContext.DisplayParamReference)
            {
                ParamReferenceHelper.Label(ParentView, metaContext.ParamReferences, row);
            }

            // FMG reference label
            if (metaContext.DisplayTextReference)
            {
                TextReferenceHelper.Label(ParentView, metaContext.TextReferences, row);
            }

            // Map FMG reference label
            if (metaContext.DisplayMapTextReference)
            {
                TextReferenceHelper.Label(ParentView, metaContext.MapTextReferences, row, "MAP FMGS");
            }

            // Texture reference label
            if (metaContext.DisplayTextureReference)
            {
                TextureReferenceHelper.Label(ParentView, metaContext.IconDisplayData, row);
            }

            // Enum label
            if (metaContext.DisplayEnum)
            {
                EnumHelper.Label(ParentView, metaContext.Enum);
            }

            // Particle list
            if (metaContext.DisplayParticleEnum)
            {
                AliasEnumHelper.Label(ParentView, "PARTICLES");
            }

            // Sound list
            if (metaContext.DisplaySoundEnum)
            {
                AliasEnumHelper.Label(ParentView, "SOUNDS");
            }

            // Flag list
            if (metaContext.DisplayEventFlagEnum)
            {
                ConditionalAliasEnumHelper.Label(ParentView, "FLAGS", row,
                    metaContext.EventFlagConditionalField, metaContext.EventFlagConditionalValue);
            }

            // Cutscene list
            if (metaContext.DisplayCutsceneEnum)
            {
                AliasEnumHelper.Label(ParentView, "CUTSCENES");
            }

            // Movie list
            if (metaContext.DisplayMovieEnum)
            {
                ConditionalAliasEnumHelper.Label(ParentView, "MOVIES", row,
                    metaContext.MovieConditionalField, metaContext.MovieConditionalValue);
            }

            // Character list
            if (metaContext.DisplayCharacterEnum)
            {
                AliasEnumHelper.Label(ParentView, "CHARACTERS");
            }

            // Project Enum
            if (metaContext.DisplayProjectEnum)
            {
                ProjectEnumHelper.Label(ParentView, metaContext.FieldMeta.ProjectEnumType);
            }

            // Tile reference
            if (metaContext.DisplayTileReference)
            {
                TileReferenceHelper.Label(ParentView, metaContext.FieldMeta.TileRef);
            }

            // AC6 Field Offset
            if (metaContext.DisplayAC6FieldOffsetData)
            {
                AC6_FieldOffsetHelper.Label(ParentView, metaContext.ActiveParam, row, metaContext.AC6FieldOffsetIndex);
            }

            ImGui.EndGroup();

            if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
            {
                if (CFG.Current.ParamEditor_Field_Context_Split)
                {
                    ImGui.OpenPopup("ParamRowNameMenu");
                }
                else
                {
                    ImGui.OpenPopup("ParamRowCommonMenu");
                }
            }
        }
    }

    public void HandleHints(FieldMetaContext metaContext, Param.Row row, string activeParam, string internalName, object oldval)
    {
        if (metaContext.HasAnyDisplayedElements())
        {
            ImGui.BeginGroup();

            // Param reference
            if (metaContext.DisplayParamReference)
            {
                ParamReferenceHelper.Hint(ParentView, metaContext.ParamReferences, row, oldval);
            }

            // FMG reference
            if (metaContext.DisplayTextReference)
            {
                TextReferenceHelper.Hint(ParentView, metaContext.TextReferences, row, oldval);
                // Restore metaContext.FmgRefRoleOverride on FMG Descriptor impl
            }

            // Map FMG reference
            if (metaContext.DisplayMapTextReference)
            {
                TextReferenceHelper.Hint(ParentView, metaContext.MapTextReferences, row, oldval);
                // Restore metaContext.FmgRefRoleOverride on FMG Descriptor impl
            }

            // Texture reference
            if (metaContext.DisplayTextureReference)
            {
                TextureReferenceHelper.Hint(ParentView, metaContext.IconDisplayData, row, oldval, internalName, 0);
            }

            // Enum label
            if (metaContext.DisplayEnum)
            {
                EnumHelper.Hint(ParentView, metaContext.Enum.Values, oldval.ToString());
            }

            // Particle list
            if (metaContext.DisplayParticleEnum)
            {
                if (Editor.Project.Handler.ProjectData.Aliases.TryGetValue(
                    ProjectAliasType.Particles, out List<AliasEntry> particles))
                {
                    AliasEnumHelper.Hint(ParentView, particles, oldval.ToString());
                }
            }

            // Sound list
            if (metaContext.DisplaySoundEnum)
            {
                if (Editor.Project.Handler.ProjectData.Aliases.TryGetValue(
                    ProjectAliasType.Sounds, out List<AliasEntry> sounds))
                {
                    AliasEnumHelper.Hint(ParentView, sounds, oldval.ToString());
                }
            }

            // Event Flag list
            if (metaContext.DisplayEventFlagEnum)
            {
                if (Editor.Project.Handler.ProjectData.Aliases.TryGetValue(
                    ProjectAliasType.EventFlags, out List<AliasEntry> eventFlags))
                {
                    ConditionalAliasEnumHelper.Hint(ParentView, eventFlags, oldval.ToString(),
                        row, metaContext.EventFlagConditionalField, metaContext.EventFlagConditionalValue);
                }
            }

            // Cutscene list
            if (metaContext.DisplayCutsceneEnum)
            {
                if (Editor.Project.Handler.ProjectData.Aliases.TryGetValue(
                    ProjectAliasType.Cutscenes, out List<AliasEntry> cutscenes))
                {
                    AliasEnumHelper.Hint(ParentView, cutscenes, oldval.ToString());
                }
            }

            // Movie list
            if (metaContext.DisplayMovieEnum)
            {
                if (Editor.Project.Handler.ProjectData.Aliases.TryGetValue(
                    ProjectAliasType.Movies, out List<AliasEntry> movies))
                {
                    ConditionalAliasEnumHelper.Hint(ParentView, movies, oldval.ToString(),
                        row, metaContext.MovieConditionalField, metaContext.MovieConditionalValue);
                }
            }

            // Character list
            if (metaContext.DisplayCharacterEnum)
            {
                if (Editor.Project.Handler.ProjectData.Aliases.TryGetValue(
                    ProjectAliasType.Characters, out List<AliasEntry> characters))
                {
                    AliasEnumHelper.Hint(ParentView, characters, oldval.ToString());
                }
            }

            // Project Enum
            if (metaContext.DisplayProjectEnum)
            {
                ProjectEnumHelper.Hint(ParentView, metaContext.FieldMeta.ProjectEnumType, oldval.ToString());
            }

            // Tile Reference
            if (metaContext.DisplayTileReference)
            {
                TileReferenceHelper.Hint(ParentView, metaContext.FieldMeta.TileRef, oldval.ToString());
            }

            // Param Field Offset
            if (metaContext.DisplayAC6FieldOffsetData)
            {
                AC6_FieldOffsetHelper.Hint(ParentView, activeParam, row, metaContext.AC6FieldOffsetIndex);
            }

            ImGui.EndGroup();
        }
    }

    public void HandleClick(FieldMetaContext metaContext, Param.Row row, object oldval)
    {
        if (metaContext.DisplayParamReference)
        {
            ParamReferenceHelper.Click(ParentView, oldval, row, metaContext.ParamReferences);
        }

        if (metaContext.DisplayTextReferences)
        {
            TextReferenceHelper.Click(ParentView, oldval, row, metaContext.TextReferences, metaContext.FmgRefRoleOverride);
        }
    }

    public bool HandleContextMenu(FieldMetaContext metaContext, Param.Row row, object oldval, ref object newval)
    {
        var result = false;

        if (metaContext.FieldMeta != null)
        {
            if (metaContext.DisplayParamReference)
            {
                result |= ParamReferenceHelper.ContextMenu(ParentView, metaContext.ParamReferences, row, oldval, ref newval, Editor.ActionManager);
            }

            if (metaContext.DisplayTextReference)
            {
                TextReferenceHelper.ContextMenu(ParentView, metaContext.TextReferences, row, oldval, Editor.ActionManager, metaContext.FmgRefRoleOverride);
            }

            if (metaContext.DisplayMapTextReference)
            {
                TextReferenceHelper.ContextMenu(ParentView, metaContext.MapTextReferences, row, oldval, Editor.ActionManager);
            }

            if (metaContext.DisplayEnum)
            {
                result |= EnumHelper.ContextMenu(ParentView, metaContext.Enum, oldval, ref newval);
            }

            if (metaContext.DisplayParticleEnum && ParentView.Project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.Particles, out List<AliasEntry> particles))
            {
                result |= AliasEnumHelper.ContextMenu(ParentView, particles, oldval, ref newval);
            }

            if (metaContext.DisplaySoundEnum && ParentView.Project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.Sounds, out List<AliasEntry> sounds))
            {
                result |= AliasEnumHelper.ContextMenu(ParentView, sounds, oldval, ref newval);
            }

            if (metaContext.DisplayEventFlagEnum && ParentView.Project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.EventFlags, out List<AliasEntry> eventFlags))
            {
                result |= AliasEnumHelper.ContextMenu(ParentView, eventFlags, oldval, ref newval);
            }

            if (metaContext.DisplayCutsceneEnum && ParentView.Project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.Cutscenes, out List<AliasEntry> cutscenes))
            {
                result |= AliasEnumHelper.ContextMenu(ParentView, cutscenes, oldval, ref newval);
            }

            if (metaContext.DisplayMovieEnum && ParentView.Project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.Movies, out List<AliasEntry> movies))
            {
                result |= AliasEnumHelper.ContextMenu(ParentView, movies, oldval, ref newval);
            }

            if (metaContext.DisplayCharacterEnum && ParentView.Project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.Characters, out List<AliasEntry> characters))
            {
                result |= AliasEnumHelper.ContextMenu(ParentView, characters, oldval, ref newval);
            }

            if (metaContext.DisplayTileReference && ParentView.Project.Handler.ProjectData.Aliases.TryGetValue(ProjectAliasType.MapNames, out List<AliasEntry> mapNames))
            {
                result |= TileReferenceHelper.ContextMenu(ParentView, mapNames, oldval, ref newval);
            }

            if (metaContext.DisplayProjectEnum && metaContext.FieldMeta.ProjectEnumType != null)
            {
                var optionList = ParentView.Project.Handler.ProjectData.ProjectEnums.List.Where(e => e.Name == metaContext.FieldMeta.ProjectEnumType).FirstOrDefault();

                if (optionList != null)
                {
                    result |= ProjectEnumHelper.ContextMenu(ParentView, optionList, oldval, ref newval);
                }
            }
        }

        return result;
    }

}

#region Field Meta Context
public class FieldMetaContext
{
    public ParamView View;
    public ParamMeta Meta;
    public ParamFieldMeta FieldMeta;

    public string ActiveParam = "";
    public string InternalName = "";

    public bool DisplayEnums = false;
    public bool DisplayTextReferences = false;
    public bool DisplayTextureReferences = false;

    public string Description = "";

    public List<ParamRef> ParamReferences = new();
    public List<FMGRef> TextReferences = new();
    public List<FMGRef> MapTextReferences = new();
    public List<ExtRef> ExternalReferences = new();

    public string FmgRefRoleOverride = "";

    public IconConfig IconDisplayData;

    public string VirtualReference = "";

    public ParamEnum Enum;

    public bool IsBool = false;
    public bool IsInvertedPercentage = false;
    public bool IsPadding = false;
    public bool IsObsolete = false;

    public bool InjectSeparator = false;

    public string EventFlagConditionalField = "";
    public string EventFlagConditionalValue = "";

    public string MovieConditionalField = "";
    public string MovieConditionalValue = "";

    public bool DisplayAC6FieldOffsetData = false;
    public string AC6FieldOffsetIndex = "";

    public bool DisplayParamReference = false;
    public bool DisplayTextReference = false;
    public bool DisplayExternalReference = false;
    public bool DisplayMapTextReference = false;
    public bool DisplayVirtualReference = false;
    public bool DisplayTextureReference = false;

    public bool DisplayEnum = false;
    public bool DisplayParticleEnum = false;
    public bool DisplaySoundEnum = false;
    public bool DisplayEventFlagEnum = false;
    public bool DisplayCutsceneEnum = false;
    public bool DisplayMovieEnum = false;
    public bool DisplayCharacterEnum = false;
    public bool DisplayProjectEnum = false;

    public bool DisplayTileReference = false;

    public FieldMetaContext(ParamView curView, ParamMeta meta, ParamFieldMeta fieldMeta, string activeParam, string internalName)
    {
        View = curView;
        Meta = meta;
        FieldMeta = fieldMeta;

        ActiveParam = activeParam;
        InternalName = internalName;

        DisplayEnums = CFG.Current.ParamEditor_Field_List_Display_Enums;
        DisplayTextReferences = CFG.Current.ParamEditor_Field_List_Display_References;
        DisplayTextureReferences = CFG.Current.ParamEditor_Field_List_Display_Icon_Preview;

        if (fieldMeta != null)
        {
            Description = fieldMeta?.Wiki;

            ParamReferences = fieldMeta?.RefTypes;
            DisplayParamReference = ParamReferences != null;

            TextReferences = fieldMeta?.FmgRef;
            DisplayTextReference = TextReferences != null;

            ExternalReferences = fieldMeta?.ExtRefs;
            DisplayExternalReference = ExternalReferences != null;

            MapTextReferences = fieldMeta?.MapFmgRef;
            DisplayMapTextReference = MapTextReferences != null;

            VirtualReference = fieldMeta?.VirtualRef;
            DisplayVirtualReference = VirtualReference != null;

            IconDisplayData = fieldMeta?.IconConfig;
            DisplayTextureReference = IconDisplayData != null;

            Enum = fieldMeta?.EnumType;
            DisplayEnum = Enum != null;

            IsBool = fieldMeta?.IsBool ?? false;
            IsInvertedPercentage = fieldMeta?.IsInvertedPercentage ?? false;
            IsPadding = fieldMeta?.IsPaddingField ?? false;
            IsObsolete = fieldMeta?.IsObsoleteField ?? false;
            InjectSeparator = fieldMeta?.AddSeparatorNextLine ?? false;

            EventFlagConditionalField = fieldMeta?.FlagAliasEnum_ConditionalField;
            EventFlagConditionalValue = fieldMeta?.FlagAliasEnum_ConditionalValue;

            MovieConditionalField = fieldMeta?.MovieAliasEnum_ConditionalField;
            MovieConditionalValue = fieldMeta?.MovieAliasEnum_ConditionalValue;

            DisplayAC6FieldOffsetData = fieldMeta.ShowParamFieldOffset;
            AC6FieldOffsetIndex = fieldMeta.ParamFieldOffsetIndex;

            DisplayParticleEnum = fieldMeta.ShowParticleEnumList;
            DisplaySoundEnum = fieldMeta.ShowSoundEnumList;
            DisplayEventFlagEnum = fieldMeta.ShowFlagEnumList;
            DisplayCutsceneEnum = fieldMeta.ShowCutsceneEnumList;
            DisplayMovieEnum = fieldMeta.ShowMovieEnumList;
            DisplayCharacterEnum = fieldMeta.ShowCharacterEnumList;
            DisplayProjectEnum = fieldMeta.ShowProjectEnumList;

            DisplayTileReference = fieldMeta.TileRef != null;

            FmgRefRoleOverride = fieldMeta?.FmgRefRoleOverride;
        }
    }

    public bool HasAnyDisplayedElements()
    {
        var display = false;

        if (DisplayParamReference)
            display = true;

        if (DisplayTextReference)
            display = true;

        if (DisplayExternalReference)
            display = true;

        if (DisplayMapTextReference)
            display = true;

        if (DisplayVirtualReference)
            display = true;

        if (DisplayTextureReference)
            display = true;

        if (DisplayEnum)
            display = true;

        if (DisplayParticleEnum)
            display = true;

        if (DisplaySoundEnum)
            display = true;

        if (DisplayEventFlagEnum)
            display = true;

        if (DisplayCutsceneEnum)
            display = true;

        if (DisplayMovieEnum)
            display = true;

        if (DisplayCharacterEnum)
            display = true;

        if (DisplayProjectEnum)
            display = true;

        if (DisplayTileReference)
            display = true;

        return display;
    }

    public bool HasAnyReferenceElements()
    {
        var display = false;

        if (DisplayParamReference)
            display = true;

        if (DisplayTextReference)
            display = true;

        if (DisplayExternalReference)
            display = true;

        if (DisplayMapTextReference)
            display = true;

        if (DisplayVirtualReference)
            display = true;

        if (DisplayTextureReference)
            display = true;

        return display;
    }
}
#endregion

#region Field Tooltip Helper
public static class FieldTooltipHelper
{
    public static void IconTooltip(ParamView curView, FieldMetaContext context, Param.Column col)
    {
        if (col == null)
            return;

        var tooltipMode = CFG.Current.ParamEditor_Field_List_Tooltip_Mode;
        var displayDescription = true;
        var displayAttributes = CFG.Current.ParamEditor_Field_List_Display_Field_Attributes;

        // Help icon text
        if (tooltipMode is ParamTooltipMode.OnIcon)
        {
            if (displayDescription || displayAttributes)
            {
                ImGui.AlignTextToFramePadding();

                if (context.Description != null)
                {
                    var helpIconText = "";

                    if (displayDescription)
                    {
                        helpIconText = context.Description;
                    }

                    if (displayAttributes)
                    {
                        if (displayDescription)
                        {
                            helpIconText = helpIconText +
                                "\n" +
                                "-----\n";
                        }

                        helpIconText = helpIconText +
                        $"Minimum: {col.Def.Minimum}\n" +
                        $"Maximum: {col.Def.Maximum}\n" +
                        $"Increment: {col.Def.Increment}";
                    }

                    if (ParamTableUtils.HelpIcon(context.InternalName, ref helpIconText, true))
                    {
                        context.FieldMeta.Wiki = context.Description;
                    }

                    ImGui.SameLine();
                }
                else
                {
                    ImGui.Text(" ");
                    ImGui.SameLine();
                }
            }
        }
    }

    public static void HoverTooltip(ParamView curView, FieldMetaContext context, Param.Column col)
    {
        if (col == null)
            return;

        var tooltipMode = CFG.Current.ParamEditor_Field_List_Tooltip_Mode;
        var displayDescription = true;
        var displayAttributes = CFG.Current.ParamEditor_Field_List_Display_Field_Attributes;

        // Help hover text
        if (tooltipMode is ParamTooltipMode.OnFieldName)
        {
            if (displayDescription || displayAttributes)
            {
                if (context.Description != null)
                {
                    var helpIconText = "";

                    if (displayDescription)
                    {
                        helpIconText = context.Description;
                    }
                    if (displayAttributes)
                    {
                        if (displayDescription)
                        {
                            helpIconText = helpIconText +
                                "\n" +
                                "-----\n";
                        }

                        helpIconText = helpIconText +
                        $"Minimum: {col.Def.Minimum}\n" +
                        $"Maximum: {col.Def.Maximum}\n" +
                        $"Increment: {col.Def.Increment}";
                    }

                    UIHelper.Tooltip(helpIconText);
                }
            }
        }
    }
}

#endregion

#region Enum Helper
public static class EnumHelper
{
    public static string enumSearchStr = "";

    public static void Label(ParamView curView, ParamEnum pEnum)
    {
        if (!CFG.Current.ParamEditor_Field_List_Display_Enums)
            return;

        if (pEnum != null && pEnum.Name != null)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumName_Text);
            ImGui.TextUnformatted($@"   {pEnum.Name}");
            ImGui.PopStyleColor(1);
        }
    }

    public static void Hint(ParamView curView, Dictionary<string, string> enumValues, string value)
    {
        if (!CFG.Current.ParamEditor_Field_List_Display_Enums)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumValue_Text);
        ImGui.TextUnformatted(enumValues.GetValueOrDefault(value, "Not Enumerated"));
        ImGui.PopStyleColor(1);
    }

    public static bool ContextMenu(ParamView curView, ParamEnum en, object oldval, ref object newval)
    {
        ImGui.InputText("##enumSearch", ref enumSearchStr, 255);

        var count = 1;
        if (en.Values.Count > 0)
            count = en.Values.Count;

        var listHeight = ImGui.GetTextLineHeightWithSpacing() * Math.Min(12, count) * 1f;

        if (ImGui.BeginChild("EnumList", new Vector2(350f, listHeight)))
        {
            try
            {
                foreach (KeyValuePair<string, string> option in en.Values)
                {
                    if (ParamSearchFilters.IsEditorSearchMatch(enumSearchStr, option.Key, " ")
                        || ParamSearchFilters.IsEditorSearchMatch(enumSearchStr, option.Value, " ")
                        || enumSearchStr == "")
                    {
                        if (ImGui.Selectable($"{option.Key}: {option.Value}"))
                        {
                            newval = Convert.ChangeType(option.Key, oldval.GetType());
                            ImGui.EndChild();
                            return true;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        ImGui.EndChild();

        return false;
    }
}
#endregion

#region Project Enum Helper
public static class ProjectEnumHelper
{
    public static string enumSearchStr = "";

    public static void Label(ParamView curView, string enumType)
    {
        if (!CFG.Current.ParamEditor_Field_List_Display_Enums)
            return;

        var enumEntry = curView.Project.Handler.ProjectData.ProjectEnums.List.Where(e => e.Name == enumType).FirstOrDefault();

        if (enumEntry != null)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumName_Text);
            ImGui.TextUnformatted($@"   {enumEntry.DisplayName}");
            ImGui.PopStyleColor(1);

            if (enumEntry.Description != "")
            {
                UIHelper.Tooltip($"   {enumEntry.Description}");
            }
        }
    }

    public static void Hint(ParamView curView, string enumType, string value)
    {
        if (!CFG.Current.ParamEditor_Field_List_Display_Enums)
            return;

        var enumEntry = curView.Project.Handler.ProjectData.ProjectEnums.List
            .Where(e => e.Name == enumType).FirstOrDefault();

        if (enumEntry != null)
        {
            var enumValueName = "";
            var enumValue = enumEntry.Options.Where(e => e.ID == value).FirstOrDefault();

            if (enumValue != null)
            {
                enumValueName = enumValue.Name;
            }

            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumValue_Text);
            ImGui.TextUnformatted(enumValueName);
            ImGui.PopStyleColor();
        }
    }

    public static bool ContextMenu(ParamView curView, ProjectEnumEntry en, object oldval, ref object newval)
    {
        ImGui.InputText("##enumSearch", ref enumSearchStr, 255);

        var count = 1;
        if (en.Options.Count > 0)
            count = en.Options.Count;

        var listHeight = ImGui.GetTextLineHeightWithSpacing() * Math.Min(12, count) * 1f;

        if (ImGui.BeginChild("EnumList", new Vector2(350f, listHeight)))
        {
            try
            {
                foreach (var option in en.Options)
                {
                    if (ParamSearchFilters.IsEditorSearchMatch(enumSearchStr, option.ID, " ")
                        || ParamSearchFilters.IsEditorSearchMatch(enumSearchStr, option.Name, " ")
                        || enumSearchStr == "")
                    {
                        if (ImGui.Selectable($"{option.ID}: {option.Name}"))
                        {
                            newval = Convert.ChangeType(option.ID, oldval.GetType());
                            ImGui.EndChild();
                            return true;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        ImGui.EndChild();

        return false;
    }
}
#endregion

#region Alias Enum Helper
public static class AliasEnumHelper
{
    public static string enumSearchStr = "";

    public static void Label(ParamView curView, string name)
    {
        if (!CFG.Current.ParamEditor_Field_List_Display_Enums)
            return;

        var inactiveEnum = false;

        if (!inactiveEnum)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumName_Text);
            ImGui.TextUnformatted($@"   {name}");
            ImGui.PopStyleColor(1);
        }
    }

    public static void Hint(ParamView curView, List<AliasEntry> entries, string value, bool isCharacterAlias = false)
    {
        if (!CFG.Current.ParamEditor_Field_List_Display_Enums)
            return;

        var inactiveEnum = false;

        if (!inactiveEnum)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumValue_Text);
            if (value == "0" || value == "-1")
            {
                var entry = entries.FirstOrDefault(e => e.ID == value);

                if (isCharacterAlias)
                {
                    entry = entries.FirstOrDefault(e => e.ID.Replace("c", "") == value);
                }

                if (entry != null)
                {
                    ImGui.TextUnformatted(entry.Name);
                }
                else
                {
                    ImGui.TextUnformatted("None");
                }
            }
            else
            {
                var entry = entries.FirstOrDefault(e => e.ID == value);

                if (isCharacterAlias)
                {
                    entry = entries.FirstOrDefault(e => e.ID.Replace("c", "") == value);
                }

                if (entry != null)
                {
                    ImGui.TextUnformatted(entry.Name);
                }
                else
                {
                    ImGui.TextUnformatted("Not Enumerated");
                }
            }

            ImGui.PopStyleColor(1);
        }
    }

    public static bool ContextMenu(ParamView curView, List<AliasEntry> entries, object oldval, ref object newval)
    {
        ImGui.InputText("##enumSearch", ref enumSearchStr, 255);

        var count = 1;
        if (entries.Count > 0)
            count = entries.Count;

        var listHeight = ImGui.GetTextLineHeightWithSpacing() * Math.Min(12, count) * 1f;

        if (ImGui.BeginChild("EnumList", new Vector2(350f, listHeight)))
        {
            try
            {
                foreach (var entry in entries)
                {
                    var id = entry.ID.Replace("c", "");

                    if (ParamSearchFilters.IsEditorSearchMatch(enumSearchStr, id, " ")
                        || ParamSearchFilters.IsEditorSearchMatch(enumSearchStr, entry.Name, " ")
                        || enumSearchStr == "")
                    {
                        if (ImGui.Selectable($"{id}: {entry.Name}"))
                        {
                            newval = Convert.ChangeType(id, oldval.GetType());
                            ImGui.EndChild();
                            return true;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        ImGui.EndChild();

        return false;
    }
}
#endregion

#region Conditional Alias Enum Helper
public static class ConditionalAliasEnumHelper
{
    public static string enumSearchStr = "";

    public static void Label(ParamView curView, string name, Param.Row row, string limitField, string limitValue)
    {
        if (!CFG.Current.ParamEditor_Field_List_Display_Enums)
            return;

        var inactiveEnum = false;

        if (limitField != "")
        {
            Param.Cell? c = row?[limitField];
            inactiveEnum = row != null && c != null && Convert.ToInt32(c.Value.Value) != Convert.ToInt32(limitValue);
        }

        if (!inactiveEnum)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumName_Text);
            ImGui.TextUnformatted($@"   {name}");
            ImGui.PopStyleColor(1);
        }
    }

    public static void Hint(ParamView curView, List<AliasEntry> entries, string value, Param.Row row, string conditionalField, string conditionalValue)
    {
        if (!CFG.Current.ParamEditor_Field_List_Display_Enums)
            return;

        var inactiveEnum = false;

        if (conditionalField != "")
        {
            Param.Cell? c = row?[conditionalField];
            inactiveEnum = row != null && c != null && Convert.ToInt32(c.Value.Value) != Convert.ToInt32(conditionalValue);
        }

        if (!inactiveEnum)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumValue_Text);
            if (value == "0" || value == "-1")
            {
                var entry = entries.FirstOrDefault(e => e.ID == value);
                if (entry != null)
                {
                    ImGui.TextUnformatted(entry.Name);
                }
                else
                {
                    ImGui.TextUnformatted("None");
                }
            }
            else
            {
                var entry = entries.FirstOrDefault(e => e.ID == value);
                if (entry != null)
                {
                    ImGui.TextUnformatted(entry.Name);
                }
                else
                {
                    ImGui.TextUnformatted("Not Enumerated");
                }
            }
            ImGui.PopStyleColor(1);
        }
    }

    public static bool ContextMenu(ParamView curView, List<AliasEntry> entries, object oldval, ref object newval)
    {
        ImGui.InputText("##enumSearch", ref enumSearchStr, 255);

        var count = 1;
        if (entries.Count > 0)
            count = entries.Count;

        var listHeight = ImGui.GetTextLineHeightWithSpacing() * Math.Min(12, count) * 1f;

        if (ImGui.BeginChild("EnumList", new Vector2(350f, listHeight)))
        {
            try
            {
                foreach (var entry in entries)
                {
                    var id = entry.ID.Replace("c", "");

                    if (ParamSearchFilters.IsEditorSearchMatch(enumSearchStr, id, " ")
                        || ParamSearchFilters.IsEditorSearchMatch(enumSearchStr, entry.Name, " ")
                        || enumSearchStr == "")
                    {
                        if (ImGui.Selectable($"{id}: {entry.Name}"))
                        {
                            newval = Convert.ChangeType(id, oldval.GetType());
                            ImGui.EndChild();
                            return true;
                        }
                    }
                }
            }
            catch
            {
            }
        }

        ImGui.EndChild();

        return false;
    }
}
#endregion

#region Param Reference Helper
public static class ParamReferenceHelper
{
    private static string _refContextCurrentAutoComplete = "";

    public static void Label(ParamView curView, List<ParamRef> paramRefs, Param.Row context)
    {
        if (!CFG.Current.ParamEditor_Field_List_Display_References)
            return;

        if (paramRefs == null || paramRefs.Count == 0)
        {
            return;
        }

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
        ImGui.TextUnformatted(@"   <");

        List<string> inactiveRefs = new();
        var first = true;
        foreach (ParamRef r in paramRefs)
        {
            var inactiveRef = false;

            if (context != null && r.ConditionField != null)
            {
                Param.Cell? c = context?[r.ConditionField];

                if (c == null)
                    continue;

                var fieldValue = c.Value.Value;
                int intValue = 0;
                var valueConvertSuccess = int.TryParse($"{fieldValue}", out intValue);

                // Only check if field value is valid uint
                if (valueConvertSuccess)
                {
                    inactiveRef = intValue != r.ConditionValue;
                }
            }

            if (inactiveRef)
            {
                inactiveRefs.Add(r.ParamName);
            }
            else
            {
                if (first)
                {
                    ImGui.SameLine();
                    ImGui.TextUnformatted(r.ParamName);
                }
                else
                {
                    ImGui.TextUnformatted("    " + r.ParamName);
                }

                first = false;
            }
        }

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRefInactive_Text);

        foreach (var inactive in inactiveRefs)
        {
            ImGui.SameLine();
            if (first)
            {
                ImGui.TextUnformatted("!" + inactive);
            }
            else
            {
                ImGui.TextUnformatted("!" + inactive);
            }

            first = false;
        }

        ImGui.PopStyleColor();

        ImGui.SameLine();

        ImGui.TextUnformatted(">");

        ImGui.PopStyleVar();
    }

    public static void Hint(ParamView curView, List<ParamRef> paramRefs, Param.Row context, dynamic oldval)
    {
        if (!CFG.Current.ParamEditor_Field_List_Display_References)
            return;

        if (paramRefs == null)
        {
            return;
        }

        // Add named row and context menu
        // Lists located params
        // May span lines
        List<(string, Param.Row, string)> matches = ParamReferenceResolver.ResolveParamReferences(curView, paramRefs, context, oldval);

        var entryFound = matches.Count > 0;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRef_Text);
        ImGui.BeginGroup();

        foreach ((var param, Param.Row row, var adjName) in matches)
        {
            ImGui.TextUnformatted(adjName);
        }

        ImGui.PopStyleColor();
        if (!entryFound)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRefMissing_Text);
            ImGui.TextUnformatted("___");
            ImGui.PopStyleColor();
        }

        ImGui.EndGroup();
    }

    public static bool ContextMenu(ParamView curView, List<ParamRef> reftypes, Param.Row context,
        object oldval, ref object newval, ActionManager executor)
    {
        if (curView.GetPrimaryBank().Params == null)
        {
            return false;
        }

        // Add Goto statements
        List<(string, Param.Row, string)> refs = ParamReferenceResolver.ResolveParamReferences(curView, reftypes, context, oldval);

        int index = 0;

        foreach ((string, Param.Row, string) rf in refs)
        {
            if (ImGui.Selectable($@"Go to {rf.Item3}##GoToElement{index}"))
            {
                EditorCommandQueue.AddCommand($@"param/select/-1/{rf.Item1}/{rf.Item2.ID}");
            }

            if (ImGui.Selectable($@"Go to {rf.Item3} in new view##GoToElementInView{index}"))
            {
                EditorCommandQueue.AddCommand($@"param/select/new/{rf.Item1}/{rf.Item2.ID}");
            }

            if (context == null || executor == null)
            {
                continue;
            }

            if (!string.IsNullOrWhiteSpace(rf.Item2.Name) &&
                (InputManager.HasCtrlDown() || string.IsNullOrWhiteSpace(context.Name)) &&
                ImGui.Selectable($@"Inherit referenced row's name ({rf.Item2.Name})##InheritName{index}"))
            {
                executor.ExecuteAction(new PropertiesChangedAction(context.GetType().GetProperty("Name"), context,
                    rf.Item2.Name));
            }
            else if ((InputManager.HasCtrlDown() || string.IsNullOrWhiteSpace(rf.Item2.Name)) &&
                     !string.IsNullOrWhiteSpace(context.Name) &&
                     ImGui.Selectable($@"Proliferate name to referenced row ({rf.Item1})##ProliferateName{index}"))
            {
                executor.ExecuteAction(new PropertiesChangedAction(rf.Item2.GetType().GetProperty("Name"), rf.Item2,
                    context.Name));
            }

            index++;
        }

        // Add searchbar for named editing
        ImGui.InputTextWithHint("##value", "Search...", ref _refContextCurrentAutoComplete, 128);

        // This should be replaced by a proper search box with a scroll and everything
        if (_refContextCurrentAutoComplete != "")
        {
            foreach (ParamRef rf in reftypes)
            {
                var rt = rf.ParamName;

                if (!curView.GetPrimaryBank().Params.ContainsKey(rt))
                {
                    continue;
                }

                var meta = curView.GetParamData().GetParamMeta(curView.GetPrimaryBank().Params[rt].AppliedParamdef);

                var maxResultsPerRefType = 15 / reftypes.Count;

                List<Param.Row> rows = curView.MassEdit.RSE.Search((curView.GetPrimaryBank(), curView.GetPrimaryBank().Params[rt]),
                    _refContextCurrentAutoComplete, true, true);

                foreach (Param.Row r in rows)
                {
                    if (maxResultsPerRefType <= 0)
                    {
                        break;
                    }

                    if (ImGui.Selectable($@"({rt}){r.ID}: {r.Name}"))
                    {
                        try
                        {
                            if (meta != null && meta.FixedOffset != 0)
                            {
                                newval = Convert.ChangeType(r.ID - meta.FixedOffset - rf.Offset, oldval.GetType());
                            }
                            else
                            {
                                newval = Convert.ChangeType(r.ID - rf.Offset, oldval.GetType());
                            }

                            _refContextCurrentAutoComplete = "";
                            return true;
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog("Unable to convert value into param field's type'", LogLevel.Warning,
                                LogPriority.Normal, e);
                        }
                    }

                    maxResultsPerRefType--;
                }
            }
        }

        return false;
    }

    public static bool Click(ParamView curView, object oldval, Param.Row context, List<ParamRef> RefTypes)
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && InputManager.HasCtrlDown())
        {
            if (RefTypes != null)
            {
                (string, Param.Row, string)? primaryRef =
                    ParamReferenceResolver.ResolveParamReferences(curView, RefTypes, context, oldval)?.FirstOrDefault();

                if (primaryRef?.Item2 != null)
                {
                    if (InputManager.HasShiftDown())
                    {
                        EditorCommandQueue.AddCommand(
                            $@"param/select/new/{primaryRef?.Item1}/{primaryRef?.Item2.ID}");
                    }
                    else
                    {
                        EditorCommandQueue.AddCommand(
                            $@"param/select/-1/{primaryRef?.Item1}/{primaryRef?.Item2.ID}");
                    }
                }
            }
        }

        return false;
    }

    public static bool Shortcut(ParamView curView, FieldMetaContext context, Param.Row row, object oldval, ref object newval)
    {
        var result = false;

        if (!ImGui.IsAnyItemActive())
        {
            if (context.ParamReferences != null)
            {
                if (curView.GetPrimaryBank().Params == null)
                {
                    return false;
                }

                if (FocusManager.Focus is EditorFocusContext.ParamEditor_RowList)
                {
                    if (InputManager.IsPressed(KeybindID.ParamEditor_RowList_Inherit_Referenced_Row_Name))
                    {
                        List<(string, Param.Row, string)> refs = ParamReferenceResolver.ResolveParamReferences(curView, context.ParamReferences, row, oldval);

                        foreach ((string, Param.Row, string) rf in refs)
                        {
                            if (row == null || curView.Editor.ActionManager == null)
                            {
                                continue;
                            }

                            curView.Editor.ActionManager.ExecuteAction(new PropertiesChangedAction(row.GetType().GetProperty("Name"), row, rf.Item2.Name));
                        }

                        result = true;
                    }
                }
            }
        }

        return result;
    }
}
#endregion

#region Virtual Param Reference Helper
public static class VirtualParamReferenceHelper
{
    public static bool ContextMenu(ParamView curView, string virtualRefName, object searchValue,
        Param.Row context, string fieldName)
    {
        // Add Goto statements
        if (curView.GetPrimaryBank().Params != null)
        {
            foreach (KeyValuePair<string, Param> param in curView.GetPrimaryBank().Params)
            {
                var curMeta = curView.GetParamData().GetParamMeta(param.Value.AppliedParamdef);

                var paramdef = param.Value.AppliedParamdef;

                if (paramdef == null)
                    continue;

                foreach (PARAMDEF.Field f in paramdef.Fields)
                {
                    var curFieldMeta = curView.GetParamData().GetParamFieldMeta(curMeta, f);

                    if (curFieldMeta != null)
                    {
                        if (curFieldMeta.VirtualRef != null &&
                            curFieldMeta.VirtualRef.Equals(virtualRefName))
                        {
                            if (ImGui.Selectable($@"Search in {param.Key} ({f.InternalName})"))
                            {
                                EditorCommandQueue.AddCommand($@"param/select/-1/{param.Key}");
                                EditorCommandQueue.AddCommand(
                                    $@"param/search/prop {f.InternalName} ^{searchValue.ToParamEditorString()}$");
                            }
                        }
                    }
                }
            }
        }

        return false;
    }
}
#endregion

#region External Reference Helper
public static class ExternalReferenceHelper
{
    public static bool ContextMenu(ParamView curView, string virtualRefName, object searchValue,
        Param.Row context, string fieldName, List<ExtRef> ExtRefs)
    {
        if (ExtRefs != null)
        {
            foreach (ExtRef currentRef in ExtRefs)
            {
                List<string> matchedExtRefPath =
                    currentRef.paths.Select(x => string.Format(x, searchValue)).ToList();

                Item(curView, context, fieldName, $"modded {currentRef.name}", matchedExtRefPath, curView.Project.Descriptor.DataPath);

                Item(curView, context, fieldName, $"vanilla {currentRef.name}", matchedExtRefPath, curView.Project.Descriptor.DataPath);
            }
        }

        return false;
    }

    public static void Item(ParamView curView, Param.Row keyRow, string fieldKey, string menuText,
        List<string> matchedExtRefPath, string dir)
    {
        var exist = CacheBank.GetCached(curView.Editor, keyRow, $"extRef{menuText}{fieldKey}",
            () => Path.Exists(Path.Join(dir, matchedExtRefPath[0])));

        if (exist && ImGui.Selectable($"Go to {menuText} file..."))
        {
            var path = ParamReferenceResolver.ResolveExternalReferences(matchedExtRefPath, dir);

            if (File.Exists(path))
            {
                Process.Start("explorer.exe", $"/select,\"{path}\"");
            }
            else
            {
                TaskLogs.AddLog($"\"{path}\" could not be found. It may be map or chr specific",
                    LogLevel.Warning);

                CacheBank.ClearCaches();
            }
        }
    }

}
#endregion

#region Text Reference Helper
public static class TextReferenceHelper
{
    public static void Label(ParamView curView, List<FMGRef> fmgRef, Param.Row context, string overrideName = "")
    {
        if (!CFG.Current.ParamEditor_Field_List_Display_References)
            return;

        if (fmgRef == null)
            return;

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));

        if (overrideName == "")
        {
            ImGui.TextUnformatted(@"   [");

            List<string> inactiveRefs = new();
            var first = true;
            foreach (FMGRef r in fmgRef)
            {
                Param.Cell? c = context?[r.conditionField];
                var inactiveRef = context != null && c != null && Convert.ToInt32(c.Value.Value) != r.conditionValue;

                if (inactiveRef)
                {
                    inactiveRefs.Add(r.fmg);
                }
                else
                {
                    if (first)
                    {
                        ImGui.SameLine();
                        ImGui.TextUnformatted(r.fmg);
                    }
                    else
                    {
                        ImGui.TextUnformatted("    " + r.fmg);
                    }

                    first = false;
                }
            }

            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_FmgRefInactive_Text);
            foreach (var inactive in inactiveRefs)
            {
                ImGui.SameLine();
                if (first)
                {
                    ImGui.TextUnformatted("!" + inactive);
                }
                else
                {
                    ImGui.TextUnformatted("!" + inactive);
                }

                first = false;
            }

            ImGui.PopStyleColor();

            ImGui.SameLine();
            ImGui.TextUnformatted("]");
        }
        else
        {
            ImGui.TextUnformatted($@"   [{overrideName}]");
        }

        ImGui.PopStyleVar();
    }

    public static void Hint(ParamView curView, List<FMGRef> fmgNames, Param.Row context, dynamic oldval)
    {
        if (!CFG.Current.ParamEditor_Field_List_Display_References)
            return;

        List<string> textsToPrint = new List<string>();

        textsToPrint = CacheBank.GetCached(curView.Editor, (int)oldval, "PARAM META FMGREF", () =>
        {
            List<TextResult> refs = ParamReferenceResolver.ResolveTextReferences(curView, fmgNames, context, oldval);
            return refs.Where(x => x.Entry != null)
                .Select(x =>
                {
                    return $"{x.Entry.Text}".TrimStart();
                }).ToList();
        });

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_FmgRef_Text);
        foreach (var text in textsToPrint)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                ImGui.TextUnformatted("%null%");
            }
            else
            {
                ImGui.TextUnformatted(text);
            }
        }

        ImGui.PopStyleColor();
    }

    public static void Click(ParamView curView, object oldval, Param.Row context, List<FMGRef> fmgRefs, string roleOverride)
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && InputManager.HasCtrlDown())
        {
            if (fmgRefs != null)
            {
                TextResult primaryRef = ParamReferenceResolver.ResolveTextReferences(curView, fmgRefs, context, oldval)?.FirstOrDefault();

                if (primaryRef != null)
                {
                    EditorCommandQueue.AddCommand($@"text/select/{primaryRef.ContainerWrapper.ContainerDisplayCategory}/{primaryRef.ContainerWrapper.FileEntry.Filename}/{primaryRef.FmgName}/{primaryRef.Entry.ID}");
                }
            }
        }
    }

    // TODO: restore on FMG descriptor impl
    //public static void Click(ParamView curView, object oldval, Param.Row context, List<FMGRef> fmgRefs, string roleOverride)
    //{
    //    if (ImGui.IsItemClicked(ImGuiMouseButton.Left) && InputManager.HasCtrlDown())
    //    {
    //        var language = CFG.Current.TextEditor_Primary_Category;

    //        if (fmgRefs != null)
    //        {
    //            TextResult primaryRef = ParamReferenceResolver.ResolveTextReferences(curView, fmgRefs, context, oldval, roleOverride)?.FirstOrDefault();

    //            if (primaryRef != null)
    //            {
    //                EditorCommandQueue.AddCommand($@"text/select/{language}/{primaryRef.ContainerWrapper.Descriptor.FileName}/{primaryRef.FmgName}/{primaryRef.Entry.ID}");
    //            }
    //        }
    //    }
    //}

    public static bool ContextMenu(ParamView curView, List<FMGRef> reftypes, Param.Row context, dynamic oldval,
        ActionManager executor, string roleOverride = "")
    {
        List<TextResult> refs = ParamReferenceResolver.ResolveTextReferences(curView, reftypes, context, oldval);

        var language = CFG.Current.TextEditor_Primary_Category;

        foreach (var result in refs)
        {
            if (result != null && result.Entry != null)
            {
                if (ImGui.Selectable($@"Go to FMG entry text"))
                {
                    EditorCommandQueue.AddCommand($@"text/select/{result.ContainerWrapper.ContainerDisplayCategory}/{result.ContainerWrapper.FileEntry.Filename}/{result.FmgName}/{result.Entry.ID}");
                }

                if (context == null || executor == null)
                {
                    continue;
                }

                // Set Row Name to X
                if (!string.IsNullOrWhiteSpace(result.Entry.Text))
                {
                    if (ImGui.Selectable($@"Replace row name with referenced FMG entry text"))
                    {
                        executor.ExecuteAction(
                            new PropertiesChangedAction(
                                context.GetType().GetProperty("Name"),
                                context,
                                result.Entry.Text));
                    }
                }

                // Apply Row Name to X
                if (!string.IsNullOrWhiteSpace(context.Name))
                {
                    if (ImGui.Selectable($@"Replace FMG entry text with current row name"))
                    {
                        executor.ExecuteAction(
                            new PropertiesChangedAction(
                                result.Entry.GetType().GetProperty("Text"),
                                result.Entry,
                                context.Name));
                    }
                }
            }
        }

        return false;
    }

    // TODO: restore on FMG descriptor impl
    //public static bool ContextMenu(ParamView curView, List<FMGRef> reftypes, Param.Row context, dynamic oldval,
    //    ActionManager executor, string roleOverride = "")
    //{
    //    List<TextResult> refs = ParamReferenceResolver.ResolveTextReferences(curView, reftypes, context, oldval, roleOverride);

    //    var language = CFG.Current.TextEditor_Primary_Category;

    //    foreach (var result in refs)
    //    {
    //        if (result != null && result.TextEntry != null)
    //        {
    //            if (ImGui.Selectable($@"Go to FMG entry text"))
    //            {
    //                EditorCommandQueue.AddCommand($@"text/select/{language}/{result.ContainerWrapper.Descriptor.FileName}/{result.FmgName}/{result.Entry.ID}");
    //            }

    //            if (context == null || executor == null)
    //            {
    //                continue;
    //            }

    //            // Set Row Name to X
    //            if (!string.IsNullOrWhiteSpace(result.Entry.Text))
    //            {
    //                if (ImGui.Selectable($@"Replace row name with referenced FMG entry text"))
    //                {
    //                    executor.ExecuteAction(
    //                        new PropertiesChangedAction(
    //                            context.GetType().GetProperty("Name"),
    //                            context,
    //                            result.Entry.Text));
    //                }
    //            }

    //            // Apply Row Name to X
    //            if (!string.IsNullOrWhiteSpace(context.Name))
    //            {
    //                if (ImGui.Selectable($@"Replace FMG entry text with current row name"))
    //                {
    //                    executor.ExecuteAction(
    //                        new PropertiesChangedAction(
    //                            result.Entry.GetType().GetProperty("Text"),
    //                            result.Entry,
    //                            context.Name));
    //                }
    //            }
    //        }
    //    }

    //    return false;
    //}
}
#endregion

#region Texture Reference Helper
public static class TextureReferenceHelper
{
    public static Vector2 DummySize = new Vector2();

    public static void Label(ParamView curView, IconConfig iconConfig, Param.Row context)
    {
        if (!CFG.Current.ParamEditor_Field_List_Display_Icon_Preview)
            return;

        if (iconConfig == null)
            return;

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
        ImGui.TextUnformatted(@"   [Icon]");
        ImGui.PopStyleVar();
    }

    public static void Hint(ParamView curView, IconConfig fieldIcon, Param.Row context, dynamic oldval, string fieldName, int columnIndex)
    {
        if (!CFG.Current.ParamEditor_Field_List_Display_Icon_Preview)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_FmgRef_Text);

        if (CFG.Current.ParamEditor_Field_List_Display_Icon_Preview)
        {
            CachedTexture cachedTexture = Smithbox.TextureManager.IconManager.LoadIcon(context, fieldIcon, oldval, fieldName, columnIndex);

            if (cachedTexture != null)
            {
                DummySize = Smithbox.TextureManager.IconManager.DisplayIcon(cachedTexture);
            }
            else
            {
                ImGui.Dummy(DummySize);
            }
        }

        ImGui.PopStyleColor();
    }

    public static bool ContextMenu(ParamView curView)
    {
        return false;
    }
}
#endregion

#region AC6 Field Offset Helper
public static class AC6_FieldOffsetHelper
{
    public static void Label(ParamView curView, string activeParam, Param.Row context, string index)
    {
        // This feature is purely for AC6 MenuPropertySpecParam.
        if (activeParam == "MenuPropertySpecParam")
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
            ImGui.TextUnformatted(@"   <PARAM>");
            ImGui.TextUnformatted(@"   <FIELD>");
            ImGui.TextUnformatted(@"   <NAME>");
            ImGui.PopStyleVar();
        }
    }

    public static void Hint(ParamView curView, string activeParam, Param.Row context, string index)
    {
        // This feature is purely for AC6 MenuPropertySpecParam.
        if (activeParam == "MenuPropertySpecParam")
        {
            if (index != "0" && index != "1")
            {
                return;
            }


            string target = ParamUtils.GetFieldValue(context, $"extract{index}_Target");
            string primitiveType = ParamUtils.GetFieldValue(context, $"extract{index}_MemberType");
            string operationType = ParamUtils.GetFieldValue(context, $"extract{index}_Operation");
            string fieldOffset = ParamUtils.GetFieldValue(context, $"extract{index}_MemberTailOffset");

            var decimalOffset = int.Parse($"{fieldOffset}");

            switch (primitiveType)
            {
                case "0": return;

                case "1": // s8
                case "2": // u8
                    decimalOffset = decimalOffset - 1;
                    break;

                case "3": // s16
                case "4": // u16
                    decimalOffset = decimalOffset - 2;
                    break;


                case "5": // s32
                case "6": // u32
                case "7": // f
                    decimalOffset = decimalOffset - 4;
                    break;
            }

            var paramString = "";

            switch (target)
            {
                case "0": return;

                case "1": // Weapon
                    paramString = "EquipParamWeapon";
                    break;
                case "2": // Armor
                    paramString = "EquipParamProtector";
                    break;
                case "3": // Booster
                    paramString = "EquipParamBooster";
                    break;
                case "4": // FCS
                    paramString = "EquipParamFcs";
                    break;
                case "5": // Generator
                    paramString = "EquipParamGenerator";
                    break;
                case "6": // Behavior Paramter
                    paramString = "BehaviorParam_PC";
                    break;
                case "7": // Attack Parameter
                    paramString = "AtkParam_Pc";
                    break;
                case "8": // Bullet Parameter
                    paramString = "Bullet";
                    break;
                case "100": // Child Bullet Parameter
                    paramString = "Bullet";
                    break;
                case "101": // Child Bullet Attack Parameter
                    paramString = "AtkParam_Pc";
                    break;
                case "110": // Parent Bullet Parameter
                    paramString = "Bullet";
                    break;
                case "111": // Parent Bullet Attack Parameter
                    paramString = "AtkParam_Pc";
                    break;
            }

            var firstRow = curView.GetPrimaryBank().Params[paramString].Rows.First();
            var internalName = "";
            var displayName = "";

            var targetMeta = curView.GetParamData().GetParamMeta(firstRow.Def);

            foreach (var col in firstRow.Columns)
            {
                var offset = (int)col.GetByteOffset();

                if (offset == decimalOffset)
                {
                    internalName = col.Def.InternalName;

                    var cellmeta = curView.GetParamData().GetParamFieldMeta(targetMeta, col.Def);
                    displayName = cellmeta.AltName;
                }
            }

            ImGui.BeginGroup();

            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);

            ImGui.TextUnformatted($"{paramString}:");
            ImGui.TextUnformatted($"{internalName}");
            ImGui.TextUnformatted($"{displayName}");

            ImGui.PopStyleColor();

            ImGui.EndGroup();
        }
    }

    public static bool ContextMenu(ParamView curView)
    {
        return false;
    }
}
#endregion

#region CalcCorrectGraph Helper
public static class CalcCorrectGraphHelper
{
    private static string ExportPath = "";
    private static GraphDataContext GraphContext = null;

    public static void Display(ParamView curView, ParamMeta meta, Param.Row row, Vector2 graphSize)
    {
        try
        {
            bool draw = true;

            var graphName = "Graph";
            var xAxisTitle = "";
            var yAxisTitle = "";

            if (curView.GetParamData().GraphLegends != null && curView.GetParamData().GraphLegends.Entries != null)
            {
                var entry = curView.GetParamData().GraphLegends.Entries
                    .FirstOrDefault(e => e.RowID == $"{row.ID}");
                if (entry != null)
                {
                    xAxisTitle = entry.X;
                    yAxisTitle = entry.Y;
                }
            }

            var fcsRow = row["inheritanceFcsParamId"];

            // Prevent draw for rows with inheritance
            if (fcsRow != null &&
                fcsRow.Value.Value.ToString() != "-1")
            {
                draw = false;
            }

            ImGui.Separator();
            ImGui.NewLine();
            ImGui.Indent();

            CalcCorrectDefinition ccd = meta.CalcCorrectDef;
            SoulCostDefinition scd = meta.SoulCostDef;

            double[] values = null;
            int xOffset = 0;
            double minY = 0;
            double maxY = 0;

            if (draw)
            {
                if (scd != null && scd.cost_row == row.ID)
                {
                    (values, maxY) = CacheBank.GetCached(curView.Editor, row, "soulCostData", () => ParamUtils.getSoulCostData(scd, row));
                }
                else if (ccd != null)
                {
                    (values, xOffset, minY, maxY) = CacheBank.GetCached(curView.Editor, row, "calcCorrectData",
                        () => ParamUtils.getCalcCorrectedData(ccd, row));
                }

                double[] xValues = (scd != null)
                    ? Enumerable.Range(0, values.Length).Select(i => (double)i).ToArray()
                    : Enumerable.Range(0, values.Length).Select(i => (double)(i + xOffset)).ToArray();

                // Axis Validation
                var validAxis_X = ImPlotHelper.TryGetSafeAxisLimits(xValues[0], xValues[^1], out var xMin, out var xMax);

                var validAxis_Y = ImPlotHelper.TryGetSafeAxisLimits(minY, maxY, out var yMin, out var yMax);

                if (!validAxis_X || !validAxis_Y)
                {
                    ImGui.Text("Invalid axis limits.");
                    ImGui.Unindent();
                    return;
                }

                // Length Validation
                if (values.Length != xValues.Length || values.Length < 2)
                {
                    ImGui.Text("Mismatched graph data.");
                    ImGui.Unindent();
                    return;
                }

                // Value Validation
                if (!ImPlotHelper.SanitizeSeries(values))
                {
                    ImGui.Text("Graph contains invalid values.");
                    ImGui.Unindent();
                    return;
                }

                unsafe
                {
                    fixed (double* xPtr = xValues)
                    fixed (double* yPtr = values)
                    {
                        if (ImPlot.BeginPlot(graphName, graphSize))
                        {
                            string xAxisName = string.IsNullOrEmpty(xAxisTitle) ? "X" : xAxisTitle;
                            string yAxisName = string.IsNullOrEmpty(yAxisTitle) ? "Y" : yAxisTitle;

                            ImPlot.SetupAxes(xAxisName, yAxisName);
                            ImPlot.SetupAxisLimits(ImAxis.X1, xValues[0], xValues[^1]);
                            ImPlot.SetupAxisLimits(ImAxis.Y1, minY, maxY > minY ? maxY : minY + 1); // Ensure valid range

                            ImPlot.PlotLine("Correction", xPtr, yPtr, values.Length);

                            ImPlot.EndPlot();
                        }
                    }
                }

                if (ImGui.Button("Export to CSV##graphCsvExplort"))
                {
                    GraphContext = new GraphDataContext(row, xValues, values);

                    var dialog = PlatformUtils.Instance.OpenFolderDialog("Select Folder", out var path);

                    if(dialog)
                    {
                        ExportPath = path;

                        try
                        {
                            string fileName = $"graph_export_{GraphContext.Row.ID}.csv";

                            ExportGraphDataToCsv(Path.Combine(ExportPath, fileName), GraphContext.xValues, GraphContext.yValues);

                            TaskLogs.AddLog($"Exported graph data for row {GraphContext.Row.ID}.");
                        }
                        catch (Exception ex)
                        {
                            TaskLogs.AddError($"Failed to export graph data for row {GraphContext.Row.ID}.", ex);
                        }
                    }
                }
                UIHelper.Tooltip("This will export the graph data for each point on the graph as generated by this row.");

            }
        }
        catch (Exception e)
        {
            ImGui.TextColored(new Vector4(1, 0, 0, 1), "Unable to draw graph");
            ImGui.TextUnformatted(e.Message);
        }

        ImGui.NewLine();
    }

    private static void ExportGraphDataToCsv(string filePath, double[] xValues, double[] yValues)
    {
        using var writer = new StreamWriter(filePath);
        writer.WriteLine("X,Y");

        for (int i = 0; i < xValues.Length && i < yValues.Length; i++)
        {
            writer.WriteLine($"{xValues[i]},{yValues[i]}");
        }
    }
}

public class GraphDataContext
{
    public Param.Row Row;
    public double[] xValues;
    public double[] yValues;

    public GraphDataContext(Param.Row row, double[] xValues, double[] yValues)
    {
        Row = row;
        this.xValues = xValues;
        this.yValues = yValues;
    }
}
#endregion

#region Tile Reference Helper
public static class TileReferenceHelper
{
    public static void Label(ParamView curView, string enumType)
    {
        if (!CFG.Current.ParamEditor_Field_List_Display_Enums)
            return;

        ImGui.TextUnformatted($@"   <Tile>");
    }

    public static void Hint(ParamView curView, string enumType, string value)
    {
        if (!CFG.Current.ParamEditor_Field_List_Display_Enums)
            return;

        if (curView.Project.Handler.ProjectData.Aliases
            .TryGetValue(ProjectAliasType.MapNames, out List<AliasEntry> mapNames))
        {
            var resultID = "";
            var resultName = "";

            foreach (var entry in mapNames)
            {
                var mapName = entry.ID;
                if (mapName.Length > 5)
                {
                    var adjustedMapName = mapName.Replace("m", "").Replace("_", "");

                    if (adjustedMapName.Substring(0, 4) == value)
                    {
                        resultID = entry.ID;
                        resultName = entry.Name;
                    }
                }
            }

            if (resultID != "")
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);
                ImGui.TextUnformatted(resultName);
                ImGui.PopStyleColor(1);
            }
        }
    }

    public static bool ContextMenu(ParamView curView, List<AliasEntry> entries, object oldval, ref object newval)
    {
        return false;
    }
}
#endregion

#region Field Color Picker
public static class FieldColorPicker
{
    private static Vector3 heldColor = new();

    public static void ColorPicker(ParamView curView, string activeParam, Param.Row row, string currentField)
    {
        if (activeParam == null)
            return;

        if (row == null)
            return;

        if (currentField == null)
            return;

        var meta = curView.GetParamData().GetParamMeta(row.Def);
        var proceed = false;
        string name = "";
        string fields = "";
        string placementField = "";

        if (meta != null)
        {
            foreach (var cEditor in meta.ColorEditors)
            {
                name = cEditor.Name;
                fields = cEditor.Fields;
                placementField = cEditor.PlacedField;

                if (currentField == placementField)
                {
                    proceed = true;
                    break;
                }
            }
        }

        if (proceed)
        {
            List<string> FieldNames = new List<string>();
            FieldNames = fields.Split(",").ToList();

            if (FieldNames.IndexExists(0) && FieldNames.IndexExists(1) && FieldNames.IndexExists(2))
            {
                DisplayColorPicker(curView, activeParam, row, name, FieldNames[0], FieldNames[1], FieldNames[2]);
            }
        }
    }

    private static void DisplayColorPicker(ParamView curView, string activeParam, Param.Row curRow, string name, string redField, string greenField, string blueField)
    {
        var color = GetVector3Color(curRow, redField, greenField, blueField);

        if (ImGui.ColorEdit3($"{name}##ColorEdit_{name}{curRow.ID}", ref color))
        {
            heldColor = color;
        }

        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            var newColor = GetRgbColor(curRow, heldColor.X, heldColor.Y, heldColor.Z);

            PropertyInfo info = typeof(Param.Cell).GetProperty("Value");

            // RED
            var redProp = curRow[redField].Value;

            var redValue = newColor.X;

            // GREEN
            var greenProp = curRow[greenField].Value;

            var greenValue = newColor.Y;

            // BLUE
            var blueProp = curRow[blueField].Value;

            var blueValue = newColor.Z;

            PropertiesChangedAction redAction = null;

            if (curRow[redField].Value.Def.InternalType == "u8")
            {
                redAction = new PropertiesChangedAction(info, redProp, (byte)redValue);
            }
            if (curRow[redField].Value.Def.InternalType == "s8")
            {
                redAction = new PropertiesChangedAction(info, redProp, (sbyte)redValue);
            }
            if (curRow[redField].Value.Def.InternalType == "u16")
            {
                redAction = new PropertiesChangedAction(info, redProp, (ushort)redValue);
            }
            if (curRow[redField].Value.Def.InternalType == "s16")
            {
                redAction = new PropertiesChangedAction(info, redProp, (short)redValue);
            }
            if (curRow[redField].Value.Def.InternalType == "u32")
            {
                redAction = new PropertiesChangedAction(info, redProp, (byte)redValue);
            }
            if (curRow[redField].Value.Def.InternalType == "s32")
            {
                redAction = new PropertiesChangedAction(info, redProp, (int)redValue);
            }
            if (curRow[redField].Value.Def.InternalType == "f32")
            {
                redAction = new PropertiesChangedAction(info, redProp, redValue);
            }

            PropertiesChangedAction greenAction = null;

            if (curRow[greenField].Value.Def.InternalType == "u8")
            {
                greenAction = new PropertiesChangedAction(info, greenProp, (byte)greenValue);
            }
            if (curRow[greenField].Value.Def.InternalType == "s8")
            {
                greenAction = new PropertiesChangedAction(info, greenProp, (sbyte)greenValue);
            }
            if (curRow[greenField].Value.Def.InternalType == "u16")
            {
                greenAction = new PropertiesChangedAction(info, greenProp, (ushort)greenValue);
            }
            if (curRow[greenField].Value.Def.InternalType == "s16")
            {
                greenAction = new PropertiesChangedAction(info, greenProp, (short)greenValue);
            }
            if (curRow[greenField].Value.Def.InternalType == "u32")
            {
                greenAction = new PropertiesChangedAction(info, greenProp, (byte)greenValue);
            }
            if (curRow[greenField].Value.Def.InternalType == "s32")
            {
                greenAction = new PropertiesChangedAction(info, greenProp, (int)greenValue);
            }
            if (curRow[greenField].Value.Def.InternalType == "f32")
            {
                greenAction = new PropertiesChangedAction(info, greenProp, greenValue);
            }

            PropertiesChangedAction blueAction = null;

            if (curRow[blueField].Value.Def.InternalType == "u8")
            {
                blueAction = new PropertiesChangedAction(info, blueProp, (byte)blueValue);
            }
            if (curRow[blueField].Value.Def.InternalType == "s8")
            {
                blueAction = new PropertiesChangedAction(info, blueProp, (sbyte)blueValue);
            }
            if (curRow[blueField].Value.Def.InternalType == "u16")
            {
                blueAction = new PropertiesChangedAction(info, blueProp, (ushort)blueValue);
            }
            if (curRow[blueField].Value.Def.InternalType == "s16")
            {
                blueAction = new PropertiesChangedAction(info, blueProp, (short)blueValue);
            }
            if (curRow[blueField].Value.Def.InternalType == "u32")
            {
                blueAction = new PropertiesChangedAction(info, blueProp, (byte)blueValue);
            }
            if (curRow[blueField].Value.Def.InternalType == "s32")
            {
                blueAction = new PropertiesChangedAction(info, blueProp, (int)blueValue);
            }
            if (curRow[blueField].Value.Def.InternalType == "f32")
            {
                blueAction = new PropertiesChangedAction(info, blueProp, blueValue);
            }

            if (redAction != null && greenAction != null && blueAction != null)
            {
                var compoundAction = new CompoundAction(new List<EditorAction> { redAction, greenAction, blueAction });

                curView.Editor.ActionManager.ExecuteAction(compoundAction);
            }
        }
    }

    public static Vector3 GetRgbColor(Param.Row curRow, float red, float green, float blue)
    {
        float rVal = red * 255;
        float gVal = green * 255;
        float bVal = blue * 255;

        return new Vector3(rVal, gVal, bVal);
    }
    public static Vector3 GetVector3Color(Param.Row curRow, string redField, string greenField, string blueField)
    {
        // RED
        var redValue = curRow[redField].Value.Value.ToString();
        float rVal = 0.0f;

        float.TryParse(redValue, out rVal);
        if (rVal > 1.0) // If greater than 1.0, then it is a 255,255,255 field
        {
            rVal = rVal / 255;
        }

        // RED
        var greenValue = curRow[greenField].Value.Value.ToString();
        float gVal = 0.0f;

        float.TryParse(greenValue, out gVal);
        if (gVal > 1.0) // If greater than 1.0, then it is a 255,255,255 field
        {
            gVal = gVal / 255;
        }

        // BLUE
        var blueValue = curRow[blueField].Value.Value.ToString();
        float bVal = 0.0f;

        float.TryParse(blueValue, out bVal);
        if (bVal > 1.0) // If greater than 1.0, then it is a 255,255,255 field
        {
            bVal = bVal / 255;
        }

        return new Vector3(rVal, gVal, bVal);
    }
}
#endregion