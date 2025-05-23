#nullable enable
using Andre.Formats;
using Hexa.NET.ImGui;
using Hexa.NET.ImPlot;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor.Data;
using StudioCore.Editors.ParamEditor.META;
using StudioCore.Editors.TextEditor.Utils;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
using StudioCore.Platform;
using StudioCore.Tasks;
using StudioCore.TextureViewer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using Veldrid;

namespace StudioCore.Editors.ParamEditor.Decorators;

public class FieldDecorators
{
    private static string _refContextCurrentAutoComplete = "";
    public static string enumSearchStr = "";

    #region Decorator
    public static bool Decorator_ContextMenuItems(ParamEditorScreen editor, ParamBank bank, ParamFieldMeta cellMeta, object oldval, ref object newval, List<ParamRef> RefTypes, Param.Row context, List<FMGRef> fmgRefs, List<FMGRef> mapFmgRefs, List<TexRef> textureRefs, ParamEnum Enum, ActionManager executor)
    {
        var result = false;
        if (RefTypes != null)
        {
            result |= ParamReference_ContextMenuItems(editor, bank, RefTypes, context, oldval, ref newval, executor);
        }

        if (fmgRefs != null)
        {
            TextReference_ContextMenuItems(editor, fmgRefs, context, oldval, executor);
        }

        if (mapFmgRefs != null)
        {
            TextReference_ContextMenuItems(editor, mapFmgRefs, context, oldval, executor);
        }

        if (textureRefs != null)
        {
            TextureReference_ContextMenuItems(editor, textureRefs, context, executor);
        }

        if (Enum != null)
        {
            result |= Enum_ContextMenuItems(Enum, oldval, ref newval);
        }

        if (cellMeta != null)
        {
            if (cellMeta.ShowParticleEnumList)
            {
                result |= AliasEnum_ContextMenuItems(editor.Project.Aliases.Particles, oldval, ref newval);
            }

            if (cellMeta.ShowSoundEnumList)
            {
                result |= AliasEnum_ContextMenuItems(editor.Project.Aliases.Sounds, oldval, ref newval);
            }

            if (cellMeta.ShowFlagEnumList)
            {
                result |= AliasEnum_ContextMenuItems(editor.Project.Aliases.EventFlags, oldval, ref newval);
            }

            if (cellMeta.ShowCutsceneEnumList)
            {
                result |= AliasEnum_ContextMenuItems(editor.Project.Aliases.Cutscenes, oldval, ref newval);
            }

            if (cellMeta.ShowMovieEnumList)
            {
                result |= AliasEnum_ContextMenuItems(editor.Project.Aliases.Movies, oldval, ref newval);
            }

            if (cellMeta.ShowProjectEnumList && cellMeta.EnumType != null)
            {
                var optionList = editor.Project.ProjectEnums.List.Where(e => e.Name == cellMeta.EnumType.Name).FirstOrDefault();

                if (optionList != null)
                {
                    result |= ProjectEnum_ContextMenuItems(optionList, oldval, ref newval);
                }
            }
        }

        return result;
    }
    #endregion

