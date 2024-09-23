using ImGuiNET;
using Microsoft.Extensions.Logging;
using StudioCore.Editors.ModelEditor.Actions;
using StudioCore.Platform;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using static SoulsFormats.PARAM;
using SoulsFormats;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor.SubEditors;
using StudioCore.Core.Project;
using StudioCore.Interface;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

public class ModelPropertyEditor
{
    private ModelEditorScreen Screen;
    private GXDataEditor GXDataEditor;
    private ModelPropertyDecorationHandler DecorationHandler;
    private MaterialInformationView MaterialInfoView;
    private HierarchyContextMenu ContextMenu;
    private CollisionPropertyEditor CollisionPropertyEditor;

    private bool SuspendView = false;

    public ModelPropertyEditor(ModelEditorScreen editor)
    {
        Screen = editor;
        GXDataEditor = new GXDataEditor(editor);
        DecorationHandler = new ModelPropertyDecorationHandler(editor);
        MaterialInfoView = new MaterialInformationView(editor);
        ContextMenu = new HierarchyContextMenu(Screen);
        CollisionPropertyEditor = new CollisionPropertyEditor(editor);
    }

    public void OnGui()
    {
        var scale = DPI.GetUIScale();

        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (!UI.Current.Interface_ModelEditor_Properties)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin($@"Properties##ModelEditorProperties"))
        {
            if (Screen.ResourceHandler.GetCurrentFLVER() != null && !SuspendView)
            {
                var entryType = Screen.ModelHierarchy._lastSelectedEntry;

                if (entryType == ModelEntrySelectionType.Header)
                {
                    DisplayProperties_Header();
                }
                else if (entryType == ModelEntrySelectionType.Dummy)
                {
                    DisplayProperties_Dummies();
                }
                else if (entryType == ModelEntrySelectionType.Material)
                {
                    DisplayProperties_Materials();
                }
                else if (entryType == ModelEntrySelectionType.GXList)
                {
                    DisplayProperties_GXLists();
                }
                else if (entryType == ModelEntrySelectionType.Node)
                {
                    DisplayProperties_Nodes();
                }
                else if (entryType == ModelEntrySelectionType.Mesh)
                {
                    DisplayProperties_Meshes();
                }
                else if (entryType == ModelEntrySelectionType.BufferLayout)
                {
                    DisplayProperties_BufferLayouts();
                }
                else if (entryType == ModelEntrySelectionType.BaseSkeleton)
                {
                    DisplayProperties_BaseSkeletons();
                }
                else if (entryType == ModelEntrySelectionType.AllSkeleton)
                {
                    DisplayProperties_AllSkeletons();
                }
                else if (entryType == ModelEntrySelectionType.CollisionLow)
                {
                    if (Screen.ResourceHandler.GetCurrentInternalFile().ER_CollisionLow != null)
                    {
                        CollisionPropertyEditor.DisplayProperties_CollisionLow();
                    }
                }
                else if (entryType == ModelEntrySelectionType.CollisionHigh)
                {
                    if (Screen.ResourceHandler.GetCurrentInternalFile().ER_CollisionHigh != null)
                    {
                        CollisionPropertyEditor.DisplayProperties_CollisionHigh();
                    }
                }
                else
                {
                    DisplayPropertes_InternalFile();
                }
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }

    private void DisplayPropertes_InternalFile()
    {
        var buttonSize = new Vector2(20, 20);
        var container = Screen.ResourceHandler.LoadedFlverContainer;
        var name = container.InternalFlvers.First().Name; // 

        ImGui.Separator();
        ImGui.Text($"Filename: {container.FlverFileName}");
        ImGui.Separator();

        ImGui.Text($"Internal Files:");
        foreach (var entry in container.InternalFlvers)
        {
            ImGui.AlignTextToFramePadding();
            if (ImGui.Button($"{ForkAwesome.Bars}", buttonSize))
            {
                UIHelper.CopyToClipboard(entry.Name);
            }
            UIHelper.ShowHoverTooltip("Copy name to clipboard.");
            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.Text($"{entry.Name}");
        }

        ImGui.Separator();

    }

    private void DisplayProperties_Header()
    {
        var entry = Screen.ResourceHandler.GetCurrentFLVER().Header;

        ImGui.Separator();
        ImGui.Text("Header");
        ImGui.Separator();

        // Variables
        var bigEndian = entry.BigEndian;
        var version = entry.Version;
        var bbMin = entry.BoundingBoxMin;
        var bbMax = entry.BoundingBoxMax;
        var unicode = entry.Unicode;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Big Endian");
        UIHelper.ShowHoverTooltip("If true FLVER will be written big-endian, if false little-endian.");
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Version");
        UIHelper.ShowHoverTooltip("Version of the format indicating presence of various features.");
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Bounding Box: Minimum");
        UIHelper.ShowHoverTooltip("Minimum extent of the entire model.");
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Bounding Box: Maximum");
        UIHelper.ShowHoverTooltip("Maximum extent of the entire model.");
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Unicode");
        UIHelper.ShowHoverTooltip("If true strings are UTF-16, if false Shift-JIS.");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.Checkbox("##BigEndian", ref bigEndian);
        if(ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.BigEndian != bigEndian)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERHeader_BigEndian(entry, entry.BigEndian, bigEndian));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##Version", ref version);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Version != version)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERHeader_Version(entry, entry.Version, version));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3("##BoundingBoxMin", ref bbMin);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.BoundingBoxMin != bbMin)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERHeader_BoundingBoxMin(entry, entry.BoundingBoxMin, bbMin));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3("##BoundingBoxMax", ref bbMax);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.BoundingBoxMax != bbMax)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERHeader_BoundingBoxMax(entry, entry.BoundingBoxMax, bbMax));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.Checkbox("##Unicode", ref unicode);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Unicode != unicode)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERHeader_Unicode(entry, entry.Unicode, unicode));
        }

