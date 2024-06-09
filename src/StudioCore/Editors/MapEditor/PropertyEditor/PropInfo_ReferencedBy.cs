using ImGuiNET;
using StudioCore.Gui;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.PropertyEditor;

public static class PropInfo_ReferencedBy
{
    public static void Display(Entity firstEnt, ref ViewportSelection selection, ref int refID)
    {
        if (firstEnt.GetReferencingObjects().Count == 0)
            return;

        ImGui.Separator();
        ImGui.Text("Referenced by");
        ImGui.Separator();
        ImguiUtils.ShowHoverTooltip("The current selection is referenced by these map objects.");

        foreach (Entity m in firstEnt.GetReferencingObjects())
        {
            var nameWithType = m.PrettyName.Insert(2, m.WrappedObject.GetType().Name + " - ");
            // Change Selection to Reference
            if (ImGui.Button(nameWithType + "##MSBRefBy" + refID))
            {
                selection.ClearSelection();
                selection.AddSelection(m);
            }

            refID++;
        }
    }
}