    #region Enum
    /// <summary>
    /// The title column decoration for the Enum decorator.
    /// </summary>
    /// <param name="pEnum"></param>
    public static void Enum_Title(ParamEnum pEnum)
    {
        if (!CFG.Current.Param_ShowFieldEnumLabels)
        {
            return;
        }

        if (pEnum != null && pEnum.Name != null)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumName_Text);
            ImGui.TextUnformatted($@"   {pEnum.Name}");
            ImGui.PopStyleColor();
        }
    }

    /// <summary>
    /// The value column decoration for the Enum decorator.
    /// </summary>
    /// <param name="enumValues"></param>
    /// <param name="value"></param>
    public static void Enum_Value(Dictionary<string, string> enumValues, string value)
    {
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumValue_Text);
        ImGui.TextUnformatted(enumValues.GetValueOrDefault(value, "Not Enumerated"));
        ImGui.PopStyleColor();
    }

    /// <summary>
    /// The context menu items for the Enum decorator
    /// </summary>
    /// <param name="en"></param>
    /// <param name="oldval"></param>
    /// <param name="newval"></param>
    /// <returns></returns>
    public static bool Enum_ContextMenuItems(ParamEnum en, object oldval, ref object newval)
    {
        ImGui.InputTextMultiline("##enumSearch", ref enumSearchStr, 255, new Vector2(350, 20), ImGuiInputTextFlags.CtrlEnterForNewLine);

        if (ImGui.BeginChild("EnumList", new Vector2(350, ImGui.GetTextLineHeightWithSpacing() * Math.Min(12, en.Values.Count))))
        {
            try
            {
                foreach (KeyValuePair<string, string> option in en.Values)
                {
                    if (SearchFilters.IsEditorSearchMatch(enumSearchStr, option.Key, " ")
                        || SearchFilters.IsEditorSearchMatch(enumSearchStr, option.Value, " ")
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
    #endregion

    #region Project Enum
    /// <summary>
    /// The title column decoration for the Project Enum decorator
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="enumType"></param>
    public static void ProjectEnum_Title(ParamEditorScreen editor, string enumType)
    {
        if (!CFG.Current.Param_ShowFieldEnumLabels)
        {
            return;
        }

        var enumEntry = editor.Project.ProjectEnums.List.Where(e => e.Name == enumType).FirstOrDefault();

        if (enumEntry != null)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumName_Text);
            ImGui.TextUnformatted($@"   {enumEntry.DisplayName}");
            ImGui.PopStyleColor();

            if (enumEntry.Description != "")
            {
                UIHelper.Tooltip($"{enumEntry.Description}");
            }
        }
    }

    /// <summary>
    /// The value column decoration for the Project Enum decorator
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="enumType"></param>
    /// <param name="value"></param>
    public static void ProjectEnum_Value(ParamEditorScreen editor, string enumType, string value)
    {
        if (CFG.Current.Param_HideEnums == false) //Move preference
        {
            var enumEntry = editor.Project.ProjectEnums.List.Where(e => e.Name == enumType).FirstOrDefault();

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
    }

    /// <summary>
    /// The context menu items for the Project Enum decorator
    /// </summary>
    /// <param name="en"></param>
    /// <param name="oldval"></param>
    /// <param name="newval"></param>
    /// <returns></returns>
    public static bool ProjectEnum_ContextMenuItems(ProjectEnumEntry en, object oldval, ref object newval)
    {
        ImGui.InputTextMultiline("##enumSearch", ref enumSearchStr, 255, new Vector2(350, 20), ImGuiInputTextFlags.CtrlEnterForNewLine);

        if (ImGui.BeginChild("EnumList", new Vector2(350, ImGui.GetTextLineHeightWithSpacing() * Math.Min(12, en.Options.Count))))
        {
            try
            {
                foreach (var option in en.Options)
                {
                    if (SearchFilters.IsEditorSearchMatch(enumSearchStr, option.ID, " ")
                        || SearchFilters.IsEditorSearchMatch(enumSearchStr, option.Name, " ")
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
    #endregion

    #region Alias Enum
    /// <summary>
    /// The title column decoration for the Alias Enum decorator.
    /// </summary>
    /// <param name="name"></param>
    public static void AliasEnum_Title(string name)
    {
        if (!CFG.Current.Param_ShowFieldEnumLabels)
        {
            return;
        }

        var inactiveEnum = false;

        if (!inactiveEnum)
        {
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_EnumName_Text);
            ImGui.TextUnformatted($@"   {name}");
            ImGui.PopStyleColor();
        }
    }

    /// <summary>
    /// The value column decoration for the Alias Enum decorator.
    /// </summary>
    /// <param name="entries"></param>
    /// <param name="value"></param>
    public static void AliasEnum_Value(List<AliasEntry> entries, string value)
    {
        var inactiveEnum = false;

        if (!CFG.Current.Param_HideEnums) //Move preference
        {
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
                ImGui.PopStyleColor();
            }
        }
    }

    public static bool AliasEnum_ContextMenuItems(List<AliasEntry> entries, object oldval, ref object newval)
    {
        ImGui.InputTextMultiline("##enumSearch", ref enumSearchStr, 255, new Vector2(350, 20), ImGuiInputTextFlags.CtrlEnterForNewLine);

        if (ImGui.BeginChild("EnumList", new Vector2(350, ImGui.GetTextLineHeightWithSpacing() * Math.Min(12, entries.Count))))
        {
            try
            {
                foreach (var entry in entries)
                {
                    if (SearchFilters.IsEditorSearchMatch(enumSearchStr, entry.ID, " ")
                        || SearchFilters.IsEditorSearchMatch(enumSearchStr, entry.Name, " ")
                        || enumSearchStr == "")
                    {
                        if (ImGui.Selectable($"{entry.ID}: {entry.Name}"))
                        {
                            newval = Convert.ChangeType(entry.ID, oldval.GetType());
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

    #endregion

    #region Conditional Alias Enum
    /// <summary>
    /// The title column decoration for the Conditional Alias Enum decorator.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="row"></param>
    /// <param name="limitField"></param>
    /// <param name="limitValue"></param>
    public static void ConditionalAliasEnum_Title(string name, Param.Row row, string limitField, string limitValue)
    {
        if (!CFG.Current.Param_ShowFieldEnumLabels)
        {
            return;
        }

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
            ImGui.PopStyleColor();
        }
    }

    /// <summary>
    /// The value column decoration for the Conditional Alias Enum decorator.
    /// </summary>
    /// <param name="entries"></param>
    /// <param name="value"></param>
    /// <param name="row"></param>
    /// <param name="conditionalField"></param>
    /// <param name="conditionalValue"></param>
    public static void ConditionalAliasEnum_Value(List<AliasEntry> entries, string value, Param.Row row, string conditionalField, string conditionalValue)
    {
        var inactiveEnum = false;

        if (conditionalField != "")
        {
            Param.Cell? c = row?[conditionalField];
            inactiveEnum = row != null && c != null && Convert.ToInt32(c.Value.Value) != Convert.ToInt32(conditionalValue);
        }

        if (CFG.Current.Param_HideEnums == false) //Move preference
        {
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
                ImGui.PopStyleColor();
            }
        }
    }
    #endregion

    #region Param Reference

    /// <summary>
    /// The title column decoration for the Param Reference decorator.
    /// </summary>
    /// <param name="paramRefs"></param>
    /// <param name="context"></param>
    public static void ParamReference_Title(List<ParamRef> paramRefs, Param.Row context)
    {
        if (paramRefs == null || paramRefs.Count == 0)
        {
            return;
        }

        if (!CFG.Current.Param_ShowFieldParamLabels)
        {
            return;
        }

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.TextUnformatted(@"   <");

        List<string> inactiveRefs = new();
        var first = true;
        foreach (ParamRef r in paramRefs)
        {
            if (context == null)
                continue;

            var inactiveRef = false;

            if (r.ConditionField != null)
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
        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
    }

    /// <summary>
    /// The value column decoration for the Param Reference decorator.
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="bank"></param>
    /// <param name="paramRefs"></param>
    /// <param name="context"></param>
    /// <param name="oldval"></param>
    public static void ParamReference_Value(ParamEditorScreen editor, ParamBank bank, List<ParamRef> paramRefs, Param.Row context,
        dynamic oldval)
    {
        if (paramRefs == null)
        {
            return;
        }

        // Add named row and context menu
        // Lists located params
        // May span lines
        List<(string, Param.Row, string)> matches = ReferenceResolver.ResolveParamReferences(editor, bank, paramRefs, context, oldval);

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

    /// <summary>
    /// The context menu in the value column for the Param Reference decorator.
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="bank"></param>
    /// <param name="oldval"></param>
    /// <param name="RefTypes"></param>
    /// <param name="context"></param>
    /// <param name="fmgRefs"></param>
    /// <param name="Enum"></param>
    /// <param name="textureRefs"></param>
    public static void ParamReference_ContextMenu(ParamEditorScreen editor, ParamBank bank, object oldval, Param.Row context, List<ParamRef> RefTypes)
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left) &&
            (InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight)))
        {
            if (RefTypes != null)
            {
                (string, Param.Row, string)? primaryRef =
                    ReferenceResolver.ResolveParamReferences(editor, bank, RefTypes, context, oldval)?.FirstOrDefault();

                if (primaryRef?.Item2 != null)
                {
                    if (InputTracker.GetKey(Key.ShiftLeft) || InputTracker.GetKey(Key.ShiftRight))
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
    }

    /// <summary>
    /// Shortcut items for the Param Reference decorator
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="bank"></param>
    /// <param name="cellMeta"></param>
    /// <param name="oldval"></param>
    /// <param name="newval"></param>
    /// <param name="RefTypes"></param>
    /// <param name="context"></param>
    /// <param name="fmgRefs"></param>
    /// <param name="mapFmgRefs"></param>
    /// <param name="textureRefs"></param>
    /// <param name="Enum"></param>
    /// <param name="executor"></param>
    /// <returns></returns>
    public static bool ParamReference_ShortcutItems(ParamEditorScreen editor, ParamBank bank, ParamFieldMeta cellMeta, object oldval, ref object newval, List<ParamRef> RefTypes, Param.Row context, List<FMGRef> fmgRefs, List<FMGRef> mapFmgRefs, List<TexRef> textureRefs, ParamEnum Enum, ActionManager executor)
    {
        var result = false;

        if (!ImGui.IsAnyItemActive())
        {
            if (RefTypes != null)
            {
                if (bank.Params == null)
                {
                    return false;
                }

                if (InputTracker.GetKeyDown(KeyBindings.Current.PARAM_InheritReferencedRowName))
                {
                    List<(string, Param.Row, string)> refs = ReferenceResolver.ResolveParamReferences(editor, bank, RefTypes, context, oldval);

                    foreach ((string, Param.Row, string) rf in refs)
                    {
                        if (context == null || executor == null)
                        {
                            continue;
                        }

                        executor.ExecuteAction(new PropertiesChangedAction(context.GetType().GetProperty("Name"), context,
                                rf.Item2.Name));
                    }

                    result = true;
                }
            }
        }

        return result;
    }

    /// <summary>
    /// The context menu options for the Param Reference decorator
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="bank"></param>
    /// <param name="reftypes"></param>
    /// <param name="context"></param>
    /// <param name="oldval"></param>
    /// <param name="newval"></param>
    /// <param name="executor"></param>
    /// <returns></returns>
    public static bool ParamReference_ContextMenuItems(ParamEditorScreen editor, ParamBank bank, List<ParamRef> reftypes, Param.Row context,
        object oldval, ref object newval, ActionManager executor)
    {
        if (bank.Params == null)
        {
            return false;
        }

        // Add Goto statements
        List<(string, Param.Row, string)> refs = ReferenceResolver.ResolveParamReferences(editor, bank, reftypes, context, oldval);
        var ctrlDown = InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight);

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
                (ctrlDown || string.IsNullOrWhiteSpace(context.Name)) &&
                ImGui.Selectable($@"Inherit referenced row's name ({rf.Item2.Name})##InheritName{index}"))
            {
                executor.ExecuteAction(new PropertiesChangedAction(context.GetType().GetProperty("Name"), context,
                    rf.Item2.Name));
            }
            else if ((ctrlDown || string.IsNullOrWhiteSpace(rf.Item2.Name)) &&
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
                if (!bank.Params.ContainsKey(rt))
                {
                    continue;
                }

                var meta = editor.Project.ParamData.GetParamMeta(bank.Params[rt].AppliedParamdef);
                var maxResultsPerRefType = 15 / reftypes.Count;
                List<Param.Row> rows = editor.MassEditHandler.rse.Search((bank, bank.Params[rt]),
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

    #endregion

    #region Virtual Param Reference
    public static void VirtualParamReference_ContextMenu(ParamEditorScreen editor, ParamBank bank, string virtualRefName, object searchValue,
        Param.Row context, string fieldName)
    {
        // Add Goto statements
        if (bank.Params != null)
        {
            foreach (KeyValuePair<string, Param> param in bank.Params)
            {
                var curMeta = editor.Project.ParamData.GetParamMeta(param.Value.AppliedParamdef);

                var paramdef = param.Value.AppliedParamdef;

                if (paramdef == null)
                    continue;

                foreach (PARAMDEF.Field f in paramdef.Fields)
                {
                    var curFieldMeta = editor.Project.ParamData.GetParamFieldMeta(curMeta, f);

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
    #endregion

    #region External Reference
    public static void ExternalReference_ContextMenu(ParamEditorScreen editor, ParamBank bank, string virtualRefName, object searchValue,
        Param.Row context, string fieldName, List<ExtRef> ExtRefs)
    {
        if (ExtRefs != null)
        {
            foreach (ExtRef currentRef in ExtRefs)
            {
                List<string> matchedExtRefPath =
                    currentRef.paths.Select(x => string.Format(x, searchValue)).ToList();
                ExtRefItem(context, fieldName, $"modded {currentRef.name}", matchedExtRefPath, editor.Project.ProjectPath,
                    editor);

                ExtRefItem(context, fieldName, $"vanilla {currentRef.name}", matchedExtRefPath,
                    editor.Project.DataPath, editor);
            }
        }
    }

    private static void ExtRefItem(Param.Row keyRow, string fieldKey, string menuText,
        List<string> matchedExtRefPath, string dir, EditorScreen cacheOwner)
    {
        var exist = UICache.GetCached(cacheOwner, keyRow, $"extRef{menuText}{fieldKey}",
            () => Path.Exists(Path.Join(dir, matchedExtRefPath[0])));
        if (exist && ImGui.Selectable($"Go to {menuText} file..."))
        {
            var path = ReferenceResolver.ResolveExternalReferences(matchedExtRefPath, dir);
            if (File.Exists(path))
            {
                Process.Start("explorer.exe", $"/select,\"{path}\"");
            }
            else
            {
                TaskLogs.AddLog($"\"{path}\" could not be found. It may be map or chr specific",
                    LogLevel.Warning);
                UICache.ClearCaches();
            }
        }
    }

    #endregion

    #region Text Reference
    /// <summary>
    /// The title column decoration for the Text Reference decorator.
    /// </summary>
    /// <param name="fmgRef"></param>
    /// <param name="context"></param>
    /// <param name="overrideName"></param>
    public static void TextReference_Title(List<FMGRef> fmgRef, Param.Row context, string overrideName = "")
    {
        if (fmgRef == null)
        {
            return;
        }

        if (!CFG.Current.Param_ShowFieldFmgLabels)
        {
            return;
        }

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);

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

        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
    }

    /// <summary>
    /// The value column decoration for the Text Reference decorator.
    /// </summary>
    /// <param name="ownerScreen"></param>
    /// <param name="fmgNames"></param>
    /// <param name="context"></param>
    /// <param name="oldval"></param>
    public static void TextReference_Value(EditorScreen ownerScreen, List<FMGRef> fmgNames, Param.Row context,
        dynamic oldval)
    {
        if (ownerScreen is not ParamEditorScreen)
        {
            return;
        }

        var paramEditor = ownerScreen as ParamEditorScreen;

        List<string> textsToPrint = new List<string>();

        textsToPrint = UICache.GetCached(paramEditor, (int)oldval, "PARAM META FMGREF", () =>
        {
            List<TextResult> refs = ReferenceResolver.ResolveTextReferences(paramEditor, fmgNames, context, oldval);
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

    /// <summary>
    /// The context menu in the value column for the Text Reference decorator.
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="bank"></param>
    /// <param name="oldval"></param>
    /// <param name="context"></param>
    /// <param name="fmgRefs"></param>
    public static void TextReference_ContextMenu(ParamEditorScreen editor, ParamBank bank, object oldval, Param.Row context, List<FMGRef> fmgRefs)
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left) &&
            (InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight)))
        {
            if (fmgRefs != null)
            {
                TextResult? primaryRef = ReferenceResolver.ResolveTextReferences(editor, fmgRefs, context, oldval)?.FirstOrDefault();
                if (primaryRef != null)
                {
                    EditorCommandQueue.AddCommand($@"text/select/{primaryRef.ContainerWrapper.FileEntry.Filename}/{primaryRef.FmgName}/{primaryRef.Entry.ID}");
                }
            }
        }
    }

    /// <summary>
    /// The context menu options for the Text Reference decorator
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="reftypes"></param>
    /// <param name="context"></param>
    /// <param name="oldval"></param>
    /// <param name="executor"></param>
    public static void TextReference_ContextMenuItems(ParamEditorScreen editor, List<FMGRef> reftypes, Param.Row context, dynamic oldval,
        ActionManager executor)
    {
        // Add Goto statements
        List<TextResult> refs = ReferenceResolver.ResolveTextReferences(editor, reftypes, context, oldval);

        var ctrlDown = InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight);

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
    }

    #endregion

    #region Texture Reference
    /// <summary>
    /// The title column decoration for the Texture Reference decorator.
    /// </summary>
    /// <param name="textureRef"></param>
    /// <param name="context"></param>
    public static void TextureReference_Title(List<TexRef> textureRef, Param.Row context)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (textureRef == null)
        {
            return;
        }

        if (!CFG.Current.Param_ShowFieldTextureLabels)
        {
            return;
        }

        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, 0));
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);

        ImGui.TextUnformatted(@"   [Image]");

        ImGui.PopStyleColor();
        ImGui.PopStyleVar();
    }

    /// <summary>
    /// The value column decoration for the Texture Reference decorator.
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="textureViewer"></param>
    /// <param name="texRefs"></param>
    /// <param name="context"></param>
    /// <param name="oldval"></param>
    public static void TextureReference_Value(ParamEditorScreen editor, TextureViewerScreen textureViewer, List<TexRef> texRefs, Param.Row context,
        dynamic oldval)
    {
        if (editor.Project.TextureViewer == null)
            return;

        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        if (textureViewer.ImagePreview == null)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_FmgRef_Text);

        ImGui.TextUnformatted("View Source Image");

        foreach (var texRef in texRefs)
        {
            if (CFG.Current.Param_FieldContextMenu_ImagePreview_FieldColumn)
            {
                var imageDisplayed = textureViewer.ImagePreview.DisplayImagePreview(context, texRef);

                // If an image has been displayed, exit the loop so we don't show multiple images
                if (imageDisplayed)
                    break;
            }
        }

        ImGui.PopStyleColor();
    }

    /// <summary>
    /// The context menu in the value column for the Texture Reference decorator.
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="bank"></param>
    /// <param name="oldval"></param>
    /// <param name="context"></param>
    /// <param name="textureRefs"></param>
    public static void TextureReference_ContextMenu(ParamEditorScreen editor, ParamBank bank, object oldval,  Param.Row context, List<TexRef> textureRefs)
    {
        if (ImGui.IsItemClicked(ImGuiMouseButton.Left) &&
            (InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight)))
        {
            
            if (textureRefs != null && textureRefs.Count > 0)
            {
                TexRef primaryRef = textureRefs.First();
                if (primaryRef?.TextureContainer != null && primaryRef?.TextureFile != null)
                {
                    EditorCommandQueue.AddCommand($@"texture/view/{primaryRef?.TextureContainer}/{primaryRef?.TextureFile}");
                }
            }
        }
    }

    /// <summary>
    /// The context menu items for the Texture Referene decorator.
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="reftypes"></param>
    /// <param name="context"></param>
    /// <param name="executor"></param>
    public static void TextureReference_ContextMenuItems(ParamEditorScreen editor, List<TexRef> reftypes, Param.Row context, ActionManager executor)
    {
        // Required to stop the LowRequirements build from failing
        if (Smithbox.LowRequirementsMode)
            return;

        var ctrlDown = InputTracker.GetKey(Key.ControlLeft) || InputTracker.GetKey(Key.ControlRight);

        foreach (var textureRef in reftypes)
        {
            if (editor.Project.TextureViewer != null)
            {
                bool displayedImage = editor.Project.TextureViewer.ImagePreview.DisplayImagePreview(context, textureRef, false);

                if (displayedImage)
                {
                    if (ImGui.Selectable($@"View {textureRef.TextureFile}"))
                    {
                        EditorCommandQueue.AddCommand($@"texture/view/{textureRef.TextureContainer}/{textureRef.TextureFile}");
                    }
                }
            }

            if (context == null || executor == null)
            {
                continue;
            }
        }
    }

    #endregion

    #region Field Offset

    /// <summary>
    /// The title column decoration for the Param Field Offset decorator.
    /// </summary>
    /// <param name="activeParam"></param>
    /// <param name="context"></param>
    /// <param name="index"></param>
    public static void ParamFieldOffset_Title(string activeParam, Param.Row context, string index)
    {
        // This feature is purely for AC6 MenuPropertySpecParam.
        if (activeParam == "MenuPropertySpecParam")
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(0, ImGui.GetStyle().ItemSpacing.Y));
            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
            ImGui.TextUnformatted(@"   <PARAM>");
            ImGui.TextUnformatted(@"   <FIELD>");
            if (CFG.Current.Param_MakeMetaNamesPrimary)
            {
                ImGui.TextUnformatted(@"   <NAME>");
            }
            ImGui.PopStyleColor();
            ImGui.PopStyleVar();
        }
    }

    /// <summary>
    /// The value column decoration for the Param Field Offset decorator.
    /// </summary>
    /// <param name="editor"></param>
    /// <param name="activeParam"></param>
    /// <param name="context"></param>
    /// <param name="index"></param>
    public static void ParamFieldOffset_Value(ParamEditorScreen editor, string activeParam, Param.Row context, string index)
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

            var firstRow = editor.Project.ParamData.PrimaryBank.Params[paramString].Rows.First();
            var internalName = "";
            var displayName = "";

            var targetMeta = editor.Project.ParamData.GetParamMeta(firstRow.Def);

            foreach (var col in firstRow.Columns)
            {
                var offset = (int)col.GetByteOffset();

                if (offset == decimalOffset)
                {
                    internalName = col.Def.InternalName;

                    var cellmeta = editor.Project.ParamData.GetParamFieldMeta(targetMeta, col.Def);
                    displayName = cellmeta.AltName;
                }
            }

            ImGui.BeginGroup();

            ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_ParamRef_Text);

            ImGui.TextUnformatted($"{paramString}:");
            ImGui.TextUnformatted($"{internalName}");
            if (CFG.Current.Param_MakeMetaNamesPrimary)
            {
                ImGui.TextUnformatted($"{displayName}");
            }

            ImGui.PopStyleColor();

            ImGui.EndGroup();
        }
    }
    #endregion

    #region Param Reverse Lookup
    public static void ParamReverseLookup_Value(EditorScreen screen, ParamBank bank, string currentParam,
        int currentID)
    {
        if (ImGui.BeginMenu("Search for references"))
        {
            Dictionary<string, List<(string, ParamRef)>> items = UICache.GetCached(screen, (bank, currentParam),
                () => ParamRefReverseLookupFieldItems((ParamEditorScreen)screen, bank, currentParam));

            foreach (KeyValuePair<string, List<(string, ParamRef)>> paramitems in items)
            {
                if (ImGui.BeginMenu($@"in {paramitems.Key}..."))
                {
                    foreach ((var fieldName, ParamRef pref) in paramitems.Value)
                    {
                        if (ImGui.BeginMenu($@"in {fieldName}"))
                        {
                            List<Param.Row> rows = UICache.GetCached(screen, (bank, currentParam, currentID, paramitems.Key, fieldName),
                                () => ParamRefReverseLookupRowItems((ParamEditorScreen)screen, bank, paramitems.Key, fieldName, currentID,
                                    pref));
                            foreach (Param.Row row in rows)
                            {
                                var nameToPrint = string.IsNullOrEmpty(row.Name) ? "Unnamed Row" : row.Name;
                                if (ImGui.Selectable($@"{row.ID} {nameToPrint}"))
                                {
                                    EditorCommandQueue.AddCommand($@"param/select/-1/{paramitems.Key}/{row.ID}");
                                }
                            }

                            if (rows.Count == 0)
                            {
                                ImGui.TextUnformatted("No rows found");
                            }

                            ImGui.EndMenu();
                        }
                    }

                    ImGui.EndMenu();
                }
            }

            if (items.Count == 0)
            {
                ImGui.TextUnformatted("This param is not referenced");
            }

            ImGui.EndMenu();
        }
    }

    private static Dictionary<string, List<(string, ParamRef)>> ParamRefReverseLookupFieldItems(ParamEditorScreen editor, ParamBank bank,
        string currentParam)
    {
        Dictionary<string, List<(string, ParamRef)>> items = new();
        foreach (KeyValuePair<string, Param> param in bank.Params)
        {
            List<(string, ParamRef)> paramitems = new();

            var curMeta = editor.Project.ParamData.GetParamMeta(param.Value.AppliedParamdef);

            if (param.Value.AppliedParamdef == null)
                continue;

            //get field
            foreach (PARAMDEF.Field f in param.Value.AppliedParamdef.Fields)
            {
                var meta = editor.Project.ParamData.GetParamFieldMeta(curMeta, f);
                if (meta.RefTypes == null)
                {
                    continue;
                }

                // get hilariously deep in loops
                foreach (ParamRef pref in meta.RefTypes)
                {
                    if (!pref.ParamName.Equals(currentParam))
                    {
                        continue;
                    }

                    paramitems.Add((f.InternalName, pref));
                }
            }

            if (paramitems.Count > 0)
            {
                items[param.Key] = paramitems;
            }
        }

        return items;
    }

    private static List<Param.Row> ParamRefReverseLookupRowItems(ParamEditorScreen editor, ParamBank bank, string paramName, string fieldName,
        int currentID, ParamRef pref)
    {
        var searchTerm = pref.ConditionField != null
            ? $@"prop {fieldName} ^{currentID}$ && prop {pref.ConditionField} ^{pref.ConditionValue}$"
            : $@"prop {fieldName} ^{currentID}$";
        return editor.MassEditHandler.rse.Search((bank, bank.Params[paramName]), searchTerm, false, false);
    }

    #endregion

    #region Calculation Correction Graph
    public static unsafe void DrawCalcCorrectGraph(ParamEditorScreen editor, ParamMeta meta, Param.Row row, Vector2 graphSize)
    {
        try
        {
            bool draw = true;

            var graphName = "Graph";
            var xAxisTitle = "";
            var yAxisTitle = "";

            if (editor.Project.ParamData.GraphLegends != null)
            {
                var entry = editor.Project.ParamData.GraphLegends.Entries
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

            double[]? values = null;
            int xOffset = 0;
            double minY = 0;
            double maxY = 0;

            if (draw)
            {
                if (scd != null && scd.cost_row == row.ID)
                {
                    (values, maxY) = UICache.GetCached(editor, row, "soulCostData", () => ParamUtils.getSoulCostData(scd, row));
                }
                else if (ccd != null)
                {
                    (values, xOffset, minY, maxY) = UICache.GetCached(editor, row, "calcCorrectData",
                        () => ParamUtils.getCalcCorrectedData(ccd, row));
                }

                // Input validation
                if (values == null || values.Length < 2 || values.Any(v => double.IsNaN(v) || double.IsInfinity(v)))
                {
                    ImGui.TextColored(new Vector4(1, 0.5f, 0, 1), "Invalid data range for graphing.");
                    ImGui.Unindent();
                    return;
                }

                double[] xValues = (scd != null)
                    ? Enumerable.Range(0, values.Length).Select(i => (double)i).ToArray()
                    : Enumerable.Range(0, values.Length).Select(i => (double)(i + xOffset)).ToArray();

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

                if (ImGui.Button("Export to CSV", new Vector2(graphSize.X, 24)))
                {
                    var exportPath = "";
                    var result = PlatformUtils.Instance.OpenFolderDialog("Select Export Directory", out exportPath);

                    if (result)
                    {
                        try
                        {
                            string fileName = $"graph_export_{row.ID}.csv";
                            ExportGraphDataToCsv(@$"{exportPath}\{fileName}", xValues, values);
                            TaskLogs.AddLog($"[{editor.Project.ProjectName}:Param Editor] Exported graph data for row {row.ID}.");
                        }
                        catch (Exception ex)
                        {
                            TaskLogs.AddLog($"[{editor.Project.ProjectName}:Param Editor] Failed to export graph data for row {row.ID}.", LogLevel.Error, LogPriority.High, ex);
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
    #endregion


    #region Param Quick Search
    public static void ParamQuickSearch(ParamEditorScreen editor, ParamBank bank, string currentParam,
        int currentID)
    {
        if (ImGui.MenuItem("Search for references in tool"))
        {
            editor.FieldValueFinder.SearchText = $"{currentID}";
            editor.FieldValueFinder.CachedSearchText = editor.FieldValueFinder.SearchText;

            editor.FieldValueFinder.Results = editor.FieldValueFinder.ConstructResults();
            editor.FieldValueFinder.Results.Sort();
        }
        UIHelper.Tooltip("Quick use action for searching in 'Find Field Value Instances' tool with this row ID.");
    }
    #endregion
}
