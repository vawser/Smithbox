using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ParamEditor;


public class ParamView
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public ParamSelection Selection;

    public ParamListWindow ParamListWindow;
    public ParamTableWindow ParamTableWindow;
    public ParamRowWindow ParamRowWindow;
    public ParamFieldWindow ParamFieldWindow;

    public ParamRowDecorators RowDecorators;
    public ParamFieldDecorators FieldDecorators;
    public ParamMetaEditor MetaEditor;
    public ParamFieldInput FieldInputHandler;
    public MassEdit MassEdit;

    public int ViewIndex;
    private int _imguiId = -1;

    public bool JumpToSelectedRow = false;
    public bool _isSearchBarActive = false;

    public ParamView(ParamEditorScreen editor, ProjectEntry project, int imguiId)
    {
        Editor = editor;
        Project = project;

        ViewIndex = imguiId;
        _imguiId = imguiId;

        Selection = new(editor, project);

        RowDecorators = new ParamRowDecorators(editor, project, this);
        FieldDecorators = new ParamFieldDecorators(editor, project, this);
        MetaEditor = new ParamMetaEditor(editor, project, this);
        FieldInputHandler = new ParamFieldInput(editor, project, this);
        MassEdit = new MassEdit(editor, project, this);

        ParamListWindow = new ParamListWindow(editor, project, this);
        ParamTableWindow = new ParamTableWindow(editor, project, this);
        ParamRowWindow = new ParamRowWindow(editor, project, this);
        ParamFieldWindow = new ParamFieldWindow(editor, project, this);
    }

    public void Display(bool doFocus, bool isActiveView)
    {
        var activeParam = Selection.GetActiveParam();

        var columnCount = 3;

        if (ParamTableWindow.IsInTableGroupMode(activeParam))
        {
            columnCount = 4;
        }

        if (EditorTableUtils.ImGuiTableStdColumns("paramsT", columnCount, true))
        {
            ImGui.TableSetupColumn("paramsCol", ImGuiTableColumnFlags.None, 0.5f);
            ImGui.TableSetupColumn("paramsCol2", ImGuiTableColumnFlags.None, 0.5f);

            if (ParamTableWindow.IsInTableGroupMode(activeParam))
            {
                ImGui.TableSetupColumn("rowGroupCol", ImGuiTableColumnFlags.None, 0.5f);
            }

            var scrollTo = 0f;
            if (ImGui.TableNextColumn())
            {
                FocusManager.SetFocus(EditorFocusContext.ParamEditor_ParamList);

                ParamListWindow.Display(doFocus, isActiveView, scrollTo);
            }

            if (ParamTableWindow.IsInTableGroupMode(activeParam))
            {
                if (ImGui.TableNextColumn())
                {
                    FocusManager.SetFocus(EditorFocusContext.ParamEditor_TableList);

                    ParamTableWindow.Display(doFocus, isActiveView, scrollTo, activeParam);
                }
            }

            if (ImGui.TableNextColumn())
            {
                FocusManager.SetFocus(EditorFocusContext.ParamEditor_RowList);

                ParamRowWindow.Display(doFocus, isActiveView, scrollTo, activeParam);
            }

            Param.Row activeRow = Selection.GetActiveRow();
            if (ImGui.TableNextColumn())
            {
                FocusManager.SetFocus(EditorFocusContext.ParamEditor_FieldList);

                ParamFieldWindow.Display(isActiveView, activeParam, activeRow);
            }

            ImGui.EndTable();
        }
    }

    public ParamData GetParamData()
    {
        return Project.Handler.ParamData;
    }

    public ParamBank GetPrimaryBank()
    {
        return Project.Handler.ParamData.PrimaryBank;
    }

    public ParamBank GetVanillaBank()
    {
        return Project.Handler.ParamData.VanillaBank;
    }

}
