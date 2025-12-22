using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;

namespace StudioCore.Editors.ParamEditor;

public class ParamEditorView
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public readonly ParamView ParamView;
    public readonly ParamTableGroupView TableGroupView;
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
        TableGroupView = new ParamTableGroupView(Editor, Project, this);
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
        var scale = DPI.UIScale();

        var activeParam = Selection.GetActiveParam();

        var columnCount = 3;
        if (TableGroupView.IsInTableGroupMode(activeParam))
        {
            columnCount = 4;
        }

        if (ParamEditorDecorations.ImGuiTableStdColumns("paramsT", columnCount, true))
        {
            ImGui.TableSetupColumn("paramsCol", ImGuiTableColumnFlags.None, 0.5f);
            ImGui.TableSetupColumn("paramsCol2", ImGuiTableColumnFlags.None, 0.5f);

            if (TableGroupView.IsInTableGroupMode(activeParam))
            {
                ImGui.TableSetupColumn("rowGroupCol", ImGuiTableColumnFlags.None, 0.5f);
            }

            var scrollTo = 0f;
            if (ImGui.TableNextColumn())
            {
                Editor.ContextManager.SetColumnContext(ParamEditorContext.ParamList);

                ParamView.Display(doFocus, isActiveView, scale, scrollTo);
            }

            if (TableGroupView.IsInTableGroupMode(activeParam))
            {
                if (ImGui.TableNextColumn())
                {
                    Editor.ContextManager.SetColumnContext(ParamEditorContext.TableGroupList);

                    TableGroupView.Display(doFocus, isActiveView, scrollTo, activeParam);
                }
            }

            if (ImGui.TableNextColumn())
            {
                Editor.ContextManager.SetColumnContext(ParamEditorContext.RowList);

                RowView.Display(doFocus, isActiveView, scrollTo, activeParam);
            }

            Param.Row activeRow = Selection.GetActiveRow();
            if (ImGui.TableNextColumn())
            {
                Editor.ContextManager.SetColumnContext(ParamEditorContext.FieldList);

                FieldView.Display(isActiveView, activeParam, activeRow);
            }

            ImGui.EndTable();
        }
    }
}
