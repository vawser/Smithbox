using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.ParamEditor.Data;
using StudioCore.Editors.TextEditor.Utils;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.EMEVD;
using static StudioCore.EventScriptEditorNS.EMEDF;

namespace StudioCore.EventScriptEditorNS;

/// <summary>
/// Handles the decoration of properties for the view classes
/// </summary>
public class EmevdPropertyDecorator
{
    public EmevdEditorScreen Editor;
    public ProjectEntry Project;

    public EmevdSelection Selection;

    private Instruction Instruction;
    private List<ArgDoc> ArgumentDocs;
    private List<object> Arguments;

    public EmevdPropertyDecorator(EmevdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void StoreInstructionInfo(Instruction instruction, List<ArgDoc> argDocs, List<object> arguments)
    {
        Instruction = instruction;
        ArgumentDocs = argDocs;
        Arguments = arguments;
    }

    #region Param Reference
    public bool HasParamReference(ArgDoc curDoc)
    {
        if(curDoc.ParamRef != null)
        {
            return true;
        }

        return false;
    }

    public void DetermineParamReference(ArgDoc curDoc, string value, int i)
    {
        if (curDoc.ParamRef != null)
        {
            var parts = new List<string>() { curDoc.ParamRef };
            if (parts.Contains(";"))
            {
                parts = curDoc.ParamRef.Split(";").ToList();
            }

            foreach(var param in parts)
            {
                ConstructParamReference(param, value, i);
            }
        }
    }
    #endregion

    #region Text Reference

    public bool HasTextReference(ArgDoc curDoc)
    {
        if (curDoc.FmgRef != null)
        {
            return true;
        }

        return false;
    }

    public void DetermineTextReference(ArgDoc curDoc, string value, int i)
    {
        if (curDoc.FmgRef != null)
        {
            var parts = new List<string>() { curDoc.FmgRef };
            if (parts.Contains(";"))
            {
                parts = curDoc.FmgRef.Split(";").ToList();
            }

            foreach (var fmgName in parts)
            {
                ConstructTextReference(fmgName, value, i);
            }
        }
    }
    #endregion

    #region Alias Reference

    public bool HasAliasReference(ArgDoc curDoc)
    {
        if (curDoc.AliasRef != null)
        {
            return true;
        }

        return false;
    }

    public void DetermineAliasReference(ArgDoc curDoc, string value, int i)
    {
        if (curDoc.AliasRef != null)
        {
            List<AliasEntry> entries = new List<AliasEntry>();

            // Event Flags
            if(curDoc.AliasRef == "EventFlag")
            {
                entries = Editor.Project.Aliases.EventFlags;
            }

            // Particles
            if (curDoc.AliasRef == "SFX")
            {
                entries = Editor.Project.Aliases.Particles;
            }

            // Sound
            if (curDoc.AliasRef == "Sound")
            {
                entries = Editor.Project.Aliases.Sounds;
            }

            if (entries.Count > 0)
            {
                foreach (var entry in entries)
                {
                    if (entry.ID == value)
                    {
                        ImGui.AlignTextToFramePadding();
                        UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"{entry.Name}");
                    }
                }
            }
        }
    }
    #endregion

    #region Entity Reference
    public bool HasMapEntityReference(ArgDoc curDoc)
    {
        if (curDoc.EntityRef != null)
        {
            return true;
        }

        return false;
    }

    public void DisplayDefaultEntityReferences(ArgDoc curDoc, string value, int i)
    {
        if (value == "10000" || value == "10010" || value == "0")
        {
            ImGui.AlignTextToFramePadding();
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Target Self");
        }
        else if (value == "10002" || value == "10012")
        {
            ImGui.AlignTextToFramePadding();
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Target Host Player");
        }
        else if (value == "10003" || value == "10013" ||
                    value == "10004" || value == "10014" ||
                    value == "10005" || value == "10015")
        {
            ImGui.AlignTextToFramePadding();
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Target Client Player");
        }
        else if (value == "20000")
        {
            ImGui.AlignTextToFramePadding();
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Target Any Player");
        }
    }

