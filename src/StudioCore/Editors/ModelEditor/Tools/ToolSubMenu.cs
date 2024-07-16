using ImGuiNET;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor.Tools;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor.Actions;

public class ToolSubMenu
{
    private ModelEditorScreen Screen;

    public ToolSubMenu(ModelEditorScreen screen) 
    {
        Screen = screen;
    }

    public void Shortcuts()
    {

    }

    public void OnProjectChanged()
    {

    }

    public void DisplayMenu()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            // Export Model
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Export Model", KeyBindings.Current.ModelEditor_ExportModel.HintText))
            {
                ModelExporter.ExportModel(Screen);
            }
            // Solve Bounding Boxes
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Solve Bounding Boxes"))
            {
                FlverTools.SolveBoundingBoxes(Screen.ResourceHandler.CurrentFLVER);
            }
            // Reverse Face Set
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Reverse Mesh Face Set"))
            {
                FlverTools.ReverseFaceSet(Screen);
            }
            // Reverse Normals
            ImguiUtils.ShowMenuIcon($"{ForkAwesome.Bars}");
            if (ImGui.MenuItem("Reverse Mesh Normals"))
            {
                FlverTools.ReverseNormals(Screen);
            }

            ImGui.EndMenu();
        }
    }
}
