using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.ParamEditor.Data;
using StudioCore.Editors.TextEditor.Utils;
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

    public void OnProjectChanged()
    {

    }

    /// <summary>
    /// Stores the instruction, argument docs and arguments for the current instruction for use in the other functions.
    /// </summary>
    public void StoreInstructionInfo(Instruction instruction, List<ArgDoc> argDocs, List<object> arguments)
    {
        Instruction = instruction;
        ArgumentDocs = argDocs;
        Arguments = arguments;
    }

    #region Param Reference
    /// <summary>
    /// Does the current property row have an Param reference?
    /// </summary>
    public bool HasParamReference(string parameterName)
    {
        // DS1
        if (Editor.Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
        {
        }

        // DS2
        if (Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            if (parameterName == "Bullet ID" ||
                parameterName == "DamageParam ID" ||
                parameterName == "ChrFullBodySFXParam ID" ||
                parameterName == "Head Armor ID" ||
                parameterName == "Chest Armor ID" ||
                parameterName == "Arm Armor ID" ||
                parameterName == "Leg Armor ID")
            {
                return true;
            }
        }

        // DS3
        if (Editor.Project.ProjectType is ProjectType.DS3)
        {
        }

        // BB
        if (Editor.Project.ProjectType is ProjectType.BB)
        {
        }

        // SDT
        if (Editor.Project.ProjectType is ProjectType.SDT)
        {
        }

        // ER
        if (Editor.Project.ProjectType is ProjectType.ER)
        {
        }

        // AC6
        if (Editor.Project.ProjectType is ProjectType.AC6)
        {
            if (parameterName == "SpEffect ID")
            {
                return true;
            }

            if (parameterName == "Action Button Parameter ID")
            {
                return true;
            }

            if (parameterName == "Item ID" )
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Add spacing in the UI so elements line up if Param reference is present
    /// </summary>
    public void DetermineParamReferenceSpacing(string parameterName, string value, int i)
    {
        // DS1
        if (Editor.Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
        {
        }

        // DS2
        if (Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            if (parameterName == "Bullet ID")
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("");
                ImGui.AlignTextToFramePadding();
                ImGui.Text("");
            }

            if (parameterName == "DamageParam ID")
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("");
                ImGui.AlignTextToFramePadding();
                ImGui.Text("");
            }

            if (parameterName == "ChrFullBodySFXParam ID")
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("");
            }

            if (parameterName == "Head Armor ID" || parameterName == "Chest Armor ID" || parameterName == "Arm Armor ID" || parameterName == "Leg Armor ID")
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("");
            }
        }

        // DS3
        if (Editor.Project.ProjectType is ProjectType.DS3)
        {
        }

        // BB
        if (Editor.Project.ProjectType is ProjectType.BB)
        {
        }

        // SDT
        if (Editor.Project.ProjectType is ProjectType.SDT)
        {
        }

        // ER   
        if (Editor.Project.ProjectType is ProjectType.ER)
        {
        }

        // AC6
        if (Editor.Project.ProjectType is ProjectType.AC6)
        {
            if (parameterName == "SpEffect ID")
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("");
            }

            if (parameterName == "Action Button Parameter ID")
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("");
            }

            if (parameterName == "Item ID")
            {
                ImGui.AlignTextToFramePadding();
                ImGui.Text("");
            }
        }
    }

    /// <summary>
    /// Find Param reference value and display it
    /// </summary>
    public void DetermineParamReference(string parameterName, string value, int i)
    {
        // For cross-ESD SpEffect stuff that AC6 does, which aligns with EventID
        var currentEventID = Selection.SelectedEvent.ID.ToString(); 

        // DS1
        if (Editor.Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
        {
        }

        // DS2
        if (Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
            if (parameterName == "Bullet ID")
            {
                ConstructParamReference("BulletParam", value, i);
                ConstructParamReference("EnemyBulletParam", value, i);
            }

            if (parameterName == "DamageParam ID")
            {
                ConstructParamReference("PlayerDamageParam", value, i);
                ConstructParamReference("EnemyDamageParam", value, i);
            }

            if (parameterName == "ChrFullBodySFXParam ID")
            {
                ConstructParamReference("ChrFullBodySfxParam", value, i);
            }

            if (parameterName == "Head Armor ID" || 
                parameterName == "Chest Armor ID" || 
                parameterName == "Arm Armor ID" || 
                parameterName == "Leg Armor ID")
            {
                ConstructParamReference("ItemParam", value, i);
            }
        }

        // DS3
        if (Editor.Project.ProjectType is ProjectType.DS3)
        {
        }

        // BB
        if (Editor.Project.ProjectType is ProjectType.BB)
        {
        }

        // SDT
        if (Editor.Project.ProjectType is ProjectType.SDT)
        {
        }

        // ER
        if (Editor.Project.ProjectType is ProjectType.ER)
        {
        }

        // AC6
        if (Editor.Project.ProjectType is ProjectType.AC6)
        {
            if (parameterName == "SpEffect ID")
            {
                // TODO: AC6 seems to get the actual SpEffect via ESD shenanigans when this is set to 0

                if (value == "0")
                {
                    ImGui.AlignTextToFramePadding();
                    UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"ESD");
                }
                else
                {
                    ConstructParamReference("SpEffectParam", value, i);
                }
            }

            if (parameterName == "Action Button Parameter ID")
            {
                ConstructParamReference("ActionButtonParam", value, i);
            }

            if (parameterName == "Item ID")
            {
                for(int k = 0; k < Arguments.Count; k++)
                {
                    var arg = Arguments[k];
                    var argDoc = ArgumentDocs[k];

                    if (argDoc.DisplayName == "Item Type")
                    {
                        string typeValue = arg as string;
                        switch (typeValue)
                        {
                            case "0": ConstructParamReference("EquipParamWeapon", value, i); break;
                            case "1": ConstructParamReference("EquipParamProtector", value, i); break;
                            case "2": ConstructParamReference("EquipParamAccessory", value, i); break;
                            case "3": ConstructParamReference("EquipParamGoods", value, i); break;
                        }
                    }
                }
            }
        }
    }
    #endregion

    #region Text Reference

    /// <summary>
    /// Does the current property row have an FMG/Text reference?
    /// </summary>
    public bool HasTextReference(string parameterName)
    {
        // DS1
        if (Editor.Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
        {
        }

        // DS2
        if (Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {

        }

        // DS3
        if (Editor.Project.ProjectType is ProjectType.DS3)
        {
        }

        // BB
        if (Editor.Project.ProjectType is ProjectType.BB)
        {
        }

        // SDT
        if (Editor.Project.ProjectType is ProjectType.SDT)
        {
        }

        // ER
        if (Editor.Project.ProjectType is ProjectType.ER)
        {
        }

        // AC6
        if (Editor.Project.ProjectType is ProjectType.AC6)
        {
            if (parameterName == "Name ID")
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Add spacing in the UI so elements line up if FMG/Text reference is present
    /// </summary>
    public void DetermineTextReferenceSpacing(string parameterName, string value, int i)
    {
        // DS1
        if (Editor.Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
        {
        }

        // DS2
        if (Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
        }

        // DS3
        if (Editor.Project.ProjectType is ProjectType.DS3)
        {
        }

        // BB
        if (Editor.Project.ProjectType is ProjectType.BB)
        {
        }

        // SDT
        if (Editor.Project.ProjectType is ProjectType.SDT)
        {
        }

        // ER
        if (Editor.Project.ProjectType is ProjectType.ER)
        {
        }

        // AC6
        if (Editor.Project.ProjectType is ProjectType.AC6)
        {
            if (parameterName == "Name ID")
            {
                ImGui.Text("");
            }
        }
    }

    /// <summary>
    /// Find FMG/Text reference value and display it
    /// </summary>
    public void DetermineTextReference(string parameterName, string value, int i)
    {
        // DS1
        if (Editor.Project.ProjectType is ProjectType.DS1 or ProjectType.DS1R)
        {
        }

        // DS2
        if (Editor.Project.ProjectType is ProjectType.DS2 or ProjectType.DS2S)
        {
        }

        // DS3
        if (Editor.Project.ProjectType is ProjectType.DS3)
        {
        }

        // BB
        if (Editor.Project.ProjectType is ProjectType.BB)
        {
        }

        // SDT
        if (Editor.Project.ProjectType is ProjectType.SDT)
        {
        }

        // ER
        if (Editor.Project.ProjectType is ProjectType.ER)
        {
        }

        // AC6
        if (Editor.Project.ProjectType is ProjectType.AC6)
        {
            if (parameterName == "Name ID")
            {
                ConstructTextReference("Title_Characters", value, i);
            }
        }
    }
    #endregion

    #region Alias Reference

    /// <summary>
    /// Does the current property row have an Editor Alias reference?
    /// </summary>
    public bool HasAliasReference(string parameterName)
    {
        if (IsFlagParameter(parameterName))
        {
            return true;
        }
        if (IsParticleParameter(parameterName))
        {
            return true;
        }
        if (IsSoundParameter(parameterName))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Add spacing in the UI so elements line up if Editor Alias reference is present
    /// </summary>
    public void DetermineAliasReferenceSpacing(string parameterName, string value, int i)
    {
        if (IsFlagParameter(parameterName))
        {
            ImGui.Text("");
        }
        if (IsParticleParameter(parameterName))
        {
            ImGui.Text("");
        }
        if (IsSoundParameter(parameterName))
        {
            ImGui.Text("");
        }
    }

    /// <summary>
    /// Find Editor Alias reference value and display it
    /// </summary>
    public void DetermineAliasReference(string parameterName, string value, int i)
    {
        if (IsFlagParameter(parameterName))
        {
            var entries = Editor.Project.Aliases.EventFlags;
            if (entries != null)
            {
                foreach(var entry in entries)
                {
                    if(entry.ID == value)
                    {
                        ImGui.AlignTextToFramePadding();
                        UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"{entry.Name}");
                    }
                    else
                    {
                        //ImguiUtils.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"---");
                    }
                }
            }
        }

        if (IsParticleParameter(parameterName))
        {
            var entries = Editor.Project.Aliases.Particles;
            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    if (entry.ID == value)
                    {
                        ImGui.AlignTextToFramePadding();
                        UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"{entry.Name}");
                    }
                    else
                    {
                        //ImguiUtils.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"---");
                    }
                }
            }
        }

        if (IsSoundParameter(parameterName))
        {
            var entries = Editor.Project.Aliases.Sounds;
            if (entries != null)
            {
                foreach (var entry in entries)
                {
                    if (entry.ID == value)
                    {
                        ImGui.AlignTextToFramePadding();
                        UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"{entry.Name}");
                    }
                    else
                    {
                        //ImguiUtils.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"---");
                    }
                }
            }
        }

    }
    #endregion

    #region Entity Reference
    /// <summary>
    /// Does the current property row have an Map Entity reference?
    /// </summary>
    public bool HasMapEntityReference(string parameterName)
    {
        if (IsEntityParameter(parameterName))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Add spacing in the UI so elements line up if Map Entity reference is present
    /// </summary>
    public void DetermineMapEntityReferenceSpacing(string parameterName, string value, int i)
    {
        if (IsEntityParameter(parameterName))
        {
            ImGui.AlignTextToFramePadding();
            ImGui.Text("");
        }
    }

    /// <summary>
    /// Find Map Entity reference value and display it, and unclude quick-link if applicable.
    /// </summary>
    public void DetermineMapEntityReference(string parameterName, string value, int i)
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

        if (IsEntityParameter(parameterName))
        {
            if (!IsSpecialEntityID(value))
            {
                ImGui.AlignTextToFramePadding();
                UIHelper.WrappedTextColored(UI.Current.ImGui_Benefit_Text_Color, "View in Map");

                // Context Menu for param ref
                if (ImGui.BeginPopupContextItem($"EntityContextMenu_{parameterName}_{i}"))
                {
                    if (ImGui.Selectable($"View"))
                    {
                        EditorCommandQueue.AddCommand($"map/load/{mapID}");
                        EditorCommandQueue.AddCommand($"map/emevd_select/{mapID}/{value}");
                    }

                    ImGui.EndPopup();
                }
            }
            else if(value == "10000" || value == "10010" || value == "0")
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
    /// Is value a sound parameter ID?
    /// </summary>
    private bool IsSoundParameter(string parameterName)
    {
        if (parameterName == "Sound ID")
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Is value a SFX parameter ID?
    /// </summary>
    private bool IsParticleParameter(string parameterName)
    {
        if (parameterName == "SFX ID")
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Is value a Event Flag ID?
    /// </summary>
    private bool IsFlagParameter(string parameterName)
    {
        if (parameterName == "Target Event Flag ID" ||
            parameterName == "Starting Target Event Flag ID" ||
            parameterName == "Ending Target Event Flag ID" ||
            parameterName == "Base Event Flag ID" ||
            parameterName == "Right-side Base Event Flag ID" ||
            parameterName == "Left-side Base Event Flag ID")
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Is value a Map Object Entity ID?
    /// </summary>
    private bool IsEntityParameter(string parameterName)
    {
        if (parameterName == "Entity ID" ||
            parameterName == "Character Entity ID" ||
            parameterName == "Target Asset Entity ID" ||
            parameterName == "Hit Entity ID" ||
            parameterName == "Target Entity ID" ||
            parameterName == "Area Entity ID" ||
            parameterName == "Target Entity ID A" ||
            parameterName == "Target Entity ID B" ||
            parameterName == "Attacker Entity ID" ||
            parameterName == "Warp Destination Entity ID")
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

    #endregion
}
