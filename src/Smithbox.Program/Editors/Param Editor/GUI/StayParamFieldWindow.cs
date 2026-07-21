using Hexa.NET.ImGui;
using Silk.NET.SDL;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Formats;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.ParamEditor;

public class StayParamFieldWindow
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;
    public ParamEditorView ParentView;

    private Dictionary<string, PropertyInfo[]> _propCache = new();

    private string _searchFilterTerm = "";

    public StayParamFieldWindow(ParamEditorScreen editor, ProjectEntry project, ParamEditorView curView)
    {
        Editor = editor;
        Project = project;
        ParentView = curView;
    }

    public void Display(bool isActiveView)
    {
        if(ParentView.Selection.ActiveStayParam == null)
            return;
        
        var activeParam = ParentView.Selection.ActiveStayParam;
        var stayParam = Project.Handler.ParamData.PrimaryBank.StayParams[activeParam];

        DisplayTitle();

        ImGui.BeginChild("columns" + activeParam);

        FocusManager.SetFocus(EditorFocusContext.ParamEditor_StayParamFieldList);

        DisplayFieldTable(isActiveView, activeParam, stayParam);

        ImGui.EndChild();
    }

    public void DisplayTitle()
    {
        GUI.SimpleHeader(
            LOC.Get("PARAM_SP_FieldWindow_Title"),
            LOC.Get("PARAM_SP_FieldWindow_Title_TT"));
    }

    private void DisplayHeader(bool isActiveView)
    {
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild("StayParamFieldListHeaderSection", searchHeight, ImGuiChildFlags.Borders);

        if (_searchFilterTerm != null)
        {
            if (isActiveView && InputManager.IsPressed(KeybindID.ParamEditor_Focus_Searchbar))
            {
                ImGui.SetKeyboardFocusHere();
            }

            // Field search
            ImGui.AlignTextToFramePadding();
            ImGui.InputTextWithHint("##fieldSearch", LOC.Get("PARAM_SP_FieldWindow_Search_Hint"), ref _searchFilterTerm,
                255);
            GUI.Tooltip(
                LOC.Get("PARAM_SP_FieldWindow_Search_TT", InputManager.GetHint(KeybindID.ParamEditor_Focus_Searchbar)));

            // Toggle Community Field Names
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Book}"))
            {
                if (CFG.Current.ParamEditor_FieldNameMode is ParamFieldNameMode.Source)
                {
                    CFG.Current.ParamEditor_FieldNameMode = ParamFieldNameMode.Community;
                }
                else if (CFG.Current.ParamEditor_FieldNameMode is ParamFieldNameMode.Community)
                {
                    CFG.Current.ParamEditor_FieldNameMode = ParamFieldNameMode.Source_Community;
                }
                else if (CFG.Current.ParamEditor_FieldNameMode is ParamFieldNameMode.Source_Community)
                {
                    CFG.Current.ParamEditor_FieldNameMode = ParamFieldNameMode.Community_Source;
                }
                else if (CFG.Current.ParamEditor_FieldNameMode is ParamFieldNameMode.Community_Source)
                {
                    CFG.Current.ParamEditor_FieldNameMode = ParamFieldNameMode.Source;
                }
            }

            GUI.Tooltip(
                LOC.Get("PARAM_SP_FieldWindow_FieldNameMode_Hint", LOC.Get(CFG.Current.ParamEditor_FieldNameMode.GetDisplayName())));

            // Toggle Field Padding
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Hubzilla}"))
            {
                CFG.Current.ParamEditor_Field_List_Display_Padding = !CFG.Current.ParamEditor_Field_List_Display_Padding;
            }

            var fieldPaddingMode = LOC.Get("PARAM_SP_FieldWindow_FieldPadding_Visible");
            if (!CFG.Current.ParamEditor_Field_List_Display_Padding)
                fieldPaddingMode = LOC.Get("PARAM_SP_FieldWindow_FieldPadding_Hidden");

            GUI.Tooltip(LOC.Get("PARAM_SP_FieldWindow_FieldPadding_Hint", fieldPaddingMode));
        }

        ImGui.EndChild();
    }

    public void DisplayFieldTable(bool isActiveView, string activeParam, StructParam stayParam)
    {
        var def = stayParam.Def;
        var meta = ParentView.GetParamData().GetParamMeta(def);
        var annotations = Editor.Project.Handler.ParamData.GetParamAnnotations(def.ParamType);

        var imguiId = 0;

        DisplayHeader(isActiveView);
        DisplayFlatTable(isActiveView, activeParam, stayParam, meta, annotations, ref imguiId);
    }

    // Default field display
    public void DisplayFlatTable(bool isActiveView, string activeParam, StructParam stayParam, ParamMeta meta, ParamAnnotationEntry annotations, ref int imguiId)
    {
        // Determine column count
        var columnCount = 2;

        // Field Table
        if (EditorTableUtils.ImGuiTableStdColumns("StayParamFieldsT", columnCount, false))
        {
            List<string> fieldOrder = meta is { AlternateOrder: not null } && CFG.Current.ParamEditor_Field_List_Allow_Rearrangement
                ? [.. meta.AlternateOrder]
                : [];

            foreach (PARAMDEF.Field field in stayParam.Def.Fields)
            {
                if (!fieldOrder.Contains(field.InternalName))
                {
                    fieldOrder.Add(field.InternalName);
                }
            }

            if (meta != null
                && CFG.Current.ParamEditor_Field_List_Allow_Rearrangement
                && (meta is { AlternateOrder: null } || meta.AlternateOrder.Count != fieldOrder.Count))
            {
                meta.AlternateOrder = [.. fieldOrder];
            }

            int displayIndex = 0;

            foreach (var fieldEntry in fieldOrder)
            {
                var field = stayParam[fieldEntry];
                var fieldMeta = Editor.Project.Handler.ParamData.GetParamFieldMeta(meta, field.Def);
                var fieldAnnotation = Editor.Project.Handler.ParamData.GetFieldAnnotation(annotations, field.Def.InternalName);

                var propType = field.Value.GetType();

                var metaContext = new FieldMetaContext(ParentView, meta, fieldMeta, fieldAnnotation, activeParam, field.Def.InternalName);

                object newval = null;

                if (!CFG.Current.ParamEditor_Field_List_Display_Padding && metaContext.IsPadding)
                {
                    continue;
                }

                if (_searchFilterTerm != "")
                {
                    if (!field.Def.InternalName.ToLower().Contains(_searchFilterTerm.ToLower()))
                    {
                        continue;
                    }
                }

                // TODO: add support for ParamRef meta
                // TODO: add relevant context menu actions

                //------------------------------
                // Name Column
                //------------------------------
                ImGui.PushID(imguiId + displayIndex);

                if (ImGui.TableNextColumn())
                {
                    if (metaContext.InjectSeparator)
                    {
                        ImGui.Separator();
                    }

                    FieldTooltipHelper.IconTooltip(ParentView, metaContext, field.Def);

                    // Field selection
                    ImGui.Selectable("", false, ImGuiSelectableFlags.AllowOverlap);

                    FieldTooltipHelper.HoverTooltip(ParentView, metaContext, field.Def);

                    //if (ImGui.IsItemClicked(ImGuiMouseButton.Right))
                    //{
                    //    ImGui.OpenPopup("ParamRowNameMenu");
                    //}

                    ImGui.SameLine();

                    // Name column
                    PropertyRowName(field.Def.InternalName, fieldMeta, fieldAnnotation);

                    // Cache
                    //ParentView.FieldDecorators.HandleCache(metaContext, row, oldval);

                    // Labels
                    //ParentView.FieldDecorators.HandleLabels(metaContext, row, oldval);
                }

                //------------------------------
                // Value Column
                //------------------------------
                if (ImGui.TableNextColumn())
                {
                    bool pushedStyle = false;

                    if (metaContext.HasAnyReferenceElements())
                    {
                        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_IsRef_Text);
                        pushedStyle = true;
                    }

                    if (metaContext.InjectSeparator)
                    {
                        ImGui.Separator();
                    }

                    // Property Editor UI
                    ParentView.FieldInputHandler.DisplayFieldInput(metaContext, propType, field.Value, ref newval);

                    if (pushedStyle)
                    {
                        ImGui.PopStyleColor();
                    }
                }

                var committed = ParentView.FieldInputHandler.UpdateProperty(field, 
                    typeof(StructCell).GetProperty("Value"), field.Value);

                if (committed)
                {
                }

                ImGui.PopID();
                imguiId++;
            }

            ImGui.EndTable();
        }
    }


    private void PropertyRowName(string internalName, ParamFieldMeta cellMeta, ParamAnnotationFieldEntry fieldAnnotation)
    {
        var altName = fieldAnnotation?.Name;

        var printedName = internalName;

        if (!string.IsNullOrWhiteSpace(altName))
        {
            switch (CFG.Current.ParamEditor_FieldNameMode)
            {
                case ParamFieldNameMode.Source:
                    printedName = internalName;
                    break;

                case ParamFieldNameMode.Community:
                    printedName = altName;
                    break;

                case ParamFieldNameMode.Source_Community:
                    printedName = $"{internalName} ({altName})";
                    break;

                case ParamFieldNameMode.Community_Source:
                    printedName = $"{altName} ({internalName})";
                    break;
            }
        }

        ImGui.AlignTextToFramePadding();
        ImGui.TextUnformatted(printedName);
    }
}