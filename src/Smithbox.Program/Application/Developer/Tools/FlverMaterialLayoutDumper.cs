using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.IO;

namespace StudioCore.Application;

public static class FlverMaterialLayoutDumper
{
    public static void Display(Smithbox baseEditor, ProjectEntry project)
    {
        if (ImGui.Button("Dump Material Layouts", DPI.StandardButtonSize))
        {
            if (PlatformUtils.Instance.SaveFileDialog("Save Flver layout dump", new[] { FilterStrings.TxtFilter },
                     out var path))
            {
                using (StreamWriter file = new(path))
                {
                    foreach (KeyValuePair<string, FLVER2.BufferLayout> mat in FlverResource.MaterialLayouts)
                    {
                        file.WriteLine(mat.Key + ":");
                        foreach (FLVER.LayoutMember member in mat.Value)
                        {
                            file.WriteLine($@"{member.Index}: {member.Type.ToString()}: {member.Semantic.ToString()}");
                        }

                        file.WriteLine();
                    }
                }
            }
        }
    }
}
