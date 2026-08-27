using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.ParamEditor;


public class ParamFindMenu
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;


    private int FindRowID = -1;
    public bool DisplayMenu = false;

    public ParamFindMenu(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Open()
    {
        ImGui.OpenPopup("findPopup");
    }

    public void Display()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (ImGui.BeginPopup("findPopup"))
        {
            DisplayMenu = true;

            if (ImGui.Button($"{Icons.LocationArrow}###jumpToAction"))
            {
                var activeParam = activeView.Selection.GetActiveParam();

                if (Editor.Project.Handler.ParamData.PrimaryBank.Params.ContainsKey(activeParam))
                {
                    var currentParam = Editor.Project.Handler.ParamData.PrimaryBank.Params[activeParam];

                    List<Param.Row> rows = CacheBank.GetCached(
                        Editor, (activeView.ViewIndex, activeParam),
                        () => activeView.MassEdit.RSE.Search(
                            (Editor.Project.Handler.ParamData.PrimaryBank, currentParam),
                           "", true, true)
                        );

                    foreach (var row in rows)
                    {
                        if (row.ID == FindRowID)
                        {
                            activeView.Selection.SetActiveRow(row, true);
                        }
                    }

                    activeView.JumpToSelectedRow = true;
                }
            }

            ImGui.SameLine();

            ImGui.InputInt("##findId", ref FindRowID);

            ImGui.EndPopup();
        }
        else
        {
            DisplayMenu = false;
        }
    }
}
