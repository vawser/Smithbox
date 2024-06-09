using ImGuiNET;
using StudioCore.BanksMain;
using StudioCore.Interface;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.PropertyEditor;

public static class PropInfo_MapObjectType
{
    public static void Display(Entity firstEnt)
    {
        var typ = firstEnt.WrappedObject.GetType();

        var name = "";
        var desc = "";

        name = MsbFormatBank.Bank.GetClassReferenceName(typ.Name);
        desc = MsbFormatBank.Bank.GetClassReferenceDescription(typ.Name);

        if (name != "")
        {
            ImGui.Separator();
            ImGui.Text($"{name}:");
            ImGui.Separator();
            ImguiUtils.WrappedText($"{desc}");
            ImGui.Text("");
        }
    }
}
