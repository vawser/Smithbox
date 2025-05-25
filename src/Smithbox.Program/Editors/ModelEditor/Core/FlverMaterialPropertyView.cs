using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Editors.ModelEditor.Core.Properties;
using StudioCore.Editors.ModelEditor.Framework;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class FlverMaterialPropertyView
{
    private ModelEditorScreen Editor;
    private ModelSelectionManager Selection;
    private ModelContextMenu ContextMenu;
    private ModelPropertyDecorator Decorator;

    private MatbinPropertyView MaterialInfoViewer;

    public FlverMaterialPropertyView(ModelEditorScreen screen)
    {
        Editor = screen;
        Selection = screen.Selection;
        ContextMenu = screen.ContextMenu;
        Decorator = screen.Decorator;

        MaterialInfoViewer = new MatbinPropertyView(screen);
    }

    public void Display()
    {
        var index = Selection._selectedMaterial;

        if (index == -1)
            return;

        if (Editor.ResManager.GetCurrentFLVER().Materials.Count < index)
            return;

        if (Selection.MaterialMultiselect.StoredIndices.Count > 1)
        {
            ImGui.Separator();
            UIHelper.WrappedText("Multiple Materials are selected.\nProperties cannot be edited whilst in this state.");
            ImGui.Separator();
            return;
        }

        ImGui.Separator();
        ImGui.Text("Material");
        ImGui.Separator();

        var entry = Editor.ResManager.GetCurrentFLVER().Materials[index];

        var name = entry.Name;
        var mtd = entry.MTD;
        var gxIndex = entry.GXIndex;
        int mtdIndex = entry.Index;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Name");
        UIHelper.Tooltip("Identifies the mesh that uses this material, may include keywords that determine hideable parts.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("MTD");
        UIHelper.Tooltip("Virtual path to an MTD file or a Matxml file in games since ER.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("GXIndex");
        UIHelper.Tooltip("Index to the flver's list of GX lists.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Index");
        UIHelper.Tooltip("Index of the material in the material list. Used since Sekiro during cutscenes.");

        ImGui.NextColumn();

        var colWidth = ImGui.GetColumnWidth();

        ImGui.AlignTextToFramePadding();
        ImGui.SetNextItemWidth(colWidth);
        ImGui.InputText("##Name", ref name, 255);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Name != name)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMaterial_Name(entry, entry.Name, name));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.SetNextItemWidth(colWidth);
        ImGui.InputText("##MTD", ref mtd, 255);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.MTD != mtd)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMaterial_MTD(entry, entry.MTD, mtd));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##GXIndex", ref gxIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.GXIndex != gxIndex)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMaterial_GXIndex(entry, entry.GXIndex, gxIndex));
        }

        Decorator.GXListIndexDecorator(gxIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##MTDIndex", ref mtdIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Index != mtdIndex)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMaterial_MTDIndex(entry, entry.Index, mtdIndex));
        }

        ImGui.Columns(1);

        if (ImGui.CollapsingHeader("Textures", ImGuiTreeNodeFlags.DefaultOpen))
        {
            if (Selection._subSelectedTextureRow == -1)
                return;

            // Textures
            for (int i = 0; i < entry.Textures.Count; i++)
            {
                if (entry.Textures[i] != null)
                {
                    DisplayTextureProperties(entry.Textures[i], i, entry);
                }
            }
        }

        ContextMenu.TextureHeaderContextMenu(entry);

        MaterialInfoViewer.Display(entry.MTD);
    }

    private void DisplayTextureProperties(FLVER2.Texture texture, int index, FLVER2.Material curMaterial)
    {
        ImGui.Separator();
        if (ImGui.Selectable($"Texture##Texture{index}", Selection._subSelectedTextureRow == index))
        {
            Selection._subSelectedTextureRow = index;
        }

        if (Selection._subSelectedTextureRow == index)
        {
            ContextMenu.TextureHeaderContextMenu(index);
        }

        ImGui.Separator();

        var type = texture.Type;
        var path = texture.Path;
        var scale = texture.Scale;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Type");
        UIHelper.Tooltip("The type of texture this is, corresponding to the entries in the MTD.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Path");
        UIHelper.Tooltip("Network path to the texture file to use.\n\nThe only important aspect of the path is the filename, as all textures are grouped into a texture pool in-game.\n\nSetting a texture filepath here will override the path used within the MATBIN.\n\nIt is recommended you include your texture within the model's texbnd.dcx, as that will be loaded into the texture pool automatically when the character is loaded (like wise for other asset types).");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Scale");
        UIHelper.Tooltip("");

        ImGui.NextColumn();

        var colWidth = ImGui.GetColumnWidth();
        ImGui.AlignTextToFramePadding();
        ImGui.SetNextItemWidth(colWidth);
        ImGui.InputText($"##Name_texture{index}", ref type, 255);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (texture.Type != type)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMaterial_Texture_Type(texture, texture.Type, type));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.SetNextItemWidth(colWidth * 0.9f);
        ImGui.InputText($"##Path_texture{index}", ref path, 255);
        ImGui.SameLine();
        if (ImGui.Button($@"{Icons.FileO}##filePicker{index}"))
        {
            if (PlatformUtils.Instance.OpenFileDialog("Select target texture...", new string[] { "png", "dds", "tif", "jpeg", "bmp" }, out var tPath))
            {
                var filename = Path.GetFileNameWithoutExtension(tPath);
                path = $"{filename}.tif"; // Purely for consistency with vanilla
            }
        }
        UIHelper.Tooltip("Select the texture you wish to assign to this entry.");

        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (texture.Path != path)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMaterial_Texture_Path(texture, texture.Path, path));
        }

        // TODO: re-render model with the new texture?

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat2($"##Scale_texture{index}", ref scale);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (texture.Scale != scale)
                Editor.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMaterial_Texture_Scale(texture, texture.Scale, scale));
        }

        ImGui.Columns(1);
    }
}