        ImGui.Columns(1);
    }

    public Vector3 _trackedDummyPosition = new Vector3();

    private void UpdateDummy(int index)
    {
        var container = Screen._universe.LoadedModelContainers[this.Screen.ViewportHandler.ContainerID];
        if (container.DummyPoly_RootNode.Children.Count <= index)
        {
            TaskLogs.AddLog($"Index {index} is past size of dummy poly array, count {container.DummyPoly_RootNode.Children.Count}", LogLevel.Warning);
            return;
        }
        container.DummyPoly_RootNode.Children[index].UpdateRenderModel();
    }

    private void DisplayProperties_Dummies()
    {
        var index = Screen.ModelHierarchy._selectedDummy;

        if (index == -1)
            return;

        if (Screen.ResourceHandler.GetCurrentFLVER().Dummies.Count < index)
            return;

        if(Screen.ModelHierarchy.DummyMultiselect.StoredIndices.Count > 1)
        {
            ImGui.Separator();
            UIHelper.WrappedText("Multiple Dummies are selected.\nProperties cannot be edited whilst in this state.");
            ImGui.Separator();
            return;
        }

        ImGui.Separator();
        ImGui.Text("Dummy");
        ImGui.Separator();

        var entry = Screen.ResourceHandler.GetCurrentFLVER().Dummies[index];

        // Variables
        var position = entry.Position;
        var forward = entry.Forward;
        var upward = entry.Upward;
        int refId = entry.ReferenceID;
        int parentBoneIndex = entry.ParentBoneIndex;
        int attachBoneIndex = entry.AttachBoneIndex;
        var flag1 = entry.Flag1;
        var useUpwardVector = entry.UseUpwardVector;
        var unk30 = entry.Unk30;
        var unk34 = entry.Unk34;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Position");
        UIHelper.ShowHoverTooltip("Location of the dummy point.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Forward");
        UIHelper.ShowHoverTooltip("Vector indicating the dummy point's forward direction.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Upward");
        UIHelper.ShowHoverTooltip("Vector indicating the dummy point's upward direction.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Reference ID");
        UIHelper.ShowHoverTooltip("Indicates the type of dummy point this is (hitbox, sfx, etc).");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Parent Bone Index");
        UIHelper.ShowHoverTooltip("Index of a bone that the dummy point is initially transformed to before binding to the attach bone.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Attach Bone Index");
        UIHelper.ShowHoverTooltip("Index of the bone that the dummy point follows physically.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Flag1");
        UIHelper.ShowHoverTooltip("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Use Upward Vector");
        UIHelper.ShowHoverTooltip("If false, the upward vector is not read.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Unk30");
        UIHelper.ShowHoverTooltip("Unknown; only used in Sekiro.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Unk34");
        UIHelper.ShowHoverTooltip("Unknown; only used in Sekiro.");

        ImGui.NextColumn();

        ViewportAction? vpAction = null;
        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3("##Position", ref position);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Position != position)
                vpAction = new UpdateProperty_FLVERDummy_Position(entry, entry.Position, position);
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3("##Forward", ref forward);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Forward != forward)
            {
                vpAction = new UpdateProperty_FLVERDummy_Forward(entry, entry.Forward, forward);
                
            }
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3("##Upward", ref upward);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Upward != upward)
                vpAction = new UpdateProperty_FLVERDummy_Upward(entry, entry.Upward, upward);
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##ReferenceID", ref refId);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.ReferenceID != refId)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERDummy_ReferenceID(entry, entry.ReferenceID, refId));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##ParentBoneIndex", ref parentBoneIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.ParentBoneIndex != parentBoneIndex)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERDummy_ParentBoneIndex(entry, entry.ParentBoneIndex, parentBoneIndex));
        }

        DecorationHandler.NodeIndexDecorator(parentBoneIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##AttachBoneIndex", ref attachBoneIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.AttachBoneIndex != attachBoneIndex)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERDummy_AttachBoneIndex(entry, entry.AttachBoneIndex, attachBoneIndex));
        }

        DecorationHandler.NodeIndexDecorator(attachBoneIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.Checkbox("##Flag1", ref flag1);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Flag1 != flag1)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERDummy_Flag1(entry, entry.Flag1, flag1));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.Checkbox("##UseUpwardVector", ref useUpwardVector);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.UseUpwardVector != useUpwardVector)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERDummy_UseUpwardVector(entry, entry.UseUpwardVector, useUpwardVector));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##Unk30", ref unk30);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Unk30 != unk30)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERDummy_Unk30(entry, entry.Unk30, unk30));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##Unk34", ref unk34);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Unk34 != unk34)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERDummy_Unk34(entry, entry.Unk34, unk34));
        }

        ImGui.Columns(1);

        if (vpAction != null)
        {
            var a = new CompoundAction([vpAction]);
            a.SetPostExecutionAction(_ => UpdateDummy(index));
            Screen.EditorActionManager.ExecuteAction(a);
        }

        // Update representative selectable
        if (_trackedDummyPosition != entry.Position)
        {
            _trackedDummyPosition = entry.Position;
            Screen.ViewportHandler.UpdateRepresentativeDummy(index, entry.Position);
        }
    }

    private void DisplayProperties_Materials()
    {
        var index = Screen.ModelHierarchy._selectedMaterial;

        if (index == -1)
            return;

        if (Screen.ResourceHandler.GetCurrentFLVER().Materials.Count < index)
            return;

        if (Screen.ModelHierarchy.MaterialMultiselect.StoredIndices.Count > 1)
        {
            ImGui.Separator();
            UIHelper.WrappedText("Multiple Materials are selected.\nProperties cannot be edited whilst in this state.");
            ImGui.Separator();
            return;
        }

        ImGui.Separator();
        ImGui.Text("Material");
        ImGui.Separator();

        var entry = Screen.ResourceHandler.GetCurrentFLVER().Materials[index];

        var name = entry.Name;
        var mtd = entry.MTD;
        var gxIndex = entry.GXIndex;
        int mtdIndex = entry.Index;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Name");
        UIHelper.ShowHoverTooltip("Identifies the mesh that uses this material, may include keywords that determine hideable parts.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("MTD");
        UIHelper.ShowHoverTooltip("Virtual path to an MTD file or a Matxml file in games since ER.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("GXIndex");
        UIHelper.ShowHoverTooltip("Index to the flver's list of GX lists.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Index");
        UIHelper.ShowHoverTooltip("Index of the material in the material list. Used since Sekiro during cutscenes.");

        ImGui.NextColumn();

        var colWidth = ImGui.GetColumnWidth();

        ImGui.AlignTextToFramePadding();
        ImGui.SetNextItemWidth(colWidth);
        ImGui.InputText("##Name", ref name, 255);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Name != name)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMaterial_Name(entry, entry.Name, name));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.SetNextItemWidth(colWidth);
        ImGui.InputText("##MTD", ref mtd, 255);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.MTD != mtd)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMaterial_MTD(entry, entry.MTD, mtd));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##GXIndex", ref gxIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.GXIndex != gxIndex)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMaterial_GXIndex(entry, entry.GXIndex, gxIndex));
        }

        DecorationHandler.GXListIndexDecorator(gxIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##MTDIndex", ref mtdIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Index != mtdIndex)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMaterial_MTDIndex(entry, entry.Index, mtdIndex));
        }

        ImGui.Columns(1);

        if (ImGui.CollapsingHeader("Textures", ImGuiTreeNodeFlags.DefaultOpen))
        {
            if (Screen.ModelHierarchy._subSelectedTextureRow == -1)
                return;

            // Textures
            for (int i = 0; i < entry.Textures.Count; i++)
            {
                if (entry.Textures[i] != null)
                {
                    DisplayProperties_Material_Texture(entry.Textures[i], i, entry);
                }
            }
        }

        ContextMenu.TextureHeaderContextMenu(entry);

        MaterialInfoView.DisplayMatbinInformation(entry.MTD);
    }

    private void DisplayProperties_Material_Texture(FLVER2.Texture texture, int index, FLVER2.Material curMaterial)
    {
        ImGui.Separator();
        if(ImGui.Selectable($"Texture##Texture{index}", Screen.ModelHierarchy._subSelectedTextureRow == index))
        {
            Screen.ModelHierarchy._subSelectedTextureRow = index;
        }

        if (Screen.ModelHierarchy._subSelectedTextureRow == index)
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
        UIHelper.ShowHoverTooltip("The type of texture this is, corresponding to the entries in the MTD.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Path");
        UIHelper.ShowHoverTooltip("Network path to the texture file to use.\n\nThe only important aspect of the path is the filename, as all textures are grouped into a texture pool in-game.\n\nSetting a texture filepath here will override the path used within the MATBIN.\n\nIt is recommended you include your texture within the model's texbnd.dcx, as that will be loaded into the texture pool automatically when the character is loaded (like wise for other asset types).");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Scale");
        UIHelper.ShowHoverTooltip("");

        ImGui.NextColumn();

        var colWidth = ImGui.GetColumnWidth();
        ImGui.AlignTextToFramePadding();
        ImGui.SetNextItemWidth(colWidth);
        ImGui.InputText($"##Name_texture{index}", ref type, 255);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (texture.Type != type)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMaterial_Texture_Type(texture, texture.Type, type));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.SetNextItemWidth(colWidth * 0.9f);
        ImGui.InputText($"##Path_texture{index}", ref path, 255);
        ImGui.SameLine();
        if(ImGui.Button($@"{ForkAwesome.FileO}##filePicker{index}"))
        {
            if (PlatformUtils.Instance.OpenFileDialog("Select target texture...", new string[] { "png", "dds", "tif", "jpeg", "bmp" }, out var tPath))
            {
                var filename = Path.GetFileNameWithoutExtension(tPath);
                path = $"{filename}.tif"; // Purely for consistency with vanilla
            }
        }
        UIHelper.ShowHoverTooltip("Select the texture you wish to assign to this entry.");

        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (texture.Path != path)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMaterial_Texture_Path(texture, texture.Path, path));
        }

        // TODO: re-render model with the new texture?

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat2($"##Scale_texture{index}", ref scale);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (texture.Scale != scale)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMaterial_Texture_Scale(texture, texture.Scale, scale));
        }

        ImGui.Columns(1);
    }
    private void DisplayProperties_GXLists()
    {
        var index = Screen.ModelHierarchy._selectedGXList;

        if (index == -1)
            return;

        if (Screen.ResourceHandler.GetCurrentFLVER().GXLists.Count < index)
            return;

        ImGui.Separator();
        ImGui.Text("GX List");
        ImGui.Separator();

        var entry = Screen.ResourceHandler.GetCurrentFLVER().GXLists[index];

        for (int i = 0; i < entry.Count; i++)
        {
            if (entry[i] != null)
            {
                DisplayProperties_Material_GXItem(entry[i], i);
            }
        }
    }

    private int byteArraySize = 32;

    private void DisplayProperties_Material_GXItem(FLVER2.GXItem item, int index)
    {
        ImGui.Separator();

        if (ImGui.Selectable($"GX Item##GXItem{index}", Screen.ModelHierarchy._subSelectedGXItemRow == index))
        {
            Screen.ModelHierarchy._subSelectedGXItemRow = index;
        }

        if (Screen.ModelHierarchy._subSelectedGXItemRow == index)
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

            if(ImGui.Button($"Create Byte Array##createByteArray{index}"))
            {
                item.Data = new byte[byteArraySize];
            }
            UIHelper.ShowHoverTooltip("Creates a byte array to the specified size. Note this is not checked for validity, that is up to the user to determine.");
        }

        GXDataEditor.DisplayProperties_GXItem_HandleData(item);
    }

    public Vector3 _trackedNodePosition = new Vector3();

    private void DisplayProperties_Nodes()
    {
        var index = Screen.ModelHierarchy._selectedNode;

        if (index == -1)
            return;

        if (Screen.ResourceHandler.GetCurrentFLVER().Nodes.Count < index)
            return;

        if (Screen.ModelHierarchy.NodeMultiselect.StoredIndices.Count > 1)
        {
            ImGui.Separator();
            UIHelper.WrappedText("Multiple Nodes are selected.\nProperties cannot be edited whilst in this state.");
            ImGui.Separator();
            return;
        }

        ImGui.Separator();
        ImGui.Text("Node");
        ImGui.Separator();

        var entry = Screen.ResourceHandler.GetCurrentFLVER().Nodes[index];

        var name = entry.Name;
        int parentIndex = entry.ParentIndex;
        int firstChildIndex = entry.FirstChildIndex;
        int nextSiblingIndex = entry.NextSiblingIndex;
        int previousSiblingIndex = entry.PreviousSiblingIndex;
        var translation = entry.Position;
        var rotation = entry.Rotation;
        var scale = entry.Scale;
        var bbMin = entry.BoundingBoxMin;
        var bbMax = entry.BoundingBoxMax;
        var flags = (int)entry.Flags;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Name");
        UIHelper.ShowHoverTooltip("The name of this node");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Parent Index");
        UIHelper.ShowHoverTooltip("Index of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("First Child Index");
        UIHelper.ShowHoverTooltip("Index of this node's first child, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Next Sibling Index");
        UIHelper.ShowHoverTooltip("Index of the next child of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Previous Sibling Index");
        UIHelper.ShowHoverTooltip("Index of the previous child of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Translation");
        UIHelper.ShowHoverTooltip("Translation of this bone.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Rotation");
        UIHelper.ShowHoverTooltip("Rotation of this bone; euler radians in XZY order.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Scale");
        UIHelper.ShowHoverTooltip("Scale of this bone.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Bounding Box: Minimum");
        UIHelper.ShowHoverTooltip("Minimum extent of the vertices weighted to this bone.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Bounding Box: Maximum");
        UIHelper.ShowHoverTooltip("Maximum extent of the vertices weighted to this bone.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Flags");
        UIHelper.ShowHoverTooltip("A set of flags denoting the properties of a node");

        ImGui.NextColumn();

        var colWidth = ImGui.GetColumnWidth();

        ImGui.AlignTextToFramePadding();
        ImGui.SetNextItemWidth(colWidth);
        ImGui.InputText($"##Name", ref name, 255);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Name != name)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERNode_Name(entry, entry.Name, name));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##ParentIndex", ref parentIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.ParentIndex != parentIndex)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERNode_ParentIndex(entry, entry.ParentIndex, parentIndex));
        }

        DecorationHandler.NodeIndexDecorator(parentIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##FirstChildIndex", ref firstChildIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.FirstChildIndex != firstChildIndex)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERNode_FirstChildIndex(entry, entry.FirstChildIndex, firstChildIndex));
        }

        DecorationHandler.NodeIndexDecorator(firstChildIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##NextSiblingIndex", ref nextSiblingIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.NextSiblingIndex != nextSiblingIndex)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERNode_NextSiblingIndex(entry, entry.NextSiblingIndex, nextSiblingIndex));
        }

        DecorationHandler.NodeIndexDecorator(nextSiblingIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##PreviousSiblingIndex", ref previousSiblingIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.PreviousSiblingIndex != previousSiblingIndex)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERNode_PreviousSiblingIndex(entry, entry.PreviousSiblingIndex, previousSiblingIndex));
        }

        DecorationHandler.NodeIndexDecorator(previousSiblingIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##Translation", ref translation);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Position != translation)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERNode_Translation(entry, entry.Position, translation));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##Rotation", ref rotation);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Rotation != rotation)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERNode_Rotation(entry, entry.Rotation, rotation));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##Scale", ref scale);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.Scale != scale)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERNode_Scale(entry, entry.Scale, scale));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##BoundingBoxMin", ref bbMin);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.BoundingBoxMin != bbMin)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERNode_BoundingBoxMin(entry, entry.BoundingBoxMin, bbMin));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##BoundingBoxMax", ref bbMax);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.BoundingBoxMax != bbMax)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERNode_BoundingBoxMax(entry, entry.BoundingBoxMax, bbMax));
        }

        // TODO: actually set this up to handle the flags properly
        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##Flags", ref flags);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if ((int)entry.Flags != flags)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERNode_Flags(entry, (int)entry.Flags, flags));
        }

        ImGui.Columns(1);

        // Update representative selectable
        if (_trackedNodePosition != entry.Position)
        {
            _trackedNodePosition = entry.Position;
            Screen.ViewportHandler.UpdateRepresentativeNode(index, entry.Position, entry.Rotation, entry.Scale);
        }
    }

    private void DisplayProperties_Meshes()
    {
        var index = Screen.ModelHierarchy._selectedMesh;

        if (index == -1)
            return;

        if (Screen.ResourceHandler.GetCurrentFLVER().Meshes.Count < index)
            return;

        if (Screen.ModelHierarchy.MeshMultiselect.StoredIndices.Count > 1)
        {
            ImGui.Separator();
            UIHelper.WrappedText("Multiple Meshes are selected.\nProperties cannot be edited whilst in this state.");
            ImGui.Separator();
            return;
        }

        ImGui.Separator();
        ImGui.Text("Mesh");
        ImGui.Separator();

        var entry = Screen.ResourceHandler.GetCurrentFLVER().Meshes[index];

        var useBoneWeights = entry.UseBoneWeights;
        int dynamic = entry.Dynamic;
        var materialIndex = entry.MaterialIndex;
        var nodeIndex = entry.NodeIndex;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Use Bone Weights");
        UIHelper.ShowHoverTooltip("Determines how the mesh is skinned.\nIf it is true the mesh is assumed to be in bind pose and is skinned using the Bone Indices and Bone Weights of the vertices.\n\nIf it is false each vertex specifies a single node to bind to using its NormalW\n\nThe mesh is assumed to not be in bind pose and the transform of the bound node is applied to each vertex.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Material Index");
        UIHelper.ShowHoverTooltip("Index of the material used by all triangles in this mesh.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Node Index");
        UIHelper.ShowHoverTooltip("Index of the node representing this mesh in the Node list.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.Checkbox($"##UseBoneWeights", ref useBoneWeights);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.UseBoneWeights != useBoneWeights)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMesh_UseBoneWeights(entry, entry.UseBoneWeights, useBoneWeights));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##MaterialIndex", ref materialIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.MaterialIndex != materialIndex)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMesh_MaterialIndex(entry, entry.MaterialIndex, materialIndex));
        }

        DecorationHandler.MaterialIndexDecorator(materialIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##NodeIndex", ref nodeIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.NodeIndex != nodeIndex)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMesh_NodeIndex(entry, entry.NodeIndex, nodeIndex));
        }

        DecorationHandler.NodeIndexDecorator(nodeIndex);

        ImGui.Columns(1);

        if (ImGui.CollapsingHeader("Bounding Box"))
        {
            if(entry != null && entry.BoundingBox != null)
            {
                DisplayProperties_Mesh_BoundingBox(entry.BoundingBox);
            }
        }

        if (ImGui.CollapsingHeader("Face Sets", ImGuiTreeNodeFlags.DefaultOpen))
        {
            for (int i = 0; i < entry.FaceSets.Count; i++)
            {
                if (entry != null && entry.FaceSets[i] != null)
                {
                    DisplayProperties_Mesh_FaceSet(entry.FaceSets[i], i);
                }
            }
        }

        if (ImGui.CollapsingHeader("Vertex Buffers"))
        {
            for (int i = 0; i < entry.VertexBuffers.Count; i++)
            {
                if (entry != null && entry.VertexBuffers[i] != null)
                {
                    DisplayProperties_Mesh_VertexBuffers(entry.VertexBuffers[i], i);
                }
            }
        }
    }

    private void DisplayProperties_Mesh_FaceSet(FLVER2.FaceSet faceset, int index)
    {
        ImGui.Separator();
        if (ImGui.Selectable($"Face Set {index}##FaceSet{index}", Screen.ModelHierarchy._subSelectedFaceSetRow == index))
        {
            Screen.ModelHierarchy._subSelectedFaceSetRow = index;
        }

        if (Screen.ModelHierarchy._subSelectedFaceSetRow == index)
        {
            ContextMenu.FaceSetHeaderContextMenu(index);
        }

        ImGui.Separator();

        var flags = (int)faceset.Flags;
        var triangleStrip = faceset.TriangleStrip;
        var cullBackfaces = faceset.CullBackfaces;
        int unk06 = faceset.Unk06;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Flags");
        UIHelper.ShowHoverTooltip("Flags on a faceset, mostly just used to determine lod level.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Triangle Strip");
        UIHelper.ShowHoverTooltip("Whether vertices are defined as a triangle strip or individual triangles.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Cull Backfaces");
        UIHelper.ShowHoverTooltip("Whether triangles can be seen through from behind.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Unk06");
        UIHelper.ShowHoverTooltip("");

        ImGui.NextColumn();

        // TODO: handle the flags properly
        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##Flags_faceset{index}", ref flags);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if ((int)faceset.Flags != flags)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMesh_FaceSet_Flags(faceset, (int)faceset.Flags, flags));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.Checkbox($"##TriangleStrip_faceset{index}", ref triangleStrip);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (faceset.TriangleStrip != triangleStrip)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMesh_FaceSet_TriangleStrip(faceset, faceset.TriangleStrip, triangleStrip));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.Checkbox($"##CullBackfaces_faceset{index}", ref cullBackfaces);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (faceset.CullBackfaces != cullBackfaces)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMesh_FaceSet_CullBackfaces(faceset, faceset.CullBackfaces, cullBackfaces));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##Unk06_faceset{index}", ref unk06);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if ((int)faceset.Unk06 != unk06)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMesh_FaceSet_Unk06(faceset, (int)faceset.Unk06, unk06));
        }

        ImGui.Columns(1);

        faceset.Flags = (FLVER2.FaceSet.FSFlags)flags;
        faceset.TriangleStrip = triangleStrip;
        faceset.CullBackfaces = cullBackfaces;

        if(unk06 > short.MaxValue)
            unk06 = short.MaxValue;

        faceset.Unk06 = (short)unk06;
    }

    private void DisplayProperties_Mesh_VertexBuffers(FLVER2.VertexBuffer vertexBuffer, int index)
    {
        var layoutIndex = vertexBuffer.LayoutIndex;

        ImGui.Separator();
        if (ImGui.Selectable($"Vertex Buffer {index}##VertexBuffer{index}", Screen.ModelHierarchy._subSelectedVertexBufferRow == index))
        {
            Screen.ModelHierarchy._subSelectedVertexBufferRow = index;
        }

        if (Screen.ModelHierarchy._subSelectedVertexBufferRow == index)
        {
            ContextMenu.VertexBufferHeaderContextMenu(index);
        }

        ImGui.Separator();

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Layout Index");
        UIHelper.ShowHoverTooltip("");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##LayoutIndex{index}", ref layoutIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (vertexBuffer.LayoutIndex != layoutIndex)
                Screen.EditorActionManager.ExecuteAction(
                    new UpdateProperty_FLVERMesh_VertexBuffer_VertexBuffer(vertexBuffer, vertexBuffer.LayoutIndex, layoutIndex));
        }

        DecorationHandler.LayoutIndexDecorator(layoutIndex);

        ImGui.Columns(1);

        vertexBuffer.LayoutIndex = layoutIndex;
    }

    private void DisplayProperties_Mesh_BoundingBox(FLVER2.Mesh.BoundingBoxes boundingBox)
    {
        var bbMin = boundingBox.Min;
        var bbMax = boundingBox.Max;
        var bbUnk = boundingBox.Unk;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Mesh Bounding Box: Minimum");
        UIHelper.ShowHoverTooltip("Minimum extent of the mesh.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Mesh Bounding Box: Maximum");
        UIHelper.ShowHoverTooltip("Maximum extent of the mesh.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Mesh Bounding Box: Unknown");
        UIHelper.ShowHoverTooltip("Unknown; only present in Sekiro.");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##Min", ref bbMin);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (boundingBox.Min != bbMin)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERMesh_BoundingBoxes_Min(boundingBox, boundingBox.Min, bbMin));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##Max", ref bbMax);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (boundingBox.Max != bbMax)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERMesh_BoundingBoxes_Max(boundingBox, boundingBox.Max, bbMax));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##Unk", ref bbUnk);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (boundingBox.Unk != bbUnk)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERMesh_BoundingBoxes_Unk(boundingBox, boundingBox.Unk, bbUnk));
        }

        ImGui.Columns(1);

        // Changes
        boundingBox.Min = bbMin;
        boundingBox.Max = bbMax;
        boundingBox.Unk = bbUnk;
    }

    private void DisplayProperties_BufferLayouts()
    {
        var index = Screen.ModelHierarchy._selectedBufferLayout;

        if (index == -1)
            return;

        if (Screen.ResourceHandler.GetCurrentFLVER().BufferLayouts.Count < index)
            return;

        if (Screen.ModelHierarchy.BufferLayoutMultiselect.StoredIndices.Count > 1)
        {
            ImGui.Separator();
            UIHelper.WrappedText("Multiple Buffer Layouts are selected.\nProperties cannot be edited whilst in this state.");
            ImGui.Separator();
            return;
        }

        var entry = Screen.ResourceHandler.GetCurrentFLVER().BufferLayouts[index];

        for(int i = 0; i < entry.Count; i++)
        {
            DisplayProperties_BufferLayout_LayoutMember(entry[i], i);
        }
    }

    private void DisplayProperties_BufferLayout_LayoutMember(FLVER.LayoutMember layout, int index)
    {
        ImGui.Separator();
        if (ImGui.Selectable($"Layout Member {index}##LayoutMember{index}", Screen.ModelHierarchy._subSelectedBufferLayoutMember == index))
        {
            Screen.ModelHierarchy._subSelectedBufferLayoutMember = index;
        }
        ImGui.Separator();

        if (Screen.ModelHierarchy._subSelectedBufferLayoutMember == index)
        {
            ContextMenu.BufferLayoutMemberHeaderContextMenu(index);
        }

        var unk00 = layout.Unk00;
        var type = (int)layout.Type;
        var semantic = (int)layout.Semantic;
        var layoutIndex = layout.Index;

        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text($"Unk00:");
        UIHelper.ShowHoverTooltip("Unknown; 0, 1, or 2.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text($"Layout Type:");
        UIHelper.ShowHoverTooltip("Format used to store this member.");
        ImGui.Text($"");

        ImGui.AlignTextToFramePadding();
        ImGui.Text($"Layout Semantic:");
        UIHelper.ShowHoverTooltip("Vertex property being stored.");
        ImGui.Text($"");

        ImGui.AlignTextToFramePadding();
        ImGui.Text($"Index:");
        UIHelper.ShowHoverTooltip("For semantics that may appear more than once such as UVs, which one this member is.");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##Unk00##unk00{index}", ref unk00);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (layout.Unk00 != unk00)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERBufferLayout_LayoutMember_Unk00(layout, layout.Unk00, unk00));
        }

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##Type##type{index}", ref type);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if ((int)layout.Type != type)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERBufferLayout_LayoutMember_Type(layout, (int)layout.Type, type));
        }

        DecorationHandler.LayoutTypeDecorator(type);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##Semantic##semantic{index}", ref semantic);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if ((int)layout.Semantic != semantic)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERBufferLayout_LayoutMember_Semantic(layout, (int)layout.Semantic, semantic));
        }

        DecorationHandler.LayoutSemanticDecorator(semantic);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##Index##index{index}", ref layoutIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (layout.Index != layoutIndex)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERBufferLayout_LayoutMember_Index(layout, layout.Index, layoutIndex));
        }

        if(layout.Size == -1)
        {
            UIHelper.WrappedTextColored(UI.Default.ImGui_Warning_Text_Color, "Invalid Layout Type. Size cannot be determined.");
        }

        ImGui.Columns(1);

    }

    private void DisplayProperties_BaseSkeletons()
    {
        var index = Screen.ModelHierarchy._selectedBaseSkeletonBone;

        if (index == -1)
            return;

        if (Screen.ResourceHandler.GetCurrentFLVER().Skeletons.BaseSkeleton.Count < index)
            return;

        if (Screen.ResourceHandler.GetCurrentFLVER().Skeletons.BaseSkeleton == null)
            return;

        if (Screen.ModelHierarchy.BaseSkeletonMultiselect.StoredIndices.Count > 1)
        {
            ImGui.Separator();
            UIHelper.WrappedText("Multiple Skeleton Bones are selected.\nProperties cannot be edited whilst in this state.");
            ImGui.Separator();
            return;
        }

        ImGui.Separator();
        ImGui.Text("Standard Skeleton Hierarchy");
        ImGui.Separator();
        UIHelper.ShowHoverTooltip("Contains the standard skeleton hierarchy, which corresponds to the node hierarchy.");

        var entry = Screen.ResourceHandler.GetCurrentFLVER().Skeletons.BaseSkeleton[index];

        int parentIndex = entry.ParentIndex;
        int firstChildIndex = entry.FirstChildIndex;
        int nextSiblingIndex = entry.NextSiblingIndex;
        int previousSiblingIndex = entry.PreviousSiblingIndex;
        int nodeIndex = entry.NodeIndex;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Parent Index");
        UIHelper.ShowHoverTooltip("Index of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("First Child Index");
        UIHelper.ShowHoverTooltip("Index of this node's first child, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Next Sibling Index");
        UIHelper.ShowHoverTooltip("Index of the next child of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Previous Sibling Index");
        UIHelper.ShowHoverTooltip("Index of the previous child of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Node Index");
        UIHelper.ShowHoverTooltip("Index of the node in the Node list.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##ParentIndex", ref parentIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.ParentIndex != parentIndex)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERSkeleton_Bone_ParentIndex(entry, entry.ParentIndex, parentIndex));
        }

        DecorationHandler.NodeIndexDecorator(parentIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##FirstChildIndex", ref firstChildIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.FirstChildIndex != firstChildIndex)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERSkeleton_Bone_FirstChildIndex(entry, entry.FirstChildIndex, firstChildIndex));
        }

        DecorationHandler.NodeIndexDecorator(firstChildIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##NextSiblingIndex", ref nextSiblingIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.NextSiblingIndex != nextSiblingIndex)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERSkeleton_Bone_NextSiblingIndex(entry, entry.NextSiblingIndex, nextSiblingIndex));
        }

        DecorationHandler.NodeIndexDecorator(nextSiblingIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##PreviousSiblingIndex", ref previousSiblingIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.PreviousSiblingIndex != previousSiblingIndex)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERSkeleton_Bone_PreviousSiblingIndex(entry, entry.PreviousSiblingIndex, previousSiblingIndex));
        }

        DecorationHandler.NodeIndexDecorator(previousSiblingIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##NodeIndex", ref nodeIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.NodeIndex != nodeIndex)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERSkeleton_Bone_NodeIndex(entry, entry.NodeIndex, nodeIndex));
        }

        DecorationHandler.NodeIndexDecorator(nodeIndex);

        ImGui.Columns(1);
    }

    private void DisplayProperties_AllSkeletons()
    {
        var index = Screen.ModelHierarchy._selectedAllSkeletonBone;

        if (index == -1)
            return;

        if (Screen.ResourceHandler.GetCurrentFLVER().Skeletons.AllSkeletons.Count < index)
            return;

        if (Screen.ResourceHandler.GetCurrentFLVER().Skeletons.AllSkeletons == null)
            return;

        if (Screen.ModelHierarchy.BaseSkeletonMultiselect.StoredIndices.Count > 1)
        {
            ImGui.Separator();
            UIHelper.WrappedText("Multiple Skeleton Bones are selected.\nProperties cannot be edited whilst in this state.");
            ImGui.Separator();
            return;
        }

        ImGui.Separator();
        ImGui.Text("Full Skeleton Hierarchy");
        ImGui.Separator();
        UIHelper.ShowHoverTooltip("Contains all skeleton hierarchies including that of the control rig and the ragdoll bones.");

        var entry = Screen.ResourceHandler.GetCurrentFLVER().Skeletons.AllSkeletons[index];

        int parentIndex = entry.ParentIndex;
        int firstChildIndex = entry.FirstChildIndex;
        int nextSiblingIndex = entry.NextSiblingIndex;
        int previousSiblingIndex = entry.PreviousSiblingIndex;
        int nodeIndex = entry.NodeIndex;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Parent Index");
        UIHelper.ShowHoverTooltip("Index of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("First Child Index");
        UIHelper.ShowHoverTooltip("Index of this node's first child, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Next Sibling Index");
        UIHelper.ShowHoverTooltip("Index of the next child of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Previous Sibling Index");
        UIHelper.ShowHoverTooltip("Index of the previous child of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Node Index");
        UIHelper.ShowHoverTooltip("Index of the node in the Node list.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##ParentIndex", ref parentIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.ParentIndex != parentIndex)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERSkeleton_Bone_ParentIndex(entry, entry.ParentIndex, parentIndex));
        }

        DecorationHandler.NodeIndexDecorator(parentIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##FirstChildIndex", ref firstChildIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.FirstChildIndex != firstChildIndex)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERSkeleton_Bone_FirstChildIndex(entry, entry.FirstChildIndex, firstChildIndex));
        }

        DecorationHandler.NodeIndexDecorator(firstChildIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##NextSiblingIndex", ref nextSiblingIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.NextSiblingIndex != nextSiblingIndex)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERSkeleton_Bone_NextSiblingIndex(entry, entry.NextSiblingIndex, nextSiblingIndex));
        }

        DecorationHandler.NodeIndexDecorator(nextSiblingIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##PreviousSiblingIndex", ref previousSiblingIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.PreviousSiblingIndex != previousSiblingIndex)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERSkeleton_Bone_PreviousSiblingIndex(entry, entry.PreviousSiblingIndex, previousSiblingIndex));
        }

        DecorationHandler.NodeIndexDecorator(previousSiblingIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##NodeIndex", ref nodeIndex);
        if (ImGui.IsItemDeactivatedAfterEdit() || !ImGui.IsAnyItemActive())
        {
            if (entry.NodeIndex != nodeIndex)
                Screen.EditorActionManager.ExecuteAction(
                new UpdateProperty_FLVERSkeleton_Bone_NodeIndex(entry, entry.NodeIndex, nodeIndex));
        }

        DecorationHandler.NodeIndexDecorator(nodeIndex);
    }

}
