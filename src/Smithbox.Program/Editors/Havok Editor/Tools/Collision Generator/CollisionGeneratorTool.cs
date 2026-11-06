using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Utilities;
using System.ComponentModel.DataAnnotations;
using System.Numerics;

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

        if (SupportsCompressedMesh)
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

            if (TargetMeshType is CollisionGeneratorMeshType.CompressedMesh)
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
                GUI.SimpleHeader(
                    LOC.Get("HAVOK_CollisionGen_Flver_Meshes_Header"),
                    LOC.Get("HAVOK_CollisionGen_Flver_Meshes_Header_TT"));

                if (ImGui.Button($"{LOC.Get("HAVOK_CollisionGen_Select_All_Meshes")}##flverMeshSelectAll"))
                {
                    Array.Fill(SelectionMenu.FlverMeshSelection, true);
                }
                ImGui.SameLine();
                if (ImGui.Button($"{LOC.Get("HAVOK_CollisionGen_Select_No_Meshes")}##flverMeshSelectNone"))
                {
                    Array.Fill(SelectionMenu.FlverMeshSelection, false);
                }

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
        }

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
        if (TargetShape is CollisionGeneratorShape.Square)
        {
            var (verts, indices) = HavokShapeGenerator.GenerateSquare(width: SquareInputs.Width, length: SquareInputs.Length);

            if (TargetFacing is CollisionGeneratorFacing.Up)
                HavokShapeGenerator.FlipTriangleWinding(indices);

            BuildAndReplaceShape(verts, indices);
        }
        else if (TargetShape is CollisionGeneratorShape.Triangle)
        {
            var (verts, indices) = HavokShapeGenerator.GenerateTriangle(size: TriangleInputs.Size);

            if (TargetFacing is CollisionGeneratorFacing.Up)
                HavokShapeGenerator.FlipTriangleWinding(indices);

            BuildAndReplaceShape(verts, indices);
        }
        else if (TargetShape is CollisionGeneratorShape.Circle)
        {
            var (verts, indices) = HavokShapeGenerator.GenerateCircle(radius: CircleInputs.Radius, segments: CircleInputs.Segments);

            if (TargetFacing is CollisionGeneratorFacing.Up)
                HavokShapeGenerator.FlipTriangleWinding(indices);

            BuildAndReplaceShape(verts, indices);
        }
        else if (TargetShape is CollisionGeneratorShape.SemiCircle)
        {
            var (verts, indices) = HavokShapeGenerator.GenerateSemiCircle(radius: SemiCircleInputs.Radius, segments: SemiCircleInputs.Segments);

            if (TargetFacing is CollisionGeneratorFacing.Up)
                HavokShapeGenerator.FlipTriangleWinding(indices);

            BuildAndReplaceShape(verts, indices);
        }
        else if (TargetShape is CollisionGeneratorShape.FLVER)
        {
            if (SelectionMenu.SourceFlver == null || SelectionMenu.FlverMeshSelection.Length == 0)
                return;

            var selectedMeshIndices = new List<int>();
            for (int i = 0; i < SelectionMenu.FlverMeshSelection.Length; i++)
            {
                if (SelectionMenu.FlverMeshSelection[i])
                    selectedMeshIndices.Add(i);
            }

            if (selectedMeshIndices.Count == 0)
                return;

            // A mesh whose material is named hkm_Name_Property gets that collision material
            var flver = SelectionMenu.SourceFlver;
            var triangleMaterials = new List<uint>();
            uint MeshMaterial(int meshIndex)
            {
                var materialName = flver.Materials[flver.Meshes[meshIndex].MaterialIndex].Name;
                return HavokCollisionMaterials.TryParse(materialName, out var id) ? id : (uint)MaterialId;
            }

            var (verts, indices) = HavokShapeGenerator.GenerateFlverCollision(flver, selectedMeshIndices, triangleMaterials, MeshMaterial);

            if (verts.Count == 0 || indices.Count == 0)
                return;

            if (TargetFacing is CollisionGeneratorFacing.Up)
                HavokShapeGenerator.FlipTriangleWinding(indices);

            BuildAndReplaceShape(verts, indices, triangleMaterials);
        }
    }

    /// <summary>
    /// Compressed meshes are built for DS3 (hk2014) and the 2018/2019 games. Older games (DS1, DS1R, BB) use the
    /// older hkp physics, so only get extern meshes.
    /// </summary>
    /// <summary>
    /// The games the generators are built for: DS3 (hk2014 hknp) and the 2018/2019 games. The others (DeS, DS1, DS1R
    /// with hkp physics; BB, DS2S and SDT, whose shapes it was never checked against) open in the editor without it.
    /// </summary>
    private bool SupportsGame => Project.Descriptor.ProjectType is ProjectType.DS3
        || HavokTypeUtils.GetSdk(Project) is Havok.HavokSdk.Hk2018 or Havok.HavokSdk.Hk2019;

    private bool SupportsCompressedMesh => HavokTypeUtils.GetSdk(Project) is Havok.HavokSdk.Hk2018 or Havok.HavokSdk.Hk2019
        || Project.Descriptor.ProjectType is ProjectType.DS3;

    private bool IsCompressedMesh => SupportsCompressedMesh && TargetMeshType is CollisionGeneratorMeshType.CompressedMesh;

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

        if (sourceObject is Havok.hk2019.hkRootLevelContainer)
        {
            if (sourceObject is Havok.hk2019.hkRootLevelContainer root)
            {
                Havok.hk2019.hknpShape shape = IsCompressedMesh
                    ? HavokCompressedMeshGenerator_HKX3_2019.BuildCompressedMeshShape(verts, indices, triangleMaterials, (uint)MaterialId)
                    : HavokShapeGenerator_HKX3_2019.BuildExternMeshShape(verts, indices);
                action = HavokShapeGenerator_HKX3_2019.CreateReplaceMeshShapeAction(fileWrapper, root, shape);
            }
        }
        else if (sourceObject is Havok.hk2018.hkRootLevelContainer)
        {
            if (sourceObject is Havok.hk2018.hkRootLevelContainer root)
            {
                Havok.hk2018.hknpShape shape = IsCompressedMesh
                    ? HavokCompressedMeshGenerator_HKX3.BuildCompressedMeshShape(verts, indices, triangleMaterials, (uint)MaterialId)
                    : HavokShapeGenerator_HKX3.BuildExternMeshShape(verts, indices);
                action = HavokShapeGenerator_HKX3.CreateReplaceMeshShapeAction(fileWrapper, root, shape);
            }
        }
        else
        {
            // TODO: check the extern mesh works in-game, changes were made HavokShapeGenerator_HKX2 since the structure of some of the Havok Classes is slightly different to HKX3
            if (sourceObject is Havok.hk2014.hkRootLevelContainer root)
            {
                Havok.hk2014.hknpShape shape = IsCompressedMesh
                    ? HavokCompressedMeshGenerator_HKX2.BuildCompressedMeshShape(verts, indices, triangleMaterials, (uint)MaterialId)
                    : HavokShapeGenerator_HKX2.BuildExternMeshShape(verts, indices);
                action = HavokShapeGenerator_HKX2.CreateReplaceMeshShapeAction(fileWrapper, root, shape);
            }
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
        FLVER
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