using Hexa.NET.ImGui;
using StudioCore.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class BoxSelectionAction
{
    public MapEditorView View;
    public ProjectEntry Project;

    public BoxSelectionAction(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnToolWindow()
    {
        UIHelper.WrappedText("Use this to configure how the box selection functions.");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Options", "");
        ImGui.Checkbox("Allow Box Selection", ref CFG.Current.Viewport_Enable_Box_Selection);
        UIHelper.Tooltip($"Toggle the usage of box selection.\nHold Ctrl, Alt and left click to make a box selection.");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Allowed Targets", "Which map objects can be selected by the box selection.");

        ImGui.Checkbox("Map Piece", ref CFG.Current.Viewport_Enable_Box_Selection_MapPiece);

        var name = "Objects";
        if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            name = "Assets";
        }

        ImGui.Checkbox(name, ref CFG.Current.Viewport_Enable_Box_Selection_Asset);

        ImGui.Checkbox("Enemy", ref CFG.Current.Viewport_Enable_Box_Selection_Enemy);

        ImGui.Checkbox("Player", ref CFG.Current.Viewport_Enable_Box_Selection_Player);

        ImGui.Checkbox("Collision", ref CFG.Current.Viewport_Enable_Box_Selection_Collision);

        ImGui.Checkbox("Light", ref CFG.Current.Viewport_Enable_Box_Selection_Light);

        ImGui.Checkbox("Region", ref CFG.Current.Viewport_Enable_Box_Selection_Region);
    }
}
