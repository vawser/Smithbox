using Hexa.NET.ImGui;
using StudioCore.Core.ProjectNS;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditorNS;

public class MaterialSourceList
{
    public MaterialEditor Editor;
    public Project Project;

    public MaterialSourceList(Project curPoject, MaterialEditor editor)
    {
        Editor = editor;
        Project = curPoject;
    }

    public void Draw()
    {
        Editor.EditorFocus.SetFocusContext(MaterialEditorContext.SourceList);

        ImGui.BeginChild("materialSourceList");

        foreach(var entry in Enum.GetValues<MaterialSourceType>())
        {
            var isSelected = Editor.Selection._selectedSourceType == entry;

            if (ImGui.Selectable($"{entry.GetDisplayName()}##entry{entry}", isSelected))
            {
                Editor.Selection._selectedSourceType = entry;
            }
        }

        ImGui.EndChild();
    }
}


