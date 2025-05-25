using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Components.Forms;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Framework.Decorators;

public static class PropInfo_MapObjectType
{
    public static void Display(MapEditorScreen editor, Entity firstEnt)
    {
        var typ = firstEnt.WrappedObject.GetType();

        var name = "";
        var desc = "";

        name = FormatInformationUtils.GetClassReferenceName(editor.Project.MsbInformation, typ.Name);
        desc = FormatInformationUtils.GetClassReferenceDescription(editor.Project.MsbInformation, typ.Name);

        if (name != "")
        {
            ImGui.Separator();
            ImGui.Text($"{name}:");
            ImGui.Separator();

            if (desc != "")
            {
                UIHelper.WrappedText($"{desc}");
                ImGui.Text("");
            }
        }
    }
}
