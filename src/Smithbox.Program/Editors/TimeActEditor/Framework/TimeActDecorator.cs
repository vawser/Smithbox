using Andre.Formats;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.ParamEditor.Data;
using StudioCore.Editors.ParamEditor.META;
using StudioCore.Editors.TimeActEditor.Actions;
using StudioCore.Formats.JSON;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActDecorator
{
    public TimeActEditorScreen Editor;
    public ProjectEntry Project;

    public TimeActDecorator(TimeActEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    private string _enumSearchInput = "";

    public void HandleTypeColumn(string propertyName)
    {
        var parameters = Editor.Selection.CurrentTimeActEvent.Parameters;
        var template = parameters.GetParamTemplate(propertyName);

        // Param Reference
        if (template.ParamRef != null)
        {
            ImGui.Text("");
        }

        // Enum List
        if (template.EnumEntries != null)
        {
            ImGui.Text("");
        }

        // Alias Enum
        if (template.AliasEnum != null)
        {
            ImGui.Text("");
        }

        // Project Enum
        if (template.ProjectEnum != null)
        {
            ImGui.Text("");
        }
    }

    public void DisplayEnumInfo(TAE.Event entry)
    {
        if (entry.Parameters == null)
            return;

        Vector4 displayColor = UI.Current.ImGui_TimeAct_InfoText_1_Color;
        bool foundAlias = false;
        var alias = "";

        // Enum Alias
        foreach(var prop in entry.Parameters.Values)
        {
            var propertyName = prop.Key;
            var template = entry.Parameters.GetParamTemplate(propertyName);
            var propertyValue = entry.Parameters[propertyName];

            // TODO: account for multiple enums
            if (template.EnumEntries != null)
            {
                if (template.EnumEntries.ContainsKey(propertyValue))
                {
                    foundAlias = true;
                    alias = $"[{template.EnumEntries[propertyValue]}]";
                    break;
                }
            }
        }

        if (foundAlias)
        {
            ImGui.PushTextWrapPos();
            ImGui.SameLine();
            ImGui.TextColored(displayColor, @$"{alias}");
            ImGui.PopTextWrapPos();
        }
    }

    public void DisplayParamRefInfo(TAE.Event entry)
    {
        if (entry.Parameters == null)
            return;

        Vector4 displayColor = UI.Current.ImGui_TimeAct_InfoText_2_Color;
        bool foundAlias = false;
        var alias = "";

        foreach (var prop in entry.Parameters.Values)
        {
            var propertyName = prop.Key;
            var template = entry.Parameters.GetParamTemplate(propertyName);
            var propertyValue = entry.Parameters[propertyName];

            if (Editor.Project.ParamEditor != null)
            {
                // Param Ref
                if (template.ParamRef != null)
                {
                    var primaryBank = Editor.Project.ParamData.PrimaryBank;
                    (string, Param.Row, string) match = ResolveParamRef(Editor.Project.ParamEditor, primaryBank, template.ParamRef, propertyValue);
                    if (match != (null, null, null))
                    {
                        foundAlias = true;
                        alias = $"{match.Item3}";
                    }
                }
            }
        }

        if (foundAlias)
        {
            ImGui.PushTextWrapPos();
            ImGui.SameLine();
            ImGui.TextColored(displayColor, @$"{alias}");
            ImGui.PopTextWrapPos();
        }
    }

    public void DisplayAliasEnumInfo(TAE.Event entry)
    {
        if (entry.Parameters == null)
            return;

        Vector4 displayColor = UI.Current.ImGui_TimeAct_InfoText_3_Color;
        bool foundAlias = false;
        var alias = "";

        foreach (var prop in entry.Parameters.Values)
        {
            var propertyName = prop.Key;
            var template = entry.Parameters.GetParamTemplate(propertyName);
            var propertyValue = entry.Parameters[propertyName];

            // Alias Enum
            if (template.AliasEnum != null)
            {
                var aliasType = template.AliasEnum;
                foundAlias = true;

                // Particle
                if (aliasType == "Particle")
                {
                    alias = $"FFX ID: {propertyValue}";

                    var enumEntry = Editor.Project.Aliases.Particles.Where(e => e.ID == propertyValue.ToString()).FirstOrDefault();
                    if (enumEntry != null)
                    {
                        if (CFG.Current.TimeActEditor_DisplayEventRow_DataAliasInfo_IncludeAliasName)
                        {
                            alias = $"FFX ID: {propertyValue} [{enumEntry.Name}]";
                        }
                    }
                }

                // Sound
                if (aliasType == "Sound")
                {
                    alias = $"Sound ID: {propertyValue}";

                    var enumEntry = Editor.Project.Aliases.Sounds.Where(e => e.ID == propertyValue.ToString()).FirstOrDefault();
                    if (enumEntry != null)
                    {
                        if(CFG.Current.TimeActEditor_DisplayEventRow_DataAliasInfo_IncludeAliasName)
                        {
                            alias = $"Sound ID: {propertyValue} [{enumEntry.Name}]";
                        }
                    }
                }
            }
        }

        if (foundAlias)
        {
            ImGui.PushTextWrapPos();
            ImGui.SameLine();
            ImGui.TextColored(displayColor, @$"{alias}");
            ImGui.PopTextWrapPos();
        }
    }

    public void DisplayProjectEnumInfo(TAE.Event entry)
    {
        if (entry.Parameters == null)
            return;

        if (Editor.Project.ProjectEnums == null)
            return;

        if (Editor.Project.ProjectEnums.List == null)
            return;

        if (Editor.Project.ProjectEnums.List.Count == 0)
            return;

        Vector4 displayColor = UI.Current.ImGui_TimeAct_InfoText_4_Color;
        bool foundAlias = false;
        var alias = "";

        foreach (var prop in entry.Parameters.Values)
        {
            var propertyName = prop.Key;
            var template = entry.Parameters.GetParamTemplate(propertyName);
            var propertyValue = entry.Parameters[propertyName];

            // Project Enum
            if (template.ProjectEnum != null && propertyValue.ToString() != "0" && propertyValue.ToString() != "-1")
            {
                var projectEnumType = template.ProjectEnum;
                var enumEntry = Editor.Project.ProjectEnums.List.Where(e => e.Name == projectEnumType).FirstOrDefault();

                var option = enumEntry.Options.Where(e => e.ID == propertyValue.ToString()).FirstOrDefault();
                if (option != null)
                {
                    foundAlias = true;
                    alias = option.Name;

                    // Speical prepend text
                    if(template.ProjectEnum == "SP_EFFECT_TYPE")
                    {
                        alias = $"Triggered by State Info {option.ID} [{option.Name}]";
                    }
                }
            }
        }

        if (foundAlias)
        {
            ImGui.PushTextWrapPos();
            ImGui.SameLine();
            ImGui.TextColored(displayColor, @$"{alias}");
            ImGui.PopTextWrapPos();
        }
    }

    public void HandleNameColumn(string propertyName)
    {
        var parameters = Editor.Selection.CurrentTimeActEvent.Parameters;
        var template = parameters.GetParamTemplate(propertyName);

        // Param Reference
        if (template.ParamRef != null)
        {
            ImGui.Text("");
        }

        // Enum List
        if (template.EnumEntries != null)
        {
            ImGui.Text("");
        }

        // Alias Enum
        if (template.AliasEnum != null)
        {
            ImGui.Text("");
        }

        // Project Enum
        if (template.ProjectEnum != null)
        {
            ImGui.Text("");
        }
    }

    public void HandleValueColumn(Dictionary<string, object> propertyParameters, int index)
    {
        var paramValues = Editor.Selection.CurrentTimeActEvent.Parameters.ParameterValues;
        var parameters = Editor.Selection.CurrentTimeActEvent.Parameters;
        var propertyName = propertyParameters.ElementAt(index).Key;
        var propertyValue = propertyParameters[propertyName];
        var template = parameters.GetParamTemplate(propertyName);

        if (Editor.Project.ParamEditor != null)
        {
            // Param Reference
            if (template.ParamRef != null)
            {
                var primaryBank = Editor.Project.ParamData.PrimaryBank;
                (string, Param.Row, string) match = ResolveParamRef(Editor.Project.ParamEditor, primaryBank, template.ParamRef, propertyValue);
                if (match != (null, null, null))
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRef_Text);
                    ImGui.Text(match.Item3);
                    ImGui.PopStyleColor();
                    if (ImGui.BeginPopupContextItem($"valueParamRefContextMenu{propertyName}"))
                    {
                        if (ImGui.Selectable($"Go to {match.Item2.ID} ({match.Item3})"))
                        {
                            EditorCommandQueue.AddCommand($@"param/select/-1/{match.Item1}/{match.Item2.ID}");
                        }

                        ImGui.EndPopup();
                    }
                }
                else
                {
                    ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRefMissing_Text);
                    ImGui.Text("___");
                    ImGui.PopStyleColor();
                }
            }
        }

        // Enum List
        if (template.EnumEntries != null)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumValue_Text);
            if (template.EnumEntries.ContainsKey(propertyValue))
            {
                var result = template.EnumEntries[propertyValue];
                ImGui.Text($"{result}");
            }
            else
            {
                ImGui.Text($"Not Enumerated");
            }
            ImGui.PopStyleColor();

            if (ImGui.BeginPopupContextItem($"valueEnumContextMenu{propertyName}"))
            {
                ImGui.InputTextMultiline($"##enumSearch{propertyName}", ref _enumSearchInput, 255, new Vector2(350, 20), ImGuiInputTextFlags.CtrlEnterForNewLine);

                var enumListHeight = ImGui.GetTextLineHeightWithSpacing() * Math.Min(12, template.EnumEntries.Keys.Count);

                if (ImGui.BeginChild($"EnumList{propertyName}", new Vector2(350, enumListHeight)))
                {
                    foreach(var entry in template.EnumEntries.Keys)
                    {
                        var result = template.EnumEntries[entry];

                        if (SearchFilters.IsEditorSearchMatch(_enumSearchInput, result, " ")
                            || SearchFilters.IsEditorSearchMatch(_enumSearchInput, entry.ToString(), " ")
                            || _enumSearchInput == "")
                        {
                            if (ImGui.Selectable($"{entry}: {result}"))
                            {
                                var action = new TaeEventParametersChange(paramValues, propertyName, propertyValue, entry, propertyValue.GetType());
                                Editor.EditorActionManager.ExecuteAction(action);
                            }
                        }
                    }
                }
                ImGui.EndChild();

                ImGui.EndPopup();
            }
        }

        // Alias Enum
        if (template.AliasEnum != null)
        {
            var aliasType = template.AliasEnum;
            List<AliasEntry> aliases = new List<AliasEntry>();

            // Particle
            if (aliasType == "Particle")
            {
                aliases = Editor.Project.Aliases.Particles;
            }
            // Sound
            if (aliasType == "Sound")
            {
                aliases = Editor.Project.Aliases.Sounds;
            }

            if (aliases.Count > 0)
            {

                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumValue_Text);
                if (aliases.Contains(propertyValue))
                {
                    var result = aliases.Where(e => e.ID == propertyValue.ToString()).FirstOrDefault();
                    ImGui.Text($"{result}");
                }
                else
                {
                    ImGui.Text($"Not Enumerated");
                }
                ImGui.PopStyleColor();

                if (ImGui.BeginPopupContextItem($"valueAliasEnumContextMenu{propertyName}"))
                {
                    ImGui.InputTextMultiline($"##enumSearch{propertyName}", ref _enumSearchInput, 255, new Vector2(350, 20), ImGuiInputTextFlags.CtrlEnterForNewLine);

                    var enumListHeight = ImGui.GetTextLineHeightWithSpacing() * Math.Min(12, aliases.Count);

                    if (ImGui.BeginChild($"EnumList{propertyName}", new Vector2(350, enumListHeight)))
                    {
                        foreach (var entry in aliases)
                        {
                            if (SearchFilters.IsEditorSearchMatch(_enumSearchInput, entry.ID, " ")
                                || SearchFilters.IsEditorSearchMatch(_enumSearchInput, entry.Name, " ")
                                || _enumSearchInput == "")
                            {
                                if (ImGui.Selectable($"{entry.ID}: {entry.Name}"))
                                {
                                    var action = new TaeEventParametersChange(paramValues, propertyName, propertyValue, entry.ID, propertyValue.GetType());
                                    Editor.EditorActionManager.ExecuteAction(action);
                                }
                            }
                        }
                    }
                    ImGui.EndChild();

                    ImGui.EndPopup();
                }
            }
        }

        // Project Enum
        if (template.ProjectEnum != null)
        {
            var enumType = template.ProjectEnum;
            ProjectEnumEntry enumEntries = Editor.Project.ProjectEnums.List.Where(e => e.Name == enumType).FirstOrDefault();
            ProjectEnumOption targetOption = enumEntries.Options.Where(e => e.ID == propertyValue.ToString()).FirstOrDefault();

            if (enumEntries != null)
            {
                ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumValue_Text);
                if (targetOption != null)
                {
                    ImGui.Text($"{targetOption.Name}");
                }
                else
                {
                    ImGui.Text($"Not Enumerated");
                }
                ImGui.PopStyleColor();

                if (ImGui.BeginPopupContextItem($"valueProjectEnumContextMenu{propertyName}"))
                {
                    ImGui.InputTextMultiline($"##enumSearch{propertyName}", ref _enumSearchInput, 255, new Vector2(350, 20), ImGuiInputTextFlags.CtrlEnterForNewLine);

                    var enumListHeight = ImGui.GetTextLineHeightWithSpacing() * Math.Min(12, enumEntries.Options.Count);

                    if (ImGui.BeginChild($"EnumList{propertyName}", new Vector2(350, enumListHeight)))
                    {
                        foreach (var entry in enumEntries.Options)
                        {
                            if (SearchFilters.IsEditorSearchMatch(_enumSearchInput, entry.ID, " ")
                                || SearchFilters.IsEditorSearchMatch(_enumSearchInput, entry.Name, " ")
                                || _enumSearchInput == "")
                            {
                                if (ImGui.Selectable($"{entry.ID}: {entry.Name}"))
                                {
                                    var action = new TaeEventParametersChange(paramValues, propertyName, propertyValue, entry.ID, propertyValue.GetType());
                                    Editor.EditorActionManager.ExecuteAction(action);
                                }
                            }
                        }
                    }
                    ImGui.EndChild();

                    ImGui.EndPopup();
                }
            }
        }
    }

    private static (string, Param.Row, string) ResolveParamRef(ParamEditorScreen editor, ParamBank bank, string paramRef, dynamic oldval)
    {
        (string, Param.Row, string) row = new();
        if (bank.Params == null)
        {
            return row;
        }

        var originalValue = (int)oldval; //make sure to explicitly cast from dynamic or C# complains. Object or Convert.ToInt32 fail.

        var hint = "";
        if (bank.Params.ContainsKey(paramRef))
        {
            var altval = originalValue;

            Param param = bank.Params[paramRef];
            var meta = editor.Project.ParamData.GetParamMeta(bank.Params[paramRef].AppliedParamdef);
            if (meta != null && meta.Row0Dummy && altval == 0)
            {
                return row;
            }

            Param.Row r = param[altval];
            if (r == null && altval > 0 && meta != null)
            {
                if (meta.FixedOffset != 0)
                {
                    altval = originalValue + meta.FixedOffset;
                    hint += meta.FixedOffset > 0 ? "+" + meta.FixedOffset : meta.FixedOffset.ToString();
                }

                if (meta.OffsetSize > 0)
                {
                    altval = altval - (altval % meta.OffsetSize);
                    hint += "+" + (originalValue % meta.OffsetSize);
                }

                r = bank.Params[paramRef][altval];
            }

            if (r == null)
            {
                return row;
            }

            if (string.IsNullOrWhiteSpace(r.Name))
            {
                row = ((paramRef, r, "Unnamed Row" + hint));
            }
            else
            {
                row = ((paramRef, r, r.Name + hint));
            }
        }

        return row;
    }
}
