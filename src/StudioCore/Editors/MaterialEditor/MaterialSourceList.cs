using Hexa.NET.ImGui;
using StudioCore.Core;
using StudioCore.MaterialEditorNS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.MaterialEditorNS;

/// <summary>
/// Determines the material source type: MTD or MATBIN
/// </summary>
public class MaterialSourceList
{
    public MaterialEditorScreen Editor;
    public ProjectEntry Project;

    public MaterialSourceList(MaterialEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Draw()
    {
    }
}
