using Hexa.NET.ImGui;
using StudioCore.Editors.HavokEditor.Enums;
using StudioCore.HavokEditor;
using StudioCore.Utilities;
using System;

namespace StudioCore.Editors.HavokEditor.Core;

public class HavokTypeSelectionView
{
    private HavokEditorScreen Screen;

    public HavokTypeSelectionView(HavokEditorScreen screen)
    {
        Screen = screen;
    }

    public void OnGui()
    {
        var currentType = Screen.Selection.GetContainerType();

        ImGui.Begin("Container Type##HavokContainerTypeList");

        foreach (HavokContainerType e in Enum.GetValues<HavokContainerType>())
        {
            var name = e.GetDisplayName();
            if (ImGui.Selectable(name, e == currentType))
            {
                Screen.Selection.SetContainerType(e);
            }
        }

        ImGui.End();
    }
}
