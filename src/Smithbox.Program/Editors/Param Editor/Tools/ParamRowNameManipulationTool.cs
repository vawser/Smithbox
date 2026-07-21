using Hexa.NET.ImGui;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioCore.Editors.ParamEditor;

public class ParamRowNameManipulationTool
{
    public ParamEditorView View;
    public ProjectEntry Project;

    // Adjust
    public string Context_RowNameAdjust_NameAdjustment = "";

    // Inherit
    public string Context_RowNameInherit_TargetField = "";

    // Replace
    public string Context_RowNameReplace_TargetString = "";
    public string Context_RowNameReplace_ReplaceString = "";

    public ParamRowNameManipulationTool(ParamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    // Displayed in the Row Context Menu of the Row List window
    public void DisplayRowContextMenu(string imguiKey)
    {
        if (imguiKey == "name")
        {
            // Name Manipulation
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Name_Manipulation_Header")}##nameManipMenuHeader"))
            {
                // Adjust
                if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Adjust_Header")}##adjustMenuHeader"))
                {
                    // Clear Text from Name
                    if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Clear_Text_From_Name")}##clearTextAction", false,
                        View.Selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                    {
                        ParamRowOperations.AdjustRowName(View, Context_RowNameAdjust_NameAdjustment, ParamRowNameAdjustType.Clear);
                    }
                    GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Clear_Text_From_Name_TT"));

                    // Prepend Text from Name
                    if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Prepend_Text_To_Name")}##prependTextAction", false,
                        View.Selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                    {
                        ParamRowOperations.AdjustRowName(View, Context_RowNameAdjust_NameAdjustment, ParamRowNameAdjustType.Prepend);
                    }
                    GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Prepend_Text_To_Name_TT"));

                    // Postpend Text from Name
                    if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Postpend_Text_To_Name")}##postpendTextAction", false,
                            View.Selection.RowSelectionExists()
                                ? ImGuiSelectableFlags.None
                                : ImGuiSelectableFlags.Disabled))
                    {
                        ParamRowOperations.AdjustRowName(View, Context_RowNameAdjust_NameAdjustment, ParamRowNameAdjustType.Postpend);
                    }
                    GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Postpend_Text_To_Name_TT"));

                    // Remove Text from Name
                    if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Remove_Text_To_Name")}##removeTextAction", false,
                            View.Selection.RowSelectionExists()
                                ? ImGuiSelectableFlags.None
                                : ImGuiSelectableFlags.Disabled))
                    {
                        ParamRowOperations.AdjustRowName(View, Context_RowNameAdjust_NameAdjustment, ParamRowNameAdjustType.Remove);
                    }
                    GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Remove_Text_To_Name_TT"));

                    // Text to Apply
                    ImGui.InputText($"{LOC.Get("PARAM_RowWindow_Context_Text_To_Apply_Input")}##nameAdjustment", ref Context_RowNameAdjust_NameAdjustment, 255);
                    GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Text_To_Apply_Input_TT"));

                    ImGui.EndMenu();

                }

                // Inherit
                if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Inherit_Header")}##inheritMenuHeader"))
                {
                    // Proliferate name into Param Reference
                    if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Proliferate_Name")}##proliferateNameAction", false,
                        View.Selection.RowSelectionExists()
                            ? ImGuiSelectableFlags.None
                            : ImGuiSelectableFlags.Disabled))
                    {
                        ParamRowOperations.ProliferateRowName(View, Context_RowNameInherit_TargetField);
                    }
                    GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Proliferate_Name_TT"));

                    // Inherit Name from Param Reference
                    if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Inherit_Name_From_Ref")}##inheritNameParamRefAction", false,
                            View.Selection.RowSelectionExists()
                                ? ImGuiSelectableFlags.None
                                : ImGuiSelectableFlags.Disabled))
                    {
                        ParamRowOperations.InheritRowName(View, Context_RowNameInherit_TargetField);
                    }
                    GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Inherit_Name_From_Ref_TT"));

                    // Inherit Name from FMG Refernece
                    if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Inherit_Name_From_FMG")}##inheritNameFmgRefAction", false,
                            View.Selection.RowSelectionExists()
                                ? ImGuiSelectableFlags.None
                                : ImGuiSelectableFlags.Disabled))
                    {
                        ParamRowOperations.InheritRowNameFromFMG(View, Context_RowNameInherit_TargetField);
                    }
                    GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Inherit_Name_From_FMG_TT"));

                    // Inherit Name from Alias
                    if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Inherit_Name_From_Alias")}##inheritNameAliasRefAction", false,
                            View.Selection.RowSelectionExists()
                                ? ImGuiSelectableFlags.None
                                : ImGuiSelectableFlags.Disabled))
                    {
                        ParamRowOperations.InheritRowNameFromAlias(View, Context_RowNameInherit_TargetField);
                    }
                    GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Inherit_Name_From_Alias_TT"));

                    // Target Field
                    ImGui.InputText($"{LOC.Get("PARAM_RowWindow_Context_Target_Field")}##targetField", ref Context_RowNameInherit_TargetField, 255);
                    GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Target_Field_TT"));

                    ImGui.EndMenu();
                }

                // Replace
                if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowWindow_Context_Replace_Header")}##replaceMenuHeader"))
                {
                    // Target String
                    ImGui.InputText($"{LOC.Get("PARAM_RowWindow_Context_Target_String")}##targetString", ref Context_RowNameReplace_TargetString, 255);
                    GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Target_String_TT"));

                    // Replacement String
                    ImGui.InputText($"{LOC.Get("PARAM_RowWindow_Context_Replace_String")}##replaceString", ref Context_RowNameReplace_ReplaceString, 255);
                    GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Replace_String_TT"));

                    // Replace
                    if (ImGui.Selectable($"{LOC.Get("PARAM_RowWindow_Context_Action_Replace")}##replaceStringAction", false,
                            View.Selection.RowSelectionExists()
                                ? ImGuiSelectableFlags.None
                                : ImGuiSelectableFlags.Disabled))
                    {
                        ParamRowOperations.ReplaceStringInRowName(View, 
                            new List<string>{ Context_RowNameReplace_TargetString }, Context_RowNameReplace_ReplaceString);
                    }
                    GUI.Tooltip(LOC.Get("PARAM_RowWindow_Context_Action_Replace_TT"));

                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }
        }
    }

    public void SetNameManpulationTargetField(string internalName)
    {
        Context_RowNameInherit_TargetField = internalName;
    }

    public void Display()
    {
        ImGui.BeginTabBar("RowNameManipTabs");

        AdjustRowNameTab();
        InheritRowNameTab();
        ReplaceRowNameTab();

        ImGui.EndTabBar();
    }

    public void AdjustRowNameTab()
    {
        // Adjust
        if (ImGui.BeginTabItem($"{LOC.Get("PARAM_RowNameManip_Adjust_Tab")}##adjustTab"))
        {
            GUI.WrappedText(LOC.Get("PARAM_RowNameManip_Adjust_Tab_Hint"));
            GUI.Spacer();

            // Text to Add
            GUI.SimpleHeader(
                LOC.Get("PARAM_RowNameManip_Text_To_Add_Header"),
                LOC.Get("PARAM_RowNameManip_Text_To_Add_Header_TT"));

            ImGui.InputText($"##nameAdjustment", 
                ref Context_RowNameAdjust_NameAdjustment, 255);

            // Actions
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_RowNameManip_Actions_Header"),
                LOC.Get("PARAM_RowNameManip_Actions_Header_TT"));

            GUI.ConditionalMultiButtonInput("rowNameAdjust",
                "clearText",
                LOC.Get("PARAM_RowNameManip_Action_Clear_Row_Name"),
                LOC.Get("PARAM_RowNameManip_Action_Clear_Row_Name_TT"),
                ClearTextFromName,
                CanUseAdjustAction(),

                "prependText",
                LOC.Get("PARAM_RowNameManip_Action_Prepend_Row_Name"),
                LOC.Get("PARAM_RowNameManip_Action_Prepend_Row_Name_TT"),
                PrependTextToName,
                CanUseAdjustAction(),

                "postpendText",
                LOC.Get("PARAM_RowNameManip_Action_Postpend_Row_Name"),
                LOC.Get("PARAM_RowNameManip_Action_Postpend_Row_Name_TT"),
                PostpendTextToName,
                CanUseAdjustAction(),

                "removeText",
                LOC.Get("PARAM_RowNameManip_Action_Remove_Row_Name"),
                LOC.Get("PARAM_RowNameManip_Action_Postpend_Row_Name_TT"),
                RemoveTextFromName,
                CanUseAdjustAction()
                );

            ImGui.EndTabItem();
        }
    }

    public bool CanUseAdjustAction()
    {
        return View.Selection.RowSelectionExists();
    }

    public void ClearTextFromName()
    {
        ParamRowOperations.AdjustRowName(View, Context_RowNameAdjust_NameAdjustment, ParamRowNameAdjustType.Clear);
    }

    public void PrependTextToName()
    {
        ParamRowOperations.AdjustRowName(View, Context_RowNameAdjust_NameAdjustment, ParamRowNameAdjustType.Prepend);
    }

    public void PostpendTextToName()
    {
        ParamRowOperations.AdjustRowName(View, Context_RowNameAdjust_NameAdjustment, ParamRowNameAdjustType.Postpend);
    }

    public void RemoveTextFromName()
    {
        ParamRowOperations.AdjustRowName(View, Context_RowNameAdjust_NameAdjustment, ParamRowNameAdjustType.Remove);
    }

    public void InheritRowNameTab()
    {
        // Adjust
        if (ImGui.BeginTabItem($"{LOC.Get("PARAM_RowNameManip_Inherit_Tab")}##inheritTab"))
        {
            GUI.WrappedText(LOC.Get("PARAM_RowNameManip_Inherit_Tab_Hint"));
            GUI.Spacer();

            // Target Field
            GUI.SimpleHeader(
                LOC.Get("PARAM_RowNameManip_Target_Field_Header"),
                LOC.Get("PARAM_RowNameManip_Target_Field_Header_TT"));

            ImGui.InputText($"##targetFieldForInherit",
                ref Context_RowNameInherit_TargetField, 255);

            // Actions
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_RowNameManip_Actions_Header"),
                LOC.Get("PARAM_RowNameManip_Actions_Header_TT"));

            GUI.ConditionalMultiButtonInput("rowNameInherit",
                "proliferateRowName",
                LOC.Get("PARAM_RowNameManip_Action_Proliferate_Name"),
                LOC.Get("PARAM_RowNameManip_Action_Proliferate_Name_TT"),
                ProliferateRowName,
                CanUseProliferateRowNameAction(),

                "inheritNameFromParamRef",
                LOC.Get("PARAM_RowNameManip_Action_Inherit_Name_From_Ref"),
                LOC.Get("PARAM_RowNameManip_Action_Inherit_Name_From_Ref_TT"),
                InheritNameFromParamRef,
                CanUseInheritNameFromParamRefAction(),

                "inheritNameFromFmgRef",
                LOC.Get("PARAM_RowNameManip_Action_Inherit_Name_From_FMG"),
                LOC.Get("PARAM_RowNameManip_Action_Inherit_Name_From_FMG_TT"),
                InheritNameFromFmgRef,
                CanUseInheritNameFromFmgRefAction(),

                "inheritNameFromAliasRef",
                LOC.Get("PARAM_RowNameManip_Action_Inherit_Name_From_Alias"),
                LOC.Get("PARAM_RowNameManip_Action_Inherit_Name_From_Alias_TT"),
                InheritNameFromAliasRef,
                CanUseInheritNameFromAliasRefAction()
                );

            ImGui.EndTabItem();
        }
    }

    public bool CanUseProliferateRowNameAction()
    {
        if (!View.Selection.RowSelectionExists())
            return false;

        var curSelection = View.Selection.GetActiveRow();

        // Only allow if the target field exists in the current row(s)
        if(curSelection.Def.Fields.Any(e => e.InternalName == Context_RowNameInherit_TargetField))
        {
            return true;
        }

        return false;
    }

    public (bool, ParamFieldMeta) CanUseInheritAction()
    {
        if (!View.Selection.RowSelectionExists())
            return (false, null);

        var curParamString = View.Selection.GetActiveParam();
        var curSelection = View.Selection.GetActiveRow();

        // Only allow if the target field exists in the current row(s)
        if (!curSelection.Def.Fields.Any(e => e.InternalName == Context_RowNameInherit_TargetField))
        {
            return (false, null);
        }

        if (!Project.Handler.ParamData.PrimaryBank.Params.ContainsKey(curParamString))
            return (false, null);

        var curParam = Project.Handler.ParamData.PrimaryBank.Params[curParamString];

        var defEntry = curSelection.Def.Fields.FirstOrDefault(
            e => e.InternalName == Context_RowNameInherit_TargetField);

        if (defEntry == null)
            return (false, null);

        var meta = Project.Handler.ParamData.GetParamMeta(curParam.AppliedParamdef);

        if (meta == null)
            return (false, null);

        var fieldMeta = Project.Handler.ParamData.GetParamFieldMeta(meta, defEntry);

        return (true, fieldMeta);
    }

    public bool CanUseInheritNameFromParamRefAction()
    {
        var (isValid, fieldMeta) = CanUseInheritAction();

        if (!isValid)
            return false;

        if (fieldMeta == null)
            return false;

        if (fieldMeta.RefTypes == null)
            return false;

        if (fieldMeta.RefTypes.Any())
            return true;

        return false;
    }

    public bool CanUseInheritNameFromFmgRefAction()
    {
        var (isValid, fieldMeta) = CanUseInheritAction();

        if (!isValid)
            return false;

        if (fieldMeta == null)
            return false;

        if (fieldMeta.FmgRef == null)
            return false;

        if (fieldMeta.FmgRef.Any())
            return true;

        return false;
    }

    public bool CanUseInheritNameFromAliasRefAction()
    {
        var (isValid, fieldMeta) = CanUseInheritAction();

        if (!isValid)
            return false;

        if (fieldMeta == null)
            return false;

        if (fieldMeta.ShowCharacterEnumList)
            return true;

        if (fieldMeta.ShowFlagEnumList)
            return true;

        if (fieldMeta.ShowParticleEnumList)
            return true;

        if (fieldMeta.ShowCutsceneEnumList)
            return true;

        if (fieldMeta.ShowMovieEnumList)
            return true;

        if (fieldMeta.ShowSoundEnumList)
            return true;

        return false;
    }

    public void ProliferateRowName()
    {
        ParamRowOperations.ProliferateRowName(View, Context_RowNameInherit_TargetField);
    }

    public void InheritNameFromParamRef()
    {
        ParamRowOperations.InheritRowName(View, Context_RowNameInherit_TargetField);
    }

    public void InheritNameFromFmgRef()
    {
        ParamRowOperations.InheritRowNameFromFMG(View, Context_RowNameInherit_TargetField);
    }

    public void InheritNameFromAliasRef()
    {
        ParamRowOperations.InheritRowNameFromAlias(View, Context_RowNameInherit_TargetField);
    }

    private List<string> TargetStrings = new()
    {
        ""
    };

    public void ReplaceRowNameTab()
    {
        // Adjust
        if (ImGui.BeginTabItem($"{LOC.Get("PARAM_RowNameManip_Replace_Tab")}##replaceTab"))
        {
            GUI.WrappedText(LOC.Get("PARAM_RowNameManip_Replace_Tab_Hint"));
            GUI.Spacer();

            // Target Strings
            GUI.SimpleHeader(
                LOC.Get("PARAM_RowNameManip_Target_Strings_Header"),
                LOC.Get("PARAM_RowNameManip_Target_Strings_Header_TT"));

            // Add
            if (ImGui.Button($"{Icons.Plus}##replaceRowName_AddTargetString"))
            {
                TargetStrings.Add("");
            }
            GUI.Tooltip(LOC.Get("PARAM_RowNameManip_Target_String_Add_TT"));

            ImGui.SameLine();

            // Remove
            if (TargetStrings.Count < 2)
            {
                ImGui.BeginDisabled();

                if (ImGui.Button($"{Icons.Minus}##replaceRowName_RemoveTargetString"))
                {
                    TargetStrings.RemoveAt(TargetStrings.Count - 1);
                }
                GUI.Tooltip(LOC.Get("PARAM_RowNameManip_Target_String_Remove_TT"));

                ImGui.EndDisabled();
            }
            else
            {
                if (ImGui.Button($"{Icons.Minus}##replaceRowName_RemoveTargetString"))
                {
                    TargetStrings.RemoveAt(TargetStrings.Count - 1);
                }
                GUI.Tooltip(LOC.Get("PARAM_RowNameManip_Target_String_Remove_TT"));
            }

            ImGui.SameLine();

            // Reset
            if (ImGui.Button($"{LOC.Get("PARAM_RowNameManip_Target_String_Reset")}##replaceRowName_ResetTargetStrings"))
            {
                TargetStrings = new List<string>()
                {
                    ""
                };
            }
            GUI.Tooltip(LOC.Get("PARAM_RowNameManip_Target_String_Reset_TT"));

            for (int i = 0; i < TargetStrings.Count; i++)
            {
                var curText = TargetStrings[i];

                ImGui.PushItemWidth(ImGui.GetWindowWidth() * 0.5f);
                if (ImGui.InputText($"##newTargetString{i}", ref curText, 255))
                {
                    TargetStrings[i] = curText;
                }
                GUI.Tooltip(LOC.Get("PARAM_RowNameManip_Target_String_Entry_TT"));
            }

            // Replacement String
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_RowNameManip_Replacement_String_Header"),
                LOC.Get("PARAM_RowNameManip_Replacement_String_Header_TT"));

            ImGui.InputText($"##replacementString",
                ref Context_RowNameReplace_ReplaceString, 255);

            // Actions
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("PARAM_RowNameManip_Actions_Header"),
                LOC.Get("PARAM_RowNameManip_Actions_Header_TT"));

            GUI.ConditionalMultiButtonInput("rowNameReplace",
                "applyReplace",
                LOC.Get("PARAM_RowNameManip_Action_Replace"),
                LOC.Get("PARAM_RowNameManip_Action_Replace_TT"),
                ReplaceStringInRowName,
                CanUseReplaceAction()
                );

            ImGui.EndTabItem();
        }
    }

    public bool CanUseReplaceAction()
    {
        return View.Selection.RowSelectionExists();
    }

    public void ReplaceStringInRowName()
    {
        ParamRowOperations.ReplaceStringInRowName(View, TargetStrings, Context_RowNameReplace_ReplaceString);
    }
}
