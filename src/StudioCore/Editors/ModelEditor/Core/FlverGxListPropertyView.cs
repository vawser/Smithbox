using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Editors.ModelEditor.Core.Properties;
using StudioCore.Editors.ModelEditor.Framework;
using StudioCore.Interface;

namespace StudioCore.Editors.ModelEditor;

public class FlverGxListPropertyView
{
    private ModelEditorScreen Screen;
    private ModelSelectionManager Selection;
    private ModelContextMenu ContextMenu;
    private ModelPropertyDecorator Decorator;

    private GxDataPropertyView GxDataEditor;

    private int byteArraySize = 32;

    public FlverGxListPropertyView(ModelEditorScreen screen)
    {
        Screen = screen;
        Selection = screen.Selection;
        ContextMenu = screen.ContextMenu;
        Decorator = screen.Decorator;

        GxDataEditor = new GxDataPropertyView(screen);
    }

    public void Display()
    {
        var index = Selection._selectedGXList;

        if (index == -1)
            return;

        if (Screen.ResManager.GetCurrentFLVER().GXLists.Count < index)
            return;

        ImGui.Separator();
        ImGui.Text("GX List");
        ImGui.Separator();

        var entry = Screen.ResManager.GetCurrentFLVER().GXLists[index];

        for (int i = 0; i < entry.Count; i++)
        {
            if (entry[i] != null)
            {
                DisplayGxItemProperties(entry[i], i);
            }
        }
    }

    private void DisplayGxItemProperties(FLVER2.GXItem item, int index)
    {
        ImGui.Separator();

        if (ImGui.Selectable($"GX Item##GXItem{index}", Selection._subSelectedGXItemRow == index))
        {
            Selection._subSelectedGXItemRow = index;
        }

        if (Selection._subSelectedGXItemRow == index)
        {
            ContextMenu.GXItemHeaderContextMenu(index);
        }

        ImGui.Separator();

        var id = item.ID;
        var unk04 = item.Unk04;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("ID");
        UIHelper.ShowHoverTooltip("In DS2, ID is just a number; in other games, it's 4 ASCII characters.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Unk04");
        UIHelper.ShowHoverTooltip("Unknown; typically 100.");

        ImGui.NextColumn();

        var colWidth = ImGui.GetColumnWidth();
        ImGui.AlignTextToFramePadding();
        ImGui.SetNextItemWidth(colWidth);
        ImGui.InputText($"##ID_item{index}", ref id, 255);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (item.ID != id)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERGXList_GXItem_ID(item, item.ID, id));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.SetNextItemWidth(colWidth);
        ImGui.InputInt($"##Unk04_item{index}", ref unk04);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (item.Unk04 != unk04)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERGXList_GXItem_Unk04(item, item.Unk04, unk04));
        }

        ImGui.Columns(1);

        if (item.Data.Length < 1)
        {
            ImGui.Separator();
            ImGui.Text($"New Byte Array##newByteArray{index}");
            ImGui.Separator();
            ImGui.InputInt("Byte Array Size", ref byteArraySize);

            if (ImGui.Button($"Create Byte Array##createByteArray{index}"))
            {
                item.Data = new byte[byteArraySize];
            }
            UIHelper.ShowHoverTooltip("Creates a byte array to the specified size. Note this is not checked for validity, that is up to the user to determine.");
        }

        GxDataEditor.Display(item);
    }
}

