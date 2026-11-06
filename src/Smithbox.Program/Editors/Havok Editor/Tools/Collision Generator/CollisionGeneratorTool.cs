using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace StudioCore.Editors.HavokEditor;

public class CollisionGeneratorTool
{
    public HavokEditorView View;
    public ProjectEntry Project;

    public CollisionGeneratorShape TargetShape = CollisionGeneratorShape.Square;
    public CollisionGeneratorFacing TargetFacing = CollisionGeneratorFacing.Up;
    public CollisionGeneratorMeshType TargetMeshType = CollisionGeneratorMeshType.CompressedMesh;

    /// <summary>
    /// The collision material of the generated triangles, unless a FLVER mesh names its own
    /// </summary>
    public int MaterialId = 1;

    public SquareParameters SquareInputs = new();
    public TriangeParameters TriangleInputs = new();
    public CircleParameters CircleInputs = new();
    public SemiCircleParameters SemiCircleInputs = new();

    public FlverSelectionMenu SelectionMenu;
    public CollisionMaterialPicker MaterialPicker;

    /// <summary>
    /// The preview of the shape the inputs describe, and what it was built from
    /// </summary>
    private MeshPreview Preview;
    private string PreviewKey;

    /// <summary>
    /// The OBJ file the OBJ shape is built from, and which of its groups go into the collision
    /// </summary>
    public ObjCollisionSource SourceObj;
    public string ObjPath = "";
    public bool[] ObjGroupSelection = [];

    public CollisionGeneratorTool(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
        SelectionMenu = new(view, project);
        MaterialPicker = new(project);
    }

    public void Display()
    {
        if (!CFG.Current.HavokEditor_ToolVisibility_CollisionGenerator)
            return;

        if(ToolSections.Header($"{LOC.Get("HAVOK_Tools_Collision_Generator_Tool")}##collisionGeneratorTool"))
        {
            ImGui.BeginChild("CollisionGeneratorSection", ImGuiChildFlags.Borders);

            if (!SupportsGame)
            {
                GUI.WrappedText(LOC.Get("HAVOK_CollisionGen_Unsupported_Game"));
            }
            else if (View.Selection.BinderFileEntry == null)
            {
                GUI.WrappedText(LOC.Get("HAVOK_CollisionGen_No_Binder_Entry"));
            }
            else if (View.Selection.FilePath == null)
            {
                GUI.WrappedText(LOC.Get("HAVOK_CollisionGen_No_File_Entry"));
            }
            else if (View.Selection.CategoryMode is HavokCategoryMode.Map_Collision or HavokCategoryMode.Asset_Collision)
            {
                DisplayGeneratorPanel();
            }
            else
            {
                GUI.WrappedText(LOC.Get("HAVOK_CollisionGen_Invalid_Category"));
            }

            ImGui.EndChild();
        }
    }

    public void DisplayGeneratorPanel()
    {
        var collisionName = Path.GetFileName(View.Selection.FilePath);
        var sourceObject = View.PropertyView.GetSourceObject();

        if (sourceObject == null)
        {
            ImGui.BeginChild("CollisionGenSection");

            GUI.WrappedText(LOC.Get("HAVOK_CollisionGen_No_Collision_Selected"));

            ImGui.EndChild();

            return;
        }

        ImGui.BeginChild("CollisionGenSection");

        GUI.WrappedText(LOC.Get("HAVOK_CollisionGen_Hint_TT", collisionName));
        GUI.Spacer();

        // Shape
        GUI.SimpleHeader(
            LOC.Get("HAVOK_CollisionGen_Shape_Header"),
            LOC.Get("HAVOK_CollisionGen_Shape_Header_TT"));

        var curMode = TargetShape;

        var previewName = LOC.Get(curMode.GetDisplayName());

        if (ImGui.BeginCombo("##subEditorMode", previewName))
        {
            foreach (var entry in Enum.GetValues(typeof(CollisionGeneratorShape)))
            {
                var curType = (CollisionGeneratorShape)entry;

                var displayName = LOC.Get(curType.GetDisplayName());

                if (ImGui.Selectable(displayName, curType == curMode))
                {
                    TargetShape = curType;
                }
            }

            ImGui.EndCombo();
        }

        // Facing
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("HAVOK_CollisionGen_Facing_Header"),
            LOC.Get("HAVOK_CollisionGen_Facing_Header_TT"));

        var curFacing = TargetFacing;

        var previewFacing = LOC.Get(curFacing.GetDisplayName());

        if (ImGui.BeginCombo("##facingMode", previewFacing))
        {
            foreach (var entry in Enum.GetValues(typeof(CollisionGeneratorFacing)))
            {
                var curType = (CollisionGeneratorFacing)entry;

                var displayName = LOC.Get(curType.GetDisplayName());

                if (ImGui.Selectable(displayName, curType == curFacing))
                {
                    TargetFacing = curType;
                }
            }

            ImGui.EndCombo();
        }

