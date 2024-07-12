using Andre.Formats;
using HKLib.hk2018.hkHashMapDetail;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Core;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.MapEditor.Toolbar;
using StudioCore.Gui;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Transactions;
using static StudioCore.Formats.PureFLVER.FLVER.Node;
using static StudioCore.Formats.PureFLVER.FLVER2.FLVER2;
using static StudioCore.Formats.PureFLVER.FLVER2.FLVER2.FaceSet;
using static StudioCore.Formats.PureFLVER.FLVER2.FLVER2.Mesh;

namespace StudioCore.Editors.ModelEditor;

public class ModelPropertyEditor
{
    private ModelEditorScreen Screen;
    private GXDataEditor GXDataEditor;
    private ModelPropertyDecorationHandler DecorationHandler;
    private MaterialInformationView MaterialInfoView;

    private bool SuspendView = false;

    public ModelPropertyEditor(ModelEditorScreen editor)
    {
        Screen = editor;
        GXDataEditor = new GXDataEditor();
        DecorationHandler = new ModelPropertyDecorationHandler(editor);
        MaterialInfoView = new MaterialInformationView(editor);
    }

    public void OnGui()
    {
        var scale = Smithbox.GetUIScale();

        if (Smithbox.ProjectType == ProjectType.Undefined)
            return;

        if (!CFG.Current.Interface_ModelEditor_Properties)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

        if (ImGui.Begin($@"Properties##ModelEditorProperties"))
        {
            if (Screen.ResourceHandler.CurrentFLVER != null && !SuspendView)
            {
                var entryType = Screen.ModelHierarchy._lastSelectedEntry;

                if (entryType == ModelEntrySelectionType.Header)
                {
                    DisplayProperties_Header();
                }
                if (entryType == ModelEntrySelectionType.Dummy)
                {
                    DisplayProperties_Dummies();
                }
                if (entryType == ModelEntrySelectionType.Material)
                {
                    DisplayProperties_Materials();
                }
                if (entryType == ModelEntrySelectionType.GXList)
                {
                    DisplayProperties_GXLists();
                }
                if (entryType == ModelEntrySelectionType.Node)
                {
                    DisplayProperties_Nodes();
                }
                if (entryType == ModelEntrySelectionType.Mesh)
                {
                    DisplayProperties_Meshes();
                }
                if (entryType == ModelEntrySelectionType.BufferLayout)
                {
                    DisplayProperties_BufferLayouts();
                }
                if (entryType == ModelEntrySelectionType.BaseSkeleton)
                {
                    DisplayProperties_BaseSkeletons();
                }
                if (entryType == ModelEntrySelectionType.AllSkeleton)
                {
                    DisplayProperties_AllSkeletons();
                }
            }
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
    }

    private void DisplayProperties_Header()
    {
        var entry = Screen.ResourceHandler.CurrentFLVER.Header;

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
        ImguiUtils.ShowHoverTooltip("If true FLVER will be written big-endian, if false little-endian.");
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Version");
        ImguiUtils.ShowHoverTooltip("Version of the format indicating presence of various features.");
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Bounding Box: Minimum");
        ImguiUtils.ShowHoverTooltip("Minimum extent of the entire model.");
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Bounding Box: Maximum");
        ImguiUtils.ShowHoverTooltip("Maximum extent of the entire model.");
        ImGui.AlignTextToFramePadding();
        ImGui.Text("Unicode");
        ImguiUtils.ShowHoverTooltip("If true strings are UTF-16, if false Shift-JIS.");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.Checkbox("##BigEndian", ref bigEndian);
        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##Version", ref version);
        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3("##BoundingBoxMin", ref bbMin);
        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3("##BoundingBoxMax", ref bbMax);
        ImGui.AlignTextToFramePadding();
        ImGui.Checkbox("##Unicode", ref unicode);

        ImGui.Columns(1);

        // Changes
        entry.BigEndian = bigEndian;
        entry.Version = version;
        entry.BoundingBoxMin = bbMin;
        entry.BoundingBoxMax = bbMax;
        entry.Unicode = unicode;
    }

    private void DisplayProperties_Dummies()
    {
        var index = Screen.ModelHierarchy._selectedDummy;

        if (Screen.ResourceHandler.CurrentFLVER.Dummies.Count < index)
            return;

        ImGui.Separator();
        ImGui.Text("Dummy");
        ImGui.Separator();

        var entry = Screen.ResourceHandler.CurrentFLVER.Dummies[index];

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
        ImguiUtils.ShowHoverTooltip("Location of the dummy point.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Forward");
        ImguiUtils.ShowHoverTooltip("Vector indicating the dummy point's forward direction.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Upward");
        ImguiUtils.ShowHoverTooltip("Vector indicating the dummy point's upward direction.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Reference ID");
        ImguiUtils.ShowHoverTooltip("Indicates the type of dummy point this is (hitbox, sfx, etc).");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Parent Bone Index");
        ImguiUtils.ShowHoverTooltip("Index of a bone that the dummy point is initially transformed to before binding to the attach bone.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Attach Bone Index");
        ImguiUtils.ShowHoverTooltip("Index of the bone that the dummy point follows physically.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Flag1");
        ImguiUtils.ShowHoverTooltip("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Use Upward Vector");
        ImguiUtils.ShowHoverTooltip("If false, the upward vector is not read.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Unk30");
        ImguiUtils.ShowHoverTooltip("Unknown; only used in Sekiro.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Unk34");
        ImguiUtils.ShowHoverTooltip("Unknown; only used in Sekiro.");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3("##Position", ref position);

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3("##Forward", ref forward);

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3("##Upward", ref upward);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##ReferenceID", ref refId);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##ParentBoneIndex", ref parentBoneIndex);

        DecorationHandler.NodeIndexDecorator(parentBoneIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##AttachBoneIndex", ref attachBoneIndex);

        DecorationHandler.NodeIndexDecorator(attachBoneIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.Checkbox("##Flag1", ref flag1);

        ImGui.AlignTextToFramePadding();
        ImGui.Checkbox("##UseUpwardVector", ref useUpwardVector);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##Unk30", ref unk30);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##Unk34", ref unk34);

        ImGui.Columns(1);

        // Changes
        entry.Position = position;
        entry.Forward = forward;
        entry.Upward = upward;

        if(refId > short.MaxValue)
            refId = short.MaxValue;

        entry.ReferenceID = (short)refId;

        if (parentBoneIndex > short.MaxValue)
            parentBoneIndex = short.MaxValue;

        entry.ParentBoneIndex = (short)parentBoneIndex;

        if (attachBoneIndex > short.MaxValue)
            attachBoneIndex = short.MaxValue;

        entry.AttachBoneIndex = (short)attachBoneIndex;

        entry.UseUpwardVector = useUpwardVector;
        entry.Flag1 = flag1;
        entry.Unk30 = unk30;
        entry.Unk34 = unk34;
    }

    private void DisplayProperties_Materials()
    {
        var index = Screen.ModelHierarchy._selectedMaterial;

        if (Screen.ResourceHandler.CurrentFLVER.Materials.Count < index)
            return;

        ImGui.Separator();
        ImGui.Text("Material");
        ImGui.Separator();

        var entry = Screen.ResourceHandler.CurrentFLVER.Materials[index];

        var name = entry.Name;
        var mtd = entry.MTD;
        var gxIndex = entry.GXIndex;
        int mtdIndex = entry.Index;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Name");
        ImguiUtils.ShowHoverTooltip("Identifies the mesh that uses this material, may include keywords that determine hideable parts.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("MTD");
        ImguiUtils.ShowHoverTooltip("Virtual path to an MTD file or a Matxml file in games since ER.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("GXIndex");
        ImguiUtils.ShowHoverTooltip("Index to the flver's list of GX lists.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Index");
        ImguiUtils.ShowHoverTooltip("Index of the material in the material list. Used since Sekiro during cutscenes.");

        ImGui.NextColumn();

        var colWidth = ImGui.GetColumnWidth();

        ImGui.AlignTextToFramePadding();
        ImGui.SetNextItemWidth(colWidth);
        ImGui.InputText("##Name", ref name, 255);

        ImGui.AlignTextToFramePadding();
        ImGui.SetNextItemWidth(colWidth);
        ImGui.InputText("##MTD", ref mtd, 255);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##GXIndex", ref gxIndex);

        DecorationHandler.GXListIndexDecorator(gxIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt("##MTDIndex", ref mtdIndex);

        ImGui.Columns(1);

        // Changes
        entry.Name = name;
        entry.MTD = mtd;
        entry.GXIndex = gxIndex;
        entry.Index = mtdIndex;

        if (ImGui.CollapsingHeader("Textures", ImGuiTreeNodeFlags.DefaultOpen))
        {
            // Textures
            for (int i = 0; i < entry.Textures.Count; i++)
            {
                entry.Textures[i] = DisplayProperties_Material_Texture(entry.Textures[i], i);
            }
        }

        MaterialInfoView.DisplayMatbinInformation(entry.MTD);
    }

    private Formats.PureFLVER.FLVER2.FLVER2.Texture DisplayProperties_Material_Texture(Formats.PureFLVER.FLVER2.FLVER2.Texture texture, int index)
    {
        ImGui.Separator();
        ImGui.Text("Texture");
        ImGui.Separator();

        var type = texture.Type;
        var path = texture.Path;
        var scale = texture.Scale;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Type");
        ImguiUtils.ShowHoverTooltip("The type of texture this is, corresponding to the entries in the MTD.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Path");
        ImguiUtils.ShowHoverTooltip("Network path to the texture file to use.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Scale");
        ImguiUtils.ShowHoverTooltip("");

        ImGui.NextColumn();

        var colWidth = ImGui.GetColumnWidth();
        ImGui.AlignTextToFramePadding();
        ImGui.SetNextItemWidth(colWidth);
        ImGui.InputText($"##Name_texture{index}", ref type, 255);
        ImGui.AlignTextToFramePadding();
        ImGui.SetNextItemWidth(colWidth);
        ImGui.InputText($"##Path_texture{index}", ref path, 255);
        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat2($"##Scale_texture{index}", ref scale);

        ImGui.Columns(1);

        // Changes
        texture.Type = type;
        texture.Path = path;
        texture.Scale = scale;

        return texture;
    }
    private void DisplayProperties_GXLists()
    {
        var index = Screen.ModelHierarchy._selectedGXList;

        if (Screen.ResourceHandler.CurrentFLVER.GXLists.Count < index)
            return;

        ImGui.Separator();
        ImGui.Text("GX List");
        ImGui.Separator();

        var entry = Screen.ResourceHandler.CurrentFLVER.GXLists[index];

        for (int i = 0; i < entry.Count; i++)
        {
            DisplayProperties_Material_GXItem(entry[i], i);
        }
    }

    private void DisplayProperties_Material_GXItem(StudioCore.Formats.PureFLVER.FLVER2.FLVER2.GXItem item, int index)
    {
        ImGui.Separator();
        ImGui.Text("GX Item");
        ImGui.Separator();

        var id = item.ID;
        var unk04 = item.Unk04;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("ID");
        ImguiUtils.ShowHoverTooltip("In DS2, ID is just a number; in other games, it's 4 ASCII characters.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Unk04");
        ImguiUtils.ShowHoverTooltip("Unknown; typically 100.");

        ImGui.NextColumn();

        var colWidth = ImGui.GetColumnWidth();
        ImGui.AlignTextToFramePadding();
        ImGui.SetNextItemWidth(colWidth);
        ImGui.InputText($"##ID_item{index}", ref id, 255);
        ImGui.AlignTextToFramePadding();
        ImGui.SetNextItemWidth(colWidth);
        ImGui.InputInt($"##Unk04_item{index}", ref unk04);

        ImGui.Columns(1);

        var data = GXDataEditor.DisplayProperties_GXItem_HandleData(item);

        // Changes
        item.ID = id;
        item.Unk04 = unk04;
        item.Data = data;
    }

    private void DisplayProperties_Nodes()
    {
        var index = Screen.ModelHierarchy._selectedNode;

        if (Screen.ResourceHandler.CurrentFLVER.Nodes.Count < index)
            return;

        ImGui.Separator();
        ImGui.Text("Node");
        ImGui.Separator();

        var entry = Screen.ResourceHandler.CurrentFLVER.Nodes[index];

        var name = entry.Name;
        int parentIndex = entry.ParentIndex;
        int firstChildIndex = entry.FirstChildIndex;
        int nextSiblingIndex = entry.NextSiblingIndex;
        int previousSiblingIndex = entry.PreviousSiblingIndex;
        var translation = entry.Translation;
        var rotation = entry.Rotation;
        var scale = entry.Scale;
        var bbMin = entry.BoundingBoxMin;
        var bbMax = entry.BoundingBoxMax;
        var flags = (int)entry.Flags;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Name");
        ImguiUtils.ShowHoverTooltip("The name of this node");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Parent Index");
        ImguiUtils.ShowHoverTooltip("Index of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("First Child Index");
        ImguiUtils.ShowHoverTooltip("Index of this node's first child, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Next Sibling Index");
        ImguiUtils.ShowHoverTooltip("Index of the next child of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Previous Sibling Index");
        ImguiUtils.ShowHoverTooltip("Index of the previous child of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Translation");
        ImguiUtils.ShowHoverTooltip("Translation of this bone.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Rotation");
        ImguiUtils.ShowHoverTooltip("Rotation of this bone; euler radians in XZY order.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Scale");
        ImguiUtils.ShowHoverTooltip("Scale of this bone.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Bounding Box: Minimum");
        ImguiUtils.ShowHoverTooltip("Minimum extent of the vertices weighted to this bone.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Bounding Box: Maximum");
        ImguiUtils.ShowHoverTooltip("Maximum extent of the vertices weighted to this bone.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Flags");
        ImguiUtils.ShowHoverTooltip("A set of flags denoting the properties of a node");

        ImGui.NextColumn();

        var colWidth = ImGui.GetColumnWidth();

        ImGui.AlignTextToFramePadding();
        ImGui.SetNextItemWidth(colWidth);
        ImGui.InputText($"##Name", ref name, 255);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##ParentIndex", ref parentIndex);

        DecorationHandler.NodeIndexDecorator(parentIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##FirstChildIndex", ref firstChildIndex);

        DecorationHandler.NodeIndexDecorator(firstChildIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##NextSiblingIndex", ref nextSiblingIndex);

        DecorationHandler.NodeIndexDecorator(nextSiblingIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##PreviousSiblingIndex", ref previousSiblingIndex);

        DecorationHandler.NodeIndexDecorator(previousSiblingIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##Translation", ref translation);

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##Rotation", ref rotation);

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##Scale", ref scale);

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##BoundingBoxMin", ref bbMin);

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##BoundingBoxMax", ref bbMax);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##Flags", ref flags);

        ImGui.Columns(1);

        // Changes
        entry.Name = name;

        if (parentIndex > short.MaxValue)
            parentIndex = short.MaxValue;

        entry.ParentIndex = (short)parentIndex;

        if (firstChildIndex > short.MaxValue)
            firstChildIndex = short.MaxValue;

        entry.FirstChildIndex = (short)firstChildIndex;

        if (nextSiblingIndex > short.MaxValue)
            nextSiblingIndex = short.MaxValue;

        entry.NextSiblingIndex = (short)nextSiblingIndex;

        if (previousSiblingIndex > short.MaxValue)
            previousSiblingIndex = short.MaxValue;

        entry.PreviousSiblingIndex = (short)previousSiblingIndex;

        entry.Translation = translation;
        entry.Rotation = rotation;
        entry.Scale = scale;
        entry.BoundingBoxMin = bbMin;
        entry.BoundingBoxMax = bbMax;
        entry.Flags = (NodeFlags)flags;
    }

    private void DisplayProperties_Meshes()
    {
        var index = Screen.ModelHierarchy._selectedMesh;

        if (Screen.ResourceHandler.CurrentFLVER.Meshes.Count < index)
            return;

        ImGui.Separator();
        ImGui.Text("Mesh");
        ImGui.Separator();

        var entry = Screen.ResourceHandler.CurrentFLVER.Meshes[index];

        var useBoneWeights = entry.UseBoneWeights;
        var materialIndex = entry.MaterialIndex;
        var nodeIndex = entry.NodeIndex;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Use Bone Weights");
        ImguiUtils.ShowHoverTooltip("Determines how the mesh is skinned.\nIf it is true the mesh is assumed to be in bind pose and is skinned using the Bone Indices and Bone Weights of the vertices.\n\nIf it is false each vertex specifies a single node to bind to using its NormalW\n\nThe mesh is assumed to not be in bind pose and the transform of the bound node is applied to each vertex.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Material Index");
        ImguiUtils.ShowHoverTooltip("Index of the material used by all triangles in this mesh.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Node Index");
        ImguiUtils.ShowHoverTooltip("Index of the node representing this mesh in the Node list.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.Checkbox($"##UseBoneWeights", ref useBoneWeights);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##MaterialIndex", ref materialIndex);

        DecorationHandler.MaterialIndexDecorator(materialIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##NodeIndex", ref nodeIndex);

        DecorationHandler.NodeIndexDecorator(nodeIndex);

        ImGui.Columns(1);

        // Changes
        entry.UseBoneWeights = useBoneWeights;
        entry.MaterialIndex = materialIndex;
        entry.NodeIndex = nodeIndex;

        if (ImGui.CollapsingHeader("Bounding Box"))
        {
            if(entry.BoundingBox != null)
            {
                entry.BoundingBox = DisplayProperties_Mesh_BoundingBox(entry.BoundingBox);
            }
        }

        if (ImGui.CollapsingHeader("Face Sets", ImGuiTreeNodeFlags.DefaultOpen))
        {
            for (int i = 0; i < entry.FaceSets.Count; i++)
            {
                entry.FaceSets[i] = DisplayProperties_Mesh_FaceSet(entry.FaceSets[i], i);
            }
        }

        if (ImGui.CollapsingHeader("Vertex Buffers"))
        {
            for (int i = 0; i < entry.VertexBuffers.Count; i++)
            {
                entry.VertexBuffers[i] = DisplayProperties_Mesh_VertexBuffers(entry.VertexBuffers[i], i);
            }
        }

        // Don't expose this
        /*
        if (ImGui.CollapsingHeader("Bone Indices"))
        {
            for (int i = 0; i < entry.BoneIndices.Count; i++)
            {
                entry.BoneIndices[i] = DisplayProperties_Mesh_BoneIndices(entry.BoneIndices[i], i);
            }
        }
        */

        // Don't expose this
        /*
        if (ImGui.CollapsingHeader("Vertices"))
        {
            for (int i = 0; i < entry.Textures.Count; i++)
            {
                DisplayProperties_Material_Texture(entry.Textures[i], i);
            }
        }
        */
    }

    private Formats.PureFLVER.FLVER2.FLVER2.FaceSet DisplayProperties_Mesh_FaceSet(Formats.PureFLVER.FLVER2.FLVER2.FaceSet faceset, int index)
    {
        ImGui.Separator();
        ImGui.Text($"Face Set {index}");
        ImGui.Separator();

        var flags = (int)faceset.Flags;
        var triangleStrip = faceset.TriangleStrip;
        var cullBackfaces = faceset.CullBackfaces;
        int unk06 = faceset.Unk06;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Flags");
        ImguiUtils.ShowHoverTooltip("Flags on a faceset, mostly just used to determine lod level.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Triangle Strip");
        ImguiUtils.ShowHoverTooltip("Whether vertices are defined as a triangle strip or individual triangles.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Cull Backfaces");
        ImguiUtils.ShowHoverTooltip("Whether triangles can be seen through from behind.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Unk06");
        ImguiUtils.ShowHoverTooltip("");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##Flags_faceset{index}", ref flags);

        ImGui.AlignTextToFramePadding();
        ImGui.Checkbox($"##TriangleStrip_faceset{index}", ref triangleStrip);

        ImGui.AlignTextToFramePadding();
        ImGui.Checkbox($"##CullBackfaces_faceset{index}", ref cullBackfaces);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##Unk06_faceset{index}", ref unk06);

        ImGui.Columns(1);

        faceset.Flags = (FSFlags)flags;
        faceset.TriangleStrip = triangleStrip;
        faceset.CullBackfaces = cullBackfaces;

        if(unk06 > short.MaxValue)
            unk06 = short.MaxValue;

        faceset.Unk06 = (short)unk06;

        // Don't expose this
        /*
        if (ImGui.CollapsingHeader("Indices"))
        {
            for (int i = 0; i < faceset.Indices.Count; i++)
            {
                faceset.Indices[i] = DisplayProperties_Mesh_FaceSetIndices(faceset.Indices[i], i);
            }
        }
        */

        return faceset;
    }

    private Formats.PureFLVER.FLVER2.FLVER2.VertexBuffer DisplayProperties_Mesh_VertexBuffers(Formats.PureFLVER.FLVER2.FLVER2.VertexBuffer vertexBuffer, int index)
    {
        var layoutIndex = vertexBuffer.LayoutIndex;

        ImGui.Separator();
        ImGui.Text($"Vertex Buffer {index}");
        ImGui.Separator();

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Layout Index");
        ImguiUtils.ShowHoverTooltip("");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##LayoutIndex{index}", ref layoutIndex);

        DecorationHandler.LayoutIndexDecorator(layoutIndex);

        ImGui.Columns(1);

        vertexBuffer.LayoutIndex = layoutIndex;

        return vertexBuffer;
    }

    private int DisplayProperties_Mesh_FaceSetIndices(int vertexIndex, int index)
    {
        var curVertexIndex = vertexIndex;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Index");
        ImguiUtils.ShowHoverTooltip("");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##VertexIndex{index}", ref curVertexIndex);

        ImGui.Columns(1);

        return curVertexIndex;
    }

    private int DisplayProperties_Mesh_BoneIndices(int boneIndex, int index)
    {
        var curBoneIndex = boneIndex;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Index");
        ImguiUtils.ShowHoverTooltip("");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##BoneIndex{index}", ref curBoneIndex);

        ImGui.Columns(1);

        return curBoneIndex;
    }

    private BoundingBoxes DisplayProperties_Mesh_BoundingBox(BoundingBoxes boundingBox)
    {
        var bbMin = boundingBox.Min;
        var bbMax = boundingBox.Max;
        var bbUnk = boundingBox.Unk;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Mesh Bounding Box: Minimum");
        ImguiUtils.ShowHoverTooltip("Minimum extent of the mesh.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Mesh Bounding Box: Maximum");
        ImguiUtils.ShowHoverTooltip("Maximum extent of the mesh.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Mesh Bounding Box: Unknown");
        ImguiUtils.ShowHoverTooltip("Unknown; only present in Sekiro.");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##Min", ref bbMin);

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##Max", ref bbMax);

        ImGui.AlignTextToFramePadding();
        ImGui.InputFloat3($"##Unk", ref bbUnk);

        ImGui.Columns(1);

        // Changes
        boundingBox.Min = bbMin;
        boundingBox.Max = bbMax;
        boundingBox.Unk = bbUnk;

        return boundingBox;
    }

    private void DisplayProperties_BufferLayouts()
    {
        var index = Screen.ModelHierarchy._selectedBufferLayout;

        if (Screen.ResourceHandler.CurrentFLVER.BufferLayouts.Count < index)
            return;

        var entry = Screen.ResourceHandler.CurrentFLVER.BufferLayouts[index];

        for(int i = 0; i < entry.Count; i++)
        {
            entry[i] = DisplayProperties_BufferLayout_LayoutMember(entry[i], i);
        }
    }

    private Formats.PureFLVER.FLVER.LayoutMember DisplayProperties_BufferLayout_LayoutMember(Formats.PureFLVER.FLVER.LayoutMember layout, int index)
    {
        ImGui.Separator();
        ImGui.Text($"Buffer Layout {index}");
        ImGui.Separator();

        ImGui.AlignTextToFramePadding();
        ImGui.Text($"Unk00:");
        ImGui.SameLine();
        ImGui.TextColored(CFG.Current.ImGui_AliasName_Text, @$"{layout.Unk00}");
        ImguiUtils.ShowHoverTooltip("Unknown; 0, 1, or 2.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text($"Layout Type:");
        ImGui.SameLine();
        ImGui.TextColored(CFG.Current.ImGui_AliasName_Text, @$"{layout.Type}");
        ImguiUtils.ShowHoverTooltip("Format used to store this member.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text($"Layout Semantic:");
        ImGui.SameLine();
        ImGui.TextColored(CFG.Current.ImGui_AliasName_Text, @$"{layout.Semantic}");
        ImguiUtils.ShowHoverTooltip("Vertex property being stored.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text($"Index:");
        ImGui.SameLine();
        ImGui.TextColored(CFG.Current.ImGui_AliasName_Text, @$"{layout.Index}");
        ImguiUtils.ShowHoverTooltip("For semantics that may appear more than once such as UVs, which one this member is.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text($"Size:");
        ImGui.SameLine();
        ImGui.TextColored(CFG.Current.ImGui_AliasName_Text, @$"{layout.Size}");
        ImguiUtils.ShowHoverTooltip("The size of this member's ValueType, in bytes.");

        return layout;
    }

    private void DisplayProperties_BaseSkeletons()
    {
        var index = Screen.ModelHierarchy._selectedBaseSkeleton;

        if (Screen.ResourceHandler.CurrentFLVER.Skeletons.BaseSkeleton.Count < index)
            return;

        ImGui.Separator();
        ImGui.Text("Standard Skeleton Hierarchy");
        ImGui.Separator();
        ImguiUtils.ShowHoverTooltip("Contains the standard skeleton hierarchy, which corresponds to the node hierarchy.");

        var entry = Screen.ResourceHandler.CurrentFLVER.Skeletons.BaseSkeleton[index];

        int parentIndex = entry.ParentIndex;
        int firstChildIndex = entry.FirstChildIndex;
        int nextSiblingIndex = entry.NextSiblingIndex;
        int previousSiblingIndex = entry.PreviousSiblingIndex;
        int nodeIndex = entry.NodeIndex;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Parent Index");
        ImguiUtils.ShowHoverTooltip("Index of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("First Child Index");
        ImguiUtils.ShowHoverTooltip("Index of this node's first child, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Next Sibling Index");
        ImguiUtils.ShowHoverTooltip("Index of the next child of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Previous Sibling Index");
        ImguiUtils.ShowHoverTooltip("Index of the previous child of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Node Index");
        ImguiUtils.ShowHoverTooltip("Index of the node in the Node list.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##ParentIndex", ref parentIndex);

        DecorationHandler.NodeIndexDecorator(parentIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##FirstChildIndex", ref firstChildIndex);

        DecorationHandler.NodeIndexDecorator(firstChildIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##NextSiblingIndex", ref nextSiblingIndex);

        DecorationHandler.NodeIndexDecorator(nextSiblingIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##PreviousSiblingIndex", ref previousSiblingIndex);

        DecorationHandler.NodeIndexDecorator(previousSiblingIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##NodeIndex", ref nodeIndex);

        DecorationHandler.NodeIndexDecorator(nodeIndex);

        ImGui.Columns(1);

        if (parentIndex > short.MaxValue)
            parentIndex = short.MaxValue;

        entry.ParentIndex = (short)parentIndex;

        if (firstChildIndex > short.MaxValue)
            firstChildIndex = short.MaxValue;

        entry.FirstChildIndex = (short)firstChildIndex;

        if (nextSiblingIndex > short.MaxValue)
            nextSiblingIndex = short.MaxValue;

        entry.NextSiblingIndex = (short)nextSiblingIndex;

        if (previousSiblingIndex > short.MaxValue)
            previousSiblingIndex = short.MaxValue;

        entry.PreviousSiblingIndex = (short)previousSiblingIndex;

        entry.NodeIndex = nodeIndex;
    }

    private void DisplayProperties_AllSkeletons()
    {
        var index = Screen.ModelHierarchy._selectedAllSkeleton;

        if (Screen.ResourceHandler.CurrentFLVER.Skeletons.AllSkeletons.Count < index)
            return;

        ImGui.Separator();
        ImGui.Text("Full Skeleton Hierarchy");
        ImGui.Separator();
        ImguiUtils.ShowHoverTooltip("Contains all skeleton hierarchies including that of the control rig and the ragdoll bones.");

        var entry = Screen.ResourceHandler.CurrentFLVER.Skeletons.AllSkeletons[index];

        int parentIndex = entry.ParentIndex;
        int firstChildIndex = entry.FirstChildIndex;
        int nextSiblingIndex = entry.NextSiblingIndex;
        int previousSiblingIndex = entry.PreviousSiblingIndex;
        int nodeIndex = entry.NodeIndex;

        // Display
        ImGui.Columns(2);

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Parent Index");
        ImguiUtils.ShowHoverTooltip("Index of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("First Child Index");
        ImguiUtils.ShowHoverTooltip("Index of this node's first child, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Next Sibling Index");
        ImguiUtils.ShowHoverTooltip("Index of the next child of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Previous Sibling Index");
        ImguiUtils.ShowHoverTooltip("Index of the previous child of this node's parent, or -1 for none.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("Node Index");
        ImguiUtils.ShowHoverTooltip("Index of the node in the Node list.");

        ImGui.AlignTextToFramePadding();
        ImGui.Text("");

        ImGui.NextColumn();

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##ParentIndex", ref parentIndex);

        DecorationHandler.NodeIndexDecorator(parentIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##FirstChildIndex", ref firstChildIndex);

        DecorationHandler.NodeIndexDecorator(firstChildIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##NextSiblingIndex", ref nextSiblingIndex);

        DecorationHandler.NodeIndexDecorator(nextSiblingIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##PreviousSiblingIndex", ref previousSiblingIndex);

        DecorationHandler.NodeIndexDecorator(previousSiblingIndex);

        ImGui.AlignTextToFramePadding();
        ImGui.InputInt($"##NodeIndex", ref nodeIndex);

        DecorationHandler.NodeIndexDecorator(nodeIndex);

        ImGui.Columns(1);

        if (parentIndex > short.MaxValue)
            parentIndex = short.MaxValue;

        entry.ParentIndex = (short)parentIndex;

        if (firstChildIndex > short.MaxValue)
            firstChildIndex = short.MaxValue;

        entry.FirstChildIndex = (short)firstChildIndex;

        if (nextSiblingIndex > short.MaxValue)
            nextSiblingIndex = short.MaxValue;

        entry.NextSiblingIndex = (short)nextSiblingIndex;

        if (previousSiblingIndex > short.MaxValue)
            previousSiblingIndex = short.MaxValue;

        entry.PreviousSiblingIndex = (short)previousSiblingIndex;

        entry.NodeIndex = nodeIndex;
    }
}
