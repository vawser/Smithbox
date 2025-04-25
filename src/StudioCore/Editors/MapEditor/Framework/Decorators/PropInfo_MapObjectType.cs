using Hexa.NET.ImGui;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Framework.Decorators;

public static class PropInfo_MapObjectType
{
    public static void Display(Entity firstEnt)
    {
        var typ = firstEnt.WrappedObject.GetType();

        var name = "";
        var desc = "";

        name = Smithbox.BankHandler.MSB_Info.GetClassReferenceName(typ.Name);
        desc = Smithbox.BankHandler.MSB_Info.GetClassReferenceDescription(typ.Name);

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
