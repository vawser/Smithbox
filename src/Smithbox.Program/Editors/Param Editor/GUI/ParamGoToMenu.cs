using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;


public class ParamGoToMenu
{
    public ParamEditorScreen Editor;
    public ProjectEntry Project;

    public bool DisplayGoToMenu = false;
    public ParamGoToMenu(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Open()
    {
        ImGui.OpenPopup("goToPopup");
    }

    private int GoToRowID = -1; 

    public void Display()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (ImGui.BeginPopup("goToPopup"))
        {
            DisplayGoToMenu = true;

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
                        if (row.ID == GoToRowID)
                        {
                            activeView.Selection.SetActiveRow(row, true);
                        }
                    }

                    activeView.JumpToSelectedRow = true;
                }
            }

            ImGui.SameLine();

            ImGui.InputInt("##goToId", ref GoToRowID);

            ImGui.EndPopup();
        }
        else
        {
            DisplayGoToMenu = false;
        }
    }
}