    public void DetermineMapEntityReference(ArgDoc curDoc, string value, int i)
    {
        var mapID = Editor.Selection.SelectedFileEntry.Filename; // To determine map ID

        if (Editor.Project.ProjectType is ProjectType.AC6)
        {
            // Referring to base map from mission EMEVD
            if (value.Length > 4)
            {
                var id = value.Substring(0, 2);
                var block = value.Substring(2, 2);

                mapID = $"m{id}_{block}_00_00";
            }

            // Referring to base map
            if (value.Length == 10)
            {
                // Ignore the start: 21
                var id = value.Substring(2, 2);
                var block = value.Substring(4, 2);

                mapID = $"m{id}_{block}_00_00";

            }
        }

        if (!IsSpecialEntityID(value))
        {
            ImGui.AlignTextToFramePadding();
            UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, "View in Map");

            // Context Menu for param ref
            if (ImGui.BeginPopupContextItem($"EntityContextMenu_{i}"))
            {
                if (ImGui.Selectable($"View"))
                {
                    EditorCommandQueue.AddCommand($"map/load/{mapID}");
                    EditorCommandQueue.AddCommand($"map/emevd_select/{mapID}/{value}");
                }

                ImGui.EndPopup();
            }
        }
    }
    #endregion

    #region Enum Reference
    public string enumSearchStr = "";

    public void DisplayEnumReference(ArgDoc argDoc, object arg, int i)
    {
        var enumDoc = Editor.Project.EmevdData.PrimaryBank.InfoBank.Enums.Where(e => e.Name == argDoc.EnumName).FirstOrDefault();
        var alias = enumDoc.Values.Where(e => e.Key == $"{arg}").FirstOrDefault();

        ImGui.AlignTextToFramePadding();
        UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"{alias.Value}");

        // Context Menu for enum
        if (ImGui.BeginPopupContextItem($"EnumContextMenu{i}"))
        {
            ImGui.InputTextMultiline("##enumSearch", ref enumSearchStr, 255, new Vector2(350, 20), ImGuiInputTextFlags.CtrlEnterForNewLine);

            if (ImGui.BeginChild("EnumList", new Vector2(350, ImGui.GetTextLineHeightWithSpacing() * Math.Min(12, enumDoc.Values.Count))))
            {
                try
                {
                    foreach (KeyValuePair<string, string> option in enumDoc.Values)
                    {
                        if (SearchFilters.IsEditorSearchMatch(enumSearchStr, option.Key, " ")
                            || SearchFilters.IsEditorSearchMatch(enumSearchStr, option.Value, " ")
                            || enumSearchStr == "")
                        {
                            if (ImGui.Selectable($"{option.Key}: {option.Value}"))
                            {
                                var newval = Convert.ChangeType(option.Key, arg.GetType());
                            }
                        }
                    }
                }
                catch
                {
                }
            }

            ImGui.EndChild();

            ImGui.EndPopup();
        }
    }
    #endregion

    #region Utils

    /// <summary>
    /// Is value considered a special Entity ID, such as the value for the player?
    /// </summary>
    private bool IsSpecialEntityID(string value)
    {
        if (value == "0" || 
            value == "10000" ||
            value == "10002" ||
            value == "10003" ||
            value == "10004" ||
            value == "10005" ||
            value == "10010" ||
            value == "10012" ||
            value == "10013" ||
            value == "10014" ||
            value == "10015" ||
            value == "20000")
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Find Param reference and display quick-link if possible.
    /// </summary>
    private void ConstructParamReference(string paramName, string value, int i)
    {
        if (Editor.BaseEditor.ProjectManager.SelectedProject.ParamEditor != null)
        {
            var refValue = int.Parse(value);

            (string, Param.Row, string) match = ResolveParamRef(Editor.BaseEditor.ProjectManager.SelectedProject.ParamEditor, Editor.Project.ParamData.PrimaryBank, paramName, refValue);

            if (match.Item1 != null && match.Item2 != null && match.Item3 != null)
            {
                ImGui.AlignTextToFramePadding();
                UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"{match.Item3}");

                // Context Menu for param ref
                if (ImGui.BeginPopupContextItem($"ParamContextMenu_{paramName}_{i}"))
                {
                    if (ImGui.Selectable($"Go to {match.Item2.ID} ({match.Item3})"))
                    {
                        EditorCommandQueue.AddCommand($@"param/select/-1/{match.Item1}/{match.Item2.ID}");
                    }

                    ImGui.EndPopup();
                }
            }
        }
    }

    /// <summary>
    /// Find FMG/Text reference and display quick-link if possible.
    /// </summary>
    private void ConstructTextReference(string fmgName, string value, int i)
    {
        if (Editor.BaseEditor.ProjectManager.SelectedProject.TextEditor != null)
        {
            var textEditor = Editor.BaseEditor.ProjectManager.SelectedProject.TextEditor;

            var refValue = int.Parse(value);

            TextResult result = TextFinder.GetTextResult(textEditor, fmgName, refValue);

            if (result != null)
            {
                ImGui.AlignTextToFramePadding();
                UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, $"{result.Entry.Text}");

                if (ImGui.BeginPopupContextItem($"TextContextMenu_{fmgName}_{i}"))
                {
                    if (ImGui.Selectable($"Go to {result.Entry.ID} ({result.Entry.Text})"))
                    {
                        EditorCommandQueue.AddCommand($@"text/select/{result.ContainerWrapper.ContainerDisplayCategory}/{result.ContainerWrapper.FileEntry.Filename}/{result.FmgName}/{result.Entry.ID}");
                    }

                    ImGui.EndPopup();
                }
            }
        }
    }

    /// <summary>
    /// Return Param reference based on passed value.
    /// </summary>
    private (string, Param.Row, string) ResolveParamRef(ParamEditorScreen editor, ParamBank bank, string paramRef, dynamic oldval)
    {
        (string, Param.Row, string) row = new(null, null, null);
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

    #endregion
}
