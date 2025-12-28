using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.MapEditor;

public static class PropInfo_MapObjectType
{
    public static void Display(MapEditorScreen editor, Entity firstEnt)
    {
        var typ = firstEnt.WrappedObject.GetType();

        var name = "";
        var desc = "";

        name = FormatInformationUtils.GetClassReferenceName(editor.Project.MapData.MsbInformation, typ.Name);
        desc = FormatInformationUtils.GetClassReferenceDescription(editor.Project.MapData.MsbInformation, typ.Name);

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