        if (SupportsExternMesh)
        {
            // Mesh Type
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("HAVOK_CollisionGen_MeshType_Header"),
                LOC.Get("HAVOK_CollisionGen_MeshType_Header_TT"));

            ImGui.SetNextItemWidth(-1);
            if (ImGui.BeginCombo("##meshTypeMode", LOC.Get(TargetMeshType.GetDisplayName())))
            {
                foreach (var entry in Enum.GetValues<CollisionGeneratorMeshType>())
                {
                    if (ImGui.Selectable($"{LOC.Get(entry.GetDisplayName())}##meshType{entry}", entry == TargetMeshType))
                    {
                        TargetMeshType = entry;
                    }
                }

                ImGui.EndCombo();
            }
            GUI.Tooltip(LOC.Get("HAVOK_CollisionGen_MeshType_Header_TT"));
        }

        if (IsCompressedMesh)
        {
            // Collision Material
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("HAVOK_CollisionGen_Material_Header"),
                LOC.Get("HAVOK_CollisionGen_Material_Header_TT"));

            MaterialPicker.Display(ref MaterialId,
                CollisionMaterialPicker.GetShapeMaterials(View.Selection.SelectedFileWrapper?.Root));

            GUI.Spacer();
            GUI.MultiButtonInput("materialActions",
                "materialFromCurrent",
                LOC.Get("HAVOK_CollisionGen_Material_From_Current"),
                LOC.Get("HAVOK_CollisionGen_Material_From_Current_TT"),
                UseCurrentMaterial);
        }

        // Parameters
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("HAVOK_CollisionGen_Parameters_Header"),
            LOC.Get("HAVOK_CollisionGen_Parameters_Header_TT"));

        // Values are stored as they're typed, so clicking Generate straight after typing uses the new value.
        // Sizes can be fractional; sizes and segment counts are kept to values that make a valid shape.
        if (TargetShape is CollisionGeneratorShape.Square)
        {
            var width = SquareInputs.Width;
            if (ImGui.InputFloat("Width##squareWidth", ref width, 0.5f, 1.0f, "%.2f"))
            {
                SquareInputs.Width = MathF.Max(width, MinShapeSize);
            }

            var length = SquareInputs.Length;
            if (ImGui.InputFloat("Length##squareLength", ref length, 0.5f, 1.0f, "%.2f"))
            {
                SquareInputs.Length = MathF.Max(length, MinShapeSize);
            }
        }

        if (TargetShape is CollisionGeneratorShape.Triangle)
        {
            var size = TriangleInputs.Size;
            if (ImGui.InputFloat("Size##triangleSize", ref size, 0.5f, 1.0f, "%.2f"))
            {
                TriangleInputs.Size = MathF.Max(size, MinShapeSize);
            }
        }

        if (TargetShape is CollisionGeneratorShape.Circle)
        {
            var radius = CircleInputs.Radius;
            if (ImGui.InputFloat("Radius##circleRadius", ref radius, 0.5f, 1.0f, "%.2f"))
            {
                CircleInputs.Radius = MathF.Max(radius, MinShapeSize);
            }

            var segments = CircleInputs.Segments;
            if (ImGui.InputInt("Segments##circleSegements", ref segments))
            {
                CircleInputs.Segments = Math.Max(segments, MinCircleSegments);
            }
        }

        if (TargetShape is CollisionGeneratorShape.SemiCircle)
        {
            var radius = SemiCircleInputs.Radius;
            if (ImGui.InputFloat("Radius##semicircleRadius", ref radius, 0.5f, 1.0f, "%.2f"))
            {
                SemiCircleInputs.Radius = MathF.Max(radius, MinShapeSize);
            }

            var segments = SemiCircleInputs.Segments;
            if (ImGui.InputInt("Segments##semicircleSegements", ref segments))
            {
                SemiCircleInputs.Segments = Math.Max(segments, MinSemiCircleSegments);
            }
        }

        if (TargetShape is CollisionGeneratorShape.FLVER)
        {
            GUI.MultiButtonInput("flverActions",
                "selectFlver",
                LOC.Get("HAVOK_CollisionGen_Select_Flver_Action"),
                LOC.Get("HAVOK_CollisionGen_Select_Flver_Action_TT"),
                SelectFlverSource,

                "selectExternalFlver",
                LOC.Get("HAVOK_CollisionGen_Select_External_Flver_Action"),
                LOC.Get("HAVOK_CollisionGen_Select_External_Flver_Action_TT"),
                SelectExternalFlverSource);

            GUI.Spacer();
            if (SelectionMenu.FlverPath != "")
            {
                GUI.WrappedText(LOC.Get("HAVOK_CollisionGen_Flver_Current_Source_FLVER", Path.GetFileName(SelectionMenu.FlverPath)));
            }
            else
            {
                GUI.WrappedText(LOC.Get("HAVOK_CollisionGen_Flver_Current_Source_FLVER_None"));
            }

            if (SelectionMenu.SourceFlver != null)
            {
                GUI.Spacer();
                GUI.ConditionalHeader(
                    LOC.Get("HAVOK_CollisionGen_Flver_Meshes_Header"),
                    LOC.Get("HAVOK_CollisionGen_Flver_Meshes_Header_TT"),
                    ref CFG.Current.HavokEditor_CollisionGen_Show_Flver_Meshes);

                // The selection buttons stay when the list is hidden
                if (ImGui.Button($"{LOC.Get("HAVOK_CollisionGen_Select_All_Meshes")}##flverMeshSelectAll"))
                {
                    Array.Fill(SelectionMenu.FlverMeshSelection, true);
                }
                ImGui.SameLine();
                if (ImGui.Button($"{LOC.Get("HAVOK_CollisionGen_Select_No_Meshes")}##flverMeshSelectNone"))
                {
                    Array.Fill(SelectionMenu.FlverMeshSelection, false);
                }

                if (CFG.Current.HavokEditor_CollisionGen_Show_Flver_Meshes)
                {
                    DisplayFlverMeshList();
                }
            }
        }

        if (TargetShape is CollisionGeneratorShape.OBJ)
        {
            DisplayObjParameters();
        }

        DisplayPreview();

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("HAVOK_CollisionGen_Action_Header"),
            LOC.Get("HAVOK_CollisionGen_Action_Header_TT"));

        GUI.MultiButtonInput("actions",
            "generateShape",
            LOC.Get("HAVOK_CollisionGen_Generate_Action"),
            LOC.Get("HAVOK_CollisionGen_Generate_Action_TT"),
            GenerateShape);

        ImGui.EndChild();
    }

    /// <summary>
    /// The source FLVER's meshes, each with a checkbox for whether it goes into the collision
    /// </summary>
    private void DisplayFlverMeshList()
    {
        ImGui.BeginChild("flverMeshSelectionList", new Vector2(0, 150), ImGuiChildFlags.Borders);

        for (int i = 0; i < SelectionMenu.SourceFlver.Meshes.Count; i++)
        {
            var mesh = SelectionMenu.SourceFlver.Meshes[i];
            var material = SelectionMenu.SourceFlver.Materials[mesh.MaterialIndex];
            var label = LOC.Get("HAVOK_CollisionGen_Flver_Mesh_Entry", i, material.Name, mesh.Vertices.Count);

            // Each mesh says which material it will get: its own hkm_ name, or the Collision Material setting
            if (IsCompressedMesh)
            {
                label = HavokCollisionMaterials.TryParse(material.Name, out var meshMaterial)
                    ? LOC.Get("HAVOK_CollisionGen_Flver_Mesh_Entry_Material", label, MaterialPicker.Describe(meshMaterial))
                    : LOC.Get("HAVOK_CollisionGen_Flver_Mesh_Entry_Default_Material", label, MaterialPicker.Describe((uint)MaterialId));
            }

            var selected = SelectionMenu.FlverMeshSelection[i];
            if (ImGui.Checkbox($"{label}##flverMesh{i}", ref selected))
            {
                SelectionMenu.FlverMeshSelection[i] = selected;
            }
        }

        ImGui.EndChild();
    }

    /// <summary>
    /// The OBJ source: its file, the axes it was exported with, and its groups
    /// </summary>
    private void DisplayObjParameters()
    {
        GUI.MultiButtonInput("objActions",
            "selectObj",
            LOC.Get("HAVOK_CollisionGen_Select_Obj_Action"),
            LOC.Get("HAVOK_CollisionGen_Select_Obj_Action_TT"),
            SelectObjSource);

        GUI.Spacer();
        if (ObjPath != "")
        {
            GUI.WrappedText(LOC.Get("HAVOK_CollisionGen_Obj_Current_Source", Path.GetFileName(ObjPath)));
        }
        else
        {
            GUI.WrappedText(LOC.Get("HAVOK_CollisionGen_Obj_Current_Source_None"));
        }

        if (SourceObj == null)
            return;

        // OBJ Mirroring
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("HAVOK_CollisionGen_Obj_Mirror_Header"),
            LOC.Get("HAVOK_CollisionGen_Obj_Mirror_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MODEL_ModelExport_Mirror_X")}##objMirrorX", ref CFG.Current.HavokEditor_CollisionGen_Obj_MirrorX);
        GUI.Tooltip(LOC.Get("HAVOK_CollisionGen_Obj_Mirror_Header_TT"));
        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("MODEL_ModelExport_Mirror_Y")}##objMirrorY", ref CFG.Current.HavokEditor_CollisionGen_Obj_MirrorY);
        GUI.Tooltip(LOC.Get("HAVOK_CollisionGen_Obj_Mirror_Header_TT"));
        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("MODEL_ModelExport_Mirror_Z")}##objMirrorZ", ref CFG.Current.HavokEditor_CollisionGen_Obj_MirrorZ);
        GUI.Tooltip(LOC.Get("HAVOK_CollisionGen_Obj_Mirror_Header_TT"));

        // OBJ Forward Axis
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("HAVOK_CollisionGen_Obj_ForwardAxis_Header"),
            LOC.Get("HAVOK_CollisionGen_Obj_ForwardAxis_Header_TT"));

        DisplayAxisCombo("##objForwardAxis", ref CFG.Current.HavokEditor_CollisionGen_Obj_ForwardAxis,
            LOC.Get("HAVOK_CollisionGen_Obj_ForwardAxis_Header_TT"));

        // OBJ Up Axis
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("HAVOK_CollisionGen_Obj_UpAxis_Header"),
            LOC.Get("HAVOK_CollisionGen_Obj_UpAxis_Header_TT"));

        DisplayAxisCombo("##objUpAxis", ref CFG.Current.HavokEditor_CollisionGen_Obj_UpAxis,
            LOC.Get("HAVOK_CollisionGen_Obj_UpAxis_Header_TT"));

        if (!ObjAxesValid)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MODEL_ModelExport_Error_Invalid_Axes"));
        }

        // OBJ Groups
        GUI.Spacer();
        GUI.ConditionalHeader(
            LOC.Get("HAVOK_CollisionGen_Obj_Groups_Header"),
            LOC.Get("HAVOK_CollisionGen_Obj_Groups_Header_TT"),
            ref CFG.Current.HavokEditor_CollisionGen_Show_Obj_Groups);

        // The selection buttons stay when the list is hidden
        if (ImGui.Button($"{LOC.Get("HAVOK_CollisionGen_Select_All_Meshes")}##objGroupSelectAll"))
        {
            Array.Fill(ObjGroupSelection, true);
        }
        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("HAVOK_CollisionGen_Select_No_Meshes")}##objGroupSelectNone"))
        {
            Array.Fill(ObjGroupSelection, false);
        }

        if (CFG.Current.HavokEditor_CollisionGen_Show_Obj_Groups)
        {
            DisplayObjGroupList();
        }
    }

    /// <summary>
    /// The OBJ's groups, each with a checkbox for whether it goes into the collision
    /// </summary>
    private void DisplayObjGroupList()
    {
        ImGui.BeginChild("objGroupSelectionList", new Vector2(0, 150), ImGuiChildFlags.Borders);

        for (int i = 0; i < SourceObj.Groups.Count; i++)
        {
            var group = SourceObj.Groups[i];
            var label = LOC.Get("HAVOK_CollisionGen_Obj_Group_Entry",
                ObjCollisionSource.GetGroupName(group, i), group.Material, group.Triangles.Count / 3);

            // Each group says which material it will get: its own hkm_ name, or the Collision Material setting
            if (IsCompressedMesh)
            {
                label = HavokCollisionMaterials.TryParse(group.Material, out var groupMaterial)
                    ? LOC.Get("HAVOK_CollisionGen_Flver_Mesh_Entry_Material", label, MaterialPicker.Describe(groupMaterial))
                    : LOC.Get("HAVOK_CollisionGen_Flver_Mesh_Entry_Default_Material", label, MaterialPicker.Describe((uint)MaterialId));
            }

            var selected = ObjGroupSelection[i];
            if (ImGui.Checkbox($"{label}##objGroup{i}", ref selected))
            {
                ObjGroupSelection[i] = selected;
            }
        }

        ImGui.EndChild();
    }

    private static void DisplayAxisCombo(string id, ref ModelExportAxis axis, string tooltip)
    {
        ImGui.SetNextItemWidth(-1);
        if (ImGui.BeginCombo(id, LOC.Get(axis.GetDisplayName())))
        {
            foreach (var entry in Enum.GetValues<ModelExportAxis>())
            {
                if (ImGui.Selectable($"{LOC.Get(entry.GetDisplayName())}{id}{entry}", entry == axis))
                {
                    axis = entry;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(tooltip);
    }

    private static bool ObjAxesValid => FlverToObj.AreAxesValid(
        CFG.Current.HavokEditor_CollisionGen_Obj_ForwardAxis, CFG.Current.HavokEditor_CollisionGen_Obj_UpAxis);

    /// <summary>
    /// Takes an OBJ back into the game's space: the inverse of the transform the Model Editor's OBJ export applies
    /// with the same axis settings, so an exported OBJ comes back where it was
    /// </summary>
    private static Matrix4x4 GetObjToGameTransform()
    {
        var gameToObj = FlverToObj.BuildTransform(
            CFG.Current.HavokEditor_CollisionGen_Obj_ForwardAxis,
            CFG.Current.HavokEditor_CollisionGen_Obj_UpAxis,
            CFG.Current.HavokEditor_CollisionGen_Obj_MirrorX,
            CFG.Current.HavokEditor_CollisionGen_Obj_MirrorY,
            CFG.Current.HavokEditor_CollisionGen_Obj_MirrorZ);

        // Only a pure rotation with mirroring is built, so it always has an inverse while the axes are valid
        return Matrix4x4.Invert(gameToObj, out var objToGame) ? objToGame : Matrix4x4.Identity;
    }

    public void SelectObjSource()
    {
        if (!PlatformUtils.Instance.OpenFileDialog(LOC.Get("HAVOK_CollisionGen_Select_Obj_Action"), ["obj"], out var path))
            return;

        try
        {
            var source = ObjCollisionSource.Read(path);

            if (source.Groups.Count == 0)
            {
                Smithbox.LogError(this, LOC.Get("HAVOK_CollisionGen_Obj_No_Faces", Path.GetFileName(path)));
                return;
            }

            SourceObj = source;
            ObjPath = path;
            ObjGroupSelection = new bool[source.Groups.Count];
            Array.Fill(ObjGroupSelection, true);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_CollisionGen_Failed_Obj_Read", path), ex);
        }
    }

    public void DetectFlverSelectionMenu()
    {
        if(SelectionMenu.TriggerFlverSelectionMenu)
        {
            SelectionMenu.TriggerFlverSelectionMenu = false;

            ImGui.OpenPopup("flverSelectionMenuPopup");
        }
    }

    public void SelectFlverSource()
    {
        SelectionMenu.TriggerFlverSelectionMenu = true;
    }

    public void DisplayFlverSelectionMenu()
    {
        if (ImGui.BeginPopup("flverSelectionMenuPopup"))
        {
            SelectionMenu.DisplayFlverSelectionTabs();

            ImGui.EndPopup();
        }
    }

    public void SelectExternalFlverSource()
    {
        var dialog = PlatformUtils.Instance.OpenFileDialog("flverSelect", out var path);
        if (dialog)
        {
            try
            {
                var fileData = File.ReadAllBytes(path);
                SelectionMenu.SourceFlver = FLVER2.Read(fileData);

                SelectionMenu.FlverPath = path;
                SelectionMenu.FlverMeshSelection = new bool[SelectionMenu.SourceFlver.Meshes.Count];
                Array.Fill(SelectionMenu.FlverMeshSelection, true);
            }
            catch (Exception ex)
            {
                Smithbox.LogError(this, LOC.Get("HAVOK_CollisionGen_Failed_FLVER_Read", path), ex);
            }
        }
    }

    public void GenerateShape()
    {
        if (TryBuildGeometry(out var verts, out var indices, out var triangleMaterials))
        {
            BuildAndReplaceShape(verts, indices, triangleMaterials);
        }
    }

    /// <summary>
    /// Builds the triangles of the shape the inputs describe, as Generate will build it, for both Generate and the
    /// preview. False when there is nothing to build, such as a FLVER with no meshes selected.
    /// </summary>
    /// <param name="triangleMaterials">The collision material of each triangle, for FLVER meshes only; null otherwise.</param>
    private bool TryBuildGeometry(out List<Vector3> verts, out List<int> indices, out List<uint> triangleMaterials)
    {
        verts = null;
        indices = null;
        triangleMaterials = null;

        if (TargetShape is CollisionGeneratorShape.Square)
        {
            (verts, indices) = HavokShapeGenerator.GenerateSquare(width: SquareInputs.Width, length: SquareInputs.Length);
        }
        else if (TargetShape is CollisionGeneratorShape.Triangle)
        {
            (verts, indices) = HavokShapeGenerator.GenerateTriangle(size: TriangleInputs.Size);
        }
        else if (TargetShape is CollisionGeneratorShape.Circle)
        {
            (verts, indices) = HavokShapeGenerator.GenerateCircle(radius: CircleInputs.Radius, segments: CircleInputs.Segments);
        }
        else if (TargetShape is CollisionGeneratorShape.SemiCircle)
        {
            (verts, indices) = HavokShapeGenerator.GenerateSemiCircle(radius: SemiCircleInputs.Radius, segments: SemiCircleInputs.Segments);
        }
        else if (TargetShape is CollisionGeneratorShape.FLVER)
        {
            if (SelectionMenu.SourceFlver == null || SelectionMenu.FlverMeshSelection.Length == 0)
                return false;

            var selectedMeshIndices = new List<int>();
            for (int i = 0; i < SelectionMenu.FlverMeshSelection.Length; i++)
            {
                if (SelectionMenu.FlverMeshSelection[i])
                    selectedMeshIndices.Add(i);
            }

            if (selectedMeshIndices.Count == 0)
                return false;

            // A mesh whose material is named hkm_Name_Property gets that collision material
            var flver = SelectionMenu.SourceFlver;
            var materials = new List<uint>();
            uint MeshMaterial(int meshIndex)
            {
                var materialName = flver.Materials[flver.Meshes[meshIndex].MaterialIndex].Name;
                return HavokCollisionMaterials.TryParse(materialName, out var id) ? id : (uint)MaterialId;
            }

            (verts, indices) = HavokShapeGenerator.GenerateFlverCollision(flver, selectedMeshIndices, materials, MeshMaterial);
            triangleMaterials = materials;
        }
        else if (TargetShape is CollisionGeneratorShape.OBJ)
        {
            if (SourceObj == null || !ObjAxesValid)
                return false;

            var selectedGroupIndices = new List<int>();
            for (int i = 0; i < ObjGroupSelection.Length; i++)
            {
                if (ObjGroupSelection[i])
                    selectedGroupIndices.Add(i);
            }

            if (selectedGroupIndices.Count == 0)
                return false;

            // A group whose material is named hkm_Name_Property gets that collision material
            var obj = SourceObj;
            var materials = new List<uint>();
            uint GroupMaterial(int groupIndex)
            {
                return HavokCollisionMaterials.TryParse(obj.Groups[groupIndex].Material, out var id) ? id : (uint)MaterialId;
            }

            (verts, indices) = HavokShapeGenerator.GenerateObjCollision(obj, selectedGroupIndices,
                GetObjToGameTransform(), materials, GroupMaterial);
            triangleMaterials = materials;
        }

        if (verts == null || indices == null || verts.Count == 0 || indices.Count == 0)
            return false;

        if (TargetFacing is CollisionGeneratorFacing.Up)
            HavokShapeGenerator.FlipTriangleWinding(indices);

        return true;
    }

    /// <summary>
    /// Describes everything the generated triangles depend on, so the preview is only rebuilt when one of them changes.
    /// The collision material isn't part of it, as the preview doesn't show materials.
    /// </summary>
    private string GetPreviewKey()
    {
        var shapeInputs = TargetShape switch
        {
            CollisionGeneratorShape.Square => $"{SquareInputs.Width}x{SquareInputs.Length}",
            CollisionGeneratorShape.Triangle => $"{TriangleInputs.Size}",
            CollisionGeneratorShape.Circle => $"{CircleInputs.Radius}/{CircleInputs.Segments}",
            CollisionGeneratorShape.SemiCircle => $"{SemiCircleInputs.Radius}/{SemiCircleInputs.Segments}",
            CollisionGeneratorShape.FLVER => SelectionMenu.SourceFlver == null
                ? ""
                : $"{SelectionMenu.FlverPath}/{RuntimeHelpers.GetHashCode(SelectionMenu.SourceFlver)}/" +
                  string.Concat(SelectionMenu.FlverMeshSelection.Select(s => s ? '1' : '0')),
            CollisionGeneratorShape.OBJ => SourceObj == null
                ? ""
                : $"{ObjPath}/{RuntimeHelpers.GetHashCode(SourceObj)}/" +
                  string.Concat(ObjGroupSelection.Select(s => s ? '1' : '0')) +
                  $"/{CFG.Current.HavokEditor_CollisionGen_Obj_ForwardAxis}/{CFG.Current.HavokEditor_CollisionGen_Obj_UpAxis}" +
                  $"/{CFG.Current.HavokEditor_CollisionGen_Obj_MirrorX}{CFG.Current.HavokEditor_CollisionGen_Obj_MirrorY}{CFG.Current.HavokEditor_CollisionGen_Obj_MirrorZ}",
            _ => ""
        };

        return $"{TargetShape}|{TargetFacing}|{shapeInputs}";
    }

    /// <summary>
    /// Asks for a new preview when the shape has changed since the current one was asked for
    /// </summary>
    private void UpdatePreview()
    {
        var key = GetPreviewKey();

        if (key == PreviewKey)
            return;

        PreviewKey = key;

        Preview?.Dispose();
        Preview = null;

        try
        {
            if (!TryBuildGeometry(out var verts, out var indices, out _))
                return;

            var color = CFG.Current.Viewport_Collision_Color;
            Preview = MeshPreviewRenderer.Request(verts, indices,
                System.Drawing.Color.FromArgb(255, (byte)color.X, (byte)color.Y, (byte)color.Z));
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_CollisionGen_Preview_Failed"), ex);
        }
    }

    /// <summary>
    /// A 3D preview of the shape Generate will build, drawn like the Model Selector's previews
    /// </summary>
    private void DisplayPreview()
    {
        // Preview
        GUI.Spacer();
        GUI.ConditionalHeader(
            LOC.Get("HAVOK_CollisionGen_Preview_Header"),
            LOC.Get("HAVOK_CollisionGen_Preview_Header_TT"),
            ref CFG.Current.HavokEditor_CollisionGen_Show_Preview);

        if (!MeshPreviewRenderer.IsSupported)
        {
            GUI.WrappedText(LOC.Get("EDITOR_ModelPreview_Unsupported"));
            return;
        }

        // Kept up to date while hidden, so the triangle count below it stays right
        UpdatePreview();

        if (CFG.Current.HavokEditor_CollisionGen_Show_Preview)
        {
            DisplayPreviewImage();
        }

        if (Preview != null)
        {
            GUI.WrappedText(LOC.Get("HAVOK_CollisionGen_Preview_Triangle_Count", Preview.TriangleCount));
        }
    }

    private void DisplayPreviewImage()
    {
        var size = MathF.Min(ImGui.GetContentRegionAvail().X, MeshPreviewRenderer.Resolution * DPI.UIScale());
        size = MathF.Max(size, ModelPreviewBrowser.MinPreviewSize * DPI.UIScale());

        var start = ImGui.GetCursorScreenPos();
        var end = start + new Vector2(size, size);

        ImGui.Dummy(new Vector2(size, size));

        var drawList = ImGui.GetWindowDrawList();

        if (Preview is { State: MeshPreviewState.Ready })
        {
            drawList.AddImage(new ImTextureID(Preview.TextureHandle), start, end);
        }
        else
        {
            // Nothing to build is shown the same as a mesh that couldn't be drawn
            var text = Preview is { State: MeshPreviewState.Pending }
                ? LOC.Get("EDITOR_ModelPreview_Pending")
                : LOC.Get("EDITOR_ModelPreview_Unavailable");

            drawList.AddRect(start, end, ImGui.GetColorU32(ImGuiCol.Border));
            drawList.AddText(start + (new Vector2(size, size) - ImGui.CalcTextSize(text)) * 0.5f,
                ImGui.GetColorU32(ImGuiCol.TextDisabled), text);
        }
    }

    /// <summary>
    /// Lets go of the preview, for when the view this tool belongs to is closed
    /// </summary>
    public void Dispose()
    {
        Preview?.Dispose();
        Preview = null;
        PreviewKey = null;
    }

    /// <summary>
    /// The games whose collision is hknp: DS3 and BB (hk2014), SDT (hk2016) and the 2018/2019 games. All of them get
    /// compressed meshes. DeS, DS1, DS1R and DS2S use the older hkp physics (MOPP trees over extended meshes), which
    /// has no generator yet (Documentation/Claude/Collision Generator All Versions.md).
    /// </summary>
    private bool SupportsGame => Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.BB
        || HavokTypeUtils.GetSdk(Project) is Havok.HavokSdk.Hk2016 or Havok.HavokSdk.Hk2018 or Havok.HavokSdk.Hk2019;

    /// <summary>
    /// Whether an extern mesh can be built instead of a compressed mesh. SDT ships extern meshes only in its f
    /// binders, with an AABB tree the generator doesn't build, so it gets compressed meshes alone.
    /// </summary>
    private bool SupportsExternMesh => SupportsGame && HavokTypeUtils.GetSdk(Project) is not Havok.HavokSdk.Hk2016;

    private bool IsCompressedMesh => SupportsGame
        && (TargetMeshType is CollisionGeneratorMeshType.CompressedMesh || !SupportsExternMesh);

    /// <summary>
    /// Takes the collision material of the shape being replaced, from the first triangle group of its FromSoftware
    /// parameters
    /// </summary>
    public void UseCurrentMaterial()
    {
        var materials = CollisionMaterialPicker.GetShapeMaterials(View.Selection.SelectedFileWrapper?.Root);

        if (materials.Count == 0)
        {
            Smithbox.Log(this, LOC.Get("HAVOK_CollisionGen_Material_From_Current_None"));
            return;
        }

        MaterialId = (int)materials[0];
    }

    public void BuildAndReplaceShape(List<Vector3> verts, List<int> indices, List<uint> triangleMaterials = null)
    {
        try
        {
            ReplaceShape(verts, indices, triangleMaterials);
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_CollisionGen_Generate_Failed", Path.GetFileName(View.Selection.FilePath)), ex);
        }
    }

    private void ReplaceShape(List<Vector3> verts, List<int> indices, List<uint> triangleMaterials)
    {
        var collisionName = Path.GetFileName(View.Selection.FilePath);
        var fileWrapper = View.Selection.SelectedFileWrapper;
        var sourceObject = fileWrapper?.Root;

        if (sourceObject == null)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_CollisionGen_No_Collision_Selected"));
            return;
        }

        // The replacement goes through the action manager, so it can be undone and marks the file as modified
        HavokDelegateAction action = null;

        // 2018 and 2019: one shared builder, compiled into each class set
        if (sourceObject is Havok.hk2019.hkRootLevelContainer root2019)
        {
            Havok.hk2019.hknpShape shape = IsCompressedMesh
                ? Havok.hk2019.Builders.CollisionShapeBuilder.BuildCompressedMeshShape(verts, indices, triangleMaterials, (uint)MaterialId)
                : Havok.hk2019.Builders.CollisionShapeBuilder.BuildExternMeshShape(verts, indices);

            if (Havok.hk2019.Builders.CollisionShapeBuilder.ReplaceMeshShape(root2019, shape) is { } swap)
                action = new HavokDelegateAction(fileWrapper, swap.Apply, swap.Undo);
        }
        else if (sourceObject is Havok.hk2018.hkRootLevelContainer root2018)
        {
            Havok.hk2018.hknpShape shape = IsCompressedMesh
                ? Havok.hk2018.Builders.CollisionShapeBuilder.BuildCompressedMeshShape(verts, indices, triangleMaterials, (uint)MaterialId)
                : Havok.hk2018.Builders.CollisionShapeBuilder.BuildExternMeshShape(verts, indices);

            if (Havok.hk2018.Builders.CollisionShapeBuilder.ReplaceMeshShape(root2018, shape) is { } swap)
                action = new HavokDelegateAction(fileWrapper, swap.Apply, swap.Undo);
        }
        // SDT (hk2016): DS3's mesh tree under 2018's shape header, compressed meshes only
        else if (sourceObject is Havok.hk2016.hkRootLevelContainer root2016)
        {
            var shape = Havok.hk2016.Builders.CollisionShapeBuilder.BuildCompressedMeshShape(verts, indices, triangleMaterials, (uint)MaterialId);

            if (Havok.hk2016.Builders.CollisionShapeBuilder.ReplaceMeshShape(root2016, shape) is { } swap)
                action = new HavokDelegateAction(fileWrapper, swap.Apply, swap.Undo);
        }
        // DS3 and BB (hk2014): their own generator, as the 2014 compressed mesh is laid out differently.
        // TODO: check the extern mesh works in-game; some of the 2014 classes differ from the 2018 ones
        else if (sourceObject is Havok.hk2014.hkRootLevelContainer root2014)
        {
            var variant = Project.Descriptor.ProjectType is ProjectType.BB
                ? HavokCompressedMeshGenerator_hk2014.Variant.BB
                : HavokCompressedMeshGenerator_hk2014.Variant.DS3;

            Havok.hk2014.hknpShape shape = IsCompressedMesh
                ? HavokCompressedMeshGenerator_hk2014.BuildCompressedMeshShape(verts, indices, triangleMaterials, (uint)MaterialId, variant)
                : HavokShapeGenerator_hk2014.BuildExternMeshShape(verts, indices);
            action = HavokShapeGenerator_hk2014.CreateReplaceMeshShapeAction(fileWrapper, root2014, shape);
        }

        if (action == null)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_CollisionGen_Generate_No_Replaceable_Shape", collisionName));
            return;
        }

        if (View.ActionManager.ExecuteAction(action))
        {
            if (IsCompressedMesh)
            {
                // Say which materials went onto the collision
                var applied = (triangleMaterials is { Count: > 0 } ? triangleMaterials.Distinct() : [(uint)MaterialId])
                    .Select(MaterialPicker.Describe);

                Smithbox.Log(this, LOC.Get("HAVOK_CollisionGen_Generate_Log_Material", collisionName, string.Join(", ", applied)));
            }
            else
            {
                Smithbox.Log(this, LOC.Get("HAVOK_CollisionGen_Generate_Log", collisionName));
            }
        }
    }

    public enum CollisionGeneratorShape
    {
        [Display(Name = "HAVOK_CollisionGeneratorShape_Square")]
        Square,
        [Display(Name = "HAVOK_CollisionGeneratorShape_Circle")]
        Circle,
        [Display(Name = "HAVOK_CollisionGeneratorShape_Triangle")]
        Triangle,
        [Display(Name = "HAVOK_CollisionGeneratorShape_SemiCircle")]
        SemiCircle,
        [Display(Name = "HAVOK_CollisionGeneratorShape_FLVER")]
        FLVER,
        [Display(Name = "HAVOK_CollisionGeneratorShape_OBJ")]
        OBJ
    }

    public enum CollisionGeneratorMeshType
    {
        [Display(Name = "HAVOK_CollisionGeneratorMeshType_Compressed")]
        CompressedMesh,
        [Display(Name = "HAVOK_CollisionGeneratorMeshType_Extern")]
        ExternMesh
    }

    public enum CollisionGeneratorFacing
    {
        [Display(Name = "HAVOK_CollisionGeneratorFacing_Up")]
        Up,
        [Display(Name = "HAVOK_CollisionGeneratorFacing_Down")]
        Down
    }

    /// <summary>
    /// The smallest size or radius the inputs accept, so a shape never collapses to zero area
    /// </summary>
    private const float MinShapeSize = 0.01f;

    private const int MinCircleSegments = 3;
    private const int MinSemiCircleSegments = 2;

    public class SquareParameters
    {
        public float Length { get; set; } = 1;
        public float Width { get; set; } = 1;
    }

    public class TriangeParameters
    {
        public float Size { get; set; } = 1;
    }

    public class CircleParameters
    {
        public float Radius { get; set; } = 1;
        public int Segments { get; set; } = 24;
    }

    public class SemiCircleParameters
    {
        public float Radius { get; set; } = 1;
        public int Segments { get; set; } = 24;
    }
}