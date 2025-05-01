using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Core.ProjectNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MaterialEditorNS;

public class MaterialDataView
{
    public MaterialEditor Editor;
    public Project Project;

    public MaterialDataView(Project curPoject, MaterialEditor editor)
    {
        Editor = editor;
        Project = curPoject;
    }

    public void Draw()
    {
        Editor.EditorFocus.SetFocusContext(MaterialEditorContext.DataView);

        ImGui.BeginChild("materialDataView");

        if (Editor.Selection._selectedSourceType is MaterialSourceType.MTD)
        {
            DisplayMaterialContents();
        }
        if (Editor.Selection._selectedSourceType is MaterialSourceType.MATBIN)
        {
            DisplayMaterialBinContents();
        }

        ImGui.EndChild();
    }

    public void DisplayMaterialContents()
    {
        MTD curMaterial = Editor.Selection._selectedMaterial;

        // TODO
    }

    public void DisplayMaterialBinContents()
    {
        MATBIN curMaterialBin = Editor.Selection._selectedMatbin;

        // TODO
    }
}

