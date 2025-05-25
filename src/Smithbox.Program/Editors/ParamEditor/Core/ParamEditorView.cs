using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.ParamEditor.Data;
using StudioCore.Editors.ParamEditor.Decorators;
using StudioCore.Editors.ParamEditor.META;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using Veldrid;

namespace StudioCore.Editors.ParamEditor;

public class ParamEditorView
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public readonly ParamView ParamView;
    public readonly ParamRowView RowView;
    public readonly ParamFieldView FieldView;

    public ParamSelection Selection;
    public int ViewIndex;

    public ParamEditorView(ParamEditorScreen editor, ProjectEntry project, int index)
    {
        Editor = editor;
        Project = project;

        ViewIndex = index;
        Selection = new ParamSelection(Editor, Project);

        ParamView = new ParamView(Editor, Project, this);
        RowView = new ParamRowView(Editor, Project, this);
        FieldView = new ParamFieldView(Editor, Project, this);
    }

    /// <summary>
    /// Entry point
    /// </summary>
    /// <param name="doFocus"></param>
    /// <param name="isActiveView"></param>
    public void Display(bool doFocus, bool isActiveView)
    {
        var scale = DPI.GetUIScale();

        if (EditorDecorations.ImGuiTableStdColumns("paramsT", 3, true))
        {
            ImGui.TableSetupColumn("paramsCol", ImGuiTableColumnFlags.None, 0.5f);
            ImGui.TableSetupColumn("paramsCol2", ImGuiTableColumnFlags.None, 0.5f);

            var scrollTo = 0f;
            if (ImGui.TableNextColumn())
            {
                ParamView.Display(doFocus, isActiveView, scale, scrollTo);
            }

            var activeParam = Selection.GetActiveParam();
            if (ImGui.TableNextColumn())
            {
                RowView.Display(doFocus, isActiveView, scrollTo, activeParam);
            }

            Param.Row activeRow = Selection.GetActiveRow();
            if (ImGui.TableNextColumn())
            {
                FieldView.Display(isActiveView, activeParam, activeRow);
            }

            ImGui.EndTable();
        }
    }
}
