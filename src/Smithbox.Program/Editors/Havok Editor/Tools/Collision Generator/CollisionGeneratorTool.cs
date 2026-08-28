using Hexa.NET.ImGui;
using HKLib.hk2018;
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

    public SquareParameters SquareInputs = new();
    public TriangeParameters TriangleInputs = new();
    public CircleParameters CircleInputs = new();
    public SemiCircleParameters SemiCircleInputs = new();
    public bool BuildMaterialPalette = false;

    public FlverSelection Selection;

    public CollisionGeneratorTool(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
        Selection = new(view, project);
    }

    public void Display()
    {
        if (!CFG.Current.HavokEditor_ToolVisibility_CollisionGenerator)
            return;

        if(ImGui.CollapsingHeader($"{LOC.Get("HAVOK_Tools_Collision_Generator_Tool")}##collisionGeneratorTool"))
        {
            ImGui.BeginChild("CollisionGeneratorSection", ImGuiChildFlags.Borders);

            if (View.Selection.BinderFileEntry == null)
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

        // Parameters
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("HAVOK_CollisionGen_Parameters_Header"),
            LOC.Get("HAVOK_CollisionGen_Parameters_Header_TT"));

        if (TargetShape is CollisionGeneratorShape.Square)
        {
            var width = SquareInputs.Width;
            ImGui.InputInt("Width##squareWidth", ref width);
            if(ImGui.IsItemDeactivatedAfterEdit())
            {
                SquareInputs.Width = width;
            }

            var length = SquareInputs.Length;
            ImGui.InputInt("Length##squareLength", ref length);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                SquareInputs.Length = length;
            }
        }

        if (TargetShape is CollisionGeneratorShape.Triangle)
        {
            var size = TriangleInputs.Size;
            ImGui.InputInt("Size##triangleSize", ref size);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                TriangleInputs.Size = size;
            }
        }

        if (TargetShape is CollisionGeneratorShape.Circle)
        {
            var radius = CircleInputs.Radius;
            ImGui.InputInt("Radius##circleRadius", ref radius);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                CircleInputs.Radius = radius;
            }

            var segments = CircleInputs.Segments;
            ImGui.InputInt("Segments##circleSegements", ref segments);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                CircleInputs.Segments = segments;
            }
        }

        if (TargetShape is CollisionGeneratorShape.SemiCircle)
        {
            var radius = SemiCircleInputs.Radius;
            ImGui.InputInt("Radius##semicircleRadius", ref radius);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                SemiCircleInputs.Radius = radius;
            }

            var segments = SemiCircleInputs.Segments;
            ImGui.InputInt("Segments##semicircleSegements", ref segments);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                SemiCircleInputs.Segments = segments;
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
            if (Selection.FlverPath != "")
            {
                GUI.WrappedText(LOC.Get("HAVOK_CollisionGen_Flver_Current_Source_FLVER", Path.GetFileName(Selection.FlverPath)));
            }
            else
            {
                GUI.WrappedText(LOC.Get("HAVOK_CollisionGen_Flver_Current_Source_FLVER_None"));
            }

            if (Selection.SourceFlver != null)
            {
                GUI.Spacer();
                GUI.SimpleHeader(
                    LOC.Get("HAVOK_CollisionGen_Flver_Meshes_Header"),
                    LOC.Get("HAVOK_CollisionGen_Flver_Meshes_Header_TT"));

                if (ImGui.Button($"{LOC.Get("HAVOK_CollisionGen_Select_All_Meshes")}##flverMeshSelectAll"))
                {
                    Array.Fill(Selection.FlverMeshSelection, true);
                }
                ImGui.SameLine();
                if (ImGui.Button($"{LOC.Get("HAVOK_CollisionGen_Select_No_Meshes")}##flverMeshSelectNone"))
                {
                    Array.Fill(Selection.FlverMeshSelection, false);
                }

                ImGui.BeginChild("flverMeshSelectionList", new Vector2(0, 150), ImGuiChildFlags.Borders);

                for (int i = 0; i < Selection.SourceFlver.Meshes.Count; i++)
                {
                    var mesh = Selection.SourceFlver.Meshes[i];
                    var material = Selection.SourceFlver.Materials[mesh.MaterialIndex];
                    var label = LOC.Get("HAVOK_CollisionGen_Flver_Mesh_Entry", i, material.Name, mesh.Vertices.Count);

                    var selected = Selection.FlverMeshSelection[i];
                    if (ImGui.Checkbox($"{label}##flverMesh{i}", ref selected))
                    {
                        Selection.FlverMeshSelection[i] = selected;
                    }
                }

                ImGui.EndChild();
            }
        }

        // TODO: need to discover method materials are applied for the extern meshes
        // Materials
        //GUI.Spacer();
        //GUI.SimpleHeader(
        //    LOC.Get("HAVOK_CollisionGen_Materials_Header"),
        //    LOC.Get("HAVOK_CollisionGen_Materials_Header_TT"));

        //ImGui.Checkbox($"{LOC.Get("HAVOK_CollisionGen_Include_Materials_Toggle")}##includeMaterials", ref BuildMaterialPalette);
        //GUI.Tooltip(LOC.Get("HAVOK_CollisionGen_Include_Materials_Toggle_TT"));

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

    public void SelectFlverSource()
    {
        ImGui.OpenPopup("flverSelectionMenuPopup");
    }

    public void DisplayFlverSelectionMenu()
    {
        if (ImGui.BeginPopup("flverSelectionMenuPopup"))
        {
            Selection.DisplayFlverSelectionTabs();

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
                Selection.SourceFlver = FLVER2.Read(fileData);

                Selection.FlverPath = path;
                Selection.FlverMeshSelection = new bool[Selection.SourceFlver.Meshes.Count];
                Array.Fill(Selection.FlverMeshSelection, true);
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
            var (verts, indices) = HKLib_MeshBuilder.GenerateSquare(width: SquareInputs.Width, length: SquareInputs.Length);

            if (TargetFacing is CollisionGeneratorFacing.Up)
                HKLib_MeshBuilder.FlipTriangleWinding(indices);

            BuildAndReplaceShape(verts, indices);
        }
        else if (TargetShape is CollisionGeneratorShape.Triangle)
        {
            var (verts, indices) = HKLib_MeshBuilder.GenerateTriangle(size: TriangleInputs.Size);

            if (TargetFacing is CollisionGeneratorFacing.Up)
                HKLib_MeshBuilder.FlipTriangleWinding(indices);

            BuildAndReplaceShape(verts, indices);
        }
        else if (TargetShape is CollisionGeneratorShape.Circle)
        {
            var (verts, indices) = HKLib_MeshBuilder.GenerateCircle(radius: CircleInputs.Radius, segments: CircleInputs.Segments);

            if (TargetFacing is CollisionGeneratorFacing.Up)
                HKLib_MeshBuilder.FlipTriangleWinding(indices);

            BuildAndReplaceShape(verts, indices);
        }
        else if (TargetShape is CollisionGeneratorShape.SemiCircle)
        {
            var (verts, indices) = HKLib_MeshBuilder.GenerateSemiCircle(radius: SemiCircleInputs.Radius, segments: SemiCircleInputs.Segments);

            if (TargetFacing is CollisionGeneratorFacing.Up)
                HKLib_MeshBuilder.FlipTriangleWinding(indices);

            BuildAndReplaceShape(verts, indices);
        }
        else if (TargetShape is CollisionGeneratorShape.FLVER)
        {
            if (Selection.SourceFlver == null || Selection.FlverMeshSelection.Length == 0)
                return;

            var selectedMeshIndices = new List<int>();
            for (int i = 0; i < Selection.FlverMeshSelection.Length; i++)
            {
                if (Selection.FlverMeshSelection[i])
                    selectedMeshIndices.Add(i);
            }

            if (selectedMeshIndices.Count == 0)
                return;

            var (verts, indices) = HKLib_MeshBuilder.GenerateFlverCollision(Selection.SourceFlver, selectedMeshIndices);

            if (verts.Count == 0 || indices.Count == 0)
                return;

            if (TargetFacing is CollisionGeneratorFacing.Up)
                HKLib_MeshBuilder.FlipTriangleWinding(indices);

            BuildAndReplaceShape(verts, indices);
        }
    }

    public void BuildAndReplaceShape(List<Vector3> verts, List<int> indices)
    {
        var collisionName = Path.GetFileName(View.Selection.FilePath);
        var sourceObject = View.PropertyView.GetSourceObject();

        var materials = HavokTreeSearch.FindAll<hknpMaterial>(sourceObject, View.PropertyCache.GetCachedHavokFields);

        if (materials.Count == 0)
            return;

        var defaultMat = materials.First();
        var defaultMatDesc = HKLib_MeshBuilder.MakeMaterialDescriptor(0, defaultMat, "Default");

        if (BuildMaterialPalette)
        {
            var shape = HKLib_MeshBuilder.BuildExternMeshShape(
                verts, indices, new[] { defaultMatDesc });

            if (HKLib_MeshBuilder.ReplaceExternMeshShape((hkRootLevelContainer)sourceObject, shape))
            {
                Smithbox.Log(this, LOC.Get("HAVOK_CollisionGen_Generate_Log", collisionName));
            }
        }
        else
        {
            var shape = HKLib_MeshBuilder.BuildExternMeshShape(
                verts, indices);

            if (HKLib_MeshBuilder.ReplaceExternMeshShape((hkRootLevelContainer)sourceObject, shape))
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

    public enum CollisionGeneratorFacing
    {
        [Display(Name = "HAVOK_CollisionGeneratorFacing_Up")]
        Up,
        [Display(Name = "HAVOK_CollisionGeneratorFacing_Down")]
        Down
    }

    public class SquareParameters
    {
        public int Length { get; set; } = 1;
        public int Width { get; set; } = 1;
    }

    public class TriangeParameters
    {
        public int Size { get; set; } = 1;
    }

    public class CircleParameters
    {
        public int Radius { get; set; } = 1;
        public int Segments { get; set; } = 24;
    }

    public class SemiCircleParameters
    {
        public int Radius { get; set; } = 1;
        public int Segments { get; set; } = 24;
    }
}